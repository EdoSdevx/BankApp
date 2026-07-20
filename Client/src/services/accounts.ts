import { api } from "@/services/api";
import type { ApiResponse, AccountListDto, AccountSelectDto, AccountCreateDto, AccountUpdateDto } from "@/types";

export async function list(): Promise<{ success: boolean; data?: AccountListDto[] } & ApiResponse> {
  return api.get("/accounts");
}

export async function select(id: number): Promise<{ success: boolean; data?: AccountSelectDto } & ApiResponse> {
  return api.get(`/accounts/${id}`);
}

export async function create(dto: AccountCreateDto): Promise<ApiResponse> {
  return api.post("/accounts", dto);
}

export async function update(id: number, dto: Partial<AccountUpdateDto>): Promise<ApiResponse> {
  return api.put(`/accounts/${id}`, { ...dto, accountId: id });
}

export async function remove(id: number): Promise<ApiResponse> {
  return api.del(`/accounts/${id}`);
}

export async function transferBetween(sourceAccountId: number, targetAccountId: number, amount: number, description?: string): Promise<ApiResponse> {
  return api.post("/accounts/transfer-between", { sourceAccountId, targetAccountId, amount, description: description || null });
}
