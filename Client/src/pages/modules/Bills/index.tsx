import { useState, useEffect, type FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import type { BillListDto, BillSelectDto, CustomerListDto, CurrencyListDto } from "@/types";
import type { ApiResponse } from "@/types";
import * as service from "@/services/bills";
import * as customerService from "@/services/customers";
import * as currencyService from "@/services/currencies";
import { ModuleActionIcon } from "@/components/icons";
import { StatusDot, formatDate } from "@/components/ui";

const ACTIONS = [
  { label: "List", description: "Review bill records with payment state, due date, and optional currency details." },
  { label: "Create", description: "Register a new bill for a customer with amount, due date, and payment state defaults." },
  { label: "Edit", description: "Update an existing bill record before settlement or status changes." },
  { label: "Mark-Paid", description: "Complete the bill by setting payment state and paid date in one focused action." },
];

export function BillsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const action = searchParams.get("action") ?? "List";
  const [data, setData] = useState<BillListDto[]>([]);
  const [detail, setDetail] = useState<BillSelectDto | null>(null);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState({ customerId: "", billType: "", amount: "", currencyCode: "", dueDate: "", isPaid: false });
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [customers, setCustomers] = useState<CustomerListDto[]>([]);
  const [currencies, setCurrencies] = useState<CurrencyListDto[]>([]);
  const [editSearch, setEditSearch] = useState("");
  const [markSearch, setMarkSearch] = useState("");
  const [listSearch, setListSearch] = useState("");

  async function load() { setLoading(true); const r = await service.list(); if (r.success && r.data) setData(r.data); setLoading(false); }
  async function loadDetail(id: number) { setLoading(true); const r = await service.select(id); if (r.success && r.data) setDetail(r.data); setLoading(false); }
  useEffect(() => { load(); }, []);
  useEffect(() => {
    customerService.list().then((r) => r.success && setCustomers(r.data ?? []));
    currencyService.list().then((r) => r.success && setCurrencies(r.data ?? []));
  }, []);

  const filterBills = (query: string) => {
    if (query.startsWith("#")) return [];
    const raw = query.trim();
    if (!raw) return [];
    const q = raw.toLowerCase();
    return data.filter((b) => {
      if (b.billId.toString().includes(q)) return true;
      const c = customers.find((x) => x.customerId === b.customerId);
      return c?.fullName.toLowerCase().includes(q) ?? false;
    }).slice(0, 8);
  };

  const searchKeyDown = (e: React.KeyboardEvent, query: string, results: ReturnType<typeof filterBills>, setId: (id: number) => void) => {
    if (e.key === "Enter") {
      e.preventDefault();
      if (query.startsWith("#")) {
        const id = Number(query.replace(/^#/, "").trim());
        if (!isNaN(id) && id > 0) setId(id);
        return;
      }
      if (results.length > 0) setId(results[0].billId);
    }
  };

  function setAction(a: string, keepId = false) { if (!keepId) setEditId(null); setDetail(null); setMessage(""); setEditSearch(""); setMarkSearch(""); setSearchParams({ action: a }); }

  async function handleCreate(e: FormEvent) {
    e.preventDefault(); setMessage(""); setLoading(true);
    const r = await service.create({
      customerId: Number(form.customerId), billType: form.billType, amount: Number(form.amount),
      currencyCode: form.currencyCode || null, dueDate: form.dueDate, isPaid: form.isPaid, paidDate: null,
    });
    if (r.success) { setMessage("Created."); setForm({ customerId: "", billType: "", amount: "", currencyCode: "", dueDate: "", isPaid: false }); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleUpdate(e: FormEvent) {
    e.preventDefault(); if (!editId) return; setMessage(""); setLoading(true);
    const body: Record<string, unknown> = {};
    if (form.billType) body.billType = form.billType;
    if (form.amount) body.amount = Number(form.amount);
    body.currencyCode = form.currencyCode || null;
    if (form.dueDate) body.dueDate = form.dueDate;
    const r = await service.update(editId, body);
    if (r.success) { setMessage("Updated."); setEditId(null); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleMarkPaid(id: number) {
    if (!window.confirm("Mark as paid?")) return;
    setLoading(true);
    const r = await service.markPaid(id);
    if (r.success) { setMarkSearch(""); load(); }
    setLoading(false);
  }

  async function handleDelete(id: number) { if (!window.confirm("Delete?")) return; await service.remove(id); load(); }

  return (
    <>
      <h1>Bills</h1>
      <p className="page-subtitle">Choose the billing operation you want to handle.</p>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>List</h2></div><span className="customer-stage-tag">Bills / List</span></div>
          {loading && <p className="status">Loading...</p>}
          <div className="customer-toolbar">
            <input type="text" value={listSearch} onChange={(e) => setListSearch(e.target.value)}
              placeholder="Filter by bill ID or customer name..." className="list-search-input" />
            {listSearch && <button type="button" onClick={() => setListSearch("")} className="customer-inline-button">✕ Clear</button>}
            <button type="button" onClick={() => setAction("Create")} className="customer-primary-button" style={{ marginLeft: "auto" }}>+ Create New Bill</button>
          </div>
          <section className="table-card">
            <table>
              <thead><tr><th>ID</th><th>Customer</th><th>Type</th><th className="num">Amount</th><th>Currency</th><th>Due</th><th>Paid</th><th>Paid Date</th><th></th></tr></thead>
              <tbody>
                {data.length === 0 && !loading && <tr><td colSpan={9}>No bills.</td></tr>}
                {(() => {
                  const filtered = listSearch.trim()
                    ? data.filter((b) => {
                        const q = listSearch.toLowerCase();
                        if (b.billId.toString().includes(q)) return true;
                        const c = customers.find((x) => x.customerId === b.customerId);
                        return c?.fullName.toLowerCase().includes(q) ?? false;
                      })
                    : data;
                  if (listSearch.trim() && filtered.length === 0 && !loading) return <tr><td colSpan={9}>No matching bills.</td></tr>;
                  return filtered.map((b) => (
                  <tr key={b.billId}>
                    <td className="mono">{b.billId}</td>
                    <td>{customers.find((c) => c.customerId === b.customerId)?.fullName ?? `Customer #${b.customerId}`}</td>
                    <td>{b.billType}</td><td className="num mono">{b.amount.toFixed(2)}</td><td>{b.currencyCode || "-"}</td><td>{formatDate(b.dueDate)}</td>
                    <td><span className="status-line"><StatusDot active={b.isPaid} />{b.isPaid ? "Yes" : "No"}</span></td>
                    <td>{formatDate(b.paidDate)}</td>
                    <td><div className="customer-row-actions">
                       <button type="button" onClick={() => { setEditId(b.billId); setMessage(""); setAction("Edit", true); loadDetail(b.billId); }} className="customer-inline-button">Edit</button>
                      {!b.isPaid && (
                        <button type="button" onClick={() => handleMarkPaid(b.billId)} className="customer-inline-button view">Mark Paid</button>
                      )}
                      <button type="button" onClick={() => handleDelete(b.billId)} className="customer-inline-button danger">Delete</button>
                    </div></td>
                  </tr>
                ));
                })()}
              </tbody>
            </table>
          </section>
        </section>
      )}

      {action === "Create" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Create</h2></div><span className="customer-stage-tag">Bills / Create</span></div>
          <form onSubmit={handleCreate} className="customer-form-shell">
            <div className="customer-field-strip customer-form-stack">
              <select value={form.customerId} onChange={(e) => setForm({ ...form, customerId: e.target.value })} required>
                <option value="">Select customer</option>
                {customers.map((c) => <option key={c.customerId} value={c.customerId}>{c.fullName}</option>)}
              </select>
              <select value={form.billType} onChange={(e) => setForm({ ...form, billType: e.target.value })} required>
                <option value="" disabled>Select bill type</option>
                <option value="Electricity">Electricity</option><option value="Internet">Internet</option><option value="Water">Water</option>
              </select>
              <input type="number" step="0.01" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} placeholder="Amount *" required />
              <select value={form.currencyCode} onChange={(e) => setForm({ ...form, currencyCode: e.target.value })}>
                <option value="">Select currency (optional)</option>
                {currencies.map((c) => <option key={c.currencyCode} value={c.currencyCode}>{c.currencyCode} — {c.currencyName}</option>)}
              </select>
              <input type="datetime-local" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} placeholder="Due Date *" required />
              <label className="customer-form-check">
                <input type="checkbox" checked={form.isPaid} onChange={(e) => setForm({ ...form, isPaid: e.target.checked })} /> Is Paid
              </label>
            </div>
            <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Creating..." : "Create Bill"}</button>
          </form>
          {message && <p className={message.includes("Created") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Edit" && (() => {
        const editResults = filterBills(editSearch);
        return (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Edit</h2></div><span className="customer-stage-tag">Bills / Edit</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Search bill by ID or customer name:</p>
            <div className="search-input-wrap">
              <input type="text" value={editSearch}
                onChange={(e) => { setEditSearch(e.target.value); setEditId(null); setDetail(null); }}
                onKeyDown={(e) => searchKeyDown(e, editSearch, editResults, (id) => { setEditId(id); setEditSearch(`${id}`); setMessage(""); loadDetail(id); })}
                placeholder="Type #123 or Ahmet..." />
              {editResults.length > 0 && (
                <div className="search-results-dropdown">
                  {editResults.map((b) => {
                    const name = customers.find((x) => x.customerId === b.customerId)?.fullName ?? `Customer #${b.customerId}`;
                    return (
                      <button type="button" key={b.billId}
                        onClick={() => { setEditId(b.billId); setEditSearch(`${b.billId} — ${name} — ${b.billType}`); setMessage(""); loadDetail(b.billId); }}>
                        #{b.billId} — {name} — {b.billType} — {b.amount} {b.currencyCode || ""}
                      </button>
                    );
                  })}
                </div>
              )}
            </div>
            <button type="button" onClick={() => { if (editId) { setMessage(""); loadDetail(editId); } }} disabled={!editId || loading} className="customer-inline-button view">Load</button>
          </div>
          {detail && (
            <form onSubmit={handleUpdate} className="customer-form-shell">
              <p className="customer-stage-copy">Editing bill #{detail.billId}</p>
              <div className="customer-field-strip customer-form-stack">
                <label>Bill Type <select value={form.billType || detail.billType} onChange={(e) => setForm({ ...form, billType: e.target.value })}><option value="Electricity">Electricity</option><option value="Internet">Internet</option><option value="Water">Water</option></select></label>
                <label>Amount <input type="number" step="0.01" value={form.amount || detail.amount || ""} onChange={(e) => setForm({ ...form, amount: e.target.value })} placeholder="Leave blank" /></label>
                <label>Currency <select value={form.currencyCode || detail.currencyCode || ""} onChange={(e) => setForm({ ...form, currencyCode: e.target.value })}>
                  <option value="">Leave unchanged</option>
                  {currencies.map((c) => <option key={c.currencyCode} value={c.currencyCode}>{c.currencyCode} — {c.currencyName}</option>)}
                </select></label>
                <label>Due Date <input type="datetime-local" value={form.dueDate || detail.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} placeholder="Leave blank" /></label>
              </div>
              <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Updating..." : "Update Bill"}</button>
            </form>
          )}
          {message && <p className={message.includes("Updated") ? "status success" : "status"}>{message}</p>}
        </section>
        );
      })()}

      {action === "Mark-Paid" && (() => {
        const markResults = filterBills(markSearch);
        return (
        <section className="customer-stage" aria-label="Mark bill paid">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Mark-Paid</h2></div><span className="customer-stage-tag">Bills / Mark-Paid</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Search bill to mark as paid:</p>
            <div className="search-input-wrap">
              <input type="text" value={markSearch}
                onChange={(e) => { setMarkSearch(e.target.value); setEditId(null); }}
                onKeyDown={(e) => searchKeyDown(e, markSearch, markResults, (id) => { setEditId(id); setMarkSearch(`${id}`); handleMarkPaid(id); })}
                placeholder="Type #123 or Ahmet..." />
              {markResults.length > 0 && (
                <div className="search-results-dropdown">
                  {markResults.filter((b) => !b.isPaid).map((b) => {
                    const name = customers.find((x) => x.customerId === b.customerId)?.fullName ?? `Customer #${b.customerId}`;
                    return (
                      <button type="button" key={b.billId}
                        onClick={() => { setEditId(b.billId); setMarkSearch(`${b.billId} — ${name} — ${b.billType}`); handleMarkPaid(b.billId); }}>
                        #{b.billId} — {name} — {b.billType} — {b.amount} {b.currencyCode || ""}
                      </button>
                    );
                  })}
                </div>
              )}
            </div>
            <button type="button" onClick={() => { if (editId) { handleMarkPaid(editId); } }} disabled={!editId || loading} className="customer-inline-button view">Mark Paid</button>
          </div>
        </section>
        );
      })()}
    </>
  );
}
