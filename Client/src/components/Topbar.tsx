import { useAuth } from "@/context/AuthContext";
import { useNavigate, useLocation } from "react-router-dom";
import { NotificationBell } from "@/components/NotificationBell";

export function Topbar() {
  const { user, getInitials, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const showLogout = !location.pathname.startsWith("/login") &&
    !location.pathname.startsWith("/forgot") &&
    !location.pathname.startsWith("/reset");

  function handleLogout() {
    logout();
    navigate("/login");
  }

  return (
    <header className="dashboard-topbar">
      <span className="topbar-note">BankApp Operations</span>
      <div>
        <span>{user?.role ?? ""}</span>
        <strong>{user?.fullName ?? ""}</strong>
      </div>
      {(user?.role === "Admin" || user?.role === "Employee") && <NotificationBell />}
      <span className="avatar">{getInitials()}</span>
      {showLogout && (
        <button type="button" className="topbar-logout-button" onClick={handleLogout}>
          Logout
        </button>
      )}
    </header>
  );
}
