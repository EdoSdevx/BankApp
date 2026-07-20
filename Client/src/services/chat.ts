import { api } from "@/services/api";
import type { ApiResponse } from "@/types";

export async function sendMessage(question: string): Promise<ApiResponse & { reply?: string }> {
  return api.post("/customer/chat", { question });
}
