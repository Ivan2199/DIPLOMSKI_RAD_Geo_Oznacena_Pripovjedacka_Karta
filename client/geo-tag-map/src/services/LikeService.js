import axios from "axios";
const BASE_URL = "https://localhost:44395/api/like";

const getAccessToken = () => {
  return localStorage.getItem("access_token");
};

const LikeService = {
  getLikes: async () => {
    try {
      const token = getAccessToken();

      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}`, {
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
      console.error("Error while fetching likes:", error);
      throw error;
    }
  },

  getCommentLike: async (commentId) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/comment/${commentId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching like:", error);
      throw error;
    }
  },

  getEventLike: async (eventId) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/event/${eventId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching like:", error);
      throw error;
    }
  },

  getTouristSiteLike: async (touristSiteId) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(
        `${BASE_URL}/touristsite/${touristSiteId}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      return response.data;
    } catch (error) {
      console.error("Error while fetching like:", error);
      throw error;
    }
  },

  getStoryLike: async (storyId) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/story/${storyId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching like:", error);
      throw error;
    }
  },

  getCommentLikes: async (commentId) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(
        `${BASE_URL}/commentlikes/${commentId}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      return response.data;
    } catch (error) {
      console.error("Error while fetching likes:", error);
      throw error;
    }
  },

  getUserLikes: async (likesOf) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(
        `${BASE_URL}/userlikes?likesOf=${likesOf}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      return response.data;
    } catch (error) {
      console.error("Error while fetching likes:", error);
      throw error;
    }
  },
  getEventLikes: async (eventId) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/eventlikes/${eventId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching likes:", error);
      throw error;
    }
  },
  getTouristSiteLikes: async (touristSiteId) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(
        `${BASE_URL}/touristsitelikes/${touristSiteId}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      return response.data;
    } catch (error) {
      console.error("Error while fetching likes:", error);
      throw error;
    }
  },
  getStoryLikes: async (storyId) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/storylikes/${storyId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching likes:", error);
      throw error;
    }
  },
  addLike: async (like) => {
    try {
      const token = getAccessToken();

      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.post(`${BASE_URL}`, like, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error creating comment:", error);
      throw error;
    }
  },

  deleteLikeAsync: async (likeId) => {
    const token = getAccessToken();
    if (!token) {
      console.error("Access token not found");
      return;
    }

    try {
      const response = await axios.delete(`${BASE_URL}/${likeId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.status === 200) {
        console.log("Like has been deleted successfully");
        return response.data;
      } else {
        console.error("Request failed with status code: " + response.status);
        throw new Error("Failed to delete like");
      }
    } catch (error) {
      console.error("Error deleting like: ", error.message);
      throw error;
    }
  },
};

export default LikeService;
