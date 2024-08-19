import React from "react";
import "../styles/EventMapTile.css";

function EventMapTile(props) {
  function formatDate(date) {
    const dateObject = new Date(date);

    const year = dateObject.getFullYear();
    const month = String(dateObject.getMonth() + 1).padStart(2, "0");
    const day = String(dateObject.getDate()).padStart(2, "0");

    const formattedDate = `${year}-${month}-${day}`;

    return formattedDate;
  }

  return (
    <div className="event-map-tile">
      <div className="event-map-tile-cover-image"></div>
      <div className="event-map-tile-container">
        <div className="event-map-tile-image-container">
          <img
            className="event-map-tile-image"
            src={props.image}
            alt={props.name}
          />
        </div>
        <div className="event-map-tile-information">
          <h2 className="event-map-tile-name">{props.name}</h2>
          <div className="event-map-tile-event-type">
            <span className="word-highlight">
              Tip Događaja: &nbsp;&nbsp;{" "}
              {props.type === "Concert" ? "Koncert" : "Festival"}
            </span>
          </div>
          <div className="event-map-tile-dates">
            <div className="event-map-tile-start">
              <span className="word-highlight">
                Datum Početka: &nbsp;&nbsp;{" "}
                {new Date(props.startDate).toLocaleDateString("hr-HR")}
              </span>
            </div>
            <div className="event-map-tile-end">
              <span className="word-highlight">
                Datum Kraja: &nbsp;&nbsp;{" "}
                {new Date(props.endDate).toLocaleDateString("hr-HR")}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default EventMapTile;
