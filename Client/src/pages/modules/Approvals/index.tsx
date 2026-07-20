import { useEffect, useState } from "react";
import type { PendingTransferDto } from "@/types";
import * as adminService from "@/services/admin";
import { formatDate, FeedbackBar } from "@/components/ui";

export function PendingTransfersPage() {
  const [transfers, setTransfers] = useState<PendingTransferDto[]>([]);
  const [message, setMessage] = useState("");
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);

  async function load() {
    setLoading(true);
    const r = await adminService.getPendingTransfers();
    if (r.success && r.data) setTransfers(r.data);
    setLoading(false);
  }

  useEffect(() => { load(); }, []);

  async function handleApprove(id: number) {
    if (!window.confirm("Approve this transfer?")) return;
    setMessage("");
    setLoading(true);
    const r = await adminService.approveTransfer(id);
    setSuccess(r.success);
    setMessage(r.message);
    if (r.success) load();
    setLoading(false);
  }

  async function handleReject(id: number) {
    const reason = window.prompt("Rejection reason (optional):");
    if (reason === null) return;
    setMessage("");
    setLoading(true);
    const r = await adminService.rejectTransfer(id, reason || undefined);
    setSuccess(r.success);
    setMessage(r.message);
    if (r.success) load();
    setLoading(false);
  }

  const sourceName = (t: PendingTransferDto) =>
    t.srcFirstName ? `${t.srcFirstName} ${t.srcLastName}` : `Customer #${t.createdByCustomerId}`;

  const targetName = (t: PendingTransferDto) =>
    t.tgtFirstName ? `${t.tgtFirstName} ${t.tgtLastName}` : `Account #${t.targetAccountId}`;

  return (
    <>
      <h1>Pending Transfer Approvals</h1>
      <p className="page-subtitle">Review and approve or reject transfers above the threshold.</p>

      <FeedbackBar message={message} type={success ? "success" : "error"} />

      <section className="table-card" style={{ marginTop: 18 }}>
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>From</th>
              <th>To</th>
              <th className="num">Amount</th>
              <th>Currency</th>
              <th>Description</th>
              <th>Created</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {transfers.length === 0 && !loading && <tr><td colSpan={8}>No pending transfers.</td></tr>}
            {transfers.map((t) => (
              <tr key={t.pendingTransferId}>
                <td className="mono">{t.pendingTransferId}</td>
                <td>{sourceName(t)}</td>
                <td>{targetName(t)}</td>
                <td className="num mono">{t.amount.toFixed(2)}</td>
                <td>{t.currencyCode}</td>
                <td>{t.description || "-"}</td>
                <td>{formatDate(t.createdAt)}</td>
                <td>
                  <div className="customer-row-actions">
                    <button type="button" onClick={() => handleApprove(t.pendingTransferId)} disabled={loading} className="customer-inline-button">Approve</button>
                    <button type="button" onClick={() => handleReject(t.pendingTransferId)} disabled={loading} className="customer-inline-button danger">Reject</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </>
  );
}
