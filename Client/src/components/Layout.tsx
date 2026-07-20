import { Outlet } from "react-router-dom";
import { Sidebar } from "@/components/Sidebar";
import { Topbar } from "@/components/Topbar";

export function Layout() {
  return (
    <div className="dashboard-page">
      <Sidebar />
      <div className="dashboard-content">
        <Topbar />
        <div className="dashboard-workspace">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
