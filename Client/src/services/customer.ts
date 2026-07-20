import { api } from "@/services/api";
import type { ApiResponse, AccountListDto, AccountSelectDto, BillListDto, BranchListDto, CurrencyListDto, ExchangeRateListDto, TransactionListDto, CustomerDashboardDto } from "@/types";

export async function getDashboard(): Promise<ApiResponse & { data: CustomerDashboardDto }> {
  return api.get("/customer/dashboard");
}

export async function getAccounts(): Promise<ApiResponse & { data: AccountListDto[] }> {
  return api.get("/customer/accounts");
}

export async function getAccount(id: number): Promise<ApiResponse & { data: AccountSelectDto }> {
  return api.get(`/customer/accounts/${id}`);
}

export async function createAccount(branchId: number, currencyCode: string): Promise<ApiResponse> {
  return api.post("/customer/accounts", { branchId, currencyCode });
}

export async function getTransactions(): Promise<ApiResponse & { data: TransactionListDto[] }> {
  return api.get("/customer/transactions");
}

export async function transfer(sourceAccountId: number, targetAccountId: number, amount: number, description?: string): Promise<ApiResponse> {
  return api.post("/customer/transfer", { sourceAccountId, targetAccountId, amount, description: description || null });
}

export async function getBills(): Promise<ApiResponse & { data: BillListDto[] }> {
  return api.get("/customer/bills");
}

export async function payBill(billId: number, accountId?: number): Promise<ApiResponse> {
  return api.post(`/customer/bills/${billId}/pay`, { accountId: accountId ?? null });
}

export async function exchange(sourceAccountId: number, targetAccountId: number, targetAmount: number): Promise<ApiResponse> {
  return api.post("/customer/exchange", { sourceAccountId, targetAccountId, targetAmount });
}

export async function lookupOwner(accountId: number): Promise<ApiResponse & { data?: { firstName: string; lastName: string } }> {
  return api.get(`/customer/accounts/${accountId}/owner`);
}

export async function getRecentTransfers(accountId: number): Promise<ApiResponse & { data?: { transactionId: number; transactionType: string; amount: number; currencyCode: string; firstName: string | null; lastName: string | null; relatedCurrencyCode: string | null }[] }> {
  return api.get(`/customer/accounts/${accountId}/recent-transfers`);
}

export async function getBranches(): Promise<ApiResponse & { data: BranchListDto[] }> {
  return api.get("/customer/branches");
}

export async function getCurrencies(): Promise<ApiResponse & { data: CurrencyListDto[] }> {
  return api.get("/customer/currencies");
}

export async function getExchangeRates(): Promise<ApiResponse & { data: ExchangeRateListDto[] }> {
  return api.get("/customer/exchange-rates");
}

export async function transferBetween(sourceAccountId: number, targetAccountId: number, amount: number, description?: string): Promise<ApiResponse> {
  return api.post("/customer/transfer-between", { sourceAccountId, targetAccountId, amount, description: description || null });
}
