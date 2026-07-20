import { useState, useEffect, useCallback, type FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import type { CustomerListDto, CustomerSelectDto, CustomerFormState, CustomerTouchedState } from "@/types";
import type { ApiResponse } from "@/types";
import * as service from "@/services/customers";
import { ModuleActionIcon } from "@/components/icons";
import { StatusDot, formatDate } from "@/components/ui";

const ACTIONS = [
  { label: "List", description: "Review all customer records with active status, contact info, and quick filtering." },
  { label: "Create", description: "Register a new customer with identity, contact, address, and initial access values." },
  { label: "Edit", description: "Load an existing customer and update profile, contact details, or activity status." },
  { label: "Detail", description: "Open a single customer record and review profile data before deeper operations." },
];

function emptyForm(): CustomerFormState {
  return { firstName: "", lastName: "", email: "", phone: "", address: "", password: "", isActive: true };
}

function emptyTouched(): CustomerTouchedState {
  return { firstName: false, lastName: false, email: false, phone: false, address: false, password: false, isActive: false };
}

export function CustomersPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const action = searchParams.get("action") ?? "List";

  const [data, setData] = useState<CustomerListDto[]>([]);
  const [detail, setDetail] = useState<CustomerSelectDto | null>(null);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState(emptyForm());
  const [touched, setTouched] = useState(emptyTouched());
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const loadList = useCallback(async () => {
    setLoading(true);
    const r = await service.list();
    if (r.success && r.data) setData(r.data);
    setLoading(false);
  }, []);

  const loadDetail = useCallback(async (id: number) => {
    setLoading(true);
    const r = await service.select(id);
    if (r.success && r.data) { setDetail(r.data); setForm(emptyForm()); setTouched(emptyTouched()); }
    else setDetail(null);
    setLoading(false);
  }, []);

  useEffect(() => { loadList(); }, [loadList]);

  function setAction(a: string, keepId = false) {
    if (!keepId) setEditId(null);
    setDetail(null);
    setMessage("");
    setSearchParams({ action: a });
  }

  async function handleCreate(e: FormEvent) {
    e.preventDefault();
    setMessage("");
    setLoading(true);
    const r = await service.create({
      firstName: form.firstName, lastName: form.lastName, email: form.email,
      phone: form.phone || null, address: form.address, password: form.password,
    });
    if (r.success) { setMessage("Customer created successfully."); setForm(emptyForm()); loadList(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleUpdate(e: FormEvent) {
    e.preventDefault();
    if (!editId) return;
    setMessage("");
    setLoading(true);
    const body: Record<string, unknown> = {};
    if (touched.firstName) body.firstName = form.firstName;
    if (touched.lastName) body.lastName = form.lastName;
    if (touched.email) body.email = form.email;
    if (touched.phone) body.phone = form.phone || null;
    if (touched.address) body.address = form.address;
    if (touched.password && form.password) body.password = form.password;
    if (touched.isActive) body.isActive = form.isActive;
    const r = await service.update(editId, body);
    if (r.success) { setMessage("Customer updated successfully."); setEditId(null); loadList(); }
    else setMessage((r as ApiResponse).errors?.[0]?.message ?? r.message);
    setLoading(false);
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Deactivate this customer?")) return;
    await service.remove(id);
    loadList();
  }

  return (
    <>
      <h1>Customers</h1>
      <p className="page-subtitle">Choose the customer flow you want to open in the workspace.</p>

      <section className="customer-actions" aria-label="Customers actions">
        {ACTIONS.map((item) => (
          <button
            key={item.label}
            type="button"
            className={`customer-action-card ${action === item.label ? "active" : ""}`}
            onClick={() => setAction(item.label)}
          >
            <span className="customer-action-top">
              <strong>{item.label}</strong>
              <ModuleActionIcon label={item.label} />
            </span>
            <span className="customer-action-text">{item.description}</span>
          </button>
        ))}
      </section>

      {action === "List" && (
        <section className="customer-stage" aria-label="Customers list">
          <div className="customer-stage-head">
            <div><p>Current workspace</p><h2>List</h2></div>
            <span className="customer-stage-tag">Customers / List</span>
          </div>
          {loading && <p className="status">Loading...</p>}
          <div className="customer-toolbar">
            <button type="button" onClick={() => setAction("Create")} className="customer-primary-button">+ Create New Customer</button>
          </div>
          <section className="table-card">
            <table>
              <thead><tr><th>ID</th><th>Name</th><th>Email</th><th>Phone</th><th>Active</th><th></th></tr></thead>
              <tbody>
                {data.length === 0 && !loading && <tr><td colSpan={6}>No customers found.</td></tr>}
                {data.map((c) => (
                  <tr key={c.customerId}>
                    <td className="mono">{c.customerId}</td>
                    <td>{c.fullName}</td>
                    <td>{c.email}</td>
                    <td>{c.phone || "-"}</td>
                    <td><span className="status-line"><StatusDot active={c.isActive} />{c.isActive ? "Yes" : "No"}</span></td>
                    <td>
                      <div className="customer-row-actions">
                        <button type="button" onClick={() => { setEditId(c.customerId); loadDetail(c.customerId); setAction("Edit", true); }} className="customer-inline-button">Edit</button>
                        <button type="button" onClick={() => handleDelete(c.customerId)} className="customer-inline-button danger">Deactivate</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        </section>
      )}

      {action === "Create" && (
        <section className="customer-stage" aria-label="Create customer">
          <div className="customer-stage-head">
            <div><p>Current workspace</p><h2>Create</h2></div>
            <span className="customer-stage-tag">Customers / Create</span>
          </div>
          <form onSubmit={handleCreate} className="customer-form-shell">
            <div className="customer-field-strip customer-form-stack">
              <input value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} placeholder="First Name *" required />
              <input value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} placeholder="Last Name *" required />
              <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} placeholder="Email *" required />
              <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} placeholder="Phone" />
              <input value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} placeholder="Address *" required />
              <input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} placeholder="Password *" required />
            </div>
            <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Creating..." : "Create Customer"}</button>
          </form>
          {message && <p className={`${message.includes("success") ? "status success" : "status"} customer-feedback`}>{message}</p>}
        </section>
      )}

      {action === "Edit" && (
        <section className="customer-stage" aria-label="Edit customer">
          <div className="customer-stage-head">
            <div><p>Current workspace</p><h2>Edit</h2></div>
            <span className="customer-stage-tag">Customers / Edit</span>
          </div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Enter customer ID to load for editing:</p>
            <input type="number" value={editId ?? ""} onChange={(e) => setEditId(e.target.value ? Number(e.target.value) : null)} placeholder="Customer ID" />
            <button type="button" onClick={() => editId && loadDetail(editId)} disabled={!editId || loading} className="customer-inline-button view">Load</button>
          </div>
          {detail && (
            <form onSubmit={handleUpdate} className="customer-form-shell">
              <p className="customer-stage-copy">Editing customer #{detail.customerId} - {detail.firstName} {detail.lastName}</p>
              <div className="customer-field-strip customer-form-stack">
                <label>First Name <input value={touched.firstName ? form.firstName : detail.firstName} onChange={(e) => { setTouched({ ...touched, firstName: true }); setForm({ ...form, firstName: e.target.value }); }} placeholder="Leave blank to keep" /></label>
                <label>Last Name <input value={touched.lastName ? form.lastName : detail.lastName} onChange={(e) => { setTouched({ ...touched, lastName: true }); setForm({ ...form, lastName: e.target.value }); }} placeholder="Leave blank to keep" /></label>
                <label>Email <input type="email" value={touched.email ? form.email : detail.email} onChange={(e) => { setTouched({ ...touched, email: true }); setForm({ ...form, email: e.target.value }); }} placeholder="Leave blank to keep" /></label>
                <label>Phone <input value={touched.phone ? form.phone : (detail.phone ?? "")} onChange={(e) => { setTouched({ ...touched, phone: true }); setForm({ ...form, phone: e.target.value }); }} placeholder="Leave blank to keep" /></label>
                <label>Address <input value={touched.address ? form.address : detail.address} onChange={(e) => { setTouched({ ...touched, address: true }); setForm({ ...form, address: e.target.value }); }} placeholder="Leave blank to keep" /></label>
                <label>Password <input type="password" value={form.password} onChange={(e) => { setTouched({ ...touched, password: true }); setForm({ ...form, password: e.target.value }); }} placeholder="Leave blank to keep" /></label>
                <label>Active <select value={(touched.isActive ? form.isActive : detail.isActive) ? "yes" : "no"} onChange={(e) => { setTouched({ ...touched, isActive: true }); setForm({ ...form, isActive: e.target.value === "yes" }); }}><option value="yes">Yes</option><option value="no">No</option></select></label>
              </div>
              <button type="submit" disabled={loading} className="customer-primary-button">{loading ? "Updating..." : "Update Customer"}</button>
            </form>
          )}
          {message && <p className={`${message.includes("success") ? "status success" : "status"} customer-feedback`}>{message}</p>}
        </section>
      )}

      {action === "Detail" && (
        <section className="customer-stage" aria-label="Customer detail">
          <div className="customer-stage-head">
            <div><p>Current workspace</p><h2>Detail</h2></div>
            <span className="customer-stage-tag">Customers / Detail</span>
          </div>
          <div className="customer-detail-toolbar">
            <p className="customer-detail-label">Enter a customer ID to view details:</p>
            <input type="number" value={editId ?? ""} onChange={(e) => setEditId(e.target.value ? Number(e.target.value) : null)} placeholder="Customer ID" />
            <button type="button" onClick={() => editId && loadDetail(editId)} disabled={!editId || loading} className="customer-inline-button view">View</button>
          </div>
          {loading && <p className="status customer-feedback">Loading...</p>}
          {detail && (
            <section className="table-card customer-detail-card">
              <table><tbody>
                <tr><td><strong>Customer ID</strong></td><td className="mono">{detail.customerId}</td></tr>
                <tr><td><strong>First Name</strong></td><td>{detail.firstName}</td></tr>
                <tr><td><strong>Last Name</strong></td><td>{detail.lastName}</td></tr>
                <tr><td><strong>Email</strong></td><td>{detail.email}</td></tr>
                <tr><td><strong>Phone</strong></td><td>{detail.phone || "-"}</td></tr>
                <tr><td><strong>Address</strong></td><td>{detail.address}</td></tr>
                <tr><td><strong>Created Date</strong></td><td>{formatDate(detail.createdDate)}</td></tr>
                <tr><td><strong>Active</strong></td><td><span className="status-line"><StatusDot active={detail.isActive} />{detail.isActive ? "Yes" : "No"}</span></td></tr>
              </tbody></table>
            </section>
          )}
        </section>
      )}
    </>
  );
}
