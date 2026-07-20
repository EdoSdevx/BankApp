import { api } from "@/services/api";
import type { ApiResponse, ExchangeRateListDto, ExchangeRateCreateDto, ExchangeRateUpdateDto } from "@/types";

export async function list(): Promise<{ success: boolean; data?: ExchangeRateListDto[] } & ApiResponse> {
  return api.get("/exchangeRates");
}

export async function select(id: number): Promise<{ success: boolean; data?: ExchangeRateListDto } & ApiResponse> {
  return api.get(`/exchangeRates/${id}`);
}

export async function create(dto: ExchangeRateCreateDto): Promise<ApiResponse> {
  return api.post("/exchangeRates", dto);
}

export async function update(id: number, dto: Partial<ExchangeRateUpdateDto>): Promise<ApiResponse> {
  return api.put(`/exchangeRates/${id}`, { ...dto, rateId: id });
}

export async function remove(id: number): Promise<ApiResponse> {
  return api.del(`/exchangeRates/${id}`);
}
