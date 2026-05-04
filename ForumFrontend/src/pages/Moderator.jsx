import { useState, useEffect } from "react";
import { getAllUsers, banUser, unbanUser } from "../services/userService";
import Header from "../components/Header";
import "../style/Auth.css";
import "../style/Moderator.css";
import { useNavigate } from "react-router-dom";

export default function Moderator() {
    const [users, setUsers] = useState([]);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState("");
    const navigate = useNavigate();

    useEffect(() => {
        getAllUsers()
            .then(data => setUsers(Array.isArray(data) ? data : []))
            .catch(err => setError(err.message))
            .finally(() => setLoading(false));
    }, []);

    const handleBan = async (userId, reason) => {
        try {
            await banUser(userId, reason);
            setUsers(users.map(u => u.Id === userId ? { ...u, IsBanned: true } : u));
        } catch (err) {
            alert(err.message);
        }
    };

    const handleUnban = async (userId) => {
        try {
            await unbanUser(userId);
            setUsers(users.map(u => u.Id === userId ? { ...u, IsBanned: false } : u));
        } catch (err) {
            alert(err.message);
        }
    };

    const filteredUsers = users.filter(u => 
    u.Username.toLowerCase().includes(search.toLowerCase())
);

    return (
        <div className="pageContainer">
            <Header />
            <div className="moderatorContainer">
                <div className="moderatorHeader">
                    <h2 className="moderatorTitle">Moderator Panel</h2>
                    <input
                        type="text"
                        placeholder="Search by username..."
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                        className="searchInput"
                    />
                </div>
                {error && <div className="errorText">{error}</div>}
                {loading ? (
                    <p>Loading users...</p>
                ) : (
                    <table className="usersTable">
                        <thead>
                        <tr>
                            <th>Username</th>
                            <th>Role</th>
                            <th>Status</th>
                            <th>Actions</th>
                        </tr>
                        </thead>
                        <tbody>
                        {filteredUsers.map(u => (
                            <tr key={u.Id}>
                                <td>{u.Username}</td>
                                <td>
                                        <span className={`roleBadge ${u.IsModerator ? "roleModerator" : "roleUser"}`}>
                                            {u.IsModerator ? "Moderator" : "User"}
                                        </span>
                                </td>
                                <td>
                                        <span className={`statusBadge ${u.IsBanned ? "statusBanned" : "statusActive"}`}>
                                            {u.IsBanned ? "Banned" : "Active"}
                                        </span>
                                </td>
                                <td>
                                    {u.IsBanned ? (
                                        <button className="unbanButton" onClick={() => handleUnban(u.Id)}>Unban</button>
                                    ) : (
                                        <button className="banButton" onClick={() => {
                                            const reason = window.prompt("Ban reason (optional):");
                                            handleBan(u.Id, reason);
                                        }}>Ban</button>
                                    )}
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                )}
                <div className="bottomActions">
                    <button onClick={() => navigate(-1)} className="backButton">
                        Back
                    </button>
                </div>
            </div>

        </div>
    );
}