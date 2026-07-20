import type { NotificationToast } from "@/types";
import { useNavigate } from "react-router-dom";

const typeColors: Record<string, string> = {
  info: "#2563eb",
  success: "#16a34a",
  warning: "#ea580c",
};

function Toast({ toast, onDismiss }: { toast: NotificationToast; onDismiss: () => void }) {
  const navigate = useNavigate();

  return (
    <div
      className="toast"
      style={{ borderLeftColor: typeColors[toast.type] || typeColors.info }}
      onClick={() => { if (toast.link) navigate(toast.link); }}
    >
      <div className="toast-body">
        <strong>{toast.title}</strong>
        <span>{toast.message}</span>
      </div>
      <button className="toast-close" onClick={(e) => { e.stopPropagation(); onDismiss(); }}>
        ×
      </button>
    </div>
  );
}

export function ToastContainer({ toasts, onDismiss }: { toasts: NotificationToast[]; onDismiss: (id: string) => void }) {
  if (toasts.length === 0) return null;

  return (
    <div className="toast-container">
      {toasts.map((t) => (
        <Toast key={t.id} toast={t} onDismiss={() => onDismiss(t.id)} />
      ))}
    </div>
  );
}
