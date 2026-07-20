import { api } from "@/services/api";
import type { ApiResponse, BillListDto, BillSelectDto, BillCreateDto, BillUpdateDto } from "@/types";

export async function list(): Promise<{ success: boolean; data?: BillListDto[] } & ApiResponse> {
  return api.get("/bills");
}

export async function select(id: number): Promise<{ success: boolean; data?: BillSelectDto } & ApiResponse> {
  return api.get(`/bills/${id}`);
}

export async function create(dto: BillCreateDto & { paidDate: string | null }): Promise<ApiResponse> {
  return api.post("/bills", dto);
}

export async function update(id: number, dto: Partial<BillUpdateDto>): Promise<ApiResponse> {
  return api.put(`/bills/${id}`, { ...dto, billId: id });
}

export async function markPaid(id: number): Promise<ApiResponse> {
  return api.put(`/bills/${id}/mark-paid`, {});
}

export async function remove(id: number): Promise<ApiResponse> {
  return api.del(`/bills/${id}`);
}
