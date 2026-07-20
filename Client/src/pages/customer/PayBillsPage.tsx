import { useEffect, useState } from "react";
import type { BillListDto, AccountListDto } from "@/types";
import * as customerService from "@/services/customer";
import { StatusDot, FeedbackBar, formatDate } from "@/components/ui";

export function PayBillsPage() {
  const [bills, setBills] = useState<BillListDto[]>([]);
  const [accounts, setAccounts] = useState<AccountListDto[]>([]);
  const [selectedAccount, setSelectedAccount] = useState<Record<number, string>>({});
  const [message, setMessage] = useState("");
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);

  async function load() {
    setLoading(true);
    const r = await customerService.getBills();
    if (r.success && r.data) setBills(r.data);
    setLoading(false);
  }

  useEffect(() => {
    load();
    customerService.getAccounts().then((r) => r.success && setAccounts(r.data ?? []));
  }, []);

  const unpaidBills = bills.filter((bill) => !bill.isPaid);
  const paidBills = bills.filter((bill) => bill.isPaid);
  const sortedBills = [...bills].sort((a, b) => {
    if (a.isPaid !== b.isPaid) return a.isPaid ? 1 : -1;
    const aDate = new Date(a.isPaid ? (a.paidDate ?? a.dueDate) : a.dueDate).getTime();
    const bDate = new Date(b.isPaid ? (b.paidDate ?? b.dueDate) : b.dueDate).getTime();
    return aDate - bDate;
  });

  const eligibleAccounts = (bill: BillListDto) =>
    accounts.filter((a) =>
      a.currencyCode === (bill.currencyCode || "TRY") && a.balance >= bill.amount && a.isActive
    );

  async function handlePay(billId: number) {
    const accountId = selectedAccount[billId] ? Number(selectedAccount[billId]) : undefined;
    if (!accountId) { setMessage("Please select an account to pay from."); return; }
    setMessage("");
    setLoading(true);
    const r = await customerService.payBill(billId, accountId);
    setSuccess(r.success);
    setMessage(r.message);
    if (r.success) { setSelectedAccount((s) => { const c = { ...s }; delete c[billId]; return c; }); load(); }
    setLoading(false);
  }

  return (
    <>
      <h1>Pay Bills</h1>
      <p className="page-subtitle">View and pay your outstanding bills by selecting the account to pay from.</p>

      <section className="bills-summary-row">
        <div className="bills-summary-card bills-summary-highlight">
          <span className="customer-eyebrow">Needs attention</span>
          <strong>{unpaidBills.length}</strong>
          <small>Unpaid bill{unpaidBills.length === 1 ? "" : "s"}</small>
        </div>
        <div className="bills-summary-card">
          <span className="customer-eyebrow">Payment history</span>
          <strong>{paidBills.length}</strong>
          <small>Paid bill{paidBills.length === 1 ? "" : "s"}</small>
        </div>
        <div className="bills-summary-card">
          <span className="customer-eyebrow">All bills</span>
          <strong>{bills.length}</strong>
          <small>Total bills on your account</small>
        </div>
      </section>

      <FeedbackBar message={message} type={success ? "success" : "error"} />

      <section className="bills-list-section">
        <div className="customer-card-heading">
          <div><span className="customer-eyebrow">Billing centre</span><h2>Your bills</h2></div>
          {loading && <span className="bills-loading-label">Refreshing...</span>}
        </div>
        {bills.length === 0 && !loading ? (
          <div className="bills-empty-state">You do not have any bills to display.</div>
        ) : (
          <div className="bills-card-grid">
            {sortedBills.map((bill) => {
              const options = eligibleAccounts(bill);
              return (
              <article className={`bill-detail-card${bill.isPaid ? " bill-paid" : ""}`} key={bill.billId}>
                <div className="bill-card-top">
                  <div><span className="bill-type-label">{bill.billType}</span><small>Bill #{bill.billId}</small></div>
                  <span className={`bill-status-pill ${bill.isPaid ? "paid" : "unpaid"}`}><StatusDot active={bill.isPaid} />{bill.isPaid ? "Paid" : "Unpaid"}</span>
                </div>
                <strong className="bill-amount">{bill.amount.toFixed(2)} <small>{bill.currencyCode || "TRY"}</small></strong>
                <div className="bill-date-row"><span>{bill.isPaid ? "Paid on" : "Due on"}</span><b>{formatDate(bill.isPaid ? bill.paidDate : bill.dueDate)}</b></div>
                {!bill.isPaid && (
                  <>
                    {options.length > 0 ? (
                      <select
                        value={selectedAccount[bill.billId] ?? ""}
                        onChange={(e) => setSelectedAccount((s) => ({ ...s, [bill.billId]: e.target.value }))}
                        className="bill-account-select"
                      >
                        <option value="">Select account to pay from</option>
                        {options.map((a) => (
                          <option key={a.accountId} value={a.accountId}>
                            #{a.accountId} — {a.balance.toFixed(2)} {a.currencyCode}
                          </option>
                        ))}
                      </select>
                    ) : (
                      <p className="status" style={{ margin: 0, fontSize: 12 }}>No eligible account with sufficient balance.</p>
                    )}
                    <button type="button" onClick={() => handlePay(bill.billId)} disabled={loading || !selectedAccount[bill.billId]} className="customer-primary-button bill-pay-button">
                      {loading ? "Paying..." : options.length > 0 ? "Pay this bill" : "Insufficient funds"}
                    </button>
                  </>
                )}
              </article>
            )})}
          </div>
        )}
      </section>
    </>
  );
}
