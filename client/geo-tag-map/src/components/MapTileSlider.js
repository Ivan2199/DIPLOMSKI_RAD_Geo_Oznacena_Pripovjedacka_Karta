import React, { useState } from "react";
import "../styles/MapTileSlider.css";

function MapTileSlider({ slides }) {
  const [currentSlide, setCurrentSlide] = useState(0);

  const prevSlide = () => {
    setCurrentSlide((prevSlide) =>
      prevSlide === 0 ? slides.length - 1 : prevSlide - 1
    );
  };

  const goToSlide = (index) => {
    setCurrentSlide(index);
  };

  return (
    <div className="slideshow-container">
      {slides.map((slide, index) => (
        <div
          className={`mySlides fade ${index === currentSlide ? "show" : ""}`}
          key={index}
        >
          <div className="slide-content">{slide}</div>
        </div>
      ))}
      {slides.length > 1 && (
        <div className="dot-container">
          {slides.map((slide, index) => (
            <span
              key={index}
              className={`dot ${index === currentSlide ? "active" : ""}`}
              onClick={() => goToSlide(index)}
            ></span>
          ))}
        </div>
      )}
      {slides.length > 1 && (
        <a className="prev" onClick={prevSlide}>
          &#10094;
        </a>
      )}
    </div>
  );
}

export default MapTileSlider;
