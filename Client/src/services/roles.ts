import { api } from "@/services/api";
import type { ApiResponse, RoleListDto, RoleSelectDto } from "@/types";

export async function list(): Promise<{ success: boolean; data?: RoleListDto[] } & ApiResponse> {
  return api.get("/roles");
}

export async function select(id: number): Promise<{ success: boolean; data?: RoleSelectDto } & ApiResponse> {
  return api.get(`/roles/${id}`);
}

export async function create(dto: { roleName: string; description: string }): Promise<ApiResponse> {
  return api.post("/roles", dto);
}

export async function update(id: number, dto: Partial<{ roleId: number; roleName: string; description: string }>): Promise<ApiResponse> {
  return api.put(`/roles/${id}`, { ...dto, roleId: id });
}

export async function remove(id: number): Promise<ApiResponse> {
  return api.del(`/roles/${id}`);
}
