import { api } from "@/services/api";
import type { ApiResponse, PendingTransferDto } from "@/types";

export async function getPendingTransfers(): Promise<ApiResponse & { data: PendingTransferDto[] }> {
  return api.get("/admin/pending-transfers");
}

export async function approveTransfer(id: number): Promise<ApiResponse> {
  return api.post(`/admin/pending-transfers/${id}/approve`, {});
}

export async function rejectTransfer(id: number, reason?: string): Promise<ApiResponse> {
  return api.post(`/admin/pending-transfers/${id}/reject`, { reason: reason || null });
}
