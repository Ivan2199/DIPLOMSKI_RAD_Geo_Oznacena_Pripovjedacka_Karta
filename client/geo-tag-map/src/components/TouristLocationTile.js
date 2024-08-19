import React, { useState, useEffect } from "react";
import "../styles/TouristLocationTile.css";
import { useNavigate } from "react-router-dom";

function TouristLocationTile(props) {
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  const [formattedImages, setFormattedImages] = useState([]);
  const [formattedImage, setformattedImage] = useState("");
  const [hasImages, setHasImages] = useState(false);
  useEffect(() => {
    if (props.images != null) {
      setHasImages(true);
      const formatted = props.images.map((image) => {
        return (
          image.ImageData || image.ImagePrefix + "original" + image.ImageSuffix
        );
      });
      console.log(formatted);
      setFormattedImages(formatted);
    } else {
      setHasImages(false);
      setformattedImage("/assets/compass.png");
    }
  }, [props.images]);

  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentImageIndex(
        (prevIndex) => (prevIndex + 1) % formattedImages.length
      );
    }, 3000);

    return () => clearInterval(interval);
  }, [formattedImages]);

  const handleSetTouristSite = () => {
    props.setSelectedTouristSiteId(props.id);
    goToMap();
  };

  const navigate = useNavigate();

  const goToMap = () => {
    const mapType = "site";
    navigate(`/home?mapType=${mapType}`);
  };

  return (
    <div
      className="tourist-location-tile-container"
      onClick={handleSetTouristSite}
    >
      <div className="tile">
        {hasImages ? (
          formattedImages.map((image, index) => (
            <img
              key={index}
              src={image}
              alt="Tourist Location"
              className={index === currentImageIndex ? "slide-in" : "slide-out"}
            />
          ))
        ) : (
          <img src={formattedImage} alt="Default" />
        )}
      </div>
      <h1>{props.name}</h1>
      <div className="text">
        <p className="information-text-description">
          <span className="word-color">Description:</span> {props.description}
        </p>
        <p className="information-text-work-hours">
          <span className="word-color">Hours Open:</span> {props.hours}
        </p>
        <p className="information-text-location">
          <span className="word-color">Location:</span> {props.country},{" "}
          {props.city}, {props.address}
        </p>
        <div className="event-tile-bottom">
          <div className="social-media"></div>
          <div className="event-tile-link">
            <div className="information-text">
              <p>Popularity: {props.popularity}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default TouristLocationTile;
