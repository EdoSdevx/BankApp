import { NavLink } from "react-router-dom";
import { SidebarIcon, type MenuIconName } from "@/components/icons";
import { useAuth } from "@/context/AuthContext";

const items: Array<[string, string, MenuIconName]> = [
  ["Dashboard", "/customer", "dashboard"],
  ["My Accounts", "/customer/accounts", "accounts"],
  ["Transfer", "/customer/transfer", "transactions"],
  ["Exchange", "/customer/exchange", "rates"],
  ["Pay Bills", "/customer/bills", "bills"],
  ["Exchange Rates", "/customer/rates", "rates"],
  ["Loans", "/customer/loans", "accounts"],
];

export function CustomerSidebar() {
  const { logout } = useAuth();

  return (
    <aside className="dashboard-sidebar">
      <div className="dashboard-brand">
        <strong>BankApp</strong>
        <p>Customer Portal</p>
      </div>
      <nav className="dashboard-menu" aria-label="Customer menu">
        {items.map(([label, path, icon]) => (
          <NavLink
            key={path}
            to={path}
            className={({ isActive }) =>
              isActive ? "menu-row active" : "menu-row"
            }
          >
            <SidebarIcon name={icon} />
            <span>{label}</span>
          </NavLink>
        ))}
        <button type="button" className="menu-row" onClick={logout}>
          <SidebarIcon name="logout" />
          <span>Logout</span>
        </button>
      </nav>
    </aside>
  );
}
