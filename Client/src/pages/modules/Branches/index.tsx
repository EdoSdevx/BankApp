import { useState, useEffect, type FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import type { BranchListDto, BranchSelectDto } from "@/types";
import type { ApiResponse } from "@/types";
import * as service from "@/services/branches";
import { ModuleActionIcon } from "@/components/icons";
import { formatDate } from "@/components/ui";

const ACTIONS = [
  { label: "List", description: "Browse branch records with code, city, address, and opening date information." },
  { label: "Create", description: "Add a new branch with code, city, address, and activation context." },
  { label: "Edit", description: "Adjust branch name, code, city, or address for an existing record." },
  { label: "Detail", description: "Inspect one branch record before employee or account assignments." },
];

export function BranchesPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const action = searchParams.get("action") ?? "List";
  const [data, setData] = useState<BranchListDto[]>([]);
  const [detail, setDetail] = useState<BranchSelectDto | null>(null);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState({ branchName: "", branchCode: "", city: "", address: "" });
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  async function load() { setLoading(true); const r = await service.list(); if (r.success && r.data) setData(r.data); setLoading(false); }
  async function loadDetail(id: number) { setLoading(true); const r = await service.select(id); if (r.success && r.data) setDetail(r.data); setLoading(false); }
  useEffect(() => { load(); }, []);

  function setAction(a: string, keepId = false) { if (!keepId) setEditId(null); setDetail(null); setMessage(""); setSearchParams({ action: a }); }

  async function handleCreate(e: FormEvent) {
    e.preventDefault(); setMessage(""); setLoading(true);
    const r = await service.create(form);
    if (r.success) { setMessage("Created."); setForm({ branchName: "", branchCode: "", city: "", address: "" }); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleUpdate(e: FormEvent) {
    e.preventDefault(); if (!editId) return; setMessage(""); setLoading(true);
    const body: Record<string, unknown> = {};
    if (form.branchName) body.branchName = form.branchName;
    if (form.branchCode) body.branchCode = form.branchCode;
    if (form.city) body.city = form.city;
    if (form.address) body.address = form.address;
    const r = await service.update(editId, body);
    if (r.success) { setMessage("Updated."); setEditId(null); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleDelete(id: number) { if (!window.confirm("Delete?")) return; await service.remove(id); load(); }

  return (
    <>
      <h1>Branches</h1>
      <p className="page-subtitle">Select the branch maintenance flow you want to open.</p>
      <section className="customer-actions">
        {ACTIONS.map((item) => (
          <button key={item.label} type="button" className={`customer-action-card ${action === item.label ? "active" : ""}`} onClick={() => setAction(item.label, item.label === "Detail")}>
            <span className="customer-action-top"><strong>{item.label}</strong><ModuleActionIcon label={item.label} /></span>
            <span className="customer-action-text">{item.description}</span>
          </button>
        ))}
      </section>

      {action === "List" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>List</h2></div><span className="customer-stage-tag">Branches / List</span></div>
          {loading && <p className="status">Loading...</p>}
          <div className="customer-toolbar"><button type="button" onClick={() => setAction("Create")} className="customer-primary-button">+ Create New Branch</button></div>
          <section className="table-card">
            <table>
              <thead><tr><th>ID</th><th>Name</th><th>Code</th><th>City</th><th></th></tr></thead>
              <tbody>
                {data.length === 0 && !loading && <tr><td colSpan={5}>No branches.</td></tr>}
                {data.map((b) => (
                  <tr key={b.branchId}>
                    <td className="mono">{b.branchId}</td><td>{b.branchName}</td><td>{b.branchCode}</td><td>{b.city}</td>
                    <td><div className="customer-row-actions">
                       <button type="button" onClick={() => { setEditId(b.branchId); setMessage(""); setAction("Edit", true); loadDetail(b.branchId); }} className="customer-inline-button">Edit</button>
                      <button type="button" onClick={() => handleDelete(b.branchId)} className="customer-inline-button danger">Delete</button>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Create</h2></div><span className="customer-stage-tag">Branches / Create</span></div>
          <form onSubmit={handleCreate} className="customer-form-shell">
            <div className="customer-field-strip customer-form-stack">
              <input value={form.branchName} onChange={(e) => setForm({ ...form, branchName: e.target.value })} placeholder="Branch Name *" required />
              <input value={form.branchCode} onChange={(e) => setForm({ ...form, branchCode: e.target.value })} placeholder="Branch Code *" required />
              <input value={form.city} onChange={(e) => setForm({ ...form, city: e.target.value })} placeholder="City *" required />
              <input value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} placeholder="Address *" required />
            </div>
            <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Creating..." : "Create Branch"}</button>
          </form>
          {message && <p className={message.includes("Created") || message.includes("Updated") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Edit" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Edit</h2></div><span className="customer-stage-tag">Branches / Edit</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Enter branch ID:</p>
            <input type="number" value={editId ?? ""} onChange={(e) => setEditId(e.target.value ? Number(e.target.value) : null)} placeholder="Branch ID" />
            <button type="button" onClick={() => { if (editId) { setMessage(""); loadDetail(editId); } }} disabled={!editId || loading} className="customer-inline-button view">Load</button>
          </div>
          {detail && (
            <form onSubmit={handleUpdate} className="customer-form-shell">
              <p className="customer-stage-copy">Editing branch #{detail.branchId} - {detail.branchName}</p>
              <div className="customer-field-strip customer-form-stack">
                <label>Branch Name <input value={form.branchName || detail.branchName} onChange={(e) => setForm({ ...form, branchName: e.target.value })} placeholder="Leave blank" /></label>
                <label>Branch Code <input value={form.branchCode || detail.branchCode} onChange={(e) => setForm({ ...form, branchCode: e.target.value })} placeholder="Leave blank" /></label>
                <label>City <input value={form.city || detail.city} onChange={(e) => setForm({ ...form, city: e.target.value })} placeholder="Leave blank" /></label>
                <label>Address <input value={form.address || detail.address} onChange={(e) => setForm({ ...form, address: e.target.value })} placeholder="Leave blank" /></label>
              </div>
              <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Updating..." : "Update Branch"}</button>
            </form>
          )}
          {message && <p className={message.includes("Updated") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Detail" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Detail</h2></div><span className="customer-stage-tag">Branches / Detail</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Enter branch ID:</p>
            <input type="number" value={editId ?? ""} onChange={(e) => setEditId(e.target.value ? Number(e.target.value) : null)} placeholder="Branch ID" />
            <button type="button" onClick={() => editId && loadDetail(editId)} disabled={!editId || loading} className="customer-inline-button view">View</button>
          </div>
          {loading && <p className="status">Loading...</p>}
          {detail && (
            <section className="table-card customer-detail-card">
              <table><tbody>
                <tr><td><strong>ID</strong></td><td className="mono">{detail.branchId}</td></tr>
                <tr><td><strong>Name</strong></td><td>{detail.branchName}</td></tr>
                <tr><td><strong>Code</strong></td><td>{detail.branchCode}</td></tr>
                <tr><td><strong>City</strong></td><td>{detail.city}</td></tr>
                <tr><td><strong>Address</strong></td><td>{detail.address}</td></tr>
                <tr><td><strong>Created</strong></td><td>{formatDate(detail.createdDate)}</td></tr>
              </tbody></table>
            </section>
          )}
        </section>
      )}
    </>
  );
}
