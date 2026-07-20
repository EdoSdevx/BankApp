import { useState, useEffect } from "react";
import { useAuth } from "@/context/AuthContext";
import type { AccountListDto, CustomerListDto, EmployeeListDto } from "@/types";
import * as accountsService from "@/services/accounts";
import * as customersService from "@/services/customers";
import * as employeesService from "@/services/employees";
import { StatCard, EmptyState } from "@/components/ui";

export function DashboardPage() {
  const { isAuthenticated } = useAuth();
  const [accounts, setAccounts] = useState<AccountListDto[]>([]);
  const [customers, setCustomers] = useState<CustomerListDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeListDto[]>([]);

  useEffect(() => {
    if (!isAuthenticated) return;
    accountsService.list().then((r) => r.success && r.data && setAccounts(r.data)).catch(() => {});
    customersService.list().then((r) => r.success && r.data && setCustomers(r.data)).catch(() => {});
    employeesService.list().then((r) => r.success && r.data && setEmployees(r.data)).catch(() => {});
  }, [isAuthenticated]);

  return (
    <>
      <h1>Dashboard</h1>
      <p className="page-subtitle">Overview of accounts, balances and activity across the bank.</p>

      <section className="stats">
        <StatCard label="Total accounts" value={accounts.length > 0 ? String(accounts.length) : "-"} />
        <StatCard label="Customers" value={customers.length > 0 ? String(customers.length) : "-"} />
        <StatCard label="Employees" value={employees.length > 0 ? String(employees.length) : "-"} />
      </section>

      <EmptyState label="Select a module from the sidebar to manage data." />
    </>
  );
}
