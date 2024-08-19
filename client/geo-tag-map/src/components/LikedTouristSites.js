import React from "react";
import TouristLocationTile from "./TouristLocationTile";
import "../styles/LikedTouristSites.css";

function LikedTouristSites(props) {
  return (
    <div className="liked-tourist-sites">
      <p className="title-liked-tourist-sites">Tourist Sites You Like</p>
      {props.lastLikedTouristSites.length > 0 ? (
        props.lastLikedTouristSites.map((touristSite) => (
          <div key={touristSite.Id} className="tourist-site-user-profile-card">
            <TouristLocationTile
              key={touristSite.Id}
              id={touristSite.Id}
              name={touristSite.Name}
              images={touristSite.Photos}
              popularity={touristSite.Popularity}
              description={touristSite.Description}
              country={touristSite.Location?.Country}
              city={touristSite.Location?.City}
              address={touristSite.Location?.Address}
              hours={touristSite.HoursOpen}
              setSelectedTouristSiteId={props.setSelectedTouristSiteId}
            />
          </div>
        ))
      ) : (
        <div>You haven't liked any tourist site</div>
      )}
    </div>
  );
}

export default LikedTouristSites;
