import { createContext, useContext, useState, useCallback, type ReactNode } from "react";
import type { AuthUser, LoginResponse, ApiResponse } from "@/types";
import { login as loginApi, logout as logoutApi, getStoredUser, forgotPassword as forgotApi, resetPassword as resetApi } from "@/services/auth";

type AuthState = {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
};

type AuthContextValue = AuthState & {
  login: (email: string, password: string) => Promise<LoginResponse>;
  logout: () => void;
  forgotPassword: (email: string) => Promise<ApiResponse>;
  resetPassword: (token: string, newPassword: string) => Promise<ApiResponse>;
  getInitials: () => string;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>(() => {
    const stored = getStoredUser();
    return { user: stored, isAuthenticated: !!stored, isLoading: false };
  });

  const handleLogin = useCallback(async (email: string, password: string): Promise<LoginResponse> => {
    setState((s) => ({ ...s, isLoading: true }));
    const result = await loginApi(email, password);
    if (result.success && result.data) {
      setState({
        user: {
          token: result.data.token,
          fullName: result.data.fullName,
          role: result.data.role,
          expiresAtUtc: result.data.expiresAtUtc,
        },
        isAuthenticated: true,
        isLoading: false,
      });
    } else {
      setState((s) => ({ ...s, isLoading: false }));
    }
    return result;
  }, []);

  const handleLogout = useCallback(() => {
    logoutApi();
    setState({ user: null, isAuthenticated: false, isLoading: false });
  }, []);

  const getInitials = useCallback(() => {
    const name = state.user?.fullName ?? "";
    return name
      .split(" ")
      .filter(Boolean)
      .slice(0, 2)
      .map((p) => p[0])
      .join("")
      .toUpperCase() || "BA";
  }, [state.user]);

  return (
    <AuthContext.Provider
      value={{
        ...state,
        login: handleLogin,
        logout: handleLogout,
        forgotPassword: forgotApi,
        resetPassword: resetApi,
        getInitials,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
