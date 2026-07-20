import { api } from "@/services/api";
import type { ApiResponse, LoanListDto, LoanDetailDto, LoanTypeDto, LoanScheduleDto, LoanPaymentDto } from "@/types";

export async function getTypes(): Promise<{ success: boolean; data?: LoanTypeDto[] } & ApiResponse> {
  return api.get("/loans/types");
}

export async function list(): Promise<{ success: boolean; data?: LoanListDto[] } & ApiResponse> {
  return api.get("/loans");
}

export async function select(id: number): Promise<{ success: boolean; data?: LoanDetailDto } & ApiResponse> {
  return api.get(`/loans/${id}`);
}

export async function approve(id: number): Promise<ApiResponse> {
  return api.post(`/loans/${id}/approve`, {});
}

export async function reject(id: number, reason?: string): Promise<ApiResponse> {
  return api.post(`/loans/${id}/reject`, { reason: reason || null });
}

export async function getSchedule(id: number): Promise<{ success: boolean; data?: LoanScheduleDto[] } & ApiResponse> {
  return api.get(`/loans/${id}/schedule`);
}

export async function getPayments(id: number): Promise<{ success: boolean; data?: LoanPaymentDto[] } & ApiResponse> {
  return api.get(`/loans/${id}/payments`);
}
