import { useState, useEffect, type FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import type { LoanListDto, LoanDetailDto, LoanScheduleDto, LoanPaymentDto, LoanTypeDto } from "@/types";
import type { ApiResponse } from "@/types";
import * as service from "@/services/loans";
import { ModuleActionIcon } from "@/components/icons";
import { StatusDot, formatDate } from "@/components/ui";

const ACTIONS = [
  { label: "List", description: "View all loan applications with status and approve/reject controls." },
  { label: "Detail", description: "Inspect a single loan record including customer and type info." },
  { label: "Schedule", description: "View the amortization schedule for a selected loan." },
  { label: "Payments", description: "View payment history for a selected loan." },
];

export function LoansPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const action = searchParams.get("action") ?? "List";
  const [data, setData] = useState<LoanListDto[]>([]);
  const [detail, setDetail] = useState<LoanDetailDto | null>(null);
  const [schedule, setSchedule] = useState<LoanScheduleDto[]>([]);
  const [payments, setPayments] = useState<LoanPaymentDto[]>([]);
  const [viewId, setViewId] = useState<number | null>(null);
  const [message, setMessage] = useState("");
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);
  const [listSearch, setListSearch] = useState("");
  const [detailSearch, setDetailSearch] = useState("");
  const [scheduleSearch, setScheduleSearch] = useState("");
  const [paymentsSearch, setPaymentsSearch] = useState("");

  async function load() { setLoading(true); const r = await service.list(); if (r.success && r.data) setData(r.data); setLoading(false); }
  async function loadDetail(id: number) {
    setLoading(true);
    const [loanR, schedR, payR] = await Promise.all([
      service.select(id),
      service.getSchedule(id),
      service.getPayments(id),
    ]);
    if (loanR.success && loanR.data) setDetail(loanR.data);
    if (schedR.success && schedR.data) setSchedule(schedR.data);
    if (payR.success && payR.data) setPayments(payR.data);
    setLoading(false);
  }
  useEffect(() => { load(); }, []);

  function setAction(a: string, keepId = false) {
    if (!keepId) { setViewId(null); setDetail(null); setSchedule([]); setPayments([]); }
    setMessage(""); setDetailSearch(""); setScheduleSearch(""); setPaymentsSearch("");
    setSearchParams({ action: a });
  }

  const filterLoans = (query: string) => {
    if (query.startsWith("#")) return [];
    const q = query.toLowerCase();
    return data.filter((l) => {
      if (l.loanId.toString().includes(q)) return true;
      const name = `${l.customerFirstName ?? ""} ${l.customerLastName ?? ""}`;
      return name.toLowerCase().includes(q);
    }).slice(0, 8);
  };

  const searchKeyDown = (e: React.KeyboardEvent, query: string, results: ReturnType<typeof filterLoans>, setId: (id: number) => void) => {
    if (e.key === "Enter") {
      e.preventDefault();
      if (query.startsWith("#")) {
        const id = Number(query.replace(/^#/, "").trim());
        if (!isNaN(id) && id > 0) setId(id);
        return;
      }
      if (results.length > 0) setId(results[0].loanId);
    }
  };

  async function handleApprove(id: number) {
    if (!window.confirm("Approve this loan? This will disburse funds.")) return;
    setMessage(""); setLoading(true);
    const r = await service.approve(id);
    setSuccess(r.success); setMessage(r.message);
    if (r.success) load();
    setLoading(false);
  }

  async function handleReject(id: number) {
    const reason = window.prompt("Rejection reason (optional):");
    if (reason === null) return;
    setMessage(""); setLoading(true);
    const r = await service.reject(id, reason || undefined);
    setSuccess(r.success); setMessage(r.message);
    if (r.success) load();
    setLoading(false);
  }

  return (
    <>
      <h1>Loan Management</h1>
      <p className="page-subtitle">Manage loan applications, approvals, and view schedules.</p>
      <section className="customer-actions">
        {ACTIONS.map((item) => (
          <button key={item.label} type="button" className={`customer-action-card ${action === item.label ? "active" : ""}`} onClick={() => setAction(item.label)}>
            <span className="customer-action-top"><strong>{item.label}</strong><ModuleActionIcon label={item.label} /></span>
            <span className="customer-action-text">{item.description}</span>
          </button>
        ))}
      </section>

      {action === "List" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>List</h2></div><span className="customer-stage-tag">Loans / List</span></div>
          {loading && <p className="status">Loading...</p>}
          <div className="customer-toolbar">
            <input type="text" value={listSearch} onChange={(e) => setListSearch(e.target.value)}
              placeholder="Filter by loan ID or customer name..." className="list-search-input" />
            {listSearch && <button type="button" onClick={() => setListSearch("")} className="customer-inline-button">✕ Clear</button>}
          </div>
          <section className="table-card">
            <table>
              <thead><tr><th>ID</th><th>Customer</th><th>Type</th><th className="num">Amount</th><th className="num">Monthly</th><th>Status</th><th className="num">Remaining</th><th>Payments</th><th></th></tr></thead>
              <tbody>
                {data.length === 0 && !loading && <tr><td colSpan={9}>No loans.</td></tr>}
                {(() => {
                  const filtered = listSearch.trim()
                    ? data.filter((l) => {
                        const q = listSearch.toLowerCase();
                        if (l.loanId.toString().includes(q)) return true;
                        const name = `${l.customerFirstName ?? ""} ${l.customerLastName ?? ""}`;
                        return name.toLowerCase().includes(q);
                      })
                    : data;
                  if (listSearch.trim() && filtered.length === 0 && !loading) return <tr><td colSpan={9}>No matching loans.</td></tr>;
                  return filtered.map((l) => (
                  <tr key={l.loanId}>
                    <td className="mono">{l.loanId}</td>
                    <td>{l.customerFirstName} {l.customerLastName}</td>
                    <td>{l.loanTypeName}</td>
                    <td className="num mono">{l.amount.toFixed(2)}</td>
                    <td className="num mono">{l.monthlyPayment.toFixed(2)}</td>
                    <td><span className={`status-line`}>{l.status}</span></td>
                    <td className="num mono">{l.remainingPrincipal.toFixed(2)}</td>
                    <td>{l.paymentsMade}/{l.termMonths}</td>
                    <td>
                      <div className="customer-row-actions">
                        <button type="button" onClick={() => { setViewId(l.loanId); setDetailSearch(`${l.loanId}`); loadDetail(l.loanId); setAction("Detail", true); }} className="customer-inline-button view">Detail</button>
                        {l.status === "Pending" && (
                          <>
                            <button type="button" onClick={() => handleApprove(l.loanId)} disabled={loading} className="customer-inline-button">Approve</button>
                            <button type="button" onClick={() => handleReject(l.loanId)} disabled={loading} className="customer-inline-button danger">Reject</button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ));
                })()}
              </tbody>
            </table>
          </section>
        </section>
      )}

      {action === "Detail" && (() => {
        const detailResults = filterLoans(detailSearch);
        return (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Detail</h2></div><span className="customer-stage-tag">Loans / Detail</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Search loan by ID or customer name:</p>
            <div className="search-input-wrap">
              <input type="text" value={detailSearch}
                onChange={(e) => { setDetailSearch(e.target.value); setViewId(null); setDetail(null); }}
                onKeyDown={(e) => searchKeyDown(e, detailSearch, detailResults, (id) => { setViewId(id); setDetailSearch(`${id}`); loadDetail(id); })}
                placeholder="Type #123 or Ahmet..." />
              {detailResults.length > 0 && (
                <div className="search-results-dropdown">
                  {detailResults.map((l) => {
                    const name = `${l.customerFirstName ?? ""} ${l.customerLastName ?? ""}`;
                    return (
                      <button type="button" key={l.loanId}
                        onClick={() => { setViewId(l.loanId); setDetailSearch(`${l.loanId} — ${name}`); loadDetail(l.loanId); }}>
                        #{l.loanId} — {name} — {l.loanTypeName} — {l.amount} ({l.status})
                      </button>
                    );
                  })}
                </div>
              )}
            </div>
            <button type="button" onClick={() => viewId && loadDetail(viewId)} disabled={!viewId || loading} className="customer-inline-button view">View</button>
          </div>
          {detail && (
            <section className="table-card customer-detail-card">
              <table><tbody>
                <tr><td><strong>ID</strong></td><td className="mono">{detail.loanId}</td></tr>
                <tr><td><strong>Customer</strong></td><td>{detail.customerFirstName} {detail.customerLastName}</td></tr>
                <tr><td><strong>Type</strong></td><td>{detail.loanTypeName}</td></tr>
                <tr><td><strong>Amount</strong></td><td className="mono">{detail.amount.toFixed(2)}</td></tr>
                <tr><td><strong>Term</strong></td><td>{detail.termMonths} months</td></tr>
                <tr><td><strong>Interest Rate</strong></td><td>{(detail.annualInterestRate * 100).toFixed(1)}%</td></tr>
                <tr><td><strong>Monthly Payment</strong></td><td className="mono">{detail.monthlyPayment.toFixed(2)}</td></tr>
                <tr><td><strong>Status</strong></td><td>{detail.status}</td></tr>
                <tr><td><strong>Applied</strong></td><td>{formatDate(detail.appliedAt)}</td></tr>
                <tr><td><strong>Approved</strong></td><td>{detail.approvedAt ? formatDate(detail.approvedAt) : "-"}</td></tr>
                <tr><td><strong>Payments</strong></td><td>{detail.paymentsMade}/{detail.termMonths}</td></tr>
                <tr><td><strong>Remaining</strong></td><td className="mono">{detail.remainingPrincipal.toFixed(2)}</td></tr>
              </tbody></table>
            </section>
          )}
        </section>
        );
      })()}

      {action === "Schedule" && (() => {
        const schedResults = filterLoans(scheduleSearch);
        return (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Schedule</h2></div><span className="customer-stage-tag">Loans / Schedule</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Search loan by ID or customer name:</p>
            <div className="search-input-wrap">
              <input type="text" value={scheduleSearch}
                onChange={(e) => { setScheduleSearch(e.target.value); setViewId(null); setSchedule([]); }}
                onKeyDown={(e) => searchKeyDown(e, scheduleSearch, schedResults, (id) => { setViewId(id); setScheduleSearch(`${id}`); loadDetail(id); })}
                placeholder="Type #123 or Ahmet..." />
              {schedResults.length > 0 && (
                <div className="search-results-dropdown">
                  {schedResults.map((l) => (
                    <button type="button" key={l.loanId}
                      onClick={() => { setViewId(l.loanId); setScheduleSearch(`${l.loanId}`); loadDetail(l.loanId); }}>
                      #{l.loanId} — {l.customerFirstName} {l.customerLastName} — {l.loanTypeName}
                    </button>
                  ))}
                </div>
              )}
            </div>
            <button type="button" onClick={() => viewId && loadDetail(viewId)} disabled={!viewId || loading} className="customer-inline-button view">View Schedule</button>
          </div>
          {schedule.length > 0 && (
            <section className="table-card">
              <table>
                <thead><tr><th>#</th><th className="num">Due</th><th className="num">Principal</th><th className="num">Interest</th><th className="num">Total</th><th className="num">Remaining</th><th>Status</th></tr></thead>
                <tbody>
                  {schedule.map((s) => (
                    <tr key={s.scheduleId} className={`${s.isLate ? "row-warning" : ""}${s.isPaid ? "row-muted" : ""}`}>
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
          )}
        </section>
        );
      })()}

      {action === "Payments" && (() => {
        const payResults = filterLoans(paymentsSearch);
        return (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Payments</h2></div><span className="customer-stage-tag">Loans / Payments</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Search loan by ID or customer name:</p>
            <div className="search-input-wrap">
              <input type="text" value={paymentsSearch}
                onChange={(e) => { setPaymentsSearch(e.target.value); setViewId(null); setPayments([]); }}
                onKeyDown={(e) => searchKeyDown(e, paymentsSearch, payResults, (id) => { setViewId(id); setPaymentsSearch(`${id}`); loadDetail(id); })}
                placeholder="Type #123 or Ahmet..." />
              {payResults.length > 0 && (
                <div className="search-results-dropdown">
                  {payResults.map((l) => (
                    <button type="button" key={l.loanId}
                      onClick={() => { setViewId(l.loanId); setPaymentsSearch(`${l.loanId}`); loadDetail(l.loanId); }}>
                      #{l.loanId} — {l.customerFirstName} {l.customerLastName}
                    </button>
                  ))}
                </div>
              )}
            </div>
            <button type="button" onClick={() => viewId && loadDetail(viewId)} disabled={!viewId || loading} className="customer-inline-button view">View Payments</button>
          </div>
          {payments.length > 0 && (
            <section className="table-card">
              <table>
                <thead><tr><th>ID</th><th className="num">Amount</th><th>Type</th><th>Date</th><th>Description</th></tr></thead>
                <tbody>
                  {payments.map((p) => (
                    <tr key={p.paymentId}>
                      <td className="mono">{p.paymentId}</td>
                      <td className="num mono">{p.amount.toFixed(2)}</td>
                      <td>{p.paymentType}</td>
                      <td>{formatDate(p.paymentDate)}</td>
                      <td>{p.description || "-"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </section>
          )}
        </section>
        );
      })()}
    </>
  );
}
