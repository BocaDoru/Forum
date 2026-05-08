import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { 
    getQuestionById, 
    deleteQuestion
} from '../services/questionService.js';
import { getUserById } from "../services/userService";
import { getCommentsByQuestionId } from "../services/commentService";
import { 
  castVote, 
  getVoteCount 
} from "../services/voteService.js"
import { fetchImageBlob } from "../services/imageService";
import "../style/QuestionDetail.css";
import Header from "../components/Header.jsx";

export default function QuestionDetail() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [question, setQuestion] = useState(null);
  const [comments, setComments] = useState([]);
  const [imageSrc, setImageSrc] = useState(null);

  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [commentText, setCommentText] = useState("");

  const loggedInUser = JSON.parse(localStorage.getItem("forum_user") || "{}");

  const canEditOrDelete = (postAuthorId) => {
    if (!loggedInUser || !loggedInUser.Id) return false;
    return (
      loggedInUser.Id === postAuthorId || loggedInUser.IsModerator === true
    );
  };

  useEffect(() => {
    const loadFullQuestionData = async () => {
      setIsLoading(true);
      try {
        const qData = await getQuestionById(id);

        const [authorData, initialQuestionVotes, loadedImageSrc] =
          await Promise.all([
            getUserById(qData.AuthorId).then(data => data || { Username: "Unknown", Score: 0 }).catch(() => ({
              Username: "Unknown",
              Score: 0,
            })),
            getVoteCount(id, true).catch(() => 0),
            qData.ImageId
              ? fetchImageBlob(qData.ImageId).catch(() => null)
              : Promise.resolve(null),
          ]);

        setQuestion({
          ...qData,
          AuthorInfo: authorData,
          VoteCount: initialQuestionVotes,
        });
        setImageSrc(loadedImageSrc);

        const commentsData = await getCommentsByQuestionId(id);

        const commentsWithAuthors = await Promise.all(
          (commentsData || []).map(async (comment) => {
            const cAuthor = await getUserById(comment.AuthorId).catch(() => ({
              Username: "Unknown",
              Score: 0,
            }));
            const cVoteCount = await getVoteCount(comment.Id, false).catch(
              () => 0,
            );

            return {
              ...comment,
              AuthorInfo: cAuthor,
              VoteCount: cVoteCount,
            };
          }),
        );

        setComments(commentsWithAuthors);
      } catch (err) {
        setError("Failed to load question details.");
      } finally {
        setIsLoading(false);
      }
    };

    loadFullQuestionData();
  }, [id]);

  const handleVote = async (postId, isQuestion, isDownvote) => {
    try {
      await castVote(postId, isQuestion, isDownvote);
      const newCount = await getVoteCount(postId, isQuestion);

      if (isQuestion) {
        setQuestion((prev) => ({ ...prev, VoteCount: newCount }));
      } else {
        setComments((prev) =>
          prev.map((c) =>
            c.Id === postId ? { ...c, VoteCount: newCount } : c,
          ),
        );
      }
    } catch (err) {
      alert(err.message || "Failed to cast vote.");
    }
  };

  const handleDeleteQuestion = async () => {
    if (
      window.confirm(
        "Are you sure you want to delete this question? This cannot be undone.",
      )
    ) {
      try {
        await deleteQuestion(id);
        navigate("/home");
      } catch (err) {
        alert(err.message || "Failed to delete question.");
      }
    }
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

  if (isLoading) return <div className="loadingText">Loading...</div>;
  if (error) return <div className="errorText">{error}</div>;
  if (!question) return <div className="errorText">Question not found.</div>;

  return (
    <div className="pageContainer">
      <Header />

      <main className="detailMainContent">
        <div className="questionCard detailCard">
          <div className="voteColumn">
            <button
              className="voteArrow"
              onClick={() => handleVote(question.Id, true, false)}
            >
              ▲
            </button>
            <span className="voteCount">{question.VoteCount}</span>
            <button
              className="voteArrow"
              onClick={() => handleVote(question.Id, true, true)}
            >
              ▼
            </button>
          </div>

          <div className="contentColumn">
            <div className="cardHeader">
              <div className="authorInfo">
                {renderStatusBadge(question.Status)}
                <span className="username">{question.AuthorInfo.Username}</span>
                <span className="score">{question.AuthorInfo.Score} pts</span>
              </div>
              <span className="timestamp">
                {new Date(question.CreatedAt).toLocaleDateString()}
              </span>
            </div>

            <h2 className="detailTitle">{question.Title}</h2>
            <p className="detailText">{question.Text}</p>

            {imageSrc && (
              <div className="imageContainer">
                <img
                  src={imageSrc}
                  alt="Question attachment"
                  className="postImage"
                />
              </div>
            )}

            {canEditOrDelete(question.AuthorId) && (
              <div className="cardFooter">
                <button
                  className="editLink"
                  onClick={() => navigate(`/edit-question/${question.Id}`)}
                >
                  Edit
                </button>
                <button className="deleteLink" onClick={handleDeleteQuestion}>
                  Delete
                </button>
              </div>
            )}
          </div>
        </div>

        <h3 className="commentsHeader">{comments.length} Comments</h3>

        <div className="commentsList">
          {comments.map((comment, idx) => (
            <div key={idx} className="questionCard commentCard">
              <div className="voteColumn">
                <button
                  className="voteArrow"
                  onClick={() => handleVote(comment.Id, false, false)}
                >
                  ▲
                </button>
                <span className="voteCount">{comment.VoteCount}</span>
                <button
                  className="voteArrow"
                  onClick={() => handleVote(comment.Id, false, true)}
                >
                  ▼
                </button>
              </div>
              <div className="contentColumn">
                <div className="cardHeader">
                  <div className="authorInfo">
                    <span className="username">
                      {comment.AuthorInfo.Username}
                    </span>
                    <span className="score">
                      {comment.AuthorInfo.Score} pts
                    </span>
                  </div>
                  <span className="timestamp">
                    {new Date(comment.CreatedAt).toLocaleDateString()}
                  </span>
                </div>
                <p className="commentText">{comment.Text}</p>

                {canEditOrDelete(comment.AuthorId) && (
                  <div className="cardFooter commentFooter">
                    <button className="editLink">Edit</button>
                    <button className="deleteLink">Delete</button>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>

        <div className="writeCommentCard">
          <label 
            htmlFor="commentInput" 
            className="writeCommentTitle" 
            style={{ display: 'block' }}
          >
            Write a comment
          </label>
          <textarea
            id="commentInput"
            className="commentInput"
            placeholder="Share your answer..."
            value={commentText}
            onChange={(e) => setCommentText(e.target.value)}
          ></textarea>
          <div className="commentActions">
            <span className="attachIcon">📎</span>
            <button className="postCommentButton">Post Comment</button>
          </div>
        </div>
      </main>
    </div>
  );
}
