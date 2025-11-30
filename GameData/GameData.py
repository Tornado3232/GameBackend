import os, json
from datetime import datetime, timedelta
import pandas as pd

BASE = os.path.dirname(os.path.dirname(__file__))
PROJECT = os.path.join(BASE,"GameData")
DATA = os.path.join(PROJECT, "Files")
REPORTS = os.path.join(BASE, "Reports")
os.makedirs(REPORTS, exist_ok=True)

# Load CSVs
pr_path = os.path.join(DATA, "purchases_raw.csv")
cf_path = os.path.join(DATA, "confirmed_purchases.csv")
c_path = os.path.join(DATA, "costs_daily.csv")
s_path = os.path.join(DATA, "sessions.csv")

purchases_raw = pd.read_csv(pr_path)
confirmed = pd.read_csv(cf_path)
costs = pd.read_csv(c_path)
sessions = pd.read_csv(s_path)

# Normalize purchases_raw
purchases_raw["revenue_usd"] = purchases_raw["revenue_usd"].astype(str).str.replace(",", ".", regex=False)
purchases_raw["revenue_usd"] = pd.to_numeric(purchases_raw["revenue_usd"], errors="coerce").fillna(0.0)
purchases_raw["campaign"] = purchases_raw["campaign"].astype(str).str.strip()
purchases_raw["campaign_norm"] = purchases_raw["campaign"].str.upper()

purchases = purchases_raw.copy()
if "status" in purchases.columns:
    purchases = purchases[purchases["status"].str.lower() == "success"]
if "event_name" in purchases.columns:
    purchases = purchases[purchases["event_name"].str.lower() == "purchase"]
purchases = purchases[purchases["revenue_usd"] > 0]

purchases["composite"] = purchases["appsflyer_id"].astype(str) + "|" + purchases["event_time_utc"].astype(str) + "|" + purchases["event_name"].astype(str) + "|" + purchases["revenue_usd"].astype(str)
purchases = purchases.sort_values(["event_time_utc"]).drop_duplicates(subset=["composite"], keep="first")

# chargeback receipts zero-out
if "receipt_id" in purchases.columns and "status" in purchases_raw.columns:
    cb = purchases_raw[purchases_raw["status"].str.lower() == "chargeback"]
    bad = set(cb["receipt_id"].astype(str)) if "receipt_id" in cb.columns else set()
    purchases.loc[purchases["receipt_id"].astype(str).isin(bad), "revenue_usd"] = 0.0

curated_path = os.path.join(REPORTS, "purchases_curated.csv")
purchases.to_csv(curated_path, index=False)

# Reconciliation ±10m by appsflyer_id
purchases["event_dt"] = pd.to_datetime(purchases["event_time_utc"], utc=True)
confirmed["event_dt"] = pd.to_datetime(confirmed["event_time_utc"], utc=True)

details = []
matched_c = set()
conf_by_af = confirmed.groupby("appsflyer_id")
for idx, row in purchases.iterrows():
    af = row["appsflyer_id"]
    sub = conf_by_af.get_group(af) if af in conf_by_af.groups else None
    if sub is None or sub.empty:
        details.append({"type":"af_only","appsflyer_id":af,"event_time_utc":row["event_time_utc"],"revenue_usd":float(row["revenue_usd"])})
    else:
        diff = (sub["event_dt"] - row["event_dt"]).abs()
        md = diff.min()
        if pd.isna(md) or md > pd.Timedelta(minutes=10):
            details.append({"type":"af_only","appsflyer_id":af,"event_time_utc":row["event_time_utc"],"revenue_usd":float(row["revenue_usd"])})
        else:
            j = diff.idxmin()
            details.append({"type":"matched","appsflyer_id":af,"af_event_time_utc":row["event_time_utc"],"cf_event_time_utc":str(sub.loc[j,"event_time_utc"]),"revenue_usd":float(row["revenue_usd"])})
            matched_c.add(j)

