import React, { useState } from "react";
import StoryService from "../services/StoryService";
import ReactDOM from "react-dom";
import "../styles/CreateStoryForm.css";

function CreateStoryForm({ longitude, latitude }) {
  const [story, setStory] = useState({ text: "", imageData: "" });
  const [error, setError] = useState(null);

  const handleInputChange = (e) => {
    setStory({ ...story, text: e.target.value });
  };

  const handlePhotoChange = (e) => {
    const file = e.target.files[0];
    const reader = new FileReader();

    reader.onloadend = () => {
      setStory((prevStory) => ({ ...prevStory, imageData: reader.result }));
    };

    if (file) {
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = async () => {
    try {
      const response = await StoryService.createStory(
        story,
        longitude,
        latitude
      );
      console.log(response);
      handleClose();
    } catch (error) {
      console.error("Error while adding story", error);
      setError("An error occurred while adding your story. Please try again.");
    }
  };

  const handleClose = () => {
    const formContainer = document.getElementById(
      "create-story-form-container"
    );
    ReactDOM.unmountComponentAtNode(formContainer);
    formContainer.style.display = "none";
  };

  return (
    <div id="create-story-container">
      <div className="create-story-form">
        <textarea
          value={story.text}
          onChange={handleInputChange}
          placeholder="Unesite priču..."
        />
        <input type="file" accept="image/*" onChange={handlePhotoChange} />
        {story.imageData && (
          <div className="photo-preview">
            <img src={story.imageData} alt="Preview" />
          </div>
        )}
        {error && <div className="error">{error}</div>}
        <div className="story-form-buttons">
          <button onClick={handleSubmit}>Objavi</button>
          <button onClick={handleClose}>Odustani</button>
        </div>
      </div>
    </div>
  );
}

export default CreateStoryForm;
