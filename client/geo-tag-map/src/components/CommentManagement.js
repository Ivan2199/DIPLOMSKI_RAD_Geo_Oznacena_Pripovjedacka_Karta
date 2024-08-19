import React, { useEffect, useState } from "react";
import CommentService from "../services/CommentService";
import "../styles/CommentManagement.css";

function CommentManagement() {
  const [comments, setComments] = useState([]);
  const [pageSize, setPageSize] = useState(20);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalNumberOfComments, setTotalNumberOfComments] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [orderBy, setOrderBy] = useState("DateCreated");
  const [sortOrder, setSortOrder] = useState("ASC");
  const [isReported, setIsReported] = useState(false);

  const getComments = async () => {
    setLoading(true);
    setError(null);
    try {
      const options = { orderBy, sortOrder, isReported };
      const response = await CommentService.fetchComments(
        pageNumber,
        pageSize,
        options
      );
      setComments(response.List);
      setTotalNumberOfComments(response.TotalSize);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    getComments();
  }, [pageSize, pageNumber, orderBy, sortOrder, isReported]);

  const handleDeleteComment = async (commentId) => {
    try {
      await CommentService.deleteCommentAsync(commentId);
      getComments();
    } catch (error) {
      console.log("Error has occurred while deleting a comment", error);
    }
  };

  return (
    <div className="comment-management">
      <h1>Comment Management</h1>
      <div className="controls">
        <label className="checkbox-container">
          Show Reported:
          <input
            type="checkbox"
            checked={isReported}
            onChange={(e) => setIsReported(e.target.checked)}
          />
          <span className="checkmark"></span>
        </label>
      </div>
      {loading ? (
        <p>Loading...</p>
      ) : (
        <>
          {isReported && comments.length === 0 ? (
            <p>There are no reported comments.</p>
          ) : (
            <table className="comment-table">
              <thead>
                <tr>
                  <th>Username</th>
                  <th>Date Created</th>
                  <th>Text</th>
                  <th>Reported Comment</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {comments.map((comment) => (
                  <tr key={comment.id}>
                    <td>{comment.User.Username}</td>
                    <td>{new Date(comment.DateCreated).toLocaleString()}</td>
                    <td>{comment.Text}</td>
                    <td className={comment.IsReported ? "reported" : ""}>
                      {comment.IsReported ? "true" : "false"}
                    </td>
                    <td>
                      <div className="comment-action-buttons">
                        <div
                          className="delete-comment-action-button"
                          onClick={() => handleDeleteComment(comment.Id)}
                        >
                          Delete Comment
                        </div>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
          <div className="pagination">
            <button
              onClick={() => setPageNumber((prev) => Math.max(prev - 1, 1))}
              disabled={pageNumber === 1}
            >
              Previous
            </button>
            <span>
              Page {pageNumber} of {Math.ceil(totalNumberOfComments / pageSize)}
            </span>
            <button
              onClick={() => setPageNumber((prev) => prev + 1)}
              disabled={
                pageNumber >= Math.ceil(totalNumberOfComments / pageSize)
              }
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}

export default CommentManagement;
