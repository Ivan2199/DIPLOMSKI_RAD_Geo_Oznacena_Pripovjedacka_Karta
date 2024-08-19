import React, { useState, useEffect, useCallback } from "react";
import "../styles/Story.css";
import CommentService from "../services/CommentService";
import LikeService from "../services/LikeService";
import UserService from "../services/UserService";
import PhotoService from "../services/PhotoService";
import Comment from "./Comment";
import CommentForm from "./CommentForm";
import StoryService from "../services/StoryService";

function Story(props) {
  const [isInformation, setIsInformation] = useState(true);
  const [isComment, setIsComment] = useState(false);
  const [isAddComment, setIsAddComment] = useState(false);
  const [isAddPhoto, setIsAddPhoto] = useState(false);
  const [isVisible, setIsVisible] = useState(true);
  const [currentStoryIndex, setCurrentStoryIndex] = useState(0);
  const [comments, setComments] = useState([]);
  const [likes, setLikes] = useState({});
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  const [formattedImages, setFormattedImages] = useState([]);
  const [deleteSuccess, setDeleteSuccess] = useState(false);
  const [photoFile, setPhotoFile] = useState(null);
  const [photoPreview, setPhotoPreview] = useState(null);
  const [numberOfLikes, setNumberOfLikes] = useState(0);
  const [isLiking, setIsLiking] = useState(false);

  const currentStory = props.slides[currentStoryIndex]?.props.story;

  const loadImages = useCallback(() => {
    if (currentStory?.Photos) {
      console.log(currentStory.Photos);
      const formatted = currentStory.Photos.map((photo) => photo.ImageData);
      console.log(formatted);
      setFormattedImages(formatted);
    }
  }, [currentStory]);

  useEffect(() => {
    setFormattedImages([]);
    loadImages();
  }, [currentStory, loadImages]);

  useEffect(() => {
    if (formattedImages.length > 0) {
      const interval = setInterval(() => {
        setCurrentImageIndex(
          (prevIndex) => (prevIndex + 1) % formattedImages.length
        );
      }, 5000);
      return () => clearInterval(interval);
    }
  }, [formattedImages]);

  useEffect(() => {
    if (!isVisible) {
      const timeout = setTimeout(() => {
        props.onClose();
      }, 500);
      return () => clearTimeout(timeout);
    }
  }, [isVisible, props.onClose, currentStory]);

  useEffect(() => {
    if (currentStory) {
      getStoryComments(currentStory.Id);
      getInitialLikes();
      getNumberOfLikes();
    }
  }, [currentStory, comments]);

  const getStoryComments = async (storyId) => {
    try {
      const response = await CommentService.getCommentsByStoryId(storyId);
      setComments(response || []);
    } catch (error) {
      console.error("Error fetching comments:", error);
      setComments([]);
    }
  };

  const getNumberOfLikes = async (storyId) => {
    try {
      const response = await StoryService.getStoryById(storyId);
      setNumberOfLikes(response.NumberOfLikes || 0);
    } catch (error) {
      console.log(error);
    }
  };

  const getInitialLikes = async () => {
    try {
      const likeExists = await existingCurrentUserLike(currentStory.Id);
      setLikes((prevLikes) => ({
        ...prevLikes,
        [currentStory.Id]: likeExists,
      }));
    } catch (error) {
      console.error("Error fetching likes:", error);
    }
  };

  const existingCurrentUserLike = async (storyId) => {
    try {
      const response = await LikeService.getStoryLike(storyId);
      return response;
    } catch (error) {
      console.error("Error fetching like:", error);
      return false;
    }
  };

  const handleAddLike = async (storyId) => {
    if (isLiking) return;
    setIsLiking(true);

    try {
      const likeExists = await existingCurrentUserLike(storyId);
      const updatedNumberOfLikes = likeExists
        ? numberOfLikes - 1
        : numberOfLikes + 1;

      setLikes((prevLikes) => ({
        ...prevLikes,
        [storyId]: !likeExists,
      }));

      await StoryService.updateStoryAsync(
        { NumberOfLikes: Math.max(updatedNumberOfLikes, 0) },
        storyId
      );

      if (likeExists) {
        await deleteLike(likeExists.Id);
        console.log("Story disliked");
      } else {
        await addLikeToBase({ storyId });
      }

      setNumberOfLikes(updatedNumberOfLikes);
    } catch (error) {
      console.error("Error updating story:", error);
    } finally {
      setIsLiking(false);
    }
  };

  const deleteLike = async (likeId) => {
    try {
      await LikeService.deleteLikeAsync(likeId);
    } catch (error) {
      console.error("Error deleting like:", error);
    }
  };

  const addLikeToBase = async (like) => {
    try {
      like.isLike = true;
      await LikeService.addLike(like);
    } catch (error) {
      console.error("Error adding like:", error);
    }
  };

  const handleReportStory = async (storyId) => {
    try {
      const updatedStory = {
        IsReported: !currentStory.IsReported,
      };

      const response = await StoryService.updateStoryAsync(
        updatedStory,
        storyId
      );
      if (!response.success) {
        console.error("Error while updating report status.", response.error);
      }
    } catch (error) {
      console.error("Error while reporting a story:", error);
    }
  };

  const handleDeleteStory = async (storyId) => {
    try {
      const response = await StoryService.deleteStoryAsync(storyId);
      handleClose();
    } catch (error) {
      console.error("Error deleting story:", error);
    }
  };

  function formatDate(date) {
    const dateObject = new Date(date);

    const year = dateObject.getFullYear();
    const month = String(dateObject.getMonth() + 1).padStart(2, "0");
    const day = String(dateObject.getDate()).padStart(2, "0");

    return `${year}-${month}-${day}`;
  }

  const handleClose = () => {
    setIsVisible(false);
  };

  const handleToggle = (tab) => {
    if (tab === "information") {
      setIsInformation(true);
      setIsComment(false);
      setIsAddComment(false);
      setIsAddPhoto(false);
    } else if (tab === "comments") {
      setIsInformation(false);
      setIsComment(true);
      setIsAddComment(false);
      setIsAddPhoto(false);
    } else if (tab === "add-comment") {
      setIsInformation(false);
      setIsComment(false);
      setIsAddComment(true);
      setIsAddPhoto(false);
    } else if (tab === "add-photo") {
      setIsInformation(false);
      setIsComment(false);
      setIsAddComment(false);
      setIsAddPhoto(true);
    }
  };

  const handleAddComment = async (newComment) => {
    try {
      const response = await CommentService.createComment(newComment);
      if (response.success) {
        setComments([...comments, response.data]);
      } else {
        console.error("Error adding comment:", response.error);
      }
    } catch (error) {
      console.error("Error adding comment:", error);
    }
  };

  const handlePhotoChange = (e) => {
    const file = e.target.files[0];
    setPhotoFile(file);
    setPhotoPreview(URL.createObjectURL(file));
  };

  const handleAddPhoto = async () => {
    if (photoFile) {
      const reader = new FileReader();
      reader.onload = async (e) => {
        const photo = { ImageData: e.target.result, StoryId: currentStory.Id };
        try {
          console.log(photo);
          const response = await PhotoService.addPhoto(photo);
          setPhotoFile(null);
          setPhotoPreview(null);
          if (response.success) {
            setFormattedImages([...formattedImages, e.target.result]);
            alert("Photo added successfully");
          } else {
            console.error("Error adding photo:", response.error);
          }
        } catch (error) {
          console.error("Error adding photo:", error);
        }
      };
      reader.readAsDataURL(photoFile);
    }
  };

  const handleCancel = () => {
    setPhotoPreview(null);
    setPhotoFile(null);
  };

  const renderUserActions = () => {
    const loggedUserId = UserService.getUserData();
    return (
      <>
        <div className="icon-elements">
          {likes[currentStory.Id] ? (
            <img
              src="/assets/liked.png"
              alt="Liked"
              className="liked"
              title="Liked"
              onClick={() => handleAddLike(currentStory.Id)}
            />
          ) : (
            <img
              src="/assets/like.svg"
              alt="Like"
              title="Like"
              onClick={() => handleAddLike(currentStory.Id)}
            />
          )}
          <p className="story-number-of-likes">{numberOfLikes}</p>
          {loggedUserId === currentStory.User.Id ? (
            <img
              className="delete-icon"
              src="/assets/delete.svg"
              alt="Delete"
              title="Delete"
              onClick={() => handleDeleteStory(currentStory.Id)}
            />
          ) : currentStory.IsReported ? (
            <img
              src="/assets/reported.png"
              alt="Reported"
              title="Reported"
              className="reported-icon"
              onClick={() => handleReportStory(currentStory.Id)}
            />
          ) : (
            <img
              src="/assets/report.svg"
              alt="Report"
              title="Report"
              className="report-icon"
              onClick={() => handleReportStory(currentStory.Id)}
            />
          )}
        </div>
      </>
    );
  };

  return (
    <div
      className={`story-page ${
        isVisible ? "show-page-animation" : "hide-page-animation"
      } ${
        props.slides.length > 1
          ? "show-page-background"
          : "hide-page-background"
      }`}
    >
      <div
        className={`story-container ${
          isVisible ? "show-animation" : "hide-animation"
        }`}
        id="storyContainer"
      >
        <button className="close-button" id="closeButton" onClick={handleClose}>
          X
        </button>
        <div className="story-information">
          <div className="story-image">
            {formattedImages.length > 0 ? (
              formattedImages.map((image, index) => (
                <img
                  key={index}
                  src={image}
                  alt={`Story Image ${index}`}
                  className={`slide-image ${
                    index === currentImageIndex ? "active" : ""
                  }`}
                  style={{
                    display: index === currentImageIndex ? "block" : "none",
                  }}
                />
              ))
            ) : (
              <img src="/assets/compass.png" alt="Default" />
            )}
            {formattedImages.length > 1 && (
              <div className="gallery-dots">
                <div className="full-gallery">
                  <div
                    onClick={() => props.goToGallery(currentStory.Id, "Story")}
                  >
                    Go to Gallery
                  </div>
                </div>
                <div className="dot-container dot-images">
                  {formattedImages.map((_, index) => (
                    <span
                      key={index}
                      className={`dot ${
                        index === currentImageIndex ? "active" : ""
                      }`}
                      onClick={() => setCurrentImageIndex(index)}
                    ></span>
                  ))}
                </div>
              </div>
            )}
          </div>
          <div className="user-information">
            <div>
              {currentStory?.User?.Image ? (
                <img
                  src={currentStory.User.Image}
                  alt={currentStory.User.Username}
                  className="user-image"
                />
              ) : (
                <p>User image not available</p>
              )}
              <p className="user-name">{currentStory?.User?.Username}</p>
            </div>
            {renderUserActions()}
          </div>
          <div className="story-navigation-bar">
            <ul>
              <li
                onClick={() => handleToggle("information")}
                className={isInformation ? "visible" : ""}
              >
                Information
              </li>
              <li
                onClick={() => handleToggle("comments")}
                className={isComment ? "visible" : ""}
              >
                Comments
              </li>
              <li
                onClick={() => handleToggle("add-comment")}
                className={isAddComment ? "visible" : ""}
              >
                Add Comment
              </li>
              {UserService.getUserData() === currentStory?.User?.Id && (
                <li
                  onClick={() => handleToggle("add-photo")}
                  className={isAddPhoto ? "visible" : ""}
                >
                  Add Photo
                </li>
              )}
            </ul>
          </div>
          <div
            className={
              isInformation ? "visible-information-card" : "hidden-card"
            }
          >
            {isInformation && currentStory && (
              <>
                <div className="story-section">
                  <p>Text: {currentStory.Text}</p>
                  <p>Date: {formatDate(currentStory.DateTime)}</p>
                </div>
                <div className="location-section">
                  {currentStory.Location?.Country && (
                    <p>Country: {currentStory.Location.Country}</p>
                  )}
                  {currentStory.Location?.City && (
                    <p>City: {currentStory.Location.City}</p>
                  )}
                  {currentStory.Location?.Address && (
                    <p>Address: {currentStory.Location.Address}</p>
                  )}
                  {currentStory.Location?.NameOfPlace && (
                    <p>Name Of Place: {currentStory.Location.NameOfPlace}</p>
                  )}
                </div>
              </>
            )}
          </div>
          <div
            className={
              isComment ? "visible-comment-card" : "hidden-comment-card"
            }
          >
            {isComment && currentStory && (
              <Comment
                commentOf="storyComment"
                id={currentStory.Id}
                comments={comments}
                setComments={setComments}
                handleAddComment={handleAddComment}
              />
            )}
          </div>
          <div
            className={
              isAddComment ? "visible-comment-card" : "hidden-comment-card"
            }
          >
            {isAddComment && currentStory && (
              <CommentForm
                id={currentStory.Id}
                commentOf="story"
                handleAddComment={handleAddComment}
              />
            )}
          </div>
          <div
            className={isAddPhoto ? "visible-add-photo-card" : "hidden-card"}
          >
            {isAddPhoto && (
              <div className="add-photo-section">
                {!photoPreview && (
                  <div>
                    <input
                      type="file"
                      onChange={handlePhotoChange}
                      className="photo-input"
                      id="photo-input"
                    />
                    <label htmlFor="photo-input">Choose File</label>
                  </div>
                )}
                {photoPreview && (
                  <>
                    <img
                      src={photoPreview}
                      alt="Photo Preview"
                      className="photo-preview"
                    />
                    <div className="button-group">
                      <button
                        onClick={handleAddPhoto}
                        className="submit-button"
                      >
                        Submit
                      </button>
                      <button onClick={handleCancel} className="cancel-button">
                        Cancel
                      </button>
                    </div>
                  </>
                )}
              </div>
            )}
          </div>
        </div>
        {props.slides.length > 1 && (
          <div className="dot-container">
            {props.slides.map((slide, index) => (
              <span
                key={index}
                className={`dot ${index === currentStoryIndex ? "active" : ""}`}
                onClick={() => setCurrentStoryIndex(index)}
              ></span>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default Story;
