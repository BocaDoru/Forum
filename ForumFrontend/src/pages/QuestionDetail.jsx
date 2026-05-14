import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { 
    getQuestionById, 
    deleteQuestion
} from '../services/questionService.js';
import { getUserById } from "../services/userService";
import { getCommentsByQuestionId, createComment, deleteComment, editComment } from "../services/commentService";
import { 
  castVote, 
  getVoteCount,
  getUserVote
} from "../services/voteService.js";
import { fetchImageBlob } from "../services/imageService";
import "../style/QuestionDetail.css";
import Header from "../components/Header.jsx";

const mapVoteToState = (voteData) => {
  if (!voteData) return 0;
  const isDownvote = voteData.IsDownvote !== undefined ? voteData.IsDownvote : voteData.isDownvote;
  if (isDownvote === undefined) return 0;
  return isDownvote ? -1 : 1;
};

export default function QuestionDetail() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [question, setQuestion] = useState(null);
  const [comments, setComments] = useState([]);
  const [imageSrc, setImageSrc] = useState(null);

  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [commentText, setCommentText] = useState("");
  const [commentImage, setCommentImage] = useState(null);
  const [editingCommentId, setEditingCommentId] = useState(null);
  const [editingCommentText, setEditingCommentText] = useState("");

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

        const [authorData, initialQuestionVotes, loadedImageSrc, initialUserVote] =
          await Promise.all([
            getUserById(qData.AuthorId).then(data => data || { Username: "Unknown", Score: 0 }).catch(() => ({
              Username: "Unknown",
              Score: 0,
            })),
            getVoteCount(id, true).catch(() => 0),
            qData.ImageId
              ? fetchImageBlob(qData.ImageId).catch(() => null)
              : Promise.resolve(null),
            getUserVote(id, true).catch(() => null),
          ]);

        setQuestion({
          ...qData,
          AuthorInfo: authorData,
          VoteCount: initialQuestionVotes,
          UserVote: mapVoteToState(initialUserVote),
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
            const cUserVote = await getUserVote(comment.Id, false).catch(() => null);

            return {
              ...comment,
              AuthorInfo: cAuthor,
              VoteCount: cVoteCount,
              UserVote: mapVoteToState(cUserVote),
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
      const newUserVote = await getUserVote(postId, isQuestion).catch(() => null);
      const newUserVoteState = mapVoteToState(newUserVote);

      if (isQuestion) {
        setQuestion((prev) => ({ ...prev, VoteCount: newCount, UserVote: newUserVoteState }));
      } else {
        setComments((prev) =>
          prev.map((c) =>
            c.Id === postId ? { ...c, VoteCount: newCount, UserVote: newUserVoteState } : c,
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
        if (comments && comments.length > 0) {
          await Promise.all(
            comments.map((c) => deleteComment(c.Id).catch(err => console.warn("Failed to delete comment", c.Id, err)))
          );
        }
        await deleteQuestion(id);
        navigate("/home");
      } catch (err) {
        alert(err.message || "Failed to delete question.");
      }
    }
  };

  const handlePostComment = async () => {
    if (!commentText.trim()) {
      alert("Comment text cannot be empty.");
      return;
    }

    try {
      await createComment(id, commentText, commentImage);
      
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
      setCommentText("");
      setCommentImage(null);
    } catch (err) {
      alert(err.message || "Failed to post comment.");
    }
  };

  const handleDeleteComment = async (commentId) => {
    if (window.confirm("Are you sure you want to delete this comment?")) {
      try {
        await deleteComment(commentId);
        setComments((prev) => prev.filter((c) => c.Id !== commentId));
      } catch (err) {
        alert(err.message || "Failed to delete comment.");
      }
    }
  };

  const handleEditClick = (comment) => {
    setEditingCommentId(comment.Id);
    setEditingCommentText(comment.Text);
  };

  const handleSaveEdit = async (commentId, imageId) => {
    if (!editingCommentText.trim()) {
      alert("Comment text cannot be empty.");
      return;
    }

    try {
      await editComment(commentId, editingCommentText, imageId || "");
      setComments((prev) =>
        prev.map((c) =>
          c.Id === commentId ? { ...c, Text: editingCommentText } : c
        )
      );
      setEditingCommentId(null);
    } catch (err) {
      alert(err.message || "Failed to edit comment.");
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
              className={`voteArrow ${question.UserVote === 1 ? 'active-upvote' : ''}`}
              onClick={() => handleVote(question.Id, true, false)}
            >
              ▲
            </button>
            <span className="voteCount">{question.VoteCount}</span>
            <button
              className={`voteArrow ${question.UserVote === -1 ? 'active-downvote' : ''}`}
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
                  className={`voteArrow ${comment.UserVote === 1 ? 'active-upvote' : ''}`}
                  onClick={() => handleVote(comment.Id, false, false)}
                >
                  ▲
                </button>
                <span className="voteCount">{comment.VoteCount}</span>
                <button
                  className={`voteArrow ${comment.UserVote === -1 ? 'active-downvote' : ''}`}
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
                {editingCommentId === comment.Id ? (
                  <div className="editModeContainer" style={{ marginTop: "10px" }}>
                    <textarea
                      className="commentInput"
                      value={editingCommentText}
                      onChange={(e) => setEditingCommentText(e.target.value)}
                      style={{ minHeight: "80px", marginBottom: "10px" }}
                    />
                    <div className="commentActions">
                      <button className="postCommentButton" onClick={() => handleSaveEdit(comment.Id, comment.ImageId)}>Save</button>
                      <button className="postCommentButton" style={{ background: "transparent", color: "inherit", border: "1px solid #444" }} onClick={() => setEditingCommentId(null)}>Cancel</button>
                    </div>
                  </div>
                ) : (
                  <>
                    <p className="commentText">{comment.Text}</p>
                    {canEditOrDelete(comment.AuthorId) && (
                      <div className="cardFooter commentFooter">
                        <button className="editLink" onClick={() => handleEditClick(comment)}>Edit</button>
                        <button className="deleteLink" onClick={() => handleDeleteComment(comment.Id)}>Delete</button>
                      </div>
                    )}
                  </>
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
            <label className="attachIcon" style={{ cursor: "pointer" }}>
              📎
              <input
                type="file"
                style={{ display: "none" }}
                onChange={(e) => setCommentImage(e.target.files[0])}
              />
            </label>
            {commentImage && <span className="imageName" style={{ fontSize: "0.8rem", marginLeft: "10px" }}>{commentImage.name}</span>}
            <button className="postCommentButton" onClick={handlePostComment}>Post Comment</button>
          </div>
        </div>
      </main>
    </div>
  );
}
