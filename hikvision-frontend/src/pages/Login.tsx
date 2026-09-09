
import React, { useState } from "react";
import axios from "axios";
import { loginApi } from "../api/loginApi";
import "./Login.css";
import { Link } from "react-router-dom";
const Login: React.FC = () => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    // Toast state
    const [toast, setToast] = useState<{
        type: "success" | "error";
        message: string;
    } | null>(null);

    const showToast = (
        type: "success" | "error",
        message: string
    ) => {
        setToast({
            type,
            message,
        });

        // Automatically hide toast after 3 seconds
        setTimeout(() => {
            setToast(null);
        }, 3000);
    };

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();

        setError("");

        // Email validation
        if (!email.trim()) {
            setError("Email is required");

            showToast("error", "Email is required");
            return;
        }

        // Password validation
        if (!password.trim()) {
            setError("Password is required");

            showToast("error", "Password is required");
            return;
        }

        setLoading(true);

        try {
            const data = await loginApi(email, password);

            console.log("Login response:", data);

            // Login successful
            if (data?.success === true) {
                showToast(
                    "success",
                    data.message || "Login successful!"
                );

                // Wait for toast to show before navigating
                setTimeout(() => {
                    window.location.href = "/cameras";
                }, 1000);
            } else {
                const message =
                    data?.message ||
                    "Invalid email or password";

                setError(message);

                showToast("error", message);
            }
        } catch (err: unknown) {
            console.error("Login error:", err);

            if (axios.isAxiosError(err)) {
                const message =
                    err.response?.data?.message ||
                    (typeof err.response?.data === "string"
                        ? err.response.data
                        : "Invalid email or password");

                setError(message);

                showToast("error", message);
            } else {
                setError("Unable to connect to server");

                showToast(
                    "error",
                    "Unable to connect to server"
                );
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="login-page">

            {/* Toast Message */}
            {toast && (
                <div
                    className={`login-toast ${
                        toast.type === "success"
                            ? "toast-success"
                            : "toast-error"
                    }`}
                >
                    <div className="toast-icon">
                        {toast.type === "success" ? "✓" : "✕"}
                    </div>

                    <div className="toast-content">
                        <strong>
                            {toast.type === "success"
                                ? "Success"
                                : "Error"}
                        </strong>

                        <span>{toast.message}</span>
                    </div>
                </div>
            )}

            <div className="login-card">

                <h2>Login</h2>

                <p className="login-subtitle">
                    Welcome back
                </p>

                <form onSubmit={handleLogin}>

                    {/* Email */}
                    <div className="form-group">

                        <label>
                            Email
                        </label>

                        <input
                            type="email"
                            placeholder="Enter your email"
                            value={email}
                            onChange={(e) =>
                                setEmail(e.target.value)
                            }
                        />

                    </div>

                    {/* Password */}
                    <div className="form-group">

                        <label>
                            Password
                        </label>

                        <input
                            type="password"
                            placeholder="Enter your password"
                            value={password}
                            onChange={(e) =>
                                setPassword(e.target.value)
                            }
                        />

                    </div>

                    {/* Error below form */}
                    {error && (
                        <div className="error-message">
                            {error}
                        </div>
                    )}

                    {/* Login Button */}
<button
    type="submit"
    disabled={loading}
>
    {loading ? "Logging in..." : "Login"}
</button>

{/* Register Link */}
<div style={{ marginTop: "15px", textAlign: "center" }}>
    <span>Don't have an account? </span>

    <Link to="/register">
        Register
    </Link>
</div>

                </form>

            </div>
        </div>
    );
};

export default Login;
