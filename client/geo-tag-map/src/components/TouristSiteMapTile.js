import React, { useState, useEffect } from "react";
import Category from "./Category";
import "../styles/TouristSiteMapTile.css";

function TouristSiteMapTile(props) {
  return (
    <div className="tourist-site-map-tile">
      <div className="tourist-site-map-tile-cover-image"></div>
      <div className="tourist-site-map-tile-container">
        <div className="tourist-site-map-tile-information">
          <h2 className="tourist-site-map-tile-name">{props.name}</h2>
          {props.hoursOpen && (
            <div className="tourist-site-map-tile-hours-open">
              <span className="word-highlight">{props.hoursOpen}</span>
            </div>
          )}
          {props.siteCategories && props.siteCategories.length > 0 && (
            <div className="tourist-site-map-tile-categories">
              {props.siteCategories.map((siteCategory, index) => (
                <div key={index} className="tourist-site-map-tile-category">
                  <Category categoryId={siteCategory.CategoryId} />
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default TouristSiteMapTile;
