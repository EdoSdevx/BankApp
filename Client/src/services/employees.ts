import { api } from "@/services/api";
import type { ApiResponse, EmployeeListDto, EmployeeSelectDto, EmployeeCreateDto, EmployeeUpdateDto } from "@/types";

export async function list(): Promise<{ success: boolean; data?: EmployeeListDto[] } & ApiResponse> {
  return api.get("/employees");
}

export async function select(id: number): Promise<{ success: boolean; data?: EmployeeSelectDto } & ApiResponse> {
  return api.get(`/employees/${id}`);
}

export async function create(dto: EmployeeCreateDto): Promise<ApiResponse> {
  return api.post("/employees", dto);
}

export async function update(id: number, dto: Partial<EmployeeUpdateDto>): Promise<ApiResponse> {
  return api.put(`/employees/${id}`, { ...dto, employeeId: id });
}

export async function remove(id: number): Promise<ApiResponse> {
  return api.del(`/employees/${id}`);
}
