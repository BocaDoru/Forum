import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { updateUsername, changePassword, deleteUser, getUserScore } from "../services/userService";
import "../style/Auth.css";
import "../style/Account.css";
import Header from "../components/Header";

export default function Account() {
    const navigate = useNavigate();
    const user = JSON.parse(sessionStorage.getItem("forum_user"));

    const [newUsername, setNewUsername] = useState(user?.Username || "");
    const [oldPassword, setOldPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [score, setScore] = useState(null);
    const [usernameError, setUsernameError] = useState("");
    const [passwordError, setPasswordError] = useState("");
    const [usernameSuccess, setUsernameSuccess] = useState("");
    const [passwordSuccess, setPasswordSuccess] = useState("");

    useEffect(() => {
        getUserScore().then(data => setScore(data.Score)).catch(() => {});
    }, []);

    const handleUpdateUsername = async (e) => {
        e.preventDefault();
        setUsernameError("");
        setUsernameSuccess("");
        try {
            await updateUsername(newUsername);
            const updatedUser = { ...user, Username: newUsername };
            sessionStorage.setItem("forum_user", JSON.stringify(updatedUser));
            setUsernameSuccess("Username updated successfully.");
        } catch (err) {
            setUsernameError(err.message);
        }
    };

    const handleChangePassword = async (e) => {
        e.preventDefault();
        setPasswordError("");
        setPasswordSuccess("");
        try {
            await changePassword(oldPassword, newPassword);
            setOldPassword("");
            setNewPassword("");
            setPasswordSuccess("Password changed successfully.");
        } catch (err) {
            setPasswordError(err.message);
        }
    };

    const handleDeleteUser = async () => {
        if (!window.confirm("Are you sure you want to delete your account? This cannot be undone.")) return;
        try {
            await deleteUser();
            sessionStorage.removeItem("forum_token");
            sessionStorage.removeItem("forum_user");
            navigate("/login");
        } catch (err) {
            alert(err.message);
        }
    };

    const initials = user?.Username?.slice(0, 2).toUpperCase() || "??";

    return (
        <div className="pageContainer">

            <Header/>

            <div className="accountContainer">
                <div className="card profileCard">
                    <div className="avatar">{initials}</div>
                    <p className="profileName">{user?.Username}</p>
                    <p className="profileEmail">{user?.Email}</p>
                    {score !== null && <div className="scoreBadge">Score: {score}</div>}
                </div>

                <div className="sectionsContainer">
                    <div className="card sectionCard">
                        <h2 className="title">Update username</h2>
                        {usernameError && <div className="errorText">{usernameError}</div>}
                        {usernameSuccess && <div className="successText">{usernameSuccess}</div>}
                        <form onSubmit={handleUpdateUsername} className="form">
                            <div className="inputGroup">
                                <label className="label" htmlFor="newUsernameInput">New username</label>
                                <input id="newUsernameInput" type="text" value={newUsername} onChange={e => setNewUsername(e.target.value)}
                                       className="input" required/>
                            </div>
                            <div className="buttonContainer">
                                <button type="submit" className="submitButton">Save</button>
                            </div>
                        </form>
                    </div>

                    <div className="card sectionCard">
                        <h2 className="title">Change password</h2>
                        {passwordError && <div className="errorText">{passwordError}</div>}
                        {passwordSuccess && <div className="successText">{passwordSuccess}</div>}
                        <form onSubmit={handleChangePassword} className="form">
                            <div className="inputGroup">
                                <label className="label" htmlFor="oldPasswordInput">Current password</label>
                                <input id="oldPasswordInput" type="password" value={oldPassword}
                                       onChange={e => setOldPassword(e.target.value)} className="input" required/>
                            </div>
                            <div className="inputGroup">
                                <label className="label" htmlFor="newPasswordInput">New password</label>
                                <input id="newPasswordInput" type="password" value={newPassword}
                                       onChange={e => setNewPassword(e.target.value)} className="input" required/>
                            </div>
                            <div className="buttonContainer">
                                <button type="submit" className="submitButton">Update</button>
                            </div>
                        </form>
                    </div>

                    <div className="card dangerCard">
                        <h2 className="title dangerTitle">Danger zone</h2>
                        <p className="dangerDescription">Once you delete your account, there is no going back.</p>
                        <button onClick={handleDeleteUser} className="deleteButton">Delete account</button>
                    </div>
                </div>
                <div className="bottomActions">
                    <button onClick={() => navigate(-1)} className="backButton">
                        Back
                    </button>
                </div>
            </div>
        </div>
    );
}