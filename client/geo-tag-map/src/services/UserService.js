import axios from "axios";

const API_BASE_URL = "https://localhost:44395/api/user";

const getUserData = () => {
  const userStorageData = localStorage.getItem("user_data");

  if (userStorageData) {
    try {
      const userData = JSON.parse(userStorageData);
      const userId = userData.userId;
      return userId;
    } catch (error) {
      console.error("Error parsing user data:", error);
    }
  } else {
    console.error("User data not found in local storage");
  }
};

const getAccessToken = () => {
  return localStorage.getItem("access_token");
};
export const registerUserAsync = async (formData) => {
  try {
    const response = await axios.post(`${API_BASE_URL}/register`, formData);
    return response.data;
  } catch (error) {
    console.error("Error registering user: ", error);
    throw error;
  }
};

export const loginUserAsync = async (username, password) => {
  try {
    const loginData = new URLSearchParams();
    loginData.append("grant_type", "password");
    loginData.append("username", username);
    loginData.append("password", password);

    const response = await axios.post(`${API_BASE_URL}/login`, loginData);

    if (response.data) {
      const responseText = response.data;

      const accessTokenMatch = responseText.match(/"access_token":"([^"]+)"/);
      const accessToken = accessTokenMatch ? accessTokenMatch[1] : null;
      const userDataMatch = responseText.match(/"user":{([^}]+)}/);
      const userData = userDataMatch
        ? JSON.parse(`{${userDataMatch[1]}}`)
        : null;

      if (accessToken && userData) {
        localStorage.setItem("access_token", accessToken);
        localStorage.setItem("user_data", JSON.stringify(userData));
      } else {
        console.error(
          "Unable to extract access token or user data from the response."
        );
      }
    } else {
      console.error("Empty response data.");
    }
  } catch (error) {
    console.error("Error while logging in:", error);
  }
};

export const logoutUser = () => {
  localStorage.removeItem("access_token");
  localStorage.removeItem("user_data");
};

export const updateUserAsync = async (formData, isChangeToAdmin) => {
  const userId = getUserData();
  const token = getAccessToken();
  if (!token) {
    console.error("Token not found");
    return;
  }
  try {
    const response = await axios.put(
      `${API_BASE_URL}/${userId}/?isChangeToAdmin=${isChangeToAdmin}`,
      formData,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );

    if (response.data) {
      return response.data;
    } else {
      console.error("Empty response from the server.");
    }
  } catch (error) {
    console.error("Error updating user data: ", error.message);
    throw error;
  }
};

export const updateUserToAdminAsync = async (
  userId,
  isChangeToAdmin,
  formData
) => {
  const token = getAccessToken();
  if (!token) {
    console.error("Token not found");
    return;
  }
  try {
    console.log("sssss", userId);
    const response = await axios.put(
      `${API_BASE_URL}/${userId}/?isChangeToAdmin=${isChangeToAdmin}`,
      formData,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );

    if (response.data) {
      return response.data;
    } else {
      console.error("Empty response from the server.");
    }
  } catch (error) {
    console.error("Error updating user data: ", error.message);
    throw error;
  }
};

export const getLoggedUserAsync = async () => {
  const token = getAccessToken();

  if (!token) {
    console.error("Token not found");
    return;
  }

  try {
    const response = await axios.get(`${API_BASE_URL}/getloggeduser`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (response.data) {
      return response.data;
    } else {
      console.error("Empty response from the server.");
    }
  } catch (error) {
    console.error("Error getting user data: ", error.message);
    throw error;
  }
};

export const getUserAsync = async (id) => {
  const token = getAccessToken();

  if (!token) {
    console.error("Token not found");
    return;
  }

  try {
    const response = await axios.get(`${API_BASE_URL}/${id}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (response.data) {
      return response.data;
    } else {
      console.error("Empty response from the server.");
    }
  } catch (error) {
    console.error("Error getting user data: ", error.message);
    throw error;
  }
};

export const getUserListAsync = async (itemsPerPage, currentPage, options) => {
  const token = getAccessToken();
  if (!token) {
    throw new Error("Token not found");
  }

  const { orderBy, sortOrder, username, firstName, lastName } = options;

  const params = {
    pageNumber: currentPage,
    pageSize: itemsPerPage,
  };

  if (orderBy) {
    params.orderBy = orderBy;
  }

  if (sortOrder) {
    params.sortOrder = sortOrder;
  }

  if (username) {
    params.username = username;
  }

  if (firstName) {
    params.firstName = firstName;
  }

  if (lastName) {
    params.lastName = lastName;
  }

  const queryString = new URLSearchParams(params).toString();
  const url = `${API_BASE_URL}/?${queryString}`;

  try {
    const response = await axios.get(url, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (response.status === 200) {
      return response.data;
    } else {
      console.error("Request failed with status code: " + response.status);
      throw new Error("Failed to retrieve user list");
    }
  } catch (error) {
    console.error("Error getting user list: ", error.message);
    throw error;
  }
};
export const deleteUserAsync = async (userId) => {
  const token = getAccessToken();
  if (!token) {
    console.error("Token not found");
    return;
  }

  try {
    const response = await axios.delete(`${API_BASE_URL}/${userId}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (response.status === 200) {
      return response.data;
    } else {
      console.error("Request failed with status code: " + response.status);
      throw new Error("Failed to delete user");
    }
  } catch (error) {
    console.error("Error deleting user: ", error.message);
    throw error;
  }
};

export default {
  getUserData,
  registerUserAsync,
  loginUserAsync,
  logoutUser,
  updateUserAsync,
  updateUserToAdminAsync,
  getLoggedUserAsync,
  getUserAsync,
  getUserListAsync,
  deleteUserAsync,
};
