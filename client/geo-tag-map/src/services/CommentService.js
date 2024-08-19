import axios from "axios";
const BASE_URL = "https://localhost:44395/api/comment";

const getAccessToken = () => {
  return localStorage.getItem("access_token");
};

const CommentService = {
  fetchComments: async (currentPage, itemsPerPage, options) => {
    const token = getAccessToken();
    if (!token) {
      throw new Error("Token not found");
    }

    const { orderBy, sortOrder, isReported, username, dateCreated } = options;

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

    if (isReported) {
      params.isReported = isReported;
    }

    if (dateCreated) {
      params.dateCreated = dateCreated;
    }

    const queryString = new URLSearchParams(params).toString();
    const url = `${BASE_URL}/?${queryString}`;

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
        throw new Error("Failed to retrieve comment list");
      }
    } catch (error) {
      console.error("Error getting comment list: ", error.message);
      throw error;
    }
  },

  getCommentsByEventId: async (id) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/eventcomments/${id}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching comments:", error);
      throw error;
    }
  },
  getCommentsByUserId: async () => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/usercomments`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching comments:", error);
      throw error;
    }
  },
  getCommentsByTouristSiteId: async (id) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(
        `${BASE_URL}/touristsitecomments/${id}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      return response.data;
    } catch (error) {
      console.error("Error while fetching comments:", error);
      throw error;
    }
  },
  getCommentsByStoryId: async (id) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/storycomments/${id}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching comments:", error);
      throw error;
    }
  },
  getParentComments: async (eventId, storyId, touristSiteId) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const queryParams = new URLSearchParams();

      if (eventId) {
        queryParams.append("eventId", eventId);
      }
      if (storyId) {
        queryParams.append("storyId", storyId);
      }
      if (touristSiteId) {
        queryParams.append("touristSiteId", touristSiteId);
      }

      const url = `${BASE_URL}/parentcomments/?${queryParams.toString()}`;

      const response = await axios.get(url, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching comments:", error);
      throw error;
    }
  },
  getChildComments: async (id) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/childcomments/${id}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching comments:", error);
      throw error;
    }
  },
  updateCommentAsync: async (formData, commentId) => {
    const token = getAccessToken();
    if (!token) {
      console.error("Token not found");
      return;
    }
    try {
      const response = await axios.put(`${BASE_URL}/${commentId}`, formData, {
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
      console.error("Error updating user data: ", error.message);
      throw error;
    }
  },

  getCommentById: async (id) => {
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
      console.error("Error while fetching comment:", error);
      throw error;
    }
  },

  createComment: async (comment) => {
    try {
      const token = getAccessToken();

      if (!token) {
        console.error("Access token not found");
        return;
      }

      if (!comment.text) {
        console.error("Comment 'Text' is required");
        return;
      }

      const response = await axios.post(`${BASE_URL}`, comment, {
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

  deleteCommentAsync: async (commentId) => {
    const token = getAccessToken();
    if (!token) {
      console.error("Access token not found");
      return;
    }

    try {
      const response = await axios.delete(`${BASE_URL}/${commentId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.status === 200) {
        console.log("Comment has been deleted successfully");
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

export default CommentService;
