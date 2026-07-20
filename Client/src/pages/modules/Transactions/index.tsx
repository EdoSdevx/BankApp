import { useState, useEffect, type FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import type { TransactionListDto, TransactionSelectDto } from "@/types";
import type { ApiResponse } from "@/types";
import * as service from "@/services/transactions";
import { ModuleActionIcon } from "@/components/icons";
import { formatDate } from "@/components/ui";

const ACTIONS = [
  { label: "List", description: "Browse transaction records with type, amount, currency, account, and timing metadata." },
  { label: "Create", description: "Log a new transaction against an account with type, amount, currency, and optional notes." },
  { label: "Edit", description: "Correct an existing transaction entry with updated type, amount, or currency values." },
  { label: "Detail", description: "Open a single transaction record and review full timeline and description data." },
];

export function TransactionsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const action = searchParams.get("action") ?? "List";
  const [data, setData] = useState<TransactionListDto[]>([]);
  const [detail, setDetail] = useState<TransactionSelectDto | null>(null);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState({ accountId: "", transactionType: "", amount: "", currencyCode: "", description: "" });
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  async function load() { setLoading(true); const r = await service.list(); if (r.success && r.data) setData(r.data); setLoading(false); }
  async function loadDetail(id: number) { setLoading(true); const r = await service.select(id); if (r.success && r.data) setDetail(r.data); setLoading(false); }
  useEffect(() => { load(); }, []);

  function setAction(a: string, keepId = false) { if (!keepId) setEditId(null); setDetail(null); setMessage(""); setSearchParams({ action: a }); }

  async function handleCreate(e: FormEvent) {
    e.preventDefault(); setMessage(""); setLoading(true);
    const r = await service.create({
      accountId: Number(form.accountId), transactionType: form.transactionType,
      amount: Number(form.amount), currencyCode: form.currencyCode,
      description: form.description || null,
    });
    if (r.success) { setMessage("Created."); setForm({ accountId: "", transactionType: "", amount: "", currencyCode: "", description: "" }); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleUpdate(e: FormEvent) {
    e.preventDefault(); if (!editId) return; setMessage(""); setLoading(true);
    const body: Record<string, unknown> = {};
    if (form.accountId) body.accountId = Number(form.accountId);
    if (form.transactionType) body.transactionType = form.transactionType;
    if (form.amount) body.amount = Number(form.amount);
    if (form.currencyCode) body.currencyCode = form.currencyCode;
    if (form.description) body.description = form.description || null;
    const r = await service.update(editId, body);
    if (r.success) { setMessage("Updated."); setEditId(null); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleDelete(id: number) { if (!window.confirm("Delete?")) return; await service.remove(id); load(); }

  return (
    <>
      <h1>Transactions</h1>
      <p className="page-subtitle">Select the transaction workspace you want to monitor or process.</p>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>List</h2></div><span className="customer-stage-tag">Transactions / List</span></div>
          {loading && <p className="status">Loading...</p>}
          <div className="customer-toolbar"><button type="button" onClick={() => setAction("Create")} className="customer-primary-button">+ Create New Transaction</button></div>
          <section className="table-card">
            <table>
              <thead><tr><th>ID</th><th>Account</th><th>Type</th><th className="num">Amount</th><th>Currency</th><th>Date</th><th></th></tr></thead>
              <tbody>
                {data.length === 0 && !loading && <tr><td colSpan={7}>No transactions.</td></tr>}
                {data.map((t) => (
                  <tr key={t.transactionId}>
                    <td className="mono">{t.transactionId}</td><td>{t.accountId}</td><td>{t.transactionType}</td><td className="num mono">{t.amount.toFixed(2)}</td><td>{t.currencyCode}</td><td>{formatDate(t.transactionDate)}</td>
                    <td><div className="customer-row-actions">
                       <button type="button" onClick={() => { setEditId(t.transactionId); setMessage(""); setAction("Edit", true); loadDetail(t.transactionId); }} className="customer-inline-button">Edit</button>
                      <button type="button" onClick={() => handleDelete(t.transactionId)} className="customer-inline-button danger">Delete</button>
                    </div></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        </section>
      )}

      {action === "Create" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Create</h2></div><span className="customer-stage-tag">Transactions / Create</span></div>
          <form onSubmit={handleCreate} className="customer-form-shell">
            <div className="customer-field-strip customer-form-stack">
              <input type="number" value={form.accountId} onChange={(e) => setForm({ ...form, accountId: e.target.value })} placeholder="Account ID *" required />
              <select value={form.transactionType} onChange={(e) => setForm({ ...form, transactionType: e.target.value })} required>
                <option value="" disabled>Select type</option>
                <option value="Deposit">Deposit</option><option value="Withdrawal">Withdrawal</option><option value="Transfer">Transfer</option>
              </select>
              <input type="number" step="0.01" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} placeholder="Amount *" required />
              <input value={form.currencyCode} onChange={(e) => setForm({ ...form, currencyCode: e.target.value })} placeholder="Currency Code *" required />
              <input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="Description" />
            </div>
            <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Creating..." : "Create Transaction"}</button>
          </form>
          {message && <p className={message.includes("Created") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Edit" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Edit</h2></div><span className="customer-stage-tag">Transactions / Edit</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Enter transaction ID:</p>
            <input type="number" value={editId ?? ""} onChange={(e) => setEditId(e.target.value ? Number(e.target.value) : null)} placeholder="Transaction ID" />
            <button type="button" onClick={() => { if (editId) { setMessage(""); loadDetail(editId); } }} disabled={!editId || loading} className="customer-inline-button view">Load</button>
          </div>
          {detail && (
            <form onSubmit={handleUpdate} className="customer-form-shell">
              <p className="customer-stage-copy">Editing transaction #{detail.transactionId}</p>
              <div className="customer-field-strip customer-form-stack">
                <label>Account ID <input type="number" value={form.accountId || detail.accountId || ""} onChange={(e) => setForm({ ...form, accountId: e.target.value })} placeholder="Leave blank" /></label>
                <label>Transaction Type <select value={form.transactionType || detail.transactionType} onChange={(e) => setForm({ ...form, transactionType: e.target.value })}><option value="Deposit">Deposit</option><option value="Withdrawal">Withdrawal</option><option value="Transfer">Transfer</option></select></label>
                <label>Amount <input type="number" step="0.01" value={form.amount || detail.amount || ""} onChange={(e) => setForm({ ...form, amount: e.target.value })} placeholder="Leave blank" /></label>
                <label>Currency Code <input value={form.currencyCode || detail.currencyCode} onChange={(e) => setForm({ ...form, currencyCode: e.target.value })} placeholder="Leave blank" /></label>
                <label>Description <input value={form.description || detail.description || ""} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="Leave blank" /></label>
              </div>
              <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Updating..." : "Update Transaction"}</button>
            </form>
          )}
          {message && <p className={message.includes("Updated") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Detail" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Detail</h2></div><span className="customer-stage-tag">Transactions / Detail</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Enter transaction ID:</p>
            <input type="number" value={editId ?? ""} onChange={(e) => setEditId(e.target.value ? Number(e.target.value) : null)} placeholder="Transaction ID" />
            <button type="button" onClick={() => editId && loadDetail(editId)} disabled={!editId || loading} className="customer-inline-button view">View</button>
          </div>
          {detail && (
            <section className="table-card customer-detail-card">
              <table><tbody>
                <tr><td><strong>ID</strong></td><td className="mono">{detail.transactionId}</td></tr>
                <tr><td><strong>Account</strong></td><td>{detail.accountId}</td></tr>
                <tr><td><strong>Type</strong></td><td>{detail.transactionType}</td></tr>
                <tr><td><strong>Amount</strong></td><td className="mono">{detail.amount.toFixed(2)}</td></tr>
                <tr><td><strong>Currency</strong></td><td>{detail.currencyCode}</td></tr>
                <tr><td><strong>Date</strong></td><td>{formatDate(detail.transactionDate)}</td></tr>
                <tr><td><strong>Description</strong></td><td>{detail.description || "-"}</td></tr>
              </tbody></table>
            </section>
          )}
        </section>
      )}
    </>
  );
}
