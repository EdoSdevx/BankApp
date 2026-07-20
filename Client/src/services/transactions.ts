import { api } from "@/services/api";
import type { ApiResponse, TransactionListDto, TransactionSelectDto, TransactionCreateDto, TransactionUpdateDto } from "@/types";

export async function list(): Promise<{ success: boolean; data?: TransactionListDto[] } & ApiResponse> {
  return api.get("/transactions");
}

export async function select(id: number): Promise<{ success: boolean; data?: TransactionSelectDto } & ApiResponse> {
  return api.get(`/transactions/${id}`);
}

export async function create(dto: TransactionCreateDto): Promise<ApiResponse> {
  return api.post("/transactions", dto);
}

export async function update(id: number, dto: Partial<TransactionUpdateDto>): Promise<ApiResponse> {
  return api.put(`/transactions/${id}`, { ...dto, transactionId: id });
}

export async function remove(id: number): Promise<ApiResponse> {
  return api.del(`/transactions/${id}`);
}
