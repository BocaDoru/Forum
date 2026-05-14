import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { registerUser } from "../services/authService";
import "../style/Auth.css";
import Header from "../components/Header";

export default function Register() {
  const [username, setUsername] = useState("");
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
      const data = await registerUser(username, email, password);

      const token = data.JwtToken;
      const user = data.User;

      localStorage.setItem("forum_token", token);
      localStorage.setItem("forum_user", JSON.stringify(user));
      navigate("/home");
      console.log(`Registration successful! Welcome ${user.Username}`);
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
        <h2 className="title">Create an Account</h2>

        {error && <div className="errorText">{error}</div>}

        <form onSubmit={handleSubmit} className="form">
          <div className="inputGroup">
            <label className="label" htmlFor="usernameInput">
              Username
            </label>
            <input
              id="usernameInput"
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="input"
              required
            />
          </div>

          <div className="inputGroup">
            <label className="label" htmlFor="emailInput">
              Email
            </label>
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
            <label className="label" htmlFor="passwordInput">
              Password
            </label>
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
              {isLoading ? "Creating account..." : "Register"}
            </button>
          </div>

          <div className="redirectContainer">
            Already have an account?{" "}
            <Link to="/login" className="redirectLink">
              Login here
            </Link>
          </div>
        </form>
      </div>
    </div>
  );
}