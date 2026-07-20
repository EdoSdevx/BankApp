import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import type { CustomerDashboardDto, AccountListDto, TransactionListDto } from "@/types";
import * as customerService from "@/services/customer";
import { EmptyState, formatDate, StatusDot } from "@/components/ui";

export function CustomerDashboardPage() {
  const { isAuthenticated, user } = useAuth();
  const navigate = useNavigate();
  const [dto, setDto] = useState<CustomerDashboardDto | null>(null);
  const [accounts, setAccounts] = useState<AccountListDto[]>([]);
  const [transactions, setTransactions] = useState<TransactionListDto[]>([]);

  useEffect(() => {
    if (!isAuthenticated) return;
    customerService.getDashboard().then((r) => r.success && setDto(r.data));
    customerService.getAccounts().then((r) => r.success && setAccounts(r.data));
    customerService.getTransactions().then((r) => r.success && setTransactions(r.data.slice(0, 5)));
  }, [isAuthenticated]);

  return (
    <>
      <section className="customer-welcome-panel">
        <div>
          <span className="customer-eyebrow">Personal banking</span>
          <h1>Good to see you, {user?.fullName?.split(" ")[0] ?? "there"}.</h1>
          <p>Here is your financial overview for today.</p>
        </div>
        <div className="customer-balance-highlight">
          <span>Combined balance</span>
          <strong>{dto ? dto.totalBalance.toFixed(2) : "-"}</strong>
          <small>Across your accounts</small>
        </div>
      </section>

      <section className="customer-quick-actions" aria-label="Quick actions">
        <button type="button" onClick={() => navigate("/customer/transfer")}>
          <span className="quick-action-icon">↗</span>
          <span><strong>Transfer money</strong><small>Send money securely</small></span>
        </button>
        <button type="button" onClick={() => navigate("/customer/exchange")}>
          <span className="quick-action-icon">↔</span>
          <span><strong>Exchange currency</strong><small>Convert between accounts</small></span>
        </button>
        <button type="button" onClick={() => navigate("/customer/bills")}>
          <span className="quick-action-icon">✓</span>
          <span><strong>Pay a bill</strong><small>{dto?.unpaidBillCount ?? 0} waiting for payment</small></span>
        </button>
      </section>

      <div className="customer-dashboard-grid">
        <section className="customer-dashboard-card">
          <div className="customer-card-heading">
            <div><span className="customer-eyebrow">Your money</span><h2>Accounts</h2></div>
            <button type="button" onClick={() => navigate("/customer/accounts")} className="customer-text-button">View all</button>
          </div>
          <div className="customer-account-list">
            {accounts.length === 0 ? <EmptyState label="No accounts available yet." /> : accounts.map((account) => (
              <div className="customer-account-card" key={account.accountId}>
                <div className="account-card-top"><span>{account.currencyCode}</span><small>#{account.accountId}</small></div>
                <strong>{account.balance.toFixed(2)}</strong>
                <small>Branch {account.branchId}</small>
              </div>
            ))}
          </div>
        </section>

        <section className="customer-dashboard-card">
          <div className="customer-card-heading">
            <div><span className="customer-eyebrow">Latest activity</span><h2>Recent transactions</h2></div>
            <button type="button" onClick={() => navigate("/customer/accounts")} className="customer-text-button">View accounts</button>
          </div>
          {transactions.length === 0 ? <EmptyState label="No recent transactions." /> : (
            <div className="customer-transaction-list">
              {transactions.map((transaction) => (
                <div className="customer-transaction-row" key={transaction.transactionId}>
                  <StatusDot active={transaction.transactionType === "Deposit"} />
                  <div><strong>{transaction.transactionType}</strong><small>{formatDate(transaction.transactionDate)}</small></div>
                  <b>{transaction.amount.toFixed(2)} {transaction.currencyCode}</b>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </>
  );
}
