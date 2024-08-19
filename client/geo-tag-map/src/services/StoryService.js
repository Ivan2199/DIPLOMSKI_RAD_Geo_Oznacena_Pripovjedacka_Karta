import axios from "axios";
const BASE_URL = "https://localhost:44395/api/story";

const getAccessToken = () => {
  return localStorage.getItem("access_token");
};

const StoryService = {
  fetchStories: async (options) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const queryParams = new URLSearchParams();

      if (options.orderBy) queryParams.append("orderBy", options.orderBy);
      if (options.sortOrder) queryParams.append("sortOrder", options.sortOrder);
      if (options.pageSize) queryParams.append("pageSize", options.pageSize);
      if (options.pageNumber)
        queryParams.append("pageNumber", options.pageNumber);
      if (options.isReported)
        queryParams.append("isReported", options.isReported);

      const urlWithParams = `${BASE_URL}?${queryParams.toString()}`;
      const response = await axios.get(urlWithParams, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      return response.data;
    } catch (error) {
      console.error("Error while fetching story:", error);
      throw error;
    }
  },

  fetchStoriesByUserId: async () => {
    try {
      const token = getAccessToken();

      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/user`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      if (response.data) {
        console.log("Server Response:", response.data);
        return response.data;
      } else {
        console.error("Empty response from the server.");
      }
    } catch (error) {
      console.error("Error while fetching stories:", error);
      throw error;
    }
  },

  updateStoryAsync: async (formData, storyId) => {
    const token = getAccessToken();
    if (!token) {
      console.error("Token not found");
      return;
    }
    console.log(formData);
    try {
      const response = await axios.put(`${BASE_URL}/${storyId}`, formData, {
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
      console.error("Error updating story data: ", error.message);
      throw error;
    }
  },

  getStoryById: async (id) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/${id}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching story:", error);
      throw error;
    }
  },

  createStory: async (story, longitude, latitude) => {
    try {
      const token = getAccessToken();

      if (!token) {
        console.error("Access token not found");
        return;
      }

      if (!story.text) {
        console.error("Story 'Text' is required");
        return;
      }

      const url = `${BASE_URL}?longitude=${longitude}&latitude=${latitude}`;

      const response = await axios.post(url, story, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error creating story:", error);
      throw error;
    }
  },

  deleteStoryAsync: async (storyId) => {
    const token = getAccessToken();
    if (!token) {
      console.error("Access token not found");
      return;
    }

    try {
      const response = await axios.delete(`${BASE_URL}/${storyId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.status === 200) {
        console.log("Story has been deleted successfully");
        return response.data;
      } else {
        console.error("Request failed with status code: " + response.status);
        throw new Error("Failed to delete user");
      }
    } catch (error) {
      console.error("Error deleting user: ", error.message);
      throw error;
    }
  },
};

export default StoryService;
