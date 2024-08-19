import React, { useState } from "react";
import userService from "../services/UserService";
import Background3DMap from "../components/Background3DMap";
import "../styles/LoginForm.css";

const LoginForm = () => {
  const [formData, setFormData] = useState({
    username: "",
    password: "",
  });

  const getUserRole = () => {
    const userStorageData = localStorage.getItem("user_data");

    if (userStorageData) {
      try {
        const userData = JSON.parse(userStorageData);
        const userRole = userData.userRole;
        return userRole;
      } catch (error) {
        console.error("Error parsing user data:", error);
      }
    } else {
      console.error("User data not found in local storage");
    }
  };

  const handleInputChange = (event) => {
    const { name, value } = event.target;
    setFormData((prevData) => ({
      ...prevData,
      [name]: value,
    }));
  };

  function logout() {
    localStorage.removeItem("access_token");
    localStorage.removeItem("user_data");
  }

  const handleSubmit = async (event) => {
    event.preventDefault();
    console.log("Submitted:", formData);
    try {
      await userService.loginUserAsync(formData.username, formData.password);
      console.log("Login successful");
      setFormData({
        username: "",
        password: "",
      });
      var user = await userService.getLoggedUserAsync();
      const userRole = getUserRole();

      if (userRole === "Admin") {
        window.location.href = "/admin";
      } else {
        window.location.href = "/home";
      }
    } catch (error) {
      console.error("Error logging in user: ", error);
    }
  };

  return (
    <div>
      <div className="disable-overlay-login"></div>
      <div className="container-login-form">
        <div className="row justify-content-center">
          <div className="col-md-6">
            <h2 className="text-center">Login</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label htmlFor="username">Username</label>
                <input
                  type="text"
                  className="form-control"
                  id="username"
                  name="username"
                  value={formData.username}
                  onChange={handleInputChange}
                  placeholder="Enter your username"
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="password">Password</label>
                <input
                  type="password"
                  className="form-control"
                  id="password"
                  name="password"
                  value={formData.password}
                  onChange={handleInputChange}
                  placeholder="Enter your password"
                  required
                />
              </div>
              <button type="submit" className="btn btn-primary btn-block">
                Login
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginForm;
