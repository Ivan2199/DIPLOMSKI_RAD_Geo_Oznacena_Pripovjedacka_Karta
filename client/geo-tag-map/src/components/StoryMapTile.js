import React from "react";
import "../styles/StoryMapTile.css";

function StoryMapTile(props) {
  function formatDate(date) {
    const dateObject = new Date(date);

    const year = dateObject.getFullYear();
    const month = String(dateObject.getMonth() + 1).padStart(2, "0");
    const day = String(dateObject.getDate()).padStart(2, "0");

    const formattedDate = `${year}-${month}-${day}`;

    return formattedDate;
  }
  const firstImage =
    props.photos && props.photos.length > 0 ? props.photos[0].ImageData : "";

  return (
    <div className="story-map-tile">
      <div className="story-map-tile-user-story">{props.text}</div>
      <div className="story-map-tile-container">
        <div className="story-map-tile-image-container">
          <img
            className="story-map-tile-image"
            src={firstImage}
            alt="Story Image"
          />
        </div>
        <div className="story-map-tile-information">
          <div className="story-user-information">
            <img src={props.user.Image} alt="User Image"></img>
            <h2 className="story-map-tile-user-name">{props.user.Username}</h2>
          </div>
          <div className="story-map-tile-date">
            <div className="story-map-tile-start">
              <span className="word-highlight">{formatDate(props.date)}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default StoryMapTile;