for idx, row in confirmed.iterrows():
    if idx not in matched_c:
        details.append({"type":"confirmed_only","appsflyer_id":row["appsflyer_id"],"event_time_utc":row["event_time_utc"],"revenue_usd":float(row["revenue_usd"])})

summary = {
    "matched": sum(1 for d in details if d["type"]=="matched"),
    "af_only": sum(1 for d in details if d["type"]=="af_only"),
    "confirmed_only": sum(1 for d in details if d["type"]=="confirmed_only"),
}
with open(os.path.join(REPORTS,"reconciliation.json"),"w",encoding="utf-8") as f:
    json.dump({"summary":summary,"details":details}, f, ensure_ascii=False, indent=2)

# ROAS D-1 + Anomaly
purchases["date"] = purchases["event_dt"].dt.strftime("%Y-%m-%d")
rev_daily = purchases.groupby(["date","campaign_norm"], as_index=False)["revenue_usd"].sum().rename(columns={"campaign_norm":"campaign"})

costs["campaign"] = costs["campaign"].str.upper().str.strip()
roas = rev_daily.merge(costs, how="left", on=["date","campaign"])
roas["roas"] = (roas["revenue_usd"] / roas["ad_cost_usd"]).fillna(0.0)

if not roas.empty:
    dates_sorted = sorted(roas["date"].unique())
    d1 = dates_sorted[-2] if len(dates_sorted)>=2 else dates_sorted[-1]
    roas_d1 = roas[roas["date"]==d1].copy()

    anomalies = []
    for camp in roas["campaign"].dropna().unique():
        hist = roas[(roas["campaign"]==camp) & (roas["date"]<=d1)]
        hist_dates = sorted(hist["date"].unique())
        last7 = hist[hist["date"].isin(hist_dates[-7:])]
        avg7 = last7["roas"].mean() if not last7.empty else 0.0
        row = roas_d1[roas_d1["campaign"]==camp]
        if not row.empty:
            val = float(row["roas"].iloc[0])
            anomalies.append({"date":d1,"campaign":camp,"roas_d1":val,"avg7":float(avg7),"anomaly":(val < 0.5*float(avg7)) if avg7>0 else False})

    with open(os.path.join(REPORTS,"roas_d1.json"),"w",encoding="utf-8") as f:
        json.dump(json.loads(roas_d1.to_json(orient="records")), f, indent=2)
    with open(os.path.join(REPORTS,"roas_anomaly.json"),"w",encoding="utf-8") as f:
        json.dump(anomalies, f, indent=2)
else:
    open(os.path.join(REPORTS,"roas_d1.json"),"w").write("[]")
    open(os.path.join(REPORTS,"roas_anomaly.json"),"w").write("[]")

# ARPDAU D-1
sessions["date"] = pd.to_datetime(sessions["event_timestamp_utc"], utc=True).dt.strftime("%Y-%m-%d")
dau = sessions.groupby("date")["user_id"].nunique().reset_index().rename(columns={"user_id":"dau"})
rev = rev_daily.rename(columns={"revenue_usd":"revenue"})
arpdau = rev.merge(dau, how="left", on="date")
arpdau["arpdau"] = (arpdau["revenue"] / arpdau["dau"]).fillna(0.0)

if not arpdau.empty:
    dates_sorted = sorted(arpdau["date"].unique())
    d1 = dates_sorted[-2] if len(dates_sorted)>=2 else dates_sorted[-1]
    arpdau_d1 = arpdau[arpdau["date"]==d1].copy()
    with open(os.path.join(REPORTS,"arpdau_d1.json"),"w",encoding="utf-8") as f:
        json.dump(json.loads(arpdau_d1.to_json(orient="records")), f, indent=2)
else:
    open(os.path.join(REPORTS,"arpdau_d1.json"),"w").write("[]")

print("Pipeline OK → reports/*.json written.")
