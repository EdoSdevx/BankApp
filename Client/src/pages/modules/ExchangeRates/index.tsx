import { useState, useEffect, type FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import type { ExchangeRateListDto, CurrencyListDto } from "@/types";
import type { ApiResponse } from "@/types";
import * as service from "@/services/exchangeRates";
import * as currencyService from "@/services/currencies";
import { ModuleActionIcon } from "@/components/icons";
import { formatDate } from "@/components/ui";

const ACTIONS = [
  { label: "List", description: "Review current and historical exchange-rate benchmarks by currency code." },
  { label: "Create", description: "Add an exchange-rate snapshot for a currency at a point in time." },
  { label: "Edit", description: "Adjust an existing exchange-rate entry with updated rate or source information." },
  { label: "Detail", description: "Inspect a single exchange-rate record before drilling into currency movements." },
];

export function ExchangeRatesPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const action = searchParams.get("action") ?? "List";
  const [data, setData] = useState<ExchangeRateListDto[]>([]);
  const [detail, setDetail] = useState<ExchangeRateListDto | null>(null);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState({ currencyCode: "", rate: "", source: "" });
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [currencies, setCurrencies] = useState<CurrencyListDto[]>([]);
  const [editSearch, setEditSearch] = useState("");
  const [detailSearch, setDetailSearch] = useState("");
  const [listSearch, setListSearch] = useState("");

  async function load() { setLoading(true); const r = await service.list(); if (r.success && r.data) setData(r.data); setLoading(false); }
  async function loadDetail(id: number) { setLoading(true); const r = await service.select(id); if (r.success && r.data) setDetail(r.data); setLoading(false); }
  useEffect(() => { load(); }, []);
  useEffect(() => { currencyService.list().then((r) => r.success && setCurrencies(r.data ?? [])); }, []);

  const filterRates = (query: string) => {
    if (query.startsWith("#")) return [];
    const raw = query.trim();
    if (!raw) return [];
    const q = raw.toLowerCase();
    return data.filter((r) => {
      if (r.rateId.toString().includes(q)) return true;
      if (r.currencyCode.toLowerCase().includes(q)) return true;
      if (r.source.toLowerCase().includes(q)) return true;
      return false;
    }).slice(0, 8);
  };

  const searchKeyDown = (e: React.KeyboardEvent, query: string, results: ReturnType<typeof filterRates>, setId: (id: number) => void) => {
    if (e.key === "Enter") {
      e.preventDefault();
      if (query.startsWith("#")) {
        const id = Number(query.replace(/^#/, "").trim());
        if (!isNaN(id) && id > 0) setId(id);
        return;
      }
      if (results.length > 0) setId(results[0].rateId);
    }
  };

  function setAction(a: string, keepId = false) { if (!keepId) setEditId(null); setDetail(null); setMessage(""); setEditSearch(""); setDetailSearch(""); setSearchParams({ action: a }); }

  async function handleCreate(e: FormEvent) {
    e.preventDefault(); setMessage(""); setLoading(true);
    const r = await service.create({ currencyCode: form.currencyCode, rate: Number(form.rate), source: form.source || "" });
    if (r.success) { setMessage("Created."); setForm({ currencyCode: "", rate: "", source: "" }); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleUpdate(e: FormEvent) {
    e.preventDefault(); if (!editId) return; setMessage(""); setLoading(true);
    const body: Record<string, unknown> = {};
    if (form.currencyCode) body.currencyCode = form.currencyCode;
    if (form.rate) body.rate = Number(form.rate);
    if (form.source) body.source = form.source;
    const r = await service.update(editId, body);
    if (r.success) { setMessage("Updated."); setEditId(null); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleDelete(id: number) { if (!window.confirm("Delete?")) return; await service.remove(id); load(); }

  return (
    <>
      <h1>Exchange Rates</h1>
      <p className="page-subtitle">Choose the exchange-rate operation you want to manage.</p>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>List</h2></div><span className="customer-stage-tag">Exchange Rates / List</span></div>
          {loading && <p className="status">Loading...</p>}
          <div className="customer-toolbar">
            <input type="text" value={listSearch} onChange={(e) => setListSearch(e.target.value)}
              placeholder="Filter by currency code or source..." className="list-search-input" />
            {listSearch && <button type="button" onClick={() => setListSearch("")} className="customer-inline-button">✕ Clear</button>}
            <button type="button" onClick={() => setAction("Create")} className="customer-primary-button" style={{ marginLeft: "auto" }}>+ Create New Rate</button>
          </div>
          <section className="table-card">
            <table>
              <thead><tr><th>ID</th><th>Currency</th><th className="num">Rate</th><th>Date</th><th>Source</th><th></th></tr></thead>
              <tbody>
                {data.length === 0 && !loading && <tr><td colSpan={6}>No rates.</td></tr>}
                {(() => {
                  const filtered = listSearch.trim()
                    ? data.filter((r) => {
                        const q = listSearch.toLowerCase();
                        if (r.currencyCode.toLowerCase().includes(q)) return true;
                        if (r.source.toLowerCase().includes(q)) return true;
                        return false;
                      })
                    : data;
                  if (listSearch.trim() && filtered.length === 0 && !loading) return <tr><td colSpan={6}>No matching rates.</td></tr>;
                  return filtered.map((r) => (
                  <tr key={r.rateId}>
                    <td className="mono">{r.rateId}</td><td>{r.currencyCode}</td><td className="num mono">{r.rate.toFixed(4)}</td><td>{formatDate(r.rateDate)}</td><td>{r.source}</td>
                    <td><div className="customer-row-actions">
                       <button type="button" onClick={() => { setEditId(r.rateId); setMessage(""); setAction("Edit", true); loadDetail(r.rateId); }} className="customer-inline-button">Edit</button>
                      <button type="button" onClick={() => handleDelete(r.rateId)} className="customer-inline-button danger">Delete</button>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Create</h2></div><span className="customer-stage-tag">Exchange Rates / Create</span></div>
          <form onSubmit={handleCreate} className="customer-form-shell">
            <div className="customer-field-strip customer-form-stack">
              <select value={form.currencyCode} onChange={(e) => setForm({ ...form, currencyCode: e.target.value })} required>
                <option value="">Select currency</option>
                {currencies.map((c) => <option key={c.currencyCode} value={c.currencyCode}>{c.currencyCode} — {c.currencyName}</option>)}
              </select>
              <input type="number" step="0.0001" value={form.rate} onChange={(e) => setForm({ ...form, rate: e.target.value })} placeholder="Rate *" required />
              <input value={form.source} onChange={(e) => setForm({ ...form, source: e.target.value })} placeholder="Source" />
            </div>
            <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Creating..." : "Create Rate"}</button>
          </form>
          {message && <p className={message.includes("Created") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Edit" && (() => {
        const editResults = filterRates(editSearch);
        return (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Edit</h2></div><span className="customer-stage-tag">Exchange Rates / Edit</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Search rate by currency code, source, or ID:</p>
            <div className="search-input-wrap">
              <input type="text" value={editSearch}
                onChange={(e) => { setEditSearch(e.target.value); setEditId(null); setDetail(null); }}
                onKeyDown={(e) => searchKeyDown(e, editSearch, editResults, (id) => { setEditId(id); setEditSearch(`${id}`); setMessage(""); loadDetail(id); })}
                placeholder="Type #123, USD, or TCMB..." />
              {editResults.length > 0 && (
                <div className="search-results-dropdown">
                  {editResults.map((r) => (
                    <button type="button" key={r.rateId}
                      onClick={() => { setEditId(r.rateId); setEditSearch(`${r.rateId} — ${r.currencyCode} — ${r.rate}`); setMessage(""); loadDetail(r.rateId); }}>
                      #{r.rateId} — {r.currencyCode} — {r.rate.toFixed(4)} ({r.source})
                    </button>
                  ))}
                </div>
              )}
            </div>
            <button type="button" onClick={() => { if (editId) { setMessage(""); loadDetail(editId); } }} disabled={!editId || loading} className="customer-inline-button view">Load</button>
          </div>
          {detail && (
            <form onSubmit={handleUpdate} className="customer-form-shell">
              <p className="customer-stage-copy">Editing rate #{detail.rateId}</p>
              <div className="customer-field-strip customer-form-stack">
                <label>Currency <select value={form.currencyCode || detail.currencyCode} onChange={(e) => setForm({ ...form, currencyCode: e.target.value })}>
                  <option value="">Leave unchanged</option>
                  {currencies.map((c) => <option key={c.currencyCode} value={c.currencyCode}>{c.currencyCode} — {c.currencyName}</option>)}
                </select></label>
                <label>Rate <input type="number" step="0.0001" value={form.rate || detail.rate || ""} onChange={(e) => setForm({ ...form, rate: e.target.value })} placeholder="Leave blank" /></label>
                <label>Source <input value={form.source || detail.source} onChange={(e) => setForm({ ...form, source: e.target.value })} placeholder="Leave blank" /></label>
              </div>
              <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Updating..." : "Update Rate"}</button>
            </form>
          )}
          {message && <p className={message.includes("Updated") ? "status success" : "status"}>{message}</p>}
        </section>
        );
      })()}

      {action === "Detail" && (() => {
        const detailResults = filterRates(detailSearch);
        return (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Detail</h2></div><span className="customer-stage-tag">Exchange Rates / Detail</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Search rate by currency code, source, or ID:</p>
            <div className="search-input-wrap">
              <input type="text" value={detailSearch}
                onChange={(e) => { setDetailSearch(e.target.value); setEditId(null); setDetail(null); }}
                onKeyDown={(e) => searchKeyDown(e, detailSearch, detailResults, (id) => { setEditId(id); setDetailSearch(`${id}`); loadDetail(id); })}
                placeholder="Type #123, USD, or TCMB..." />
              {detailResults.length > 0 && (
                <div className="search-results-dropdown">
                  {detailResults.map((r) => (
                    <button type="button" key={r.rateId}
                      onClick={() => { setEditId(r.rateId); setDetailSearch(`${r.rateId} — ${r.currencyCode} — ${r.rate}`); loadDetail(r.rateId); }}>
                      #{r.rateId} — {r.currencyCode} — {r.rate.toFixed(4)} ({r.source})
                    </button>
                  ))}
                </div>
              )}
            </div>
            <button type="button" onClick={() => editId && loadDetail(editId)} disabled={!editId || loading} className="customer-inline-button view">View</button>
          </div>
          {detail && (
            <section className="table-card customer-detail-card">
              <table><tbody>
                <tr><td><strong>ID</strong></td><td className="mono">{detail.rateId}</td></tr>
                <tr><td><strong>Currency</strong></td><td>{detail.currencyCode}</td></tr>
                <tr><td><strong>Rate</strong></td><td className="mono">{detail.rate.toFixed(4)}</td></tr>
                <tr><td><strong>Date</strong></td><td>{formatDate(detail.rateDate)}</td></tr>
                <tr><td><strong>Source</strong></td><td>{detail.source}</td></tr>
              </tbody></table>
            </section>
          )}
        </section>
        );
      })()}
    </>
  );
}
