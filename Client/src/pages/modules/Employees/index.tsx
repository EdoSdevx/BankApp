import { useState, useEffect, type FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import type { EmployeeListDto, EmployeeSelectDto, BranchListDto, RoleListDto } from "@/types";
import type { ApiResponse } from "@/types";
import * as service from "@/services/employees";
import * as branchService from "@/services/branches";
import * as roleService from "@/services/roles";
import { ModuleActionIcon } from "@/components/icons";
import { formatDate } from "@/components/ui";

const ACTIONS = [
  { label: "List", description: "Review employee records across branches and roles with contact information." },
  { label: "Create", description: "Register a new employee with branch, role, identity, and login-related values." },
  { label: "Edit", description: "Update employee profile, branch assignment, role, and contact details." },
  { label: "Detail", description: "Open one employee record and inspect core organizational data before changes." },
];

export function EmployeesPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const action = searchParams.get("action") ?? "List";
  const [data, setData] = useState<EmployeeListDto[]>([]);
  const [detail, setDetail] = useState<EmployeeSelectDto | null>(null);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState({ branchId: "", roleId: "", firstName: "", lastName: "", email: "", phone: "", authRole: "Employee", password: "" });
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [branches, setBranches] = useState<BranchListDto[]>([]);
  const [roles, setRoles] = useState<RoleListDto[]>([]);
  const [editSearch, setEditSearch] = useState("");
  const [detailSearch, setDetailSearch] = useState("");
  const [listSearch, setListSearch] = useState("");

  async function load() { setLoading(true); const r = await service.list(); if (r.success && r.data) setData(r.data); setLoading(false); }
  async function loadDetail(id: number) { setLoading(true); const r = await service.select(id); if (r.success && r.data) setDetail(r.data); setLoading(false); }
  useEffect(() => { load(); }, []);
  useEffect(() => {
    branchService.list().then((r) => r.success && setBranches(r.data ?? []));
    roleService.list().then((r) => r.success && setRoles(r.data ?? []));
  }, []);

  const filterEmployees = (query: string) => {
    if (query.startsWith("#")) return [];
    const raw = query.trim();
    if (!raw) return [];
    const q = raw.toLowerCase();
    return data.filter((e) => {
      if (e.employeeId.toString().includes(q)) return true;
      if (e.fullName.toLowerCase().includes(q)) return true;
      if (e.email.toLowerCase().includes(q)) return true;
      return false;
    }).slice(0, 8);
  };

  const searchKeyDown = (e: React.KeyboardEvent, query: string, results: ReturnType<typeof filterEmployees>, setId: (id: number) => void) => {
    if (e.key === "Enter") {
      e.preventDefault();
      if (query.startsWith("#")) {
        const id = Number(query.replace(/^#/, "").trim());
        if (!isNaN(id) && id > 0) setId(id);
        return;
      }
      if (results.length > 0) setId(results[0].employeeId);
    }
  };

  function setAction(a: string, keepId = false) {
    if (!keepId) setEditId(null);
    setDetail(null);
    setMessage(""); setEditSearch(""); setDetailSearch("");
    setSearchParams({ action: a });
  }

  async function handleCreate(e: FormEvent) {
    e.preventDefault(); setMessage(""); setLoading(true);
    const r = await service.create({
      branchId: Number(form.branchId), roleId: Number(form.roleId), firstName: form.firstName,
      lastName: form.lastName, email: form.email, phone: form.phone, authRole: form.authRole, password: form.password,
    });
    if (r.success) { setMessage("Created."); setForm({ branchId: "", roleId: "", firstName: "", lastName: "", email: "", phone: "", authRole: "Employee", password: "" }); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleUpdate(e: FormEvent) {
    e.preventDefault(); if (!editId) return; setMessage(""); setLoading(true);
    const body: Record<string, unknown> = {};
    if (form.branchId) body.branchId = Number(form.branchId);
    if (form.roleId) body.roleId = Number(form.roleId);
    if (form.firstName) body.firstName = form.firstName;
    if (form.lastName) body.lastName = form.lastName;
    if (form.email) body.email = form.email;
    if (form.phone) body.phone = form.phone;
    if (form.authRole) body.authRole = form.authRole;
    if (form.password) body.password = form.password;
    const r = await service.update(editId, body);
    if (r.success) { setMessage("Updated."); setEditId(null); load(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleDelete(id: number) { if (!window.confirm("Delete?")) return; await service.remove(id); load(); }

  return (
    <>
      <h1>Employees</h1>
      <p className="page-subtitle">Choose the employee administration flow you want to open.</p>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>List</h2></div><span className="customer-stage-tag">Employees / List</span></div>
          {loading && <p className="status">Loading...</p>}
          <div className="customer-toolbar">
            <input type="text" value={listSearch} onChange={(e) => setListSearch(e.target.value)}
              placeholder="Filter by name, email, or ID..." className="list-search-input" />
            {listSearch && <button type="button" onClick={() => setListSearch("")} className="customer-inline-button">✕ Clear</button>}
            <button type="button" onClick={() => setAction("Create")} className="customer-primary-button" style={{ marginLeft: "auto" }}>+ Create New Employee</button>
          </div>
          <section className="table-card">
            <table>
              <thead><tr><th>ID</th><th>Name</th><th>Email</th><th>Branch</th><th>Role</th><th>Auth</th><th></th></tr></thead>
              <tbody>
                {data.length === 0 && !loading && <tr><td colSpan={7}>No employees.</td></tr>}
                {(() => {
                  const filtered = listSearch.trim()
                    ? data.filter((e) => {
                        const q = listSearch.toLowerCase();
                        if (e.employeeId.toString().includes(q)) return true;
                        if (e.fullName.toLowerCase().includes(q)) return true;
                        if (e.email.toLowerCase().includes(q)) return true;
                        return false;
                      })
                    : data;
                  if (listSearch.trim() && filtered.length === 0 && !loading) return <tr><td colSpan={7}>No matching employees.</td></tr>;
                  return filtered.map((emp) => (
                  <tr key={emp.employeeId}>
                    <td className="mono">{emp.employeeId}</td>
                    <td>{emp.fullName}</td><td>{emp.email}</td>
                    <td>{branches.find((b) => b.branchId === emp.branchId)?.branchName ?? `Branch #${emp.branchId}`}</td>
                    <td>{roles.find((r) => r.roleId === emp.roleId)?.roleName ?? `Role #${emp.roleId}`}</td>
                    <td>{emp.authRole}</td>
                    <td><div className="customer-row-actions">
                      <button type="button" onClick={() => { setEditId(emp.employeeId); setMessage(""); setAction("Edit", true); loadDetail(emp.employeeId); }} className="customer-inline-button">Edit</button>
                      <button type="button" onClick={() => handleDelete(emp.employeeId)} className="customer-inline-button danger">Delete</button>
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
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Create</h2></div><span className="customer-stage-tag">Employees / Create</span></div>
          <form onSubmit={handleCreate} className="customer-form-shell">
            <div className="customer-field-strip customer-form-stack">
              <select value={form.branchId} onChange={(e) => setForm({ ...form, branchId: e.target.value })} required>
                <option value="">Select branch</option>
                {branches.map((b) => <option key={b.branchId} value={b.branchId}>{b.branchName}</option>)}
              </select>
              <select value={form.roleId} onChange={(e) => setForm({ ...form, roleId: e.target.value })} required>
                <option value="">Select role</option>
                {roles.map((r) => <option key={r.roleId} value={r.roleId}>{r.roleName}</option>)}
              </select>
              <input value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} placeholder="First Name *" required />
              <input value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} placeholder="Last Name *" required />
              <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} placeholder="Email *" required />
              <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} placeholder="Phone *" required />
              <select value={form.authRole} onChange={(e) => setForm({ ...form, authRole: e.target.value })}>
                <option value="Employee">Employee</option><option value="Admin">Admin</option>
              </select>
              <input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} placeholder="Password *" required />
            </div>
            <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Creating..." : "Create Employee"}</button>
          </form>
          {message && <p className={message.includes("Created") ? "status success" : "status"}>{message}</p>}
        </section>
      )}

      {action === "Edit" && (() => {
        const editResults = filterEmployees(editSearch);
        return (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Edit</h2></div><span className="customer-stage-tag">Employees / Edit</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Search employee by name, email, or ID:</p>
            <div className="search-input-wrap">
              <input type="text" value={editSearch}
                onChange={(e) => { setEditSearch(e.target.value); setEditId(null); setDetail(null); }}
                onKeyDown={(e) => searchKeyDown(e, editSearch, editResults, (id) => { setEditId(id); setEditSearch(`${id}`); setMessage(""); loadDetail(id); })}
                placeholder="Type #123, name, or email..." />
              {editResults.length > 0 && (
                <div className="search-results-dropdown">
                  {editResults.map((emp) => (
                    <button type="button" key={emp.employeeId}
                      onClick={() => { setEditId(emp.employeeId); setEditSearch(`${emp.employeeId} — ${emp.fullName} — ${emp.email}`); setMessage(""); loadDetail(emp.employeeId); }}>
                      #{emp.employeeId} — {emp.fullName} — {emp.email}
                    </button>
                  ))}
                </div>
              )}
            </div>
            <button type="button" onClick={() => { if (editId) { setMessage(""); loadDetail(editId); } }} disabled={!editId || loading} className="customer-inline-button view">Load</button>
          </div>
          {detail && (
            <form onSubmit={handleUpdate} className="customer-form-shell">
              <p className="customer-stage-copy">Editing employee #{detail.employeeId} - {detail.firstName} {detail.lastName}</p>
              <div className="customer-field-strip customer-form-stack">
                <label>Branch <select value={form.branchId || detail.branchId || ""} onChange={(e) => setForm({ ...form, branchId: e.target.value })}>
                  <option value="">Leave unchanged</option>
                  {branches.map((b) => <option key={b.branchId} value={b.branchId}>{b.branchName}</option>)}
                </select></label>
                <label>Role <select value={form.roleId || detail.roleId || ""} onChange={(e) => setForm({ ...form, roleId: e.target.value })}>
                  <option value="">Leave unchanged</option>
                  {roles.map((r) => <option key={r.roleId} value={r.roleId}>{r.roleName}</option>)}
                </select></label>
                <label>First Name <input value={form.firstName || detail.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} placeholder="Leave blank" /></label>
                <label>Last Name <input value={form.lastName || detail.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} placeholder="Leave blank" /></label>
                <label>Email <input type="email" value={form.email || detail.email} onChange={(e) => setForm({ ...form, email: e.target.value })} placeholder="Leave blank" /></label>
                <label>Phone <input value={form.phone || detail.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} placeholder="Leave blank" /></label>
                <label>Auth Role <select value={form.authRole || detail.authRole} onChange={(e) => setForm({ ...form, authRole: e.target.value })}><option value="Employee">Employee</option><option value="Admin">Admin</option></select></label>
                <label>Password <input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} placeholder="Leave blank" /></label>
              </div>
              <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Updating..." : "Update Employee"}</button>
            </form>
          )}
          {message && <p className={message.includes("Updated") ? "status success" : "status"}>{message}</p>}
        </section>
        );
      })()}

      {action === "Detail" && (() => {
        const detailResults = filterEmployees(detailSearch);
        return (
        <section className="customer-stage">
          <div className="customer-stage-head"><div><p>Current workspace</p><h2>Detail</h2></div><span className="customer-stage-tag">Employees / Detail</span></div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Search employee by name, email, or ID:</p>
            <div className="search-input-wrap">
              <input type="text" value={detailSearch}
                onChange={(e) => { setDetailSearch(e.target.value); setEditId(null); setDetail(null); }}
                onKeyDown={(e) => searchKeyDown(e, detailSearch, detailResults, (id) => { setEditId(id); setDetailSearch(`${id}`); loadDetail(id); })}
                placeholder="Type #123, name, or email..." />
              {detailResults.length > 0 && (
                <div className="search-results-dropdown">
                  {detailResults.map((emp) => (
                    <button type="button" key={emp.employeeId}
                      onClick={() => { setEditId(emp.employeeId); setDetailSearch(`${emp.employeeId} — ${emp.fullName} — ${emp.email}`); loadDetail(emp.employeeId); }}>
                      #{emp.employeeId} — {emp.fullName} — {emp.email}
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
                <tr><td><strong>ID</strong></td><td className="mono">{detail.employeeId}</td></tr>
                <tr><td><strong>First Name</strong></td><td>{detail.firstName}</td></tr>
                <tr><td><strong>Last Name</strong></td><td>{detail.lastName}</td></tr>
                <tr><td><strong>Email</strong></td><td>{detail.email}</td></tr>
                <tr><td><strong>Phone</strong></td><td>{detail.phone}</td></tr>
                <tr><td><strong>Auth Role</strong></td><td>{detail.authRole}</td></tr>
                <tr><td><strong>Hire Date</strong></td><td>{formatDate(detail.hireDate)}</td></tr>
                <tr><td><strong>Branch</strong></td><td>#{detail.branchId} — {branches.find((b) => b.branchId === detail.branchId)?.branchName ?? "Unknown"}</td></tr>
                <tr><td><strong>Role</strong></td><td>#{detail.roleId} — {roles.find((r) => r.roleId === detail.roleId)?.roleName ?? "Unknown"}</td></tr>
              </tbody></table>
            </section>
          )}
        </section>
        );
      })()}
    </>
  );
}
