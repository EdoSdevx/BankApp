import { useState, type FormEvent } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";

export function LoginPage() {
  const { login, isLoading } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");
  const [messageType, setMessageType] = useState<"error" | "success">("error");
  const [splash, setSplash] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setMessage("");
    const result = await login(email, password);
    if (result.success) {
      setMessageType("success");
      setSplash(true);
      const destination = result.data?.role === "Customer" ? "/customer" : "/dashboard";
      setTimeout(() => navigate(destination), 2500);
    } else {
      setMessage(result.errors?.[0]?.message ?? result.message);
      setMessageType("error");
    }
  }

  if (splash) {
    return (
      <div className="splash-page">
        <div className="splash-center">
          <div className="bankapp-logo">
            <span className="bankapp-mark">BA</span>
            <span className="bankapp-text">BankApp</span>
          </div>
          <h1 className="brand-reveal">
            {"BankApp".split("").map((c, i) => (
              <span key={i} style={{ animationDelay: `${i * 0.08}s` }}>{c}</span>
            ))}
          </h1>
          <p className="signed-in">Signed in as {email}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="login-page">
      <div className="login-shell">
        <div className="login-brand">
          <span className="brand-name">Bank</span>
          <span className="brand-name light">App</span>
        </div>
        <div className="login-intro">
          <h1>Welcome to BankApp portal.</h1>
          <p>Sign in with your registered credentials</p>
        </div>

        <form className="login-form" onSubmit={handleSubmit}>
          <label>
            Email
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
            />
          </label>
          <label>
            Password
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter your password"
              required
            />
          </label>
          <Link to="/forgot-password" className="forgot-button">Forgot password?</Link>
          <button type="submit" disabled={isLoading}>
            {isLoading ? "Signing in..." : "Sign in"}
          </button>
        </form>

        {message && <p className={`status${messageType === "success" ? " success" : ""}`}>{message}</p>}
      </div>
    </div>
  );
}
