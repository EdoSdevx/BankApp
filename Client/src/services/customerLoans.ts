import { api } from "@/services/api";
import type { ApiResponse, LoanListDto, LoanDetailDto, LoanScheduleDto, LoanPaymentDto, LoanTypeDto } from "@/types";

export async function getTypes(): Promise<{ success: boolean; data?: LoanTypeDto[] } & ApiResponse> {
  return api.get("/customer/loans/types");
}

export async function myLoans(): Promise<{ success: boolean; data?: LoanListDto[] } & ApiResponse> {
  return api.get("/customer/loans");
}

export async function getDetail(id: number): Promise<{ success: boolean; data?: LoanDetailDto } & ApiResponse> {
  return api.get(`/customer/loans/${id}`);
}

export async function getSchedule(id: number): Promise<{ success: boolean; data?: LoanScheduleDto[] } & ApiResponse> {
  return api.get(`/customer/loans/${id}/schedule`);
}

export async function apply(dto: { loanTypeId: number; amount: number; termMonths: number; disbursementAccountId: number; paymentAccountId: number }): Promise<ApiResponse> {
  return api.post("/customer/loans/apply", dto);
}

export async function pay(loanId: number, scheduleId: number, accountId: number): Promise<ApiResponse> {
  return api.post("/customer/loans/pay", { loanId, scheduleId, accountId });
}

export async function closeEarly(loanId: number, accountId: number): Promise<ApiResponse> {
  return api.post("/customer/loans/close-early", { loanId, accountId });
}
