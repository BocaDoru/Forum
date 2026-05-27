import { useState } from "react";
import { Link, useNavigate } from 'react-router-dom';
import { loginUser } from "../services/authService";
import "../style/Auth.css";
import Header from "../components/Header"

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setIsLoading(true);

    try {
      const data = await loginUser(email, password);
      sessionStorage.setItem("forum_token", data.JwtToken);
      sessionStorage.setItem("forum_user", JSON.stringify(data.User));

      navigate("/home");
    } catch (err) {
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="pageContainer">
      <Header />

      <div className="card">
        <h2 className="title">Login</h2>

        {error && <div className="errorText">{error}</div>}

        <form onSubmit={handleSubmit} className="form">
          <div className="inputGroup">
            <label className="label" htmlFor="emailInput">Email</label>
            <input
              id="emailInput"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="input"
              required
            />
          </div>

          <div className="inputGroup">
            <label className="label" htmlFor="passwordInput">Password</label>
            <input
              id="passwordInput"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="input"
              required
            />
          </div>

          <div className="buttonContainer">
            <button type="submit" disabled={isLoading} className="submitButton">
              {isLoading ? "Logging in..." : "Login"}
            </button>
          </div>
        </form>
        <div className="redirectContainer">
          Don't have an account?{" "}
          <Link to="/register" className="redirectLink">
            Register here
          </Link>
        </div>
      </div>
    </div>
  );
}
