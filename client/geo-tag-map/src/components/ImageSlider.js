import React, { useState, useEffect } from "react";
import "../styles/ImageSlider.css";

const ImageSlider = ({ images }) => {
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentImageIndex((prevIndex) => (prevIndex + 1) % images.length);
    }, 5000);

    return () => clearInterval(interval);
  }, [images]);

  return (
    <div className="story-image-slider-container">
      {images.map((image, index) => (
        <img
          key={index}
          src={image.ImageData}
          alt={`Story Image ${index}`}
          className={index === currentImageIndex ? "slide-in" : "slide-out"}
        />
      ))}
    </div>
  );
};

export default ImageSlider;
