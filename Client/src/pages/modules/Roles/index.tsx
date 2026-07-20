import { useState, useEffect, type FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import type { RoleListDto, RoleSelectDto } from "@/types";
import type { ApiResponse } from "@/types";
import * as service from "@/services/roles";
import { ModuleActionIcon } from "@/components/icons";

const ACTIONS = [
  { label: "List", description: "Review the internal role catalog and optional descriptions." },
  { label: "Create", description: "Add a new internal role with its display name and optional description." },
  { label: "Edit", description: "Update the role name or optional description for an existing record." },
  { label: "Detail", description: "Inspect one role record before employee mapping or permission review." },
];

export function RolesPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const action = searchParams.get("action") ?? "List";
  const [data, setData] = useState<RoleListDto[]>([]);
  const [detail, setDetail] = useState<RoleSelectDto | null>(null);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState({ roleName: "", description: "" });
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  async function load() { setLoading(true); const r = await service.list(); if (r.success && r.data) setData(r.data); setLoading(false); }
  async function loadDetail(id: number) { setLoading(true); const r = await service.select(id); if (r.success && r.data) setDetail(r.data); setLoading(false); }
  useEffect(() => { load(); }, []);

  function setAction(a: string, keepId = false) { if (!keepId) setEditId(null); setDetail(null); setMessage(""); setSearchParams({ action: a }); }

  async function handleCreate(e: FormEvent) {
    e.preventDefault(); setMessage(""); setLoading(true);
    const r = await service.create(form);
    if (r.success) { setMessage("Created."); setForm({ roleName: "", description: "" }); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleUpdate(e: FormEvent) {
    e.preventDefault(); if (!editId) return; setMessage(""); setLoading(true);
    const body: Record<string, unknown> = {};
    if (form.roleName) body.roleName = form.roleName;
    body.description = form.description || null;
    const r = await service.update(editId, body);
    if (r.success) { setMessage("Updated."); setEditId(null); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleDelete(id: number) { if (!window.confirm("Delete?")) return; await service.remove(id); load(); }

  return (
    <>
      <h1>Roles</h1>
      <p className="page-subtitle">Open role administration for internal user permissions and role naming.</p>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>List</h2></div><span className="customer-stage-tag">Roles / List</span></div>
          {loading && <p className="status">Loading...</p>}
          <div className="customer-toolbar"><button type="button" onClick={() => setAction("Create")} className="customer-primary-button">+ Create New Role</button></div>
          <section className="table-card">
            <table>
              <thead><tr><th>ID</th><th>Name</th><th>Description</th><th></th></tr></thead>
              <tbody>
                {data.length === 0 && !loading && <tr><td colSpan={4}>No roles.</td></tr>}
                {data.map((r) => (
                  <tr key={r.roleId}>
                    <td className="mono">{r.roleId}</td><td>{r.roleName}</td><td>{r.description || "-"}</td>
                    <td><div className="customer-row-actions">
                       <button type="button" onClick={() => { setEditId(r.roleId); setMessage(""); setAction("Edit", true); loadDetail(r.roleId); }} className="customer-inline-button">Edit</button>
                      <button type="button" onClick={() => handleDelete(r.roleId)} className="customer-inline-button danger">Delete</button>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Create</h2></div><span className="customer-stage-tag">Roles / Create</span></div>
          <form onSubmit={handleCreate} className="customer-form-shell">
            <div className="customer-field-strip customer-form-stack">
              <input value={form.roleName} onChange={(e) => setForm({ ...form, roleName: e.target.value })} placeholder="Role Name *" required />
              <input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="Description" />
            </div>
            <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Creating..." : "Create Role"}</button>
          </form>
          {message && <p className={message.includes("Created") || message.includes("Updated") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Edit" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Edit</h2></div><span className="customer-stage-tag">Roles / Edit</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Enter role ID:</p>
            <input type="number" value={editId ?? ""} onChange={(e) => setEditId(e.target.value ? Number(e.target.value) : null)} placeholder="Role ID" />
            <button type="button" onClick={() => { if (editId) { setMessage(""); loadDetail(editId); } }} disabled={!editId || loading} className="customer-inline-button view">Load</button>
          </div>
          {detail && (
            <form onSubmit={handleUpdate} className="customer-form-shell">
              <p className="customer-stage-copy">Editing role #{detail.roleId} - {detail.roleName}</p>
              <div className="customer-field-strip customer-form-stack">
                <label>Role Name <input value={form.roleName || detail.roleName} onChange={(e) => setForm({ ...form, roleName: e.target.value })} placeholder="Leave blank" /></label>
                <label>Description <input value={form.description || detail.description || ""} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="Leave blank" /></label>
              </div>
              <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Updating..." : "Update Role"}</button>
            </form>
          )}
          {message && <p className={message.includes("Updated") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Detail" && (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Detail</h2></div><span className="customer-stage-tag">Roles / Detail</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Enter role ID:</p>
            <input type="number" value={editId ?? ""} onChange={(e) => setEditId(e.target.value ? Number(e.target.value) : null)} placeholder="Role ID" />
            <button type="button" onClick={() => editId && loadDetail(editId)} disabled={!editId || loading} className="customer-inline-button view">View</button>
          </div>
          {detail && (
            <section className="table-card customer-detail-card">
              <table><tbody>
                <tr><td><strong>ID</strong></td><td className="mono">{detail.roleId}</td></tr>
                <tr><td><strong>Name</strong></td><td>{detail.roleName}</td></tr>
                <tr><td><strong>Description</strong></td><td>{detail.description || "-"}</td></tr>
              </tbody></table>
            </section>
          )}
        </section>
      )}
    </>
  );
}
