import React, { useEffect, useState, useCallback } from "react";
import { useParams, Link } from "react-router-dom";
import { useLocation } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faChevronRight,
  faChevronLeft,
} from "@fortawesome/free-solid-svg-icons";
import PhotoService from "../services/PhotoService";
import "../styles/Gallery.css";

const Gallery = () => {
  const { id } = useParams();
  const [formattedImages, setFormattedImages] = useState([]);
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  const location = useLocation();
  const { imageOf } = location.state || {};

  const loadImages = useCallback(async () => {
    try {
      let response;
      if (imageOf === "TouristSite") {
        response = await PhotoService.fetchPhotosByTouristSiteId(id);
      } else if (imageOf === "Story") {
        response = await PhotoService.fetchPhotosByStoryId(id);
      }
      console.log(response);
      const formatted = response.map((photo) => ({
        id: photo.id,
        imageUrl:
          photo.ImageData || `${photo.ImagePrefix}original${photo.ImageSuffix}`,
      }));
      setFormattedImages(formatted);
    } catch (error) {
      console.error("Error fetching photos:", error);
    }
  }, [id, imageOf]);

  useEffect(() => {
    loadImages();
  }, [loadImages]);

  const nextSlide = () => {
    setCurrentImageIndex((prevIndex) =>
      prevIndex < formattedImages.length - 1 ? prevIndex + 1 : 0
    );
  };

  const prevSlide = () => {
    setCurrentImageIndex((prevIndex) =>
      prevIndex > 0 ? prevIndex - 1 : formattedImages.length - 1
    );
  };

  const getStyles = (index) => {
    if (currentImageIndex === index)
      return {
        opacity: 1,
        transform: "translateX(0px) translateZ(0px) rotateY(0deg)",
        zIndex: 10,
      };
    else if (currentImageIndex - 1 === index)
      return {
        opacity: 0.6,
        transform: "translateX(-240px) translateZ(-400px) rotateY(35deg)",
        zIndex: 9,
      };
    else if (currentImageIndex + 1 === index)
      return {
        opacity: 0.6,
        transform: "translateX(240px) translateZ(-400px) rotateY(-35deg)",
        zIndex: 9,
      };
    else if (currentImageIndex - 2 === index)
      return {
        opacity: 0.3,
        transform: "translateX(-480px) translateZ(-500px) rotateY(35deg)",
        zIndex: 8,
      };
    else if (currentImageIndex + 2 === index)
      return {
        opacity: 0.3,
        transform: "translateX(480px) translateZ(-500px) rotateY(-35deg)",
        zIndex: 8,
      };
    else if (index < currentImageIndex - 2)
      return {
        opacity: 0,
        transform: "translateX(-480px) translateZ(-500px) rotateY(35deg)",
        zIndex: 7,
      };
    else if (index > currentImageIndex + 2)
      return {
        opacity: 0,
        transform: "translateX(480px) translateZ(-500px) rotateY(-35deg)",
        zIndex: 7,
      };
  };

  if (formattedImages.length === 0) {
    return <div>Loading...</div>;
  }

  return (
    <div className="gallery">
      <div className="slideC">
        {formattedImages.map((image, index) => (
          <div
            key={image.id}
            className="slide"
            style={{
              ...getStyles(index),
            }}
          >
            <img
              src={image.imageUrl}
              alt={image.title}
              className="slide-image"
            />
          </div>
        ))}
      </div>

      <div className="btns">
        <FontAwesomeIcon
          className="btn"
          onClick={prevSlide}
          icon={faChevronLeft}
          color="#fff"
          size="2x"
        />
        <FontAwesomeIcon
          className="btn"
          onClick={nextSlide}
          icon={faChevronRight}
          color="#fff"
          size="2x"
        />
      </div>
    </div>
  );
};

export default Gallery;
