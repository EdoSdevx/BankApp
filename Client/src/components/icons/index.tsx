export type MenuIconName =
  | "dashboard" | "customers" | "accounts" | "transactions"
  | "bills" | "rates" | "employees" | "branches" | "roles"
  | "password" | "logout";

const iconPaths: Record<MenuIconName, string> = {
  dashboard: "M4 13h6V4H4v9Zm10 7h6V4h-6v16ZM4 20h6v-3H4v3Z",
  customers: "M16 11a4 4 0 1 0-8 0 4 4 0 0 0 8 0Zm-10 9a6 6 0 0 1 12 0M18 8a3 3 0 0 1 0 6M19 17a5 5 0 0 1 3 3",
  accounts: "M4 7h16v11H4V7Zm2-3h12M7 12h4M15 12h2",
  transactions: "M7 7h12l-3-3M17 17H5l3 3M6 12h12",
  bills: "M7 3h10l3 3v15l-3-2-3 2-3-2-3 2-3-2V3Zm3 6h7M10 13h7",
  rates: "M4 16c3-7 6-7 9 0s5 6 7-2M5 8h4M7 6v4M15 7h5M16 17h4",
  employees: "M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8Zm-7 8a7 7 0 0 1 14 0M19 6h3M20.5 4.5v3",
  branches: "M12 4v5M6 20v-5h12v5M6 15a3 3 0 1 1 0-6 3 3 0 0 1 0 6Zm12 0a3 3 0 1 1 0-6 3 3 0 0 1 0 6Zm-6-1a3 3 0 1 1 0-6 3 3 0 0 1 0 6Z",
  roles: "M12 3l7 4v5c0 4-3 7-7 9-4-2-7-5-7-9V7l7-4Zm-3 9 2 2 4-5",
  password: "M7 11V8a5 5 0 0 1 10 0v3M6 11h12v10H6V11Zm6 4v2",
  logout: "M10 5H5v14h5M14 8l4 4-4 4M18 12H9",
};

export function SidebarIcon({ name }: { name: MenuIconName }) {
  return (
    <svg className="menu-icon" viewBox="0 0 24 24" aria-hidden="true">
      <path d={iconPaths[name]} />
    </svg>
  );
}

export function ModuleActionIcon({ label }: { label: string }) {
  const normalized = label.toLowerCase();
  let path = "M7 5h10v14H7V5Zm3 4h4M10 12h4M10 16h3";

  if (normalized.includes("list")) path = "M5 7h14M5 12h14M5 17h14";
  else if (normalized.includes("create")) path = "M12 5v14M5 12h14";
  else if (normalized.includes("edit")) path = "M4 20h4l10-10-4-4L4 16v4Zm8-12 4 4";
  else if (normalized.includes("detail")) path = "M7 5h10v14H7V5Zm3 4h4M10 12h4M10 16h3";
  else if (normalized.includes("mark-paid")) path = "M5 12l4 4 10-10M7 5h10v14H7V5Z";
  else if (normalized.includes("operations")) path = "M6 7h12M6 12h12M6 17h8M17 16l2 2 3-4";
  else if (normalized.includes("update")) path = "M4 20h4l10-10-4-4L4 16v4Zm8-12 4 4";

  return (
    <svg className="customer-action-icon" viewBox="0 0 24 24" aria-hidden="true">
      <path d={path} />
    </svg>
  );
}
