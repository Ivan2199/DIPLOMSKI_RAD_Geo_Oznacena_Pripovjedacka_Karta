import React from "react";
import "../styles/EventTile.css";
import { useNavigate } from "react-router-dom";

function EventTile(props) {
  function formatDate(date) {
    const dateObject = new Date(date);

    const year = dateObject.getFullYear();
    const month = String(dateObject.getMonth() + 1).padStart(2, "0");
    const day = String(dateObject.getDate()).padStart(2, "0");

    const formattedDate = `${year}-${month}-${day}`;

    return formattedDate;
  }

  const handleSetEvent = () => {
    props.setSelectedEventId(props.id);
    goToMap();
  };

  const navigate = useNavigate();

  const goToMap = () => {
    const mapType = "event";
    navigate(`/home?mapType=${mapType}`);
  };

  return (
    <div className="event-tile-container" onClick={handleSetEvent}>
      <div className="tile">
        <img src={props.image} alt="Event" />
      </div>
      <h1>{props.name}</h1>
      <div className="text">
        <p className="information-text-type">
          <span className="word-color">Tip Događaja:</span> {props.type}
        </p>
        <p className="information-text-location">
          <span className="word-color">Lokacija:</span> {props.country}
          &nbsp;,&nbsp;{props.city},&nbsp;
          {props.address}
        </p>
        <div className="event-tile-bottom">
          <div className="event-tile-date-position">
            <p className="information-text">
              <span className="word-color">Datum Početka:</span>{" "}
              {new Date(props.startDate).toLocaleDateString("hr-HR")}
            </p>
            <p>&nbsp;&nbsp;</p>
            <p className="information-text">
              <span className="word-color">Datum Kraja:</span>{" "}
              {new Date(props.endDate).toLocaleDateString("hr-HR")}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default EventTile;
