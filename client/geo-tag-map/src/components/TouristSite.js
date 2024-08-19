import React, { useState, useEffect, useCallback } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faGlobe } from "@fortawesome/free-solid-svg-icons";
import {
  faFacebook,
  faInstagram,
  faTwitter,
} from "@fortawesome/free-brands-svg-icons";
import CommentService from "../services/CommentService";
import PhotoService from "../services/PhotoService";
import TouristSiteService from "../services/TouristSiteService";
import LikeService from "../services/LikeService";
import Comment from "./Comment";
import CommentForm from "./CommentForm";
import Category from "./Category";
import "../styles/TouristSite.css";

function TouristSite({ slides, onClose, goToGallery }) {
  const [isInformation, setIsInformation] = useState(true);
  const [currentTab, setCurrentTab] = useState("Information");
  const [isVisible, setIsVisible] = useState(true);
  const [currentTouristSiteIndex, setCurrentTouristSiteIndex] = useState(0);
  const [comments, setComments] = useState([]);
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  const [formattedImages, setFormattedImages] = useState([]);
  const [image, setImage] = useState(null);
  const [imagePreviewUrl, setImagePreviewUrl] = useState("");
  const [likes, setLikes] = useState({});
  const [numberOfLikes, setNumberOfLikes] = useState(0);
  const [isLiking, setIsLiking] = useState(false);

  const currentTouristSite = slides[currentTouristSiteIndex]?.props.touristSite;
  console.log(currentTouristSite);

  const loadImages = useCallback(() => {
    if (currentTouristSite?.Photos) {
      const formatted = currentTouristSite.Photos.map(
        (photo) =>
          photo.ImageData || `${photo.ImagePrefix}original${photo.ImageSuffix}`
      ).slice(0, 5);
      setFormattedImages(formatted);
    }
  }, [currentTouristSite]);

  useEffect(() => {
    loadImages();
  }, [currentTouristSite, loadImages]);

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
      const timeout = setTimeout(onClose, 500);
      return () => clearTimeout(timeout);
    }
  }, [isVisible, onClose, currentTouristSite]);

  useEffect(() => {
    if (currentTouristSite) {
      getTouristSiteComments(currentTouristSite.Id);
      getInitialLikes();
      getNumberOfLikes();
    }
  }, [currentTouristSite, comments]);

  const getNumberOfLikes = async (touristSiteId) => {
    try {
      const response = await TouristSiteService.getTouristSiteById(
        touristSiteId
      );
      setNumberOfLikes(response.NumberOfLikes || 0);
    } catch (error) {
      console.log(error);
    }
  };

  const handleClose = () => setIsVisible(false);

  const handleToggle = (tab) => {
    setIsInformation(tab === "Information");
    setCurrentTab(tab);
    setImagePreviewUrl("");
  };

  const getTouristSiteComments = async (touristSiteId) => {
    try {
      const response = await CommentService.getCommentsByTouristSiteId(
        touristSiteId
      );
      setComments(response || []);
    } catch (error) {
      console.error("Error fetching comments:", error);
      setComments([]);
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

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    setImage(file);
    setImagePreviewUrl(URL.createObjectURL(file));
  };

  const convertImageToData = (imageFile) => {
    return new Promise((resolve, reject) => {
      if (!imageFile) return reject("No image file provided");
      const reader = new FileReader();
      reader.readAsDataURL(imageFile);
      reader.onload = () => resolve(reader.result);
      reader.onerror = (error) => reject(error);
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!image) return console.error("No image selected");

    try {
      const imageData = await convertImageToData(image);
      const response = await PhotoService.addPhoto({
        imageData,
        touristSiteId: currentTouristSite.Id,
      });
      setImagePreviewUrl("");
      if (response.success) {
        setFormattedImages((prevImages) => {
          const newImages = [...prevImages, imageData];
          return newImages.length > 5 ? newImages.slice(-5) : newImages;
        });
      } else {
        console.error("Error adding photo:", response.error);
      }
    } catch (error) {
      console.error("Error adding photo:", error);
    }
  };

  const getInitialLikes = async () => {
    try {
      const likeExists = await existingCurrentUserLike(currentTouristSite.Id);
      setLikes((prevLikes) => ({
        ...prevLikes,
        [currentTouristSite.Id]: likeExists,
      }));
    } catch (error) {
      console.error("Error fetching likes:", error);
    }
  };

  const existingCurrentUserLike = async (touristSiteId) => {
    try {
      const response = await LikeService.getTouristSiteLike(touristSiteId);
      return response;
    } catch (error) {
      console.error("Error fetching like:", error);
      return false;
    }
  };

  const handleAddLike = async (touristSiteId) => {
    if (isLiking) return;
    setIsLiking(true);

    try {
      const likeExists = await existingCurrentUserLike(touristSiteId);
      const updatedNumberOfLikes = likeExists
        ? numberOfLikes - 1
        : numberOfLikes + 1;

      setLikes((prevLikes) => ({
        ...prevLikes,
        [touristSiteId]: !likeExists,
      }));

      await TouristSiteService.updateTouristSiteAsync(
        { NumberOfLikes: Math.max(updatedNumberOfLikes, 0) },
        touristSiteId
      );

      if (likeExists) {
        await deleteLike(likeExists.Id);
        console.log("Tourist site disliked");
      } else {
        await addLikeToBase({ touristSiteId });
      }

      setNumberOfLikes(updatedNumberOfLikes);
    } catch (error) {
      console.error("Error updating tourist site:", error);
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

  const renderUserActions = () => {
    return (
      <div className="icon-elements">
        {likes[currentTouristSite.Id] ? (
          <img
            src="/assets/liked.png"
            alt="Liked"
            className="liked"
            title="Liked"
            onClick={() => handleAddLike(currentTouristSite.Id)}
          />
        ) : (
          <img
            src="/assets/like.svg"
            alt="Like"
            title="Like"
            onClick={() => handleAddLike(currentTouristSite.Id)}
          />
        )}
        <p className="story-number-of-likes">{numberOfLikes}</p>
      </div>
    );
  };

  return (
    <div
      className={`tourist-site-page ${
        isVisible ? "show-page-animation" : "hide-page-animation"
      } ${slides.length > 1 ? "show-page-background" : "hide-page-background"}`}
    >
      <div
        className={`tourist-site-container ${
          isVisible ? "show-animation" : "hide-animation"
        }`}
        id="touristSiteContainer"
      >
        <button className="close-button" id="closeButton" onClick={handleClose}>
          X
        </button>
        {currentTab !== "Comments" && currentTab !== "Add-photo" && (
          <div className="tourist-site-image-container">
            {formattedImages.length > 0 ? (
              formattedImages.map((image, index) => (
                <img
                  key={index}
                  src={image}
                  alt="Tourist Location"
                  className={
                    index === currentImageIndex ? "slide-in" : "slide-out"
                  }
                />
              ))
            ) : (
              <img src="/assets/compass.png" alt="Default" />
            )}
            {formattedImages.length > 1 && (
              <div className="gallery-dots">
                <div className="full-gallery">
                  <div
                    onClick={() =>
                      goToGallery(currentTouristSite.Id, "TouristSite")
                    }
                  >
                    Prikaži Galeriju
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
        )}
        <div
          className={`tourist-site-information ${
            currentTab === "Comments" || currentTab === "Add-photo"
              ? "full-height"
              : ""
          }`}
        >
          <div className="tourist-site-name-like-button">
            {currentTouristSite?.Name} {renderUserActions()}
          </div>
          <div className="tourist-site-navigation-bar">
            <ul>
              {["Information", "Comments", "Add-comment", "Add-photo"].map(
                (tab) => (
                  <li
                    key={tab}
                    onClick={() => handleToggle(tab)}
                    className={currentTab === tab ? "visible" : ""}
                  >
                    {tab.replace("-", " ")}
                  </li>
                )
              )}
            </ul>
          </div>
          {currentTab === "Information" && currentTouristSite && (
            <div className="visible-information-card">
              {currentTouristSite.HoursOpen && (
                <div className="tourist-site-section">
                  <p>Hours Open:&nbsp;&nbsp;{currentTouristSite.HoursOpen}</p>
                </div>
              )}
              {currentTouristSite.Description && (
                <div className="tourist-site-section">
                  <p>
                    Description:&nbsp;&nbsp;{currentTouristSite.Description}
                  </p>
                </div>
              )}
              {(currentTouristSite.Location?.Country ||
                currentTouristSite.Location?.City ||
                currentTouristSite.Location?.Address) && (
                <div className="location-section">
                  {currentTouristSite.Location?.Country && (
                    <p>
                      Country:&nbsp;&nbsp;{currentTouristSite.Location.Country}
                    </p>
                  )}
                  {currentTouristSite.Location?.City && (
                    <p>City:&nbsp;&nbsp;{currentTouristSite.Location.City}</p>
                  )}
                  {currentTouristSite.Location?.Address && (
                    <p>
                      Address:&nbsp;&nbsp;{currentTouristSite.Location.Address}
                    </p>
                  )}
                </div>
              )}
            </div>
          )}
          <div className="tourist-site-footer">
            {currentTouristSite.SiteCategories?.length > 0 && (
              <div className="category-section">
                {currentTouristSite.SiteCategories.map(
                  (siteCategory, index) => (
                    <div key={index} className="tourist-site-category">
                      <Category categoryId={siteCategory.CategoryId} />
                    </div>
                  )
                )}
              </div>
            )}
            <div className="social-media-section">
              {currentTouristSite.WebsiteUrl && (
                <a
                  href={currentTouristSite.WebsiteUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <FontAwesomeIcon icon={faGlobe} size="2x" />
                </a>
              )}
              {currentTouristSite.FacebookId && (
                <a
                  href={`https://www.facebook.com/${currentTouristSite.FacebookId}`}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <FontAwesomeIcon icon={faFacebook} size="2x" />
                </a>
              )}
              {currentTouristSite.Instagram && (
                <a
                  href={`https://www.instagram.com/${currentTouristSite.Instagram}`}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <FontAwesomeIcon icon={faInstagram} size="2x" />
                </a>
              )}
              {currentTouristSite.Twitter && (
                <a
                  href={`https://x.com/${currentTouristSite.Twitter}`}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <FontAwesomeIcon icon={faTwitter} size="2x" />
                </a>
              )}
            </div>
          </div>
          {currentTab === "Comments" && currentTouristSite && (
            <div className="visible-comment-card">
              <Comment
                commentOf="touristSiteComment"
                id={currentTouristSite.Id}
                comments={comments}
                setComments={setComments}
                handleAddComment={handleAddComment}
              />
            </div>
          )}
          {currentTab === "Add-comment" && currentTouristSite && (
            <div className="visible-comment-card">
              <CommentForm
                id={currentTouristSite.Id}
                commentOf="touristSite"
                handleAddComment={handleAddComment}
              />
            </div>
          )}
          {currentTab === "Add-photo" && (
            <div className="visible-add-photo-card">
              <form onSubmit={handleSubmit}>
                <div className="choose-image-submit-buttons">
                  <div className="mb-3">
                    <input
                      type="file"
                      className="form-add-photo-control"
                      id="image"
                      name="image"
                      accept="image/*"
                      onChange={handleImageChange}
                    />
                  </div>
                  {imagePreviewUrl && (
                    <button type="submit" className="btn btn-primary">
                      Submit
                    </button>
                  )}
                </div>
                {imagePreviewUrl && (
                  <div className="tourist-site-image-insert">
                    <img
                      id="image-preview"
                      src={imagePreviewUrl}
                      alt="Preview"
                    />
                  </div>
                )}
              </form>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default TouristSite;
