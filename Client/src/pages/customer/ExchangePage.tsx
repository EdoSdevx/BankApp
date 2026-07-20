import { useEffect, useState, type FormEvent } from "react";
import type { AccountListDto, ExchangeRateListDto } from "@/types";
import * as customerService from "@/services/customer";
import { FeedbackBar } from "@/components/ui";

export function ExchangePage() {
  const [accounts, setAccounts] = useState<AccountListDto[]>([]);
  const [rates, setRates] = useState<ExchangeRateListDto[]>([]);
  const [form, setForm] = useState({ sourceAccountId: "", targetAccountId: "", amount: "" });
  const [message, setMessage] = useState("");
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    customerService.getAccounts().then((r) => r.success && setAccounts(r.data));
    customerService.getExchangeRates().then((r) => r.success && setRates(r.data));
  }, []);

  const selectedSource = accounts.find((a) => a.accountId === Number(form.sourceAccountId));
  const selectedTarget = accounts.find((a) => a.accountId === Number(form.targetAccountId));

  const sourceAccounts = accounts.filter((a) => a.accountId !== Number(form.targetAccountId));
  const targetAccounts = accounts.filter((a) => {
    const src = accounts.find((x) => x.accountId === Number(form.sourceAccountId));
    return src ? a.currencyCode !== src.currencyCode : true;
  });

  const getRate = (code: string) => code === "TRY" ? 1 : (rates.find((r) => r.currencyCode === code)?.rate ?? 0);

  const srcRate = selectedSource ? getRate(selectedSource.currencyCode) : 0;
  const tgtRate = selectedTarget ? getRate(selectedTarget.currencyCode) : 0;
  const sourceAmount = srcRate > 0 && tgtRate > 0 && form.amount ? (Number(form.amount) * (tgtRate / srcRate)).toFixed(2) : "0.00";

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setMessage("");
    if (form.sourceAccountId === form.targetAccountId) {
      setMessage("Source and target accounts must be different.");
      return;
    }
    setLoading(true);
    const r = await customerService.exchange(
      Number(form.sourceAccountId),
      Number(form.targetAccountId),
      Number(form.amount),
    );
    setSuccess(r.success);
    setMessage(r.message);
    if (r.success) {
      setForm({ sourceAccountId: "", targetAccountId: "", amount: "" });
      customerService.getAccounts().then((res) => res.success && setAccounts(res.data));
    }
    setLoading(false);
  }

  return (
    <>
      <h1>Currency Exchange</h1>
      <p className="page-subtitle">Move money between your accounts using the latest available exchange rates.</p>

      <section className="exchange-panel">
        <div className="exchange-panel-heading">
          <div><span className="customer-eyebrow">Currency service</span><h2>Exchange between accounts</h2></div>
          <span className="exchange-rate-badge">Live rates</span>
        </div>
        <form onSubmit={handleSubmit} className="customer-form-shell">
          <div className="exchange-account-grid">
            <label>
              <span>Pay from</span>
              <select value={form.sourceAccountId} onChange={(e) => setForm({ ...form, sourceAccountId: e.target.value, targetAccountId: "" })} required>
                <option value="">Select source account</option>
                {sourceAccounts.map((a) => (
                  <option key={a.accountId} value={a.accountId}>
                    #{a.accountId} — {a.currencyCode} ({a.balance.toFixed(2)})
                  </option>
                ))}
              </select>
            </label>

            <label>
              <span>Receive to</span>
              <select value={form.targetAccountId} onChange={(e) => setForm({ ...form, targetAccountId: e.target.value })} required>
                <option value="">Select target account</option>
                {targetAccounts.map((a) => (
                  <option key={a.accountId} value={a.accountId}>
                    #{a.accountId} — {a.currencyCode} ({a.balance.toFixed(2)})
                  </option>
                ))}
              </select>
            </label>

            <label>
              <span>Amount to receive</span>
              <input type="number" step="0.01" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} required placeholder="0.00" />
            </label>
          </div>

          {selectedSource && selectedTarget && selectedSource.currencyCode !== selectedTarget.currencyCode && form.amount && (
            <div className="exchange-preview-card">
              <span className="customer-eyebrow">Conversion preview</span>
              <div className="exchange-preview-rate">1 {selectedTarget.currencyCode} = {(tgtRate / srcRate).toFixed(4)} {selectedSource.currencyCode}</div>
              <div className="exchange-preview-result"><span>You pay</span><strong>{sourceAmount} {selectedSource.currencyCode}</strong><span>You receive</span><strong>{form.amount} {selectedTarget.currencyCode}</strong></div>
            </div>
          )}

          <button type="submit" disabled={loading || accounts.length < 2} className="customer-primary-button exchange-submit-button">
            {loading ? "Exchanging..." : "Exchange"}
          </button>
        </form>
        <FeedbackBar message={message} type={success ? "success" : "error"} />
      </section>
    </>
  );
}
