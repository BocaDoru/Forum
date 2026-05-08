import { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";

import {
  getQuestions,
  getQuestionsByTitle,
  getQuestionsByTag,
  getQuestionsByAuthor,
} from "../services/questionService.js";

import { getUserById, getUserByEmail } from "../services/userService";

import "../style/Home.css";
import Header from "../components/Header.jsx";

export default function Home() {
  const [questions, setQuestions] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");

  const [searchTerm, setSearchTerm] = useState("");
  const [filterType, setFilterType] = useState("title");

  const navigate = useNavigate();

  const hydrateAndSetQuestions = async (rawQuestions) => {
    const questionsWithAuthors = await Promise.all(
      rawQuestions.map(async (q) => {
        try {
          const authorData = await getUserById(q.AuthorId).catch(() => null);
          return { ...q, AuthorInfo: authorData || { Username: "Unknown", Score: 0 } };
        } catch (err) {
          return { ...q, AuthorInfo: { Username: "Unknown", Score: 0 } };
        }
      }),
    );

    setQuestions(questionsWithAuthors);
  };

  const loadDefaultQuestions = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const data = await getQuestions(1);
      await hydrateAndSetQuestions(data);
    } catch (err) {
      setError(err.message || "Failed to load questions.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadDefaultQuestions();
  }, [loadDefaultQuestions]);

  const handleApplyFilter = async () => {
    if (!searchTerm && filterType !== "my_questions") {
      loadDefaultQuestions();
      return;
    }

    setIsLoading(true);
    setError("");
    setQuestions([]);

    try {
      let questionsData = [];

      switch (filterType) {
        case "title":
          questionsData = await getQuestionsByTitle(searchTerm, 1);
          break;
        case "tag":
          questionsData = await getQuestionsByTag(searchTerm, 1);
          break;
        case "my_questions": {
          const userJson = localStorage.getItem("forum_user");
          if (!userJson) throw new Error("You are not logged in.");
          const loggedInUser = JSON.parse(userJson);

          questionsData = await getQuestionsByAuthor(loggedInUser.Id, 1);
          break;
        }
        case "author": {
          const userResponse = await getUserByEmail(searchTerm);
          questionsData = await getQuestionsByAuthor(userResponse.Id, 1);
          break;
        }
        default:
          questionsData = await getQuestions(1);
      }

      await hydrateAndSetQuestions(questionsData);
    } catch (err) {
      setError(
        err.message || "Failed to apply filter. Check your search term.",
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleClear = () => {
    setSearchTerm("");
    loadDefaultQuestions();
  };

  const handleLogout = () => {
    localStorage.removeItem("forum_token");
    localStorage.removeItem("forum_user");
    navigate("/login");
  };

  const renderStatusBadge = (status) => {
    switch (status) {
      case 0:
        return <span className="badge badge-received">Received</span>;
      case 1:
        return <span className="badge badge-progress">In progress</span>;
      case 2:
        return <span className="badge badge-solved">✓ Solved</span>;
      default:
        return <span className="badge badge-received">Received</span>;
    }
  };

  return (
    <div className="homeContainer">
      <Header />

      <main className="mainContent">
        <div className="topActions">
          <div className="searchWrapper">
            <select
              className="filterDropdown"
              value={filterType}
              onChange={(e) => {
                setFilterType(e.target.value);
                if (e.target.value === "my_questions") setSearchTerm("");
              }}
            >
              <option value="title">Search by Title</option>
              <option value="author">Filter by Author Email</option>
              <option value="tag">Filter by Tag</option>
              <option value="my_questions">My Questions</option>
            </select>
            <input
              type="text"
              className="searchInput"
              placeholder={
                filterType === "author"
                  ? "Enter author email..."
                  : "Type to search..."
              }
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              disabled={filterType === "my_questions"}
            />
            <button className="applyButton" onClick={handleApplyFilter}>
              Apply
            </button>
            <button className="clearButton" onClick={handleClear}>
              Clear
            </button>
          </div>

          <button onClick={() => navigate("/ask")} className="askButton">
            + Ask Question
          </button>
        </div>

        <h2 className="sectionTitle">
          {filterType === "my_questions" ? "My Questions" : "All Questions"}
        </h2>

        {isLoading && <p>Loading questions...</p>}
        {error && <p className="errorText">{error}</p>}

        <div className="questionList">
          {questions.map((q, index) => (
            <div key={index} className="questionCard" onClick={() => navigate(`/question/${q.Id}`)} style={{ cursor: 'pointer' }}>
              <div className="voteColumn">
                <button className="voteArrow">▲</button>
                <span className="voteCount">{q.VoteCount}</span>
                <button className="voteArrow">▼</button>
              </div>

              <div className="contentColumn">
                <div className="cardHeader">
                  <div className="authorInfo">
                    {renderStatusBadge(q.Status)}
                    <span className="username">{q.AuthorInfo.Username}</span>
                    <span className="score">{q.AuthorInfo.Score} pts</span>
                  </div>
                  <span className="timestamp">
                    {new Date(q.CreatedAt).toLocaleDateString()}
                  </span>
                </div>
                <h3 className="questionTitle">{q.Title}</h3>
                <p className="questionText">{q.Text}</p>
              </div>
            </div>
          ))}
          {!isLoading && questions.length === 0 && !error && (
            <p>No questions found.</p>
          )}
        </div>
      </main>
    </div>
  );
}
