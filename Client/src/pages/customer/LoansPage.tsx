import { useEffect, useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import type { LoanListDto, LoanTypeDto, AccountListDto } from "@/types";
import * as loanService from "@/services/customerLoans";
import * as customerService from "@/services/customer";
import { FeedbackBar, formatDate } from "@/components/ui";

export function LoansPage() {
  const navigate = useNavigate();
  const [loans, setLoans] = useState<LoanListDto[]>([]);
  const [loanTypes, setLoanTypes] = useState<LoanTypeDto[]>([]);
  const [accounts, setAccounts] = useState<AccountListDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [showApply, setShowApply] = useState(false);
  const [form, setForm] = useState({ loanTypeId: "", amount: "", termMonths: "", disbursementAccountId: "", paymentAccountId: "" });
  const [message, setMessage] = useState("");
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    setLoading(true);
    Promise.all([
      loanService.myLoans(),
      customerService.getAccounts(),
    ]).then(([loanR, accR]) => {
      if (loanR.success && loanR.data) setLoans(loanR.data);
      if (accR.success && accR.data) setAccounts(accR.data);
      setLoading(false);
    });
    customerService.getExchangeRates().finally(() => {});
  }, []);

  async function handleApply(e: FormEvent) {
    e.preventDefault(); setMessage("");
    if (!form.loanTypeId || !form.amount || !form.termMonths || !form.disbursementAccountId || !form.paymentAccountId) {
      setMessage("All fields are required."); return;
    }
    setLoading(true);
    const r = await loanService.apply({
      loanTypeId: Number(form.loanTypeId),
      amount: Number(form.amount),
      termMonths: Number(form.termMonths),
      disbursementAccountId: Number(form.disbursementAccountId),
      paymentAccountId: Number(form.paymentAccountId),
    });
    setSuccess(r.success); setMessage(r.message);
    if (r.success) { setShowApply(false); setForm({ loanTypeId: "", amount: "", termMonths: "", disbursementAccountId: "", paymentAccountId: "" }); loanService.myLoans().then((res) => res.success && setLoans(res.data)); }
    setLoading(false);
  }

  async function handleLoanTypeChange(loanTypeId: string) {
    setForm({ ...form, loanTypeId });
    const lt = loanTypes.find((t) => t.loanTypeId === Number(loanTypeId));
    if (lt) {
      setForm((prev) => ({ ...prev, loanTypeId, amount: lt.minAmount.toString(), termMonths: lt.minTermMonths.toString() }));
    }
  }

  useEffect(() => {
    if (!showApply) return;
    loanService.getTypes()
      .then((res) => { if (res.success && res.data) setLoanTypes(res.data); })
      .catch(() => {});
  }, [showApply]);

  const activeLoans = loans.filter((l) => l.status === "Active" || l.status === "Pending");
  const paidLoans = loans.filter((l) => l.status === "Paid");

  return (
    <>
      <h1>My Loans</h1>
      <p className="page-subtitle">Apply for a loan or manage your existing loans.</p>

      <section className="bills-summary-row">
        <div className="bills-summary-card bills-summary-highlight">
          <span className="customer-eyebrow">Active</span>
          <strong>{activeLoans.length}</strong>
          <small>Active/Pending loans</small>
        </div>
        <div className="bills-summary-card">
          <span className="customer-eyebrow">Paid</span>
          <strong>{paidLoans.length}</strong>
          <small>Completed loans</small>
        </div>
        <div className="bills-summary-card">
          <span className="customer-eyebrow">Total</span>
          <strong>{loans.length}</strong>
          <small>All loans</small>
        </div>
      </section>

      <FeedbackBar message={message} type={success ? "success" : "error"} />

      {!showApply && (
        <button type="button" onClick={() => setShowApply(true)} className="customer-primary-button" style={{ marginBottom: 20 }}>
          + Apply for Loan
        </button>
      )}

      {showApply && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Apply</p><h2>New Loan Application</h2></div></div>
          <form onSubmit={handleApply} className="customer-form-shell">
            <div className="customer-field-strip customer-form-stack">
              <select value={form.loanTypeId} onChange={(e) => handleLoanTypeChange(e.target.value)} required>
                <option value="">Select loan type</option>
                {loanTypes.map((t) => <option key={t.loanTypeId} value={t.loanTypeId}>{t.name} ({(t.annualInterestRate * 100).toFixed(1)}%)</option>)}
              </select>
              <input type="number" step="0.01" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} placeholder="Amount *" required />
              <input type="number" value={form.termMonths} onChange={(e) => setForm({ ...form, termMonths: e.target.value })} placeholder="Term (months) *" required />
              <select value={form.disbursementAccountId} onChange={(e) => setForm({ ...form, disbursementAccountId: e.target.value })} required>
                <option value="">Select disbursement account</option>
                {accounts.filter((a) => a.isActive).map((a) => <option key={a.accountId} value={a.accountId}>#{a.accountId} — {a.balance} {a.currencyCode}</option>)}
              </select>
              <select value={form.paymentAccountId} onChange={(e) => setForm({ ...form, paymentAccountId: e.target.value })} required>
                <option value="">Select payment account</option>
                {accounts.filter((a) => a.isActive).map((a) => <option key={a.accountId} value={a.accountId}>#{a.accountId} — {a.balance} {a.currencyCode}</option>)}
              </select>
            </div>
            <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Applying..." : "Submit Application"}</button>
          </form>
          <button type="button" onClick={() => setShowApply(false)} className="customer-inline-button" style={{ marginTop: 8 }}>Cancel</button>
        </section>
      )}

      <section className="bills-list-section">
        <div className="customer-card-heading">
          <div><span className="customer-eyebrow">Your Loans</span><h2>Loans</h2></div>
        </div>
        {loading && <p>Loading...</p>}
        {loans.length === 0 && !loading ? (
          <div style={{ padding: 24, textAlign: "center", color: "#94a3b8" }}>No loans yet.</div>
        ) : (
          <div className="bills-card-grid">
            {loans.map((l) => (
              <article key={l.loanId} className="bill-detail-card" style={{ cursor: "pointer" }} onClick={() => navigate(`/customer/loans/${l.loanId}`)}>
                <div className="bill-card-top">
                  <div><span className="bill-type-label">{l.loanTypeName}</span><small>Loan #{l.loanId}</small></div>
                  <span className={`bill-status-pill ${l.status === "Paid" ? "paid" : l.status === "Pending" ? "" : "unpaid"}`}>{l.status}</span>
                </div>
                <strong className="bill-amount">{l.amount.toFixed(2)} <small>TRY</small></strong>
                <div style={{ fontSize: 12, color: "#64748b", marginTop: 4 }}>
                  {l.status === "Active" && <>Payments: {l.paymentsMade}/{l.termMonths} | {l.monthlyPayment.toFixed(2)}/month | Remaining: {l.remainingPrincipal.toFixed(2)}</>}
                  {l.status === "Pending" && <>Applied: {formatDate(l.appliedAt)}</>}
                  {l.status === "Paid" && <>Completed</>}
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
    </>
  );
}
