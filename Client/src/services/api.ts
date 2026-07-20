import type { ApiResponse, ApiErrorResponse } from "@/types";

const API_BASE = "http://localhost:5000/api";

function authHeaders(): Record<string, string> {
  const token = localStorage.getItem("token");
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = `Bearer ${token}`;
  return headers;
}

async function get<T>(path: string): Promise<T & ApiResponse> {
  const res = await fetch(`${API_BASE}${path}`, { headers: authHeaders() });
  return res.json() as Promise<T & ApiResponse>;
}

async function post<T = ApiErrorResponse>(path: string, body: unknown): Promise<ApiResponse & T> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify(body),
  });
  return res.json() as Promise<ApiResponse & T>;
}

async function put<T = ApiErrorResponse>(path: string, body: unknown): Promise<ApiResponse & T> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: "PUT",
    headers: authHeaders(),
    body: JSON.stringify(body),
  });
  return res.json() as Promise<ApiResponse & T>;
}

async function del(path: string): Promise<ApiResponse> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: "DELETE",
    headers: authHeaders(),
  });
  return res.json() as Promise<ApiResponse>;
}

export const api = { get, post, put, del };
