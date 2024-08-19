import React, { useState } from "react";
import "../styles/CommentForm.css";

function CommentForm({
  id,
  parentCommentId,
  commentOf,
  handleAddComment,
  handleCancelReply,
}) {
  const [comment, setComment] = useState({
    text: "",
    eventId: "",
    touristSiteId: null,
    storyId: null,
    parentComment: null,
    numberOfLikes: null,
    isReported: null,
  });

  const handleChange = (e) => {
    setComment({
      ...comment,
      text: e.target.value,
    });
  };

  const postComment = async () => {
    try {
      let updatedComment = { ...comment };
      if (commentOf === "event") {
        updatedComment.eventId = id;
      } else if (commentOf === "touristSite") {
        updatedComment.touristSiteId = id;
      } else if (commentOf === "story") {
        updatedComment.storyId = id;
      } else if (commentOf === "eventComment") {
        updatedComment.parentComment = parentCommentId;
        updatedComment.eventId = id;
      } else if (commentOf === "touristSiteComment") {
        updatedComment.parentComment = parentCommentId;
        updatedComment.touristSiteId = id;
      } else if (commentOf === "storyComment") {
        updatedComment.parentComment = parentCommentId;
        updatedComment.storyId = id;
      }

      await handleAddComment(updatedComment);

      if (handleCancelReply) {
        handleCancelReply();
      }

      setComment({
        text: "",
        eventId: "",
        touristSiteId: null,
        storyId: null,
        parentComment: null,
        numberOfLikes: null,
        isReported: null,
      });
    } catch (error) {
      console.error("Error with creating comment:", error);
    }
  };

  return (
    <div className="comment-form-container">
      <textarea
        className="comment-input"
        placeholder="Write comment..."
        value={comment.text}
        onChange={handleChange}
      ></textarea>
      <button className="comment-button" onClick={postComment}>
        Send
      </button>
    </div>
  );
}

export default CommentForm;
