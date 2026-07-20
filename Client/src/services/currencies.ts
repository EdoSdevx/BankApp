import { api } from "@/services/api";
import type { ApiResponse, CurrencyListDto, CurrencySelectDto } from "@/types";

export async function list(): Promise<{ success: boolean; data?: CurrencyListDto[] } & ApiResponse> {
  return api.get("/currencies");
}

export async function select(code: string): Promise<{ success: boolean; data?: CurrencySelectDto } & ApiResponse> {
  return api.get(`/currencies/${encodeURIComponent(code)}`);
}

export async function create(dto: { currencyCode: string; currencyName: string }): Promise<ApiResponse> {
  return api.post("/currencies", dto);
}

export async function update(code: string, dto: { currencyName: string }): Promise<ApiResponse> {
  return api.put(`/currencies/${encodeURIComponent(code)}`, { ...dto, currencyCode: code });
}

export async function remove(code: string): Promise<ApiResponse> {
  return api.del(`/currencies/${encodeURIComponent(code)}`);
}
