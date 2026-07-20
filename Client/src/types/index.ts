export type ApiResponse = {
  success: boolean;
  message: string;
  statusCode: number;
  errors?: { propertyName: string; message: string }[];
};

export type ApiErrorResponse = ApiResponse & {
  errors?: { propertyName: string; message: string }[];
};

export type LoginResponse = {
  success: boolean;
  message: string;
  statusCode: number;
  data?: {
    userId: number;
    fullName: string;
    email: string;
    role: "Customer" | "Employee" | "Admin";
    token: string;
    expiresAtUtc: string;
  };
  errors?: { propertyName: string; message: string }[];
};

export type AuthUser = {
  token: string;
  fullName: string;
  role: string;
  expiresAtUtc: string;
};

export type CustomerListDto = {
  customerId: number;
  fullName: string;
  email: string;
  phone: string | null;
  isActive: boolean;
};

export type CustomerSelectDto = {
  customerId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  address: string;
  createdDate: string;
  isActive: boolean;
};

export type CustomerCreateDto = {
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  address: string;
  password: string;
};

export type CustomerUpdateDto = {
  customerId: number;
  firstName?: string | null;
  lastName?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
  isActive?: boolean | null;
  password?: string | null;
};

export type CustomerFormState = {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address: string;
  password: string;
  isActive: boolean;
};

export type CustomerTouchedState = Record<keyof CustomerFormState, boolean>;

export type BranchListDto = {
  branchId: number;
  branchName: string;
  branchCode: string;
  city: string;
};

export type BranchSelectDto = {
  branchId: number;
  branchName: string;
  branchCode: string;
  city: string;
  address: string;
  createdDate: string;
};

export type BranchCreateDto = {
  branchName: string;
  branchCode: string;
  city: string;
  address: string;
};

export type BranchUpdateDto = {
  branchId: number;
  branchName?: string | null;
  branchCode?: string | null;
  city?: string | null;
  address?: string | null;
};

export type RoleListDto = {
  roleId: number;
  roleName: string;
  description: string | null;
};

export type RoleSelectDto = {
  roleId: number;
  roleName: string;
  description: string | null;
};

export type RoleCreateDto = {
  roleName: string;
  description: string;
};

export type RoleUpdateDto = {
  roleId: number;
  roleName?: string | null;
  description?: string | null;
};

export type CurrencyListDto = {
  currencyCode: string;
  currencyName: string;
};

export type CurrencySelectDto = {
  currencyCode: string;
  currencyName: string;
};

export type CurrencyCreateDto = {
  currencyCode: string;
  currencyName: string;
};

export type CurrencyUpdateDto = {
  currencyCode: string;
  currencyName?: string | null;
};

export type AccountListDto = {
  accountId: number;
  customerId: number;
  branchId: number;
  currencyCode: string;
  balance: number;
  isActive: boolean;
};

export type AccountSelectDto = {
  accountId: number;
  customerId: number;
  branchId: number;
  currencyCode: string;
  balance: number;
  createdDate: string;
  isActive: boolean;
};

export type AccountCreateDto = {
  customerId: number;
  branchId: number;
  currencyCode: string;
  balance: number;
};

export type AccountUpdateDto = {
  accountId: number;
  currencyCode?: string | null;
  balance?: number | null;
};

export type EmployeeListDto = {
  employeeId: number;
  branchId: number;
  roleId: number;
  fullName: string;
  email: string;
  phone: string;
  authRole: string;
};

export type EmployeeSelectDto = {
  employeeId: number;
  branchId: number;
  roleId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  authRole: string;
  hireDate: string;
};

export type EmployeeCreateDto = {
  branchId: number;
  roleId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  authRole: string;
  password: string;
};

export type EmployeeUpdateDto = {
  employeeId: number;
  branchId?: number | null;
  roleId?: number | null;
  firstName?: string | null;
  lastName?: string | null;
  email?: string | null;
  phone?: string | null;
  authRole?: string | null;
  password?: string | null;
};

export type ExchangeRateListDto = {
  rateId: number;
  currencyCode: string;
  rate: number;
  rateDate: string;
  source: string;
};

export type ExchangeRateSelectDto = ExchangeRateListDto;

