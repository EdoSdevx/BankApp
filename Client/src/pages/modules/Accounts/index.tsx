import { useState, useEffect, type FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import type { AccountListDto, AccountSelectDto, CustomerListDto, BranchListDto, CurrencyListDto } from "@/types";
import type { ApiResponse } from "@/types";
import * as service from "@/services/accounts";
import * as customerService from "@/services/customers";
import * as branchService from "@/services/branches";
import * as currencyService from "@/services/currencies";
import { ModuleActionIcon } from "@/components/icons";
import { StatusDot, formatDate } from "@/components/ui";

const ACTIONS = [
  { label: "List", description: "View account rows with customer, branch, currency, balance, and opening date data." },
  { label: "Create", description: "Open a new account with customer, branch, currency, and starting balance values." },
  { label: "Edit", description: "Update an existing account with a different currency or balance value." },
  { label: "Detail", description: "Inspect one account record before moving to connected operational screens." },
  { label: "Transfer", description: "Move funds between two accounts of the same currency instantly." },
  { label: "Operations", description: "Jump from an account into related movement screens and supporting operational actions." },
];

export function AccountsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const action = searchParams.get("action") ?? "List";
  const [data, setData] = useState<AccountListDto[]>([]);
  const [detail, setDetail] = useState<AccountSelectDto | null>(null);
  const [viewId, setViewId] = useState<number | null>(null);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState({ customerId: "", branchId: "", currencyCode: "", balance: "" });
  const [updateForm, setUpdateForm] = useState({ currencyCode: "", balance: "" });
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [transferForm, setTransferForm] = useState({ sourceAccountId: "", targetAccountId: "", amount: "", description: "" });
  const [customers, setCustomers] = useState<CustomerListDto[]>([]);
  const [branches, setBranches] = useState<BranchListDto[]>([]);
  const [currencies, setCurrencies] = useState<CurrencyListDto[]>([]);
  const [detailSearch, setDetailSearch] = useState("");
  const [editSearch, setEditSearch] = useState("");
  const [listSearch, setListSearch] = useState("");

  const filterAccounts = (query: string) => {
    if (query.startsWith("#")) return [];
    const raw = query.trim();
    if (!raw) return [];
    const num = Number(raw);
    const exact = !isNaN(num) ? data.filter((a) => a.accountId === num) : [];
    const fuzzy = data.filter((a) => {
      if (a.accountId.toString().includes(raw)) return true;
      const c = customers.find((x) => x.customerId === a.customerId);
      return c?.fullName.toLowerCase().includes(raw.toLowerCase()) ?? false;
    });
    return [...exact, ...fuzzy.filter((a) => !exact.some((e) => e.accountId === a.accountId))].slice(0, 8);
  };

  const handleSearchKeyDown = (e: React.KeyboardEvent, query: string, results: ReturnType<typeof filterAccounts>, setId: (id: number) => void) => {
    if (e.key === "Enter") {
      e.preventDefault();
      if (query.startsWith("#")) {
        const id = Number(query.replace(/^#/, "").trim());
        if (!isNaN(id) && id > 0) setId(id);
        return;
      }
      if (results.length > 0) {
        setId(results[0].accountId);
      }
    }
  };

  async function handleTransfer(e: FormEvent) {
    e.preventDefault(); setMessage(""); setLoading(true);
    const r = await service.transferBetween(
      Number(transferForm.sourceAccountId),
      Number(transferForm.targetAccountId),
      Number(transferForm.amount),
      transferForm.description || undefined,
    );
    if (r.success) { setMessage("Transfer completed."); setTransferForm({ sourceAccountId: "", targetAccountId: "", amount: "", description: "" }); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function load() { setLoading(true); const r = await service.list(); if (r.success && r.data) setData(r.data); setLoading(false); }
  async function loadDetail(id: number) { setLoading(true); const r = await service.select(id); if (r.success && r.data) setDetail(r.data); setLoading(false); }
  useEffect(() => { load(); }, []);
  useEffect(() => {
    customerService.list().then((r) => r.success && setCustomers(r.data ?? []));
    branchService.list().then((r) => r.success && setBranches(r.data ?? []));
    currencyService.list().then((r) => r.success && setCurrencies(r.data ?? []));
  }, []);

  function setAction(a: string) { setViewId(null); setEditId(null); setDetail(null); setMessage(""); setSearchParams({ action: a }); }

  async function handleCreate(e: FormEvent) {
    e.preventDefault(); setMessage(""); setLoading(true);
    const r = await service.create({ customerId: Number(form.customerId), branchId: Number(form.branchId), currencyCode: form.currencyCode, balance: Number(form.balance) });
    if (r.success) { setMessage("Created."); setForm({ customerId: "", branchId: "", currencyCode: "", balance: "" }); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleUpdate(e: FormEvent) {
    e.preventDefault(); if (!editId) return; setMessage(""); setLoading(true);
    const body: Record<string, unknown> = {};
    if (updateForm.currencyCode) body.currencyCode = updateForm.currencyCode;
    if (updateForm.balance) body.balance = Number(updateForm.balance);
    const r = await service.update(editId, body);
    if (r.success) { setMessage("Updated."); setEditId(null); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleDelete(id: number) { if (!window.confirm("Delete?")) return; await service.remove(id); load(); }

  return (
    <>
      <h1>Accounts</h1>
      <p className="page-subtitle">Select the account workspace you want to open.</p>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>List</h2></div><span className="customer-stage-tag">Accounts / List</span></div>
          {loading && <p className="status">Loading...</p>}
          <div className="customer-toolbar">
            <input type="text" value={listSearch} onChange={(e) => setListSearch(e.target.value)}
              placeholder="Filter by account ID or customer name..." className="list-search-input" />
            {listSearch && <button type="button" onClick={() => setListSearch("")} className="customer-inline-button">✕ Clear</button>}
            <button type="button" onClick={() => setAction("Create")} className="customer-primary-button">+ Create New Account</button>
          </div>
          <section className="table-card">
            <table>
              <thead><tr><th>ID</th><th>Customer</th><th>Branch</th><th>Currency</th><th className="num">Balance</th><th>Status</th><th></th></tr></thead>
              <tbody>
                {data.length === 0 && !loading && <tr><td colSpan={7}>No accounts.</td></tr>}
                {(() => {
                  const filtered = listSearch.trim()
                    ? (() => {
                        const raw = listSearch.trim();
                        if (raw.startsWith("#")) {
                          const id = Number(raw.replace(/^#/, ""));
                          return !isNaN(id) ? data.filter((a) => a.accountId === id) : [];
                        }
                        const q = raw.toLowerCase();
                        return data.filter((a) => {
                          if (a.accountId.toString().includes(q)) return true;
                          const c = customers.find((x) => x.customerId === a.customerId);
                          return c?.fullName.toLowerCase().includes(q) ?? false;
                        });
                      })()
                    : data;
                  if (listSearch.trim() && filtered.length === 0 && !loading) return <tr><td colSpan={7}>No matching accounts.</td></tr>;
                  return filtered.map((a) => (
                  <tr key={a.accountId}>
                    <td className="mono">{a.accountId}</td>
                    <td>{customers.find((c) => c.customerId === a.customerId)?.fullName ?? a.customerId}</td>
                    <td>{branches.find((b) => b.branchId === a.branchId)?.branchName ?? a.branchId}</td>
                    <td>{a.currencyCode}</td><td className="num mono">{a.balance.toFixed(2)}</td>
                    <td><span className="status-line"><StatusDot active={a.isActive} />{a.isActive ? "Active" : "Inactive"}</span></td>
                    <td><div className="customer-row-actions">
                      <button type="button" onClick={() => { setViewId(a.accountId); loadDetail(a.accountId); setAction("Detail"); }} className="customer-inline-button view">Detail</button>
                      <button type="button" onClick={() => handleDelete(a.accountId)} className="customer-inline-button danger">Deactivate</button>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Create</h2></div><span className="customer-stage-tag">Accounts / Create</span></div>
          <form onSubmit={handleCreate} className="customer-form-shell">
            <div className="customer-field-strip customer-form-stack">
              <select value={form.customerId} onChange={(e) => setForm({ ...form, customerId: e.target.value })} required>
                <option value="">Select customer</option>
                {customers.map((c) => <option key={c.customerId} value={c.customerId}>{c.fullName}</option>)}
              </select>
              <select value={form.branchId} onChange={(e) => setForm({ ...form, branchId: e.target.value })} required>
                <option value="">Select branch</option>
                {branches.map((b) => <option key={b.branchId} value={b.branchId}>{b.branchName}</option>)}
              </select>
              <select value={form.currencyCode} onChange={(e) => setForm({ ...form, currencyCode: e.target.value })} required>
                <option value="">Select currency</option>
                {currencies.map((c) => <option key={c.currencyCode} value={c.currencyCode}>{c.currencyCode} — {c.currencyName}</option>)}
              </select>
              <input type="number" step="0.01" value={form.balance} onChange={(e) => setForm({ ...form, balance: e.target.value })} placeholder="Balance *" required />
            </div>
            <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Creating..." : "Create Account"}</button>
          </form>
          {message && <p className={message.includes("Created") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Detail" && (() => {
        const detailResults = filterAccounts(detailSearch);
        return (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Detail</h2></div><span className="customer-stage-tag">Accounts / Detail</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Search account by ID or customer name:</p>
            <div className="search-input-wrap">
              <input type="text" value={detailSearch}
                onChange={(e) => { setDetailSearch(e.target.value); setViewId(null); setDetail(null); }}
                onKeyDown={(e) => handleSearchKeyDown(e, detailSearch, detailResults, (id) => { setViewId(id); setDetailSearch(`${id}`); loadDetail(id); })}
                placeholder="Type #123 or Ahmet..." />
              {detailResults.length > 0 && (
                <div className="search-results-dropdown">
                  {detailResults.map((a) => {
                    const name = customers.find((x) => x.customerId === a.customerId)?.fullName ?? `Customer #${a.customerId}`;
                    return (
                      <button type="button" key={a.accountId}
                        onClick={() => { setViewId(a.accountId); setDetailSearch(`${a.accountId} — ${name}`); loadDetail(a.accountId); }}>
                        #{a.accountId} — {name} — {a.balance} {a.currencyCode}
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
                <tr><td><strong>ID</strong></td><td className="mono">{detail.accountId}</td></tr>
                <tr><td><strong>Customer</strong></td><td>#{detail.customerId} — {customers.find((c) => c.customerId === detail.customerId)?.fullName ?? "Unknown"}</td></tr>
                <tr><td><strong>Branch</strong></td><td>#{detail.branchId} — {branches.find((b) => b.branchId === detail.branchId)?.branchName ?? "Unknown"}</td></tr>
                <tr><td><strong>Currency</strong></td><td>{detail.currencyCode}</td></tr>
                <tr><td><strong>Balance</strong></td><td className="mono">{detail.balance.toFixed(2)}</td></tr>
                <tr><td><strong>Created</strong></td><td>{formatDate(detail.createdDate)}</td></tr>
                <tr><td><strong>Status</strong></td><td><span className="status-line"><StatusDot active={detail.isActive} />{detail.isActive ? "Active" : "Inactive"}</span></td></tr>
              </tbody></table>
            </section>
          )}
        </section>
        );
      })()}

      {action === "Edit" && (() => {
        const editResults = filterAccounts(editSearch);
        return (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Edit</h2></div><span className="customer-stage-tag">Accounts / Edit</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Search account by ID or customer name:</p>
            <div className="search-input-wrap">
              <input type="text" value={editSearch}
                onChange={(e) => { setEditSearch(e.target.value); setEditId(null); setDetail(null); }}
                onKeyDown={(e) => handleSearchKeyDown(e, editSearch, editResults, (id) => { setEditId(id); setEditSearch(`${id}`); setMessage(""); loadDetail(id); })}
                placeholder="Type #123 or Ahmet..." />
              {editResults.length > 0 && (
                <div className="search-results-dropdown">
                  {editResults.map((a) => {
                    const name = customers.find((x) => x.customerId === a.customerId)?.fullName ?? `Customer #${a.customerId}`;
                    return (
                      <button type="button" key={a.accountId}
                        onClick={() => { setEditId(a.accountId); setEditSearch(`${a.accountId} — ${name}`); setMessage(""); loadDetail(a.accountId); }}>
                        #{a.accountId} — {name} — {a.balance} {a.currencyCode}
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
              <p className="customer-stage-copy">Editing account #{detail.accountId}</p>
              <div className="customer-field-strip customer-form-stack">
                <label>Currency <select value={updateForm.currencyCode || detail.currencyCode} onChange={(e) => setUpdateForm({ ...updateForm, currencyCode: e.target.value })}>
                  {currencies.map((c) => <option key={c.currencyCode} value={c.currencyCode}>{c.currencyCode} — {c.currencyName}</option>)}
                </select></label>
                <label>Balance <input type="number" step="0.01" value={updateForm.balance || detail.balance || ""} onChange={(e) => setUpdateForm({ ...updateForm, balance: e.target.value })} placeholder="Leave blank" /></label>
              </div>
              <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Updating..." : "Update Account"}</button>
            </form>
          )}
          {message && <p className={message.includes("Updated") ? "status success" : "status"}>{message}</p>}
        </section>
        );
      })()}

      {action === "Transfer" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Transfer</h2></div><span className="customer-stage-tag">Accounts / Transfer</span></div>
          <p className="customer-stage-copy">Transfer funds between two accounts of the same currency. No approval required.</p>
          <form onSubmit={handleTransfer} className="customer-form-shell">
            <div className="customer-field-strip customer-form-stack">
              <label>Source Account <select value={transferForm.sourceAccountId} onChange={(e) => setTransferForm({ ...transferForm, sourceAccountId: e.target.value })} required>
                <option value="">Select source account</option>
                {[...data].sort((a, b) => a.accountId - b.accountId).map((a) => <option key={a.accountId} value={a.accountId}>#{a.accountId} — {a.balance.toFixed(2)} {a.currencyCode}</option>)}
              </select></label>
              <label>Target Account <select value={transferForm.targetAccountId} onChange={(e) => setTransferForm({ ...transferForm, targetAccountId: e.target.value })} required>
                <option value="">Select target account</option>
                {[...data].sort((a, b) => a.accountId - b.accountId).map((a) => <option key={a.accountId} value={a.accountId}>#{a.accountId} — {a.balance.toFixed(2)} {a.currencyCode}</option>)}
              </select></label>
              <input type="number" step="0.01" value={transferForm.amount} onChange={(e) => setTransferForm({ ...transferForm, amount: e.target.value })} placeholder="Amount *" required />
              <input value={transferForm.description} onChange={(e) => setTransferForm({ ...transferForm, description: e.target.value })} placeholder="Description (optional)" />
            </div>
            <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Transferring..." : "Execute Transfer"}</button>
          </form>
          {message && <p className={message.includes("completed") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Operations" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Operations</h2></div><span className="customer-stage-tag">Accounts / Operations</span></div>
          <p className="customer-stage-copy">Select an account first from List, then access operations.</p>
        </section>
      )}
    </>
  );
}
