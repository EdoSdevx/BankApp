import { api } from "@/services/api";
import type { LoginResponse, AuthUser, ApiResponse } from "@/types";

function setSession(user: NonNullable<LoginResponse["data"]>) {
  localStorage.setItem("token", user.token);
  localStorage.setItem("userName", user.fullName);
  localStorage.setItem("role", user.role);
  localStorage.setItem("expiresAtUtc", user.expiresAtUtc);
}

export async function login(email: string, password: string): Promise<LoginResponse> {
  const result = await api.post<LoginResponse>("/auth/login", { email, password });
  if (result.success && result.data) {
    setSession(result.data);
  }
  return result;
}

export function getStoredUser(): AuthUser | null {
  const token = localStorage.getItem("token");
  const fullName = localStorage.getItem("userName");
  const role = localStorage.getItem("role");
  const expiresAtUtc = localStorage.getItem("expiresAtUtc");
  if (!token || !fullName || !role || !expiresAtUtc) return null;
  return { token, fullName, role, expiresAtUtc };
}

export function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("userName");
  localStorage.removeItem("role");
  localStorage.removeItem("expiresAtUtc");
}

export async function forgotPassword(email: string): Promise<ApiResponse> {
  return api.post("/auth/forgot-password", { email });
}

export async function resetPassword(token: string, newPassword: string): Promise<ApiResponse> {
  return api.post("/auth/reset-password", { token, newPassword });
}
