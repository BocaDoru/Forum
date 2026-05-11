import { useNavigate } from "react-router-dom";
import "../style/Header.css";

export default function Header() {
    const navigate = useNavigate();
    const token = localStorage.getItem("forum_token");
    const user = JSON.parse(localStorage.getItem("forum_user"));

    const handleLogout = () => {
        localStorage.removeItem("forum_token");
        localStorage.removeItem("forum_user");
        navigate("/login");
    };

    return (
        <header className="header">
            <h1 className="logo logoClickable" onClick={() => navigate(token ? "/home" : "/login")}>
                F<span style={{ color: "#ff9800" }}>&</span>rum
            </h1>
            {token && user && (
                <div className="headerNav">
                {(user.IsModerator === true || user.IsModerator === "true") && (
                    <>
                        <span 
                            className="moderatorPanelLink" 
                            onClick={() => navigate("/moderator")}
                        >
                            Moderator Panel
                        </span>
                        <span className="moderatorBadge">Moderator</span>
                    </>
                )}
                <span className="headerUsername" onClick={() => navigate("/account")}>
                    {user.Username}
                </span>
                    <button className="logoutButton" onClick={handleLogout}>Logout</button>
                </div>
            )}
        </header>
    );
}