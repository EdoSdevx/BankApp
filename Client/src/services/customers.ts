import { api } from "@/services/api";
import type { ApiResponse, CustomerListDto, CustomerSelectDto, CustomerCreateDto, CustomerUpdateDto } from "@/types";

export async function list(): Promise<{ success: boolean; data?: CustomerListDto[] } & ApiResponse> {
  return api.get("/customers");
}

export async function select(id: number): Promise<{ success: boolean; data?: CustomerSelectDto } & ApiResponse> {
  return api.get(`/customers/${id}`);
}

export async function create(dto: CustomerCreateDto): Promise<ApiResponse> {
  return api.post("/customers", dto);
}

export async function update(id: number, dto: Partial<CustomerUpdateDto>): Promise<ApiResponse> {
  return api.put(`/customers/${id}`, { ...dto, customerId: id });
}

export async function remove(id: number): Promise<ApiResponse> {
  return api.del(`/customers/${id}`);
}
