import React, { useState, useEffect } from "react";
import UserService from "../services/UserService";
import CommentService from "../services/CommentService";
import LikeService from "../services/LikeService";
import CommentForm from "./CommentForm";
import "../styles/Comment.css";

function Comment({ comments, setComments, handleAddComment, id, commentOf }) {
  const [users, setUsers] = useState({});
  const [likes, setLikes] = useState({});
  const [parentComments, setParentComments] = useState([]);
  const [childComments, setChildComments] = useState({});
  const [replyingTo, setReplyingTo] = useState(null);
  const [
    parentFetchingFunctionParameters,
    setParentFetchingFunctionParameters,
  ] = useState({
    eventId: null,
    storyId: null,
    touristSiteId: null,
  });

  useEffect(() => {
    if (comments.length > 0) {
      getCommentUsers();
      getInitialLikes();
      fetchParentComments();
    }
  }, [comments]);

  const fetchParentComments = async () => {
    try {
      const params = {
        eventComment: [id, null, null],
        storyComment: [null, id, null],
        touristSiteComment: [null, null, id],
      };

      const parentCommentsList = await CommentService.getParentComments(
        ...params[commentOf]
      );

      setParentComments(parentCommentsList);
      fetchChildCommentsForParents(parentCommentsList);
    } catch (error) {
      console.error("Error fetching parent comments:", error);
    }
  };

  const fetchChildCommentsForParents = async (parents) => {
    try {
      const childrenComments = {};
      await Promise.all(
        parents.map(async (parent) => {
          const children = await CommentService.getChildComments(parent.Id);
          childrenComments[parent.Id] = children;
        })
      );
      setChildComments(childrenComments);
    } catch (error) {
      console.error("Error fetching child comments:", error);
    }
  };

  const getImageSrc = (image) => {
    if (!image) return "";
    if (typeof image === "string") {
      return image;
    } else if (image instanceof Blob || image instanceof File) {
      return URL.createObjectURL(image);
    }
    return "";
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString("hr-HR");
  };

  const getCommentUsers = async () => {
    try {
      const uniqueUserIds = Array.from(
        new Set(comments.map((comment) => comment.CreatedBy))
      );
      const fetchedUsers = {};

      await Promise.all(
        uniqueUserIds.map(async (userId) => {
          if (!fetchedUsers[userId]) {
            const response = await UserService.getUserAsync(userId);
            fetchedUsers[userId] = response;
          }
        })
      );
      setUsers({ ...fetchedUsers });
    } catch (error) {
      console.error("Error fetching user data:", error);
    }
  };

  const getInitialLikes = async () => {
    try {
      const fetchedLikes = {};
      await Promise.all(
        comments.map(async (comment) => {
          const likeExists = await existingCurrentUserLike(comment.Id);
          fetchedLikes[comment.Id] = !!likeExists;
        })
      );
      setLikes(fetchedLikes);
    } catch (error) {
      console.error("Error fetching likes:", error);
    }
  };

  const handleShowDelete = (commentUserId) => {
    const loggedUserId = UserService.getUserData();
    return loggedUserId === commentUserId;
  };

  const handleDelete = async (commentId) => {
    try {
      const response = await CommentService.deleteCommentAsync(commentId);
      if (response.success) {
        setComments(comments.filter((comment) => comment.Id !== commentId));
      } else {
        console.error("Error deleting comment:", response.error);
      }
    } catch (error) {
      console.error("Error deleting comment:", error);
    }
  };

  const existingCurrentUserLike = async (commentId) => {
    try {
      const response = await LikeService.getCommentLike(commentId);
      if (!response) {
        return false;
      }
      return response;
    } catch (error) {
      console.error("Error fetching like:", error);
      return false;
    }
  };

  const handleAddLike = async (commentId) => {
    try {
      const commentToUpdate = comments.find(
        (comment) => comment.Id === commentId
      );

      const likeExists = await existingCurrentUserLike(commentId);

      if (likeExists) {
        let updatedNumberOfLikes = commentToUpdate.NumberOfLikes;
        updatedNumberOfLikes--;

        if (updatedNumberOfLikes === -1) {
          updatedNumberOfLikes = null;
        }

        const updatedComment = {
          ...commentToUpdate,
          NumberOfLikes: updatedNumberOfLikes,
        };
        const updatedComments = comments.map((comment) =>
          comment.Id === commentId ? updatedComment : comment
        );
        setComments(updatedComments);
        setLikes((prevLikes) => ({
          ...prevLikes,
          [commentId]: false,
        }));

        await CommentService.updateCommentAsync(
          {
            parentComment: updatedComment.ParentComment,
            numberOfLikes: updatedNumberOfLikes,
            isReported: updatedComment.IsReported,
          },
          commentId
        );

        await deleteLike(likeExists.Id);
        console.log("Comment disliked");

        return;
      }

      if (commentToUpdate) {
        let updatedNumberOfLikes = commentToUpdate.NumberOfLikes || 0;
        updatedNumberOfLikes++;

        const updatedComment = {
          ...commentToUpdate,
          NumberOfLikes: updatedNumberOfLikes,
        };
        const updatedComments = comments.map((comment) =>
          comment.Id === commentId ? updatedComment : comment
        );
        setComments(updatedComments);
        setLikes((prevLikes) => ({
          ...prevLikes,
          [commentId]: true,
        }));

        await CommentService.updateCommentAsync(
          {
            parentComment: updatedComment.ParentComment,
            numberOfLikes: updatedNumberOfLikes,
            isReported: updatedComment.IsReported,
          },
          commentId
        );

        await addLikeToBase({ commentId });
      }
    } catch (error) {
      console.error("Error updating comment:", error);
    }
  };

  const deleteLike = async (likeId) => {
    try {
      const response = await LikeService.deleteLikeAsync(likeId);
    } catch (error) {
      console.error("Error deleting like:", error);
    }
  };

  const addLikeToBase = async (like) => {
    try {
      like.isLike = true;
      const response = await LikeService.addLike(like);
    } catch (error) {
      console.error("Error adding like:", error);
    }
  };

  const handleReportComment = async (commentId) => {
    try {
      const commentToUpdate = comments.find(
        (comment) => comment.Id === commentId
      );
      if (!commentToUpdate) {
        console.error("Comment Not Found");
        return;
      }

      if (commentToUpdate.IsReported) {
        const updatedComments = comments.map((comment) =>
          comment.Id === commentId ? { ...comment, IsReported: false } : comment
        );
        setComments(updatedComments);

        const response = await CommentService.updateCommentAsync(
          { ...commentToUpdate, IsReported: false },
          commentId
        );
        if (!response.success) {
          console.error("Error while deleting report.", response.error);
        }
        return;
      }

      const updatedComments = comments.map((comment) =>
        comment.Id === commentId ? { ...comment, IsReported: true } : comment
      );
      setComments(updatedComments);

      const response = await CommentService.updateCommentAsync(
        { ...commentToUpdate, IsReported: true },
        commentId
      );
      if (!response.success) {
        console.error("Error while reporting a comment:", response.error);
      }
    } catch (error) {
      console.error("Error while reporting a comment:", error);
    }
  };

  const handleReply = (commentId) => {
    setReplyingTo(commentId);
  };

  const handleCancelReply = () => {
    setReplyingTo(null);
  };

  const renderComments = () => {
    return parentComments.map((parent, index) => (
      <div key={index}>
        {renderComment(parent)}
        {childComments[parent.Id] &&
          childComments[parent.Id].map((child, idx) => (
            <div key={idx} className="child-comment">
              {renderComment(child, true)}
            </div>
          ))}
      </div>
    ));
  };

  const renderComment = (comment, isChild = false) => {
    return (
      <div className={`comment-reply ${isChild ? "child-comment" : ""}`}>
        <div className="comment">
          {users[comment.CreatedBy] && (
            <div className="comment-user">
              <div className="comment-user-image">
                <img
                  id="image-preview"
                  src={getImageSrc(users[comment.CreatedBy].Image)}
                  alt={users[comment.CreatedBy].Username}
                />
              </div>
              <div className="comment-user-info">
                <div className="comment-user-name">
                  {users[comment.CreatedBy].Username}
                </div>
                <div className="comment-date">
                  {formatDate(comment.DateCreated)}
                </div>
              </div>
            </div>
          )}
          <div className="text-icons">
            <div className="comment-text">{comment.Text}</div>
            <div className="icon-elements">
              {likes[comment.Id] ? (
                <img
                  src="/assets/liked.png"
                  alt="Liked"
                  className="liked"
                  title="Liked"
                  onClick={() => handleAddLike(comment.Id, isChild)}
                ></img>
              ) : (
                <img
                  src="/assets/like.svg"
                  alt="Like"
                  title="Like"
                  onClick={() => handleAddLike(comment.Id, isChild)}
                ></img>
              )}
              <p>{comment.NumberOfLikes}</p>
              {!isChild && (
                <img
                  src="/assets/reply.svg"
                  alt="Reply"
                  title="Reply"
                  onClick={() => handleReply(comment.Id)}
                ></img>
              )}
              {handleShowDelete(comment.CreatedBy) ? (
                <img
                  className="delete-icon"
                  src="/assets/delete.svg"
                  alt="Delete"
                  title="Delete"
                  onClick={() => handleDelete(comment.Id)}
                />
              ) : comment.IsReported ? (
                <img
                  src="/assets/reported.png"
                  alt="Reported"
                  title="Reported"
                  className="report-icon"
                  onClick={() => handleReportComment(comment.Id)}
                />
              ) : (
                <img
                  src="/assets/report.svg"
                  alt="Report"
                  title="Report"
                  className="report-icon"
                  onClick={() => handleReportComment(comment.Id)}
                />
              )}
            </div>
          </div>
        </div>
        {replyingTo === comment.Id && (
          <div>
            <CommentForm
              id={id}
              parentCommentId={comment.Id}
              commentOf={commentOf}
              handleAddComment={handleAddComment}
              handleCancelReply={handleCancelReply}
            />
            <div className="cancel-button" onClick={handleCancelReply}>
              Cancel
            </div>
          </div>
        )}
      </div>
    );
  };

  return <div className="comment-container">{renderComments()}</div>;
}

export default Comment;
