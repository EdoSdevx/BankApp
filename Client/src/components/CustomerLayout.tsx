import { Outlet } from "react-router-dom";
import { CustomerSidebar } from "@/components/CustomerSidebar";
import { Topbar } from "@/components/Topbar";
import { ChatWidget } from "@/components/ChatWidget";

export function CustomerLayout() {
  return (
    <div className="dashboard-page">
      <CustomerSidebar />
      <div className="dashboard-content">
        <Topbar />
        <div className="dashboard-workspace">
          <Outlet />
        </div>
      </div>
      <ChatWidget />
    </div>
  );
}
