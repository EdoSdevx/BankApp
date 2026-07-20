import { useState, type FormEvent } from "react";
import { useSearchParams, Link } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";

export function ResetPasswordPage() {
  const { resetPassword } = useAuth();
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token")?.trim() ?? "";

  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [message, setMessage] = useState("");
  const [messageType, setMessageType] = useState<"error" | "success">("error");
  const [isLoading, setIsLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setMessage("");

    if (!token) {
      setMessage("Reset token is missing.");
      return;
    }
    if (newPassword.length < 6) {
      setMessage("Password must be at least 6 characters.");
      return;
    }
    if (newPassword !== confirmPassword) {
      setMessage("Passwords do not match.");
      return;
    }

    setIsLoading(true);
    try {
      const result = await resetPassword(token, newPassword);
      if (result.success) {
        setMessageType("success");
        setMessage("Password has been reset successfully. You can now log in.");
        setNewPassword("");
        setConfirmPassword("");
      } else {
        setMessageType("error");
        setMessage(result.message);
      }
    } catch {
      setMessage("Could not reach the API.");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="reset-page">
      <div className="reset-shell">
        <div className="reset-intro">
          <h2>Set new password</h2>
          <p>Choose a new password for your account.</p>
        </div>

        <form className="reset-form" onSubmit={handleSubmit}>
          <label>
            New password
            <input
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              placeholder="Min. 8 characters"
              required
            />
          </label>
          <label>
            Confirm password
            <input
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder="Re-enter new password"
              required
            />
          </label>
          <button type="submit" disabled={isLoading}>
            {isLoading ? "Resetting..." : "Reset password"}
          </button>
        </form>

        {message && <p className={`status${messageType === "success" ? " success" : ""}`}>{message}</p>}

        <Link to="/login" className="forgot-button">Back to login</Link>
      </div>
    </div>
  );
}
