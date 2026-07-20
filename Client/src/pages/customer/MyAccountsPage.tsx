import { useEffect, useState, type FormEvent } from "react";
import type { AccountListDto, BranchListDto, CurrencyListDto } from "@/types";
import * as customerService from "@/services/customer";
import { FeedbackBar } from "@/components/ui";

export function MyAccountsPage() {
  const [accounts, setAccounts] = useState<AccountListDto[]>([]);
  const [branches, setBranches] = useState<BranchListDto[]>([]);
  const [currencies, setCurrencies] = useState<CurrencyListDto[]>([]);
  const [form, setForm] = useState({ branchId: "", currencyCode: "" });
  const [message, setMessage] = useState("");
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);

  async function load() {
    setLoading(true);
    const r = await customerService.getAccounts();
    if (r.success && r.data) setAccounts(r.data);
    setLoading(false);
  }

  useEffect(() => {
    load();
    customerService.getBranches().then((r) => r.success && setBranches(r.data));
    customerService.getCurrencies().then((r) => r.success && setCurrencies(r.data));
  }, []);

  const combinedBalance = accounts.filter((a) => a.isActive).reduce((total, a) => total + a.balance, 0);
  const activeCount = accounts.filter((a) => a.isActive).length;

  async function handleCreate(e: FormEvent) {
    e.preventDefault();
    setMessage("");
    setLoading(true);
    const r = await customerService.createAccount(Number(form.branchId), form.currencyCode);
    setSuccess(r.success);
    setMessage(r.message);
    if (r.success) { setForm({ branchId: "", currencyCode: "" }); load(); }
    setLoading(false);
  }

  return (
    <>
      <h1>My Accounts</h1>
      <p className="page-subtitle">Manage your accounts, balances, and currencies in one place.</p>

      <section className="accounts-summary-row">
        <div className="accounts-summary-card accounts-summary-primary">
          <span className="customer-eyebrow">Portfolio overview</span>
          <strong>{combinedBalance.toFixed(2)}</strong>
          <small>Combined balance across {accounts.length} account{accounts.length === 1 ? "" : "s"}</small>
        </div>
        <div className="accounts-summary-card">
          <span className="customer-eyebrow">Active accounts</span>
          <strong>{accounts.length}</strong>
          <small>{activeCount} active{activeCount !== accounts.length ? `, ${accounts.length - activeCount} inactive` : ""}</small>
        </div>
      </section>

      <section className="account-opening-panel">
        <div className="account-opening-copy">
          <span className="customer-eyebrow">Grow your portfolio</span>
          <h2>Open a new account</h2>
          <p>Choose a branch and currency to create an account.</p>
        </div>
        <form onSubmit={handleCreate} className="account-opening-form">
          <div className="customer-field-strip customer-form-stack">
            <label>
              Branch
              <select value={form.branchId} onChange={(e) => setForm({ ...form, branchId: e.target.value })} required>
                <option value="">Select branch</option>
                {branches.map((b) => (
                  <option key={b.branchId} value={b.branchId}>{b.branchName} ({b.city})</option>
                ))}
              </select>
            </label>
            <label>
              Currency
              <select value={form.currencyCode} onChange={(e) => setForm({ ...form, currencyCode: e.target.value })} required>
                <option value="">Select currency</option>
                {currencies.map((c) => (
                  <option key={c.currencyCode} value={c.currencyCode}>{c.currencyCode} - {c.currencyName}</option>
                ))}
              </select>
            </label>
          </div>
          <button type="submit" disabled={loading} className="customer-primary-button">
            {loading ? "Creating..." : "Create Account"}
          </button>
        </form>
        <FeedbackBar message={message} type={success ? "success" : "error"} />
      </section>

      {loading && <p className="status">Loading...</p>}

      <section className="account-cards-section">
        <div className="customer-card-heading">
          <div><span className="customer-eyebrow">Your money</span><h2>Accounts</h2></div>
          <span className="account-count-badge">{accounts.length} total</span>
        </div>
        {accounts.length === 0 && !loading ? (
          <div className="account-empty-state">Your accounts will appear here after you open one.</div>
        ) : (
          <div className="account-cards-grid">
            {accounts.map((account) => (
              <article className={`account-detail-card${account.isActive ? "" : " inactive"}`} key={account.accountId}>
                <div className="account-detail-top">
                  <span className="account-currency-badge">{account.currencyCode}</span>
                  {!account.isActive && <span className="account-inactive-badge">Inactive</span>}
                </div>
                <span className="account-balance-label">Available balance</span>
                <strong className="account-detail-balance">{account.balance.toFixed(2)} <small>{account.currencyCode}</small></strong>
                <div className="account-detail-footer"><span>Branch {account.branchId}</span><span>{account.isActive ? "Active" : "Inactive"}</span></div>
              </article>
            ))}
          </div>
        )}
      </section>
    </>
  );
}
