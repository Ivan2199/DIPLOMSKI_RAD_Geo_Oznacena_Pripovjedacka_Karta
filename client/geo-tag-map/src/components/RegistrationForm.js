import React, { useState } from "react";
import userService from "../services/UserService";
import { Link } from "react-router-dom";
import "../styles/RegistrationForm.css";
import Background3DMap from "./Background3DMap";

const RegistrationForm = ({ user, setUser }) => {
  const [isRegistered, setIsRegistered] = useState(false);
  const [image, setImage] = useState(null);
  const [imagePreviewUrl, setImagePreviewUrl] = useState("");

  const handleRegistration = async (e) => {
    e.preventDefault();
    try {
      const imageData = await convertImageToData(image);
      const userWithImage = { ...user, image: imageData };
      const response = await userService.registerUserAsync(userWithImage);
      console.log("Registration successful:", response);
      setIsRegistered(true);
      setUser({
        username: "",
        firstName: "",
        lastName: "",
        email: "",
        newPassword: "",
      });
    } catch (error) {
      console.error("Error registering user: ", error);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setUser((prevUser) => ({
      ...prevUser,
      [name]: value,
    }));
  };

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    setImage(file);
    setImagePreviewUrl(URL.createObjectURL(file));
  };

  const convertImageToData = (imageFile) => {
    return new Promise((resolve, reject) => {
      if (!imageFile) {
        reject("No image file provided");
      }
      const reader = new FileReader();
      reader.readAsDataURL(imageFile);
      reader.onload = () => resolve(reader.result);
      reader.onerror = (error) => reject(error);
    });
  };

  return (
    <div>
      <Background3DMap />
      <div className="disable-overlay"></div>
      <div
        className={`container-registration-form ${
          imagePreviewUrl ? "with-image" : ""
        }`}
      >
        <form onSubmit={handleRegistration}>
          <h2>Registration</h2>
          <div className="form-content">
            <div
              className={`user-data-insert ${
                imagePreviewUrl ? "has-image" : ""
              }`}
            >
              <div className="mb-3">
                <label htmlFor="username" className="form-label">
                  Username
                </label>
                <input
                  type="text"
                  className="form-control"
                  id="username"
                  name="username"
                  placeholder="Enter your username"
                  value={user.username}
                  onChange={handleChange}
                />
              </div>
              <div className="mb-3">
                <label htmlFor="firstName" className="form-label">
                  First Name
                </label>
                <input
                  type="text"
                  className="form-control"
                  id="firstName"
                  name="firstName"
                  placeholder="Enter your first name"
                  value={user.firstName}
                  onChange={handleChange}
                />
              </div>
              <div className="mb-3">
                <label htmlFor="lastName" className="form-label">
                  Last Name
                </label>
                <input
                  type="text"
                  className="form-control"
                  id="lastName"
                  name="lastName"
                  placeholder="Enter your last name"
                  value={user.lastName}
                  onChange={handleChange}
                />
              </div>
              <div className="mb-3">
                <label htmlFor="email" className="form-label">
                  Email
                </label>
                <input
                  type="email"
                  className="form-control"
                  id="email"
                  name="email"
                  placeholder="Enter your email"
                  value={user.email}
                  onChange={handleChange}
                />
              </div>
              <div className="mb-3">
                <label htmlFor="newPassword" className="form-label">
                  Password
                </label>
                <input
                  type="password"
                  className="form-control"
                  id="newPassword"
                  name="newPassword"
                  placeholder="Enter your password"
                  value={user.newPassword}
                  onChange={handleChange}
                />
              </div>
              <div className="mb-3">
                <label htmlFor="image" className="form-label">
                  Profile Image
                </label>
                <input
                  type="file"
                  className="form-control"
                  id="image"
                  name="image"
                  accept="image/*"
                  onChange={handleImageChange}
                />
              </div>
              <div>
                <button type="submit" className="btn btn-primary">
                  Register
                </button>
                <p className="login-paragraph">
                  Already have an account? <Link to="/login">Go to login</Link>
                </p>
              </div>
            </div>
            {imagePreviewUrl && (
              <div className="profile-image-insert">
                <img
                  id="image-preview"
                  src={imagePreviewUrl}
                  alt="Profile Preview"
                />
              </div>
            )}
          </div>
          {isRegistered && (
            <div className="go-to-login">
              <Link className="login" to="/login">
                Go To Login
              </Link>
            </div>
          )}
        </form>
      </div>
    </div>
  );
};

export default RegistrationForm;
