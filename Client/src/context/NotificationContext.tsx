import { createContext, useContext, useState, useRef, useEffect, useCallback, type ReactNode } from "react";
import type { NotificationToast, ExchangeRateUpdate } from "@/types";
import { useAuth } from "@/context/AuthContext";
import * as signalR from "@microsoft/signalr";
import { ToastContainer } from "@/components/ToastContainer";

type NotificationContextValue = {
  toasts: NotificationToast[];
  dismissToast: (id: string) => void;
  bellNotifications: NotificationToast[];
  unreadCount: number;
  clearBellNotifications: () => void;
  markAllRead: () => void;
  latestRates: ExchangeRateUpdate[] | null;
};

const NotificationContext = createContext<NotificationContextValue | null>(null);

export function NotificationProvider({ children }: { children: ReactNode }) {
  const { user, isAuthenticated } = useAuth();
  const [toasts, setToasts] = useState<NotificationToast[]>([]);
  const [bellNotifications, setBellNotifications] = useState<NotificationToast[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [latestRates, setLatestRates] = useState<ExchangeRateUpdate[] | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const tokenRef = useRef<string | null>(null);
  const mountedRef = useRef(true);

  const addToast = useCallback((toast: Omit<NotificationToast, "id" | "createdAt">) => {
    const newToast: NotificationToast = { ...toast, id: crypto.randomUUID(), createdAt: Date.now() };
    setToasts((prev) => [newToast, ...prev].slice(0, 5));
    return newToast;
  }, []);

  const dismissToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const addBellNotification = useCallback((toast: NotificationToast) => {
    setBellNotifications((prev) => [toast, ...prev].slice(0, 20));
    setUnreadCount((c) => c + 1);
  }, []);

  const clearBellNotifications = useCallback(() => {
    setBellNotifications([]);
    setUnreadCount(0);
  }, []);

  const markAllRead = useCallback(() => {
    setUnreadCount(0);
  }, []);

  useEffect(() => {
    if (!isAuthenticated || !user?.token) return;

    if (tokenRef.current === user.token && connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    tokenRef.current = user.token;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5000/hubs/notifications", {
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    connection.onreconnecting(() => console.warn("[SignalR] Reconnecting..."));
    connection.onreconnected(() => console.log("[SignalR] Reconnected"));
    connection.onclose(() => console.log("[SignalR] Connection closed"));

    connection.on("NewPendingTransfer", (data: { pendingTransferId: number; amount: number; sourceAccountId: number; targetAccountId: number; description?: string }) => {
      console.log("[SignalR] NewPendingTransfer received", data);
      const toast = addToast({
        type: "info",
        title: "New Pending Transfer",
        message: `Transfer ${data.amount} — needs approval`,
        link: "/approvals",
      });
      addBellNotification(toast);
    });

    connection.on("TransferResolved", (data: { transferId: number; status: string }) => {
      console.log("[SignalR] TransferResolved received", data);
      const isApproved = data.status === "Approved";
      addToast({
        type: isApproved ? "success" : "warning",
        title: isApproved ? "Transfer Approved" : "Transfer Rejected",
        message: `Your transfer has been ${data.status.toLowerCase()}.`,
        link: "/customer/transactions",
      });
    });

    connection.on("RatesUpdated", (data: ExchangeRateUpdate[]) => {
      setLatestRates(data);
    });

    connection
      .start()
      .then(() => {
        console.log("[SignalR] Connected successfully");
        connectionRef.current = connection;
      })
      .catch((err) => {
        if (mountedRef.current) {
          console.error("[SignalR] Connection failed:", err);
        }
      });

    return () => {
      mountedRef.current = false;
      connection.stop().catch(() => {});
      connectionRef.current = null;
    };
  }, [isAuthenticated, user?.token, addToast, addBellNotification]);

  return (
    <NotificationContext.Provider value={{ toasts, dismissToast, bellNotifications, unreadCount, clearBellNotifications, markAllRead, latestRates }}>
      {children}
      <ToastContainer toasts={toasts} onDismiss={dismissToast} />
    </NotificationContext.Provider>
  );
}

export function useNotifications() {
  const ctx = useContext(NotificationContext);
  if (!ctx) throw new Error("useNotifications must be used within NotificationProvider");
  return ctx;
}
