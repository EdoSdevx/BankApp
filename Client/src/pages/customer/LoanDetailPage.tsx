import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { LoanDetailDto, LoanScheduleDto, LoanPaymentDto } from "@/types";
import * as loanService from "@/services/customerLoans";
import * as customerService from "@/services/customer";
import { formatDate, FeedbackBar } from "@/components/ui";

export function LoanDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [loan, setLoan] = useState<LoanDetailDto | null>(null);
  const [schedule, setSchedule] = useState<LoanScheduleDto[]>([]);
  const [accounts, setAccounts] = useState<{ accountId: number; balance: number; currencyCode: string; isActive: boolean }[]>([]);
  const [selectedAccount, setSelectedAccount] = useState("");
  const [message, setMessage] = useState("");
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    Promise.all([
      loanService.getDetail(Number(id)),
      loanService.getSchedule(Number(id)),
      customerService.getAccounts(),
    ]).then(([loanR, schedR, accR]) => {
      if (loanR.success && loanR.data) setLoan(loanR.data);
      if (schedR.success && schedR.data) setSchedule(schedR.data);
      if (accR.success && accR.data) setAccounts(accR.data);
      setLoading(false);
    });
  }, [id]);

  const nextDue = schedule.filter((s) => !s.isPaid).sort((a, b) => a.periodNumber - b.periodNumber)[0];
  const remainingPrincipal = schedule.filter((s) => !s.isPaid).reduce((sum, s) => sum + s.principal, 0);

  async function handlePay(scheduleId: number) {
    if (!selectedAccount) { setMessage("Select an account first."); return; }
    if (!window.confirm("Pay this installment?")) return;
    setLoading(true); setMessage("");
    const r = await loanService.pay(Number(id!), scheduleId, Number(selectedAccount));
    setSuccess(r.success); setMessage(r.message);
    if (r.success) {
      const [loanR, schedR] = await Promise.all([
        loanService.getDetail(Number(id)),
        loanService.getSchedule(Number(id)),
      ]);
      if (loanR.success && loanR.data) setLoan(loanR.data);
      if (schedR.success && schedR.data) setSchedule(schedR.data);
    }
    setLoading(false);
  }

  async function handleCloseEarly() {
    if (!selectedAccount) { setMessage("Select an account first."); return; }
    const penalty = remainingPrincipal * 0.02;
    const total = remainingPrincipal + penalty;
    if (!window.confirm(`Close loan early?\nRemaining: ${remainingPrincipal.toFixed(2)}\nPenalty (2%): ${penalty.toFixed(2)}\nTotal: ${total.toFixed(2)}`)) return;
    setLoading(true); setMessage("");
    const r = await loanService.closeEarly(Number(id!), Number(selectedAccount));
    setSuccess(r.success); setMessage(r.message);
    if (r.success) {
      setLoan((prev) => prev ? { ...prev, status: "Paid", closedAt: new Date().toISOString() } : prev);
      const schedR = await loanService.getSchedule(Number(id));
      if (schedR.success && schedR.data) setSchedule(schedR.data);
    }
    setLoading(false);
  }

  if (loading && !loan) return <><h1>Loan Detail</h1><p className="status">Loading...</p></>;
  if (!loan) return <><h1>Loan Detail</h1><p className="status">Loan not found.</p></>;

  return (
    <>
      <h1>Loan #{loan.loanId}</h1>
      <p className="page-subtitle">{loan.loanTypeName} — {loan.status}</p>

      <FeedbackBar message={message} type={success ? "success" : "error"} />

      <section className="customer-stage">
        <div className="customer-stage-head"><div><p>Loan Details</p></div></div>
        <div className="customer-field-strip">
          <div><strong>Amount:</strong> {loan.amount.toFixed(2)} TRY</div>
          <div><strong>Term:</strong> {loan.termMonths} months</div>
          <div><strong>Rate:</strong> {(loan.annualInterestRate * 100).toFixed(1)}%</div>
          <div><strong>Monthly:</strong> {loan.monthlyPayment.toFixed(2)} TRY</div>
          <div><strong>Status:</strong> {loan.status}</div>
          <div><strong>Payments:</strong> {loan.paymentsMade}/{loan.termMonths}</div>
        </div>
      </section>

      {loan.status === "Active" && (
        <section className="customer-stage">
          <h3>Actions</h3>
          <select value={selectedAccount} onChange={(e) => setSelectedAccount(e.target.value)} style={{ marginBottom: 8, padding: "6px 10px", border: "1px solid #e2e8f0", borderRadius: 6, width: "100%" }}>
            <option value="">Select account for payment</option>
            {accounts.filter((a) => a.isActive).map((a) => (
              <option key={a.accountId} value={a.accountId}>#{a.accountId} — {a.balance.toFixed(2)} {a.currencyCode}</option>
            ))}
          </select>
          <div style={{ display: "flex", gap: 8 }}>
            {nextDue && (
              <button type="button" onClick={() => handlePay(nextDue.scheduleId)} disabled={loading} className="customer-primary-button">
                Pay Next Installment ({nextDue.totalDue.toFixed(2)} TRY)
              </button>
            )}
            <button type="button" onClick={handleCloseEarly} disabled={loading} className="customer-inline-button danger">
              Close Early (Penalty inc.)
            </button>
          </div>
        </section>
      )}

      <section className="table-card" style={{ marginTop: 18 }}>
        <h3>Amortization Schedule</h3>
        <table>
          <thead><tr><th>#</th><th>Due</th><th className="num">Principal</th><th className="num">Interest</th><th className="num">Total</th><th className="num">Remaining</th><th>Status</th></tr></thead>
          <tbody>
            {schedule.map((s) => (
              <tr key={s.scheduleId} style={{ opacity: s.isPaid ? 0.6 : 1 }}>
                <td>{s.periodNumber}</td>
                <td className="num">{formatDate(s.dueDate)}</td>
                <td className="num mono">{s.principal.toFixed(2)}</td>
                <td className="num mono">{s.interest.toFixed(2)}</td>
                <td className="num mono">{s.totalDue.toFixed(2)}</td>
                <td className="num mono">{s.remainingBalance.toFixed(2)}</td>
                <td>{s.isPaid ? "Paid" : s.isLate ? "Late" : "Due"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <button type="button" onClick={() => navigate("/customer/loans")} className="customer-inline-button" style={{ marginTop: 12 }}>← Back to Loans</button>
    </>
  );
}
