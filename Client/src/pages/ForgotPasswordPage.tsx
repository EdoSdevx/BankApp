import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";

export function ForgotPasswordPage() {
  const { forgotPassword } = useAuth();
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setMessage("");
    if (!email.trim()) {
      setMessage("Email is required.");
      return;
    }
    setIsLoading(true);
    try {
      const result = await forgotPassword(email);
      setMessage(result.message);
    } catch {
      setMessage("Could not reach the API.");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="login-page">
      <div className="login-shell">
        <div className="login-brand">
          <span className="brand-name">Bank</span>
          <span className="brand-name light">App</span>
        </div>
        <div className="login-intro">
          <h1>Reset your password.</h1>
          <p>Enter your email and we'll send a reset link</p>
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
          <button type="submit" disabled={isLoading}>
            {isLoading ? "Sending..." : "Send reset link"}
          </button>
        </form>

        {message && <p className="status">{message}</p>}

        <Link to="/login" className="forgot-button">Back to login</Link>
      </div>
    </div>
  );
}
