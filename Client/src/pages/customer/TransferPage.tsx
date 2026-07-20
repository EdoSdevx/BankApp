import { useEffect, useState, type FormEvent } from "react";
import type { AccountListDto } from "@/types";
import * as customerService from "@/services/customer";
import { FeedbackBar, maskName } from "@/components/ui";

export function TransferPage() {
  const [accounts, setAccounts] = useState<AccountListDto[]>([]);
  const [form, setForm] = useState({ sourceAccountId: "", targetAccountId: "", amount: "", description: "" });
  const [message, setMessage] = useState("");
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);
  const [owner, setOwner] = useState<{ firstName: string; lastName: string } | null>(null);
  const [recentTransfers, setRecentTransfers] = useState<{ transactionType: string; amount: number; currencyCode: string; transactionDate?: string; relatedAccountId?: number | null; firstName: string | null; lastName: string | null }[]>([]);
    const [searching, setSearching] = useState(false);
    const [directForm, setDirectForm] = useState({ sourceAccountId: "", targetAccountId: "", amount: "", description: "" });
    const [directMessage, setDirectMessage] = useState("");
    const [directSuccess, setDirectSuccess] = useState(false);
    const [directLoading, setDirectLoading] = useState(false);

  useEffect(() => {
    customerService.getAccounts().then((r) => r.success && setAccounts(r.data));
  }, []);

    useEffect(() => {
      setOwner(null);
      setRecentTransfers([]);
      const id = Number(form.targetAccountId);
      if (!id || id <= 0) return;

      setSearching(true);
      const timer = setTimeout(async () => {
        const r = await customerService.lookupOwner(id);
        if (r.success && r.data) {
          setOwner({
            firstName: maskName(r.data.firstName),
            lastName: maskName(r.data.lastName),
          });
        }
        setSearching(false);
      }, 800);

      return () => clearTimeout(timer);
    }, [form.targetAccountId]);

    useEffect(() => {
      const id = Number(form.targetAccountId);
      if (!id || id <= 0) return;
      customerService.getRecentTransfers(id).then((r) => {
        if (r.success && r.data) setRecentTransfers(r.data);
      });
    }, [form.targetAccountId]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setMessage("");
    if (form.sourceAccountId === form.targetAccountId) {
      setMessage("Source and target accounts must be different.");
      return;
    }
    setLoading(true);
    const r = await customerService.transfer(
      Number(form.sourceAccountId),
      Number(form.targetAccountId),
      Number(form.amount),
      form.description || undefined,
    );
    setSuccess(r.success);
    setMessage(r.message);
    if (r.success) {
      setForm({ sourceAccountId: "", targetAccountId: "", amount: "", description: "" });
      setOwner(null);
      customerService.getAccounts().then((res) => res.success && setAccounts(res.data));
    }
    setLoading(false);
  }

  async function handleDirectTransfer(e: FormEvent) {
    e.preventDefault();
    setDirectMessage("");
    if (directForm.sourceAccountId === directForm.targetAccountId) {
      setDirectMessage("Source and target accounts must be different.");
      return;
    }
    setDirectLoading(true);
    const r = await customerService.transferBetween(
      Number(directForm.sourceAccountId),
      Number(directForm.targetAccountId),
      Number(directForm.amount),
      directForm.description || undefined,
    );
    setDirectSuccess(r.success);
    setDirectMessage(r.message);
    if (r.success) {
      setDirectForm({ sourceAccountId: "", targetAccountId: "", amount: "", description: "" });
      customerService.getAccounts().then((res) => res.success && setAccounts(res.data));
    }
    setDirectLoading(false);
  }

  return (
    <>
      <h1>Transfer Money</h1>
      <p className="page-subtitle">Send money from one of your accounts to any other account.</p>

      <section className="customer-stage">
        <p className="customer-stage-copy">Select your source account, enter the target account ID and amount.</p>
        <form onSubmit={handleSubmit} className="customer-form-shell">
          <div className="customer-field-strip customer-form-stack">
            <label>
              From Account
              <select value={form.sourceAccountId} onChange={(e) => setForm({ ...form, sourceAccountId: e.target.value })} required>
                <option value="">Select your account</option>
                {accounts.map((a) => (
                  <option key={a.accountId} value={a.accountId}>
                    #{a.accountId} — {a.balance.toFixed(2)} {a.currencyCode}
                  </option>
                ))}
              </select>
            </label>
            <label>
              To Account ID
              <input type="number" value={form.targetAccountId} onChange={(e) => setForm({ ...form, targetAccountId: e.target.value })} required placeholder="Target account ID" />
            </label>
            {form.targetAccountId && (
              <div className="recipient-lookup-card" aria-live="polite">
                <span className="recipient-lookup-label">Recipient</span>
                {searching ? (
                  <span className="recipient-lookup-status">Looking up account owner...</span>
                ) : owner ? (
                  <div className="recipient-lookup-row">
                    <span><strong>Name:</strong> {owner.firstName}</span>
                    <span><strong>Last name:</strong> {owner.lastName}</span>
                  </div>
                ) : (
                  <span className="recipient-lookup-status">Account owner not found</span>
                )}
              </div>
            )}
            {form.targetAccountId && !searching && owner && recentTransfers.length > 0 && (
              <div className="recent-transfers">
                <div className="recent-transfers-heading">
                  <span className="recipient-lookup-label">Your transfer history</span>
                  <span>Last 3 transfers</span>
                </div>
                {recentTransfers.map((t, i) => (
                  <div className={`recent-transfer-row${i < recentTransfers.length - 1 ? " with-divider" : ""}`} key={i}>
                    <span className={`recent-transfer-kind ${t.relatedAccountId === Number(form.sourceAccountId) ? "sent" : t.transactionType === 'Withdrawal' ? "sent" : "received"}`}>
                      <span className="recent-transfer-icon">{t.relatedAccountId === Number(form.sourceAccountId) || t.transactionType === 'Withdrawal' ? "↗" : "↙"}</span>
                      {t.relatedAccountId === Number(form.sourceAccountId) || t.transactionType === 'Withdrawal' ? 'Sent' : 'Received'}
                    </span>
                    <span className="recent-transfer-meta">{t.transactionDate ? new Date(t.transactionDate).toLocaleDateString("tr-TR") : "Recent"}</span>
                    <strong className="recent-transfer-amount">{t.amount.toFixed(2)} {t.currencyCode}</strong>
                  </div>
                ))}
              </div>
            )}
            <label>
              Amount
              <input type="number" step="0.01" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} required placeholder="0.00" />
            </label>
            <label>
              Description (optional)
              <input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="e.g. Rent payment" />
            </label>
          </div>
          {Number(form.amount) > 5000 && (
            <p className="status">Transfers above 5,000 require admin approval. Funds will be held until approved.</p>
          )}
          <button type="submit" disabled={loading} className="customer-primary-button">
            {loading ? "Transferring..." : "Send Transfer"}
          </button>
        </form>
        <FeedbackBar message={message} type={success ? "success" : "error"} />
      </section>

      <section className="customer-stage" style={{ marginTop: 32 }}>
        <p className="customer-stage-copy">Direct Transfer (Same Currency Only) — transfers instantly with no approval threshold.</p>
        <form onSubmit={handleDirectTransfer} className="customer-form-shell">
          <div className="customer-field-strip customer-form-stack">
            <label>
              From Account
              <select value={directForm.sourceAccountId} onChange={(e) => setDirectForm({ ...directForm, sourceAccountId: e.target.value, targetAccountId: "" })} required>
                <option value="">Select your account</option>
                {accounts.map((a) => (
                  <option key={a.accountId} value={a.accountId}>
                    #{a.accountId} — {a.balance.toFixed(2)} {a.currencyCode}
                  </option>
                ))}
              </select>
            </label>
            <label>
              To Account
              <select value={directForm.targetAccountId} onChange={(e) => setDirectForm({ ...directForm, targetAccountId: e.target.value })} required disabled={!directForm.sourceAccountId}>
                <option value="">{directForm.sourceAccountId ? "Select target account" : "Select source account first"}</option>
                {accounts
                  .filter((a) => {
                    const src = accounts.find((x) => x.accountId === Number(directForm.sourceAccountId));
                    return src && a.currencyCode === src.currencyCode && a.accountId !== src.accountId;
                  })
                  .map((a) => (
                    <option key={a.accountId} value={a.accountId}>
                      #{a.accountId} — {a.balance.toFixed(2)} {a.currencyCode}
                    </option>
                  ))}
              </select>
            </label>
            <label>
              Amount
              <input type="number" step="0.01" value={directForm.amount} onChange={(e) => setDirectForm({ ...directForm, amount: e.target.value })} required placeholder="0.00" />
            </label>
            <label>
              Description (optional)
              <input value={directForm.description} onChange={(e) => setDirectForm({ ...directForm, description: e.target.value })} placeholder="e.g. Internal transfer" />
            </label>
          </div>
          <button type="submit" disabled={directLoading} className="customer-primary-button">
            {directLoading ? "Transferring..." : "Send Direct Transfer"}
          </button>
        </form>
        <FeedbackBar message={directMessage} type={directSuccess ? "success" : "error"} />
      </section>
    </>
  );
}
