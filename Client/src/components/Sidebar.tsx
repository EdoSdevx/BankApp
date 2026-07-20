import { useNavigate, useLocation } from "react-router-dom";
import { SidebarIcon, type MenuIconName } from "@/components/icons";

type MenuItem = { label: string; icon: MenuIconName; path: string };

const menuItems: MenuItem[] = [
  { label: "Dashboard", icon: "dashboard", path: "/dashboard" },
  { label: "Customers", icon: "customers", path: "/customers" },
  { label: "Accounts", icon: "accounts", path: "/accounts" },
  { label: "Transactions", icon: "transactions", path: "/transactions" },
  { label: "Bills", icon: "bills", path: "/bills" },
  { label: "Exchange Rates", icon: "rates", path: "/exchange-rates" },
  { label: "Employees", icon: "employees", path: "/employees" },
  { label: "Branches", icon: "branches", path: "/branches" },
  { label: "Roles", icon: "roles", path: "/roles" },
  { label: "Approvals", icon: "rates", path: "/approvals" },
  { label: "Loans", icon: "customers", path: "/loans" },
];

export function Sidebar() {
  const navigate = useNavigate();
  const location = useLocation();

  function isActive(path: string) {
    if (path === "/dashboard") return location.pathname === "/dashboard";
    return location.pathname.startsWith(path);
  }

  return (
    <aside className="dashboard-sidebar">
      <div className="dashboard-brand">
        <strong>BankApp</strong>
        <p>Admin Console</p>
      </div>
      <nav className="dashboard-menu">
        {menuItems.map((item) => (
          <button
            key={item.label}
            type="button"
            className={`menu-row ${isActive(item.path) ? "active" : ""}`}
            onClick={() => navigate(item.path)}
          >
            <SidebarIcon name={item.icon} />
            {item.label}
          </button>
        ))}
      </nav>
    </aside>
  );
}
