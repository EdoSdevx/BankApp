import { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useNotifications } from "@/context/NotificationContext";

export function NotificationBell() {
  const { bellNotifications, unreadCount, markAllRead } = useNotifications();
  const [open, setOpen] = useState(false);
  const navigate = useNavigate();
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  return (
    <div className="bell-wrapper" ref={ref}>
      <button className="bell-button" onClick={() => { setOpen(!open); if (open) markAllRead(); }}>
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
          <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>
        {unreadCount > 0 && <span className="bell-badge">{unreadCount > 99 ? "99+" : unreadCount}</span>}
      </button>
      {open && (
        <div className="bell-dropdown">
          {bellNotifications.length === 0 ? (
            <div className="bell-empty">No notifications</div>
          ) : (
            bellNotifications.slice(0, 10).map((n) => (
              <div
                key={n.id}
                className="bell-item"
                onClick={() => { navigate(n.link || "/approvals"); setOpen(false); }}
              >
                <span className={`bell-dot ${n.type}`} />
                <div>
                  <strong>{n.title}</strong>
                  <small>{n.message}</small>
                </div>
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
}
