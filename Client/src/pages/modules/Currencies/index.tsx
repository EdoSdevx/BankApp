import { useState, useEffect, type FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import type { CurrencyListDto, CurrencySelectDto } from "@/types";
import type { ApiResponse } from "@/types";
import * as service from "@/services/currencies";
import { ModuleActionIcon } from "@/components/icons";

const ACTIONS = [
  { label: "List", description: "Browse currency codes and their display names." },
  { label: "Create", description: "Add a new currency code with its display name." },
  { label: "Update", description: "Change the display name of an existing currency." },
  { label: "Detail", description: "Inspect a single currency record." },
];

export function CurrenciesPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const action = searchParams.get("action") ?? "List";
  const [data, setData] = useState<CurrencyListDto[]>([]);
  const [detail, setDetail] = useState<CurrencySelectDto | null>(null);
  const [editCode, setEditCode] = useState<string | null>(null);
  const [form, setForm] = useState({ currencyCode: "", currencyName: "" });
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  async function load() { setLoading(true); const r = await service.list(); if (r.success && r.data) setData(r.data); setLoading(false); }
  async function loadDetail(code: string) { setLoading(true); const r = await service.select(code); if (r.success && r.data) setDetail(r.data); setLoading(false); }
  useEffect(() => { load(); }, []);

  function setAction(a: string) { setEditCode(null); setDetail(null); setMessage(""); setSearchParams({ action: a }); }

  async function handleCreate(e: FormEvent) {
    e.preventDefault(); setMessage(""); setLoading(true);
    const r = await service.create(form);
    if (r.success) { setMessage("Created."); setForm({ currencyCode: "", currencyName: "" }); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleUpdate(e: FormEvent) {
    e.preventDefault(); if (!editCode) return; setMessage(""); setLoading(true);
    const r = await service.update(editCode, { currencyName: form.currencyName });
    if (r.success) { setMessage("Updated."); setEditCode(null); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleDelete(code: string) { if (!window.confirm("Delete?")) return; await service.remove(code); load(); }

  return (
    <>
      <h1>Currencies</h1>
      <p className="page-subtitle">Manage currency codes and names across the system.</p>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>List</h2></div><span className="customer-stage-tag">Currencies / List</span></div>
          {loading && <p className="status">Loading...</p>}
          <div className="customer-toolbar"><button type="button" onClick={() => setAction("Create")} className="customer-primary-button">+ Create New Currency</button></div>
          <section className="table-card">
            <table>
              <thead><tr><th>Code</th><th>Name</th><th></th></tr></thead>
              <tbody>
                {data.length === 0 && !loading && <tr><td colSpan={3}>No currencies.</td></tr>}
                {data.map((c) => (
                  <tr key={c.currencyCode}>
                    <td className="mono">{c.currencyCode}</td><td>{c.currencyName}</td>
                    <td><div className="customer-row-actions">
                      <button type="button" onClick={() => { setEditCode(c.currencyCode); setForm({ currencyCode: "", currencyName: c.currencyName }); setMessage(""); setAction("Update"); }} className="customer-inline-button">Update</button>
                      <button type="button" onClick={() => handleDelete(c.currencyCode)} className="customer-inline-button danger">Delete</button>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Create</h2></div><span className="customer-stage-tag">Currencies / Create</span></div>
          <form onSubmit={handleCreate} className="customer-form-shell">
            <div className="customer-field-strip customer-form-stack">
              <input value={form.currencyCode} onChange={(e) => setForm({ ...form, currencyCode: e.target.value })} placeholder="Currency Code *" required />
              <input value={form.currencyName} onChange={(e) => setForm({ ...form, currencyName: e.target.value })} placeholder="Currency Name *" required />
            </div>
            <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Creating..." : "Create Currency"}</button>
          </form>
          {message && <p className={message.includes("Created") || message.includes("Updated") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Update" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Update</h2></div><span className="customer-stage-tag">Currencies / Update</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Enter currency code:</p>
            <input value={editCode ?? ""} onChange={(e) => { setEditCode(e.target.value); setDetail(null); }} placeholder="Currency Code" />
            <button type="button" onClick={() => { if (editCode) { setMessage(""); loadDetail(editCode); } }} disabled={!editCode || loading} className="customer-inline-button view">Load</button>
          </div>
          {detail && (
            <form onSubmit={handleUpdate} className="customer-form-shell">
              <p className="customer-stage-copy">Updating {detail.currencyCode} - {detail.currencyName}</p>
              <div className="customer-field-strip customer-form-stack">
                <label>Currency Name <input value={form.currencyName || detail.currencyName} onChange={(e) => setForm({ ...form, currencyName: e.target.value })} placeholder="Leave blank" /></label>
              </div>
              <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Updating..." : "Update Currency"}</button>
            </form>
          )}
          {message && <p className={message.includes("Updated") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Detail" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Detail</h2></div><span className="customer-stage-tag">Currencies / Detail</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Enter currency code:</p>
            <input value={editCode ?? ""} onChange={(e) => setEditCode(e.target.value)} placeholder="Currency Code" />
            <button type="button" onClick={() => { if (editCode) loadDetail(editCode); }} disabled={!editCode || loading} className="customer-inline-button view">View</button>
          </div>
          {detail && (
            <section className="table-card customer-detail-card">
              <table><tbody>
                <tr><td><strong>Code</strong></td><td className="mono">{detail.currencyCode}</td></tr>
                <tr><td><strong>Name</strong></td><td>{detail.currencyName}</td></tr>
              </tbody></table>
            </section>
          )}
        </section>
      )}
    </>
  );
}
