import { api } from "@/services/api";
import type { ApiResponse, BranchListDto, BranchSelectDto, BranchCreateDto, BranchUpdateDto } from "@/types";

export async function list(): Promise<{ success: boolean; data?: BranchListDto[] } & ApiResponse> {
  return api.get("/branches");
}

export async function select(id: number): Promise<{ success: boolean; data?: BranchSelectDto } & ApiResponse> {
  return api.get(`/branches/${id}`);
}

export async function create(dto: BranchCreateDto): Promise<ApiResponse> {
  return api.post("/branches", dto);
}

export async function update(id: number, dto: Partial<BranchUpdateDto>): Promise<ApiResponse> {
  return api.put(`/branches/${id}`, { ...dto, branchId: id });
}

export async function remove(id: number): Promise<ApiResponse> {
  return api.del(`/branches/${id}`);
}
