import React from "react";
import EventTile from "./EventTile";
import "../styles/LikedEvents.css";

function LikedEvents(props) {
  return (
    <div className="liked-events">
      <p className="title-liked-events">Events You Like</p>
      {props.lastLikedEvents.length > 0 ? (
        props.lastLikedEvents.map((event) => (
          <div key={event.Id} className="user-liked-event-card">
            <EventTile
              id={event.Id}
              name={event.Name}
              url={event.Url}
              image={event.Image}
              startDate={event.StartDate}
              endDate={event.EndDate}
              type={event.Type}
              country={event.Location?.Country}
              city={event.Location?.City}
              address={event.Location?.Address}
              setSelectedEventId={props.setSelectedEventId}
            />
          </div>
        ))
      ) : (
        <div>You haven't liked any event</div>
      )}
    </div>
  );
}

export default LikedEvents;
