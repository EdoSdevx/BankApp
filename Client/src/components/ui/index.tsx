import type { ReactNode } from "react";

export function StatCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="stat-card">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}

export function TableCard({ children }: { children: ReactNode }) {
  return <section className="table-card">{children}</section>;
}

export function StatusDot({ active }: { active: boolean }) {
  return <span className={`dot ${active ? "active" : "pending"}`} />;
}

export function FeedbackBar({ message, type = "error" }: { message: string; type?: "error" | "success" }) {
  if (!message) return null;
  return <p className={`status${type === "success" ? " success" : ""}`}>{message}</p>;
}

export function EmptyState({ label }: { label: string }) {
  return (
    <section className="table-card empty-state">
      <strong>{label}</strong>
    </section>
  );
}

export function formatDate(value: string | null | undefined): string {
  if (!value) return "-";
  return new Date(value).toLocaleString("tr-TR", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function maskName(name: string): string {
  if (name.length <= 2) return name;
  return name.slice(0, 2) + "*".repeat(name.length - 2);
}
