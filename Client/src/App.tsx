import { Navigate, Route, Routes } from "react-router-dom";
import { Layout } from "@/components/Layout";
import { CustomerLayout } from "@/components/CustomerLayout";
import { useAuth } from "@/context/AuthContext";
import { LoginPage } from "@/pages/LoginPage";
import { ForgotPasswordPage } from "@/pages/ForgotPasswordPage";
import { ResetPasswordPage } from "@/pages/ResetPasswordPage";
import { DashboardPage } from "@/pages/DashboardPage";
import { CustomersPage } from "@/pages/modules/Customers";
import { AccountsPage } from "@/pages/modules/Accounts";
import { TransactionsPage } from "@/pages/modules/Transactions";
import { BillsPage } from "@/pages/modules/Bills";
import { ExchangeRatesPage } from "@/pages/modules/ExchangeRates";
import { EmployeesPage } from "@/pages/modules/Employees";
import { BranchesPage } from "@/pages/modules/Branches";
import { RolesPage } from "@/pages/modules/Roles";
import { CurrenciesPage } from "@/pages/modules/Currencies";
import { PendingTransfersPage } from "@/pages/modules/Approvals";
import { CustomerDashboardPage } from "@/pages/customer/DashboardPage";
import { MyAccountsPage } from "@/pages/customer/MyAccountsPage";
import { TransferPage } from "@/pages/customer/TransferPage";
import { ExchangePage } from "@/pages/customer/ExchangePage";
import { PayBillsPage } from "@/pages/customer/PayBillsPage";
import { ExchangeRatesPage as CustomerExchangeRatesPage } from "@/pages/customer/ExchangeRatesPage";
import { LoansPage } from "@/pages/modules/Loans";
import { LoansPage as CustomerLoansPage } from "@/pages/customer/LoansPage";
import { LoanDetailPage } from "@/pages/customer/LoanDetailPage";

function Protected() {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  if (user.role === "Customer") return <CustomerLayout />;
  return <Layout />;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />
      <Route element={<Protected />}>
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/customers" element={<CustomersPage />} />
        <Route path="/accounts" element={<AccountsPage />} />
        <Route path="/transactions" element={<TransactionsPage />} />
        <Route path="/bills" element={<BillsPage />} />
        <Route path="/exchange-rates" element={<ExchangeRatesPage />} />
        <Route path="/employees" element={<EmployeesPage />} />
        <Route path="/branches" element={<BranchesPage />} />
        <Route path="/roles" element={<RolesPage />} />
        <Route path="/currencies" element={<CurrenciesPage />} />
        <Route path="/approvals" element={<PendingTransfersPage />} />
        <Route path="/loans" element={<LoansPage />} />
        <Route path="/customer" element={<CustomerDashboardPage />} />
        <Route path="/customer/accounts" element={<MyAccountsPage />} />
        <Route path="/customer/transfer" element={<TransferPage />} />
        <Route path="/customer/exchange" element={<ExchangePage />} />
        <Route path="/customer/bills" element={<PayBillsPage />} />
        <Route path="/customer/rates" element={<CustomerExchangeRatesPage />} />
        <Route path="/customer/loans" element={<CustomerLoansPage />} />
        <Route path="/customer/loans/:id" element={<LoanDetailPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
}