export type ExchangeRateCreateDto = {
  currencyCode: string;
  rate: number;
  source: string;
};

export type ExchangeRateUpdateDto = {
  rateId: number;
  currencyCode?: string | null;
  rate?: number | null;
  source?: string | null;
};

export type TransactionListDto = {
  transactionId: number;
  accountId: number;
  transactionType: string;
  amount: number;
  currencyCode: string;
  transactionDate: string;
};

export type TransactionSelectDto = {
  transactionId: number;
  accountId: number;
  transactionType: string;
  amount: number;
  currencyCode: string;
  transactionDate: string;
  description: string | null;
};

export type TransactionCreateDto = {
  accountId: number;
  transactionType: string;
  amount: number;
  currencyCode: string;
  description: string | null;
};

export type TransactionUpdateDto = {
  transactionId: number;
  accountId?: number | null;
  transactionType?: string | null;
  amount?: number | null;
  currencyCode?: string | null;
  description?: string | null;
};

export type BillListDto = {
  billId: number;
  customerId: number;
  billType: string;
  amount: number;
  currencyCode: string | null;
  dueDate: string;
  isPaid: boolean;
  paidDate: string | null;
};

export type BillSelectDto = {
  billId: number;
  customerId: number;
  billType: string;
  amount: number;
  currencyCode: string | null;
  dueDate: string;
  isPaid: boolean;
  paidDate: string | null;
};

export type BillCreateDto = {
  customerId: number;
  billType: string;
  amount: number;
  currencyCode: string | null;
  dueDate: string;
  isPaid: boolean;
};

export type BillUpdateDto = {
  billId: number;
  billType?: string | null;
  amount?: number | null;
  currencyCode?: string | null;
  dueDate?: string | null;
  isPaid?: boolean | null;
  paidDate?: string | null;
};

export type CustomerDashboardDto = {
  accountCount: number;
  totalBalance: number;
  unpaidBillCount: number;
};

export type PendingTransferDto = {
  pendingTransferId: number;
  sourceAccountId: number;
  targetAccountId: number;
  amount: number;
  currencyCode: string;
  description: string | null;
  status: string;
  createdByCustomerId: number;
  createdAt: string;
  srcFirstName: string | null;
  srcLastName: string | null;
  tgtFirstName: string | null;
  tgtLastName: string | null;
};

export type RecentTransferDto = {
  transactionId: number;
  accountId: number;
  transactionType: string;
  amount: number;
  currencyCode: string;
  transactionDate: string;
  description: string | null;
  relatedAccountId: number | null;
  firstName: string | null;
  lastName: string | null;
  relatedCurrencyCode: string | null;
};

export type NotificationToast = {
  id: string;
  type: "info" | "success" | "warning";
  title: string;
  message: string;
  link?: string;
  createdAt: number;
};

export type ExchangeRateUpdate = {
  currencyCode: string;
  rate: number;
  rateDate: string;
  source: string;
};

export type LoanTypeDto = {
  loanTypeId: number;
  name: string;
  annualInterestRate: number;
  minAmount: number;
  maxAmount: number;
  minTermMonths: number;
  maxTermMonths: number;
};

export type LoanListDto = {
  loanId: number;
  customerId: number;
  customerFirstName: string | null;
  customerLastName: string | null;
  loanTypeName: string | null;
  loanTypeId: number;
  amount: number;
  termMonths: number;
  annualInterestRate: number;
  monthlyPayment: number;
  status: string;
  appliedAt: string;
  approvedAt: string | null;
  paymentsMade: number;
  paymentsMissed: number;
  remainingPrincipal: number;
};

export type LoanDetailDto = LoanListDto & {
  disbursementAccountId: number;
  paymentAccountId: number;
  closedAt: string | null;
};

export type LoanScheduleDto = {
  scheduleId: number;
  loanId: number;
  periodNumber: number;
  dueDate: string;
  principal: number;
  interest: number;
  totalDue: number;
  remainingBalance: number;
  isPaid: boolean;
  paidDate: string | null;
  isLate: boolean;
};

export type LoanPaymentDto = {
  paymentId: number;
  scheduleId: number | null;
  loanId: number;
  amount: number;
  paymentType: string;
  paymentDate: string;
  description: string | null;
};
