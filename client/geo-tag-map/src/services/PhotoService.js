import axios from "axios";
const BASE_URL = "https://localhost:44395/api/photo";

const getAccessToken = () => {
  return localStorage.getItem("access_token");
};

const PhotoService = {
  fetchPhotos: async () => {
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
      console.error("Error while fetching photos:", error);
      throw error;
    }
  },
  fetchPhotosByTouristSiteId: async (id) => {
    try {
      const token = getAccessToken();

      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/touristsite/${id}`, {
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
      console.error("Error while fetching photos:", error);
      throw error;
    }
  },
  fetchPhotosByStoryId: async (id) => {
    try {
      const token = getAccessToken();

      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/story/${id}`, {
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
      console.error("Error while fetching photos:", error);
      throw error;
    }
  },
  getPhotoById: async (id) => {
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
      console.error("Error while fetching photo:", error);
      throw error;
    }
  },

  addPhoto: async (photo) => {
    try {
      const token = getAccessToken();

      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.post(`${BASE_URL}`, photo, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error adding photo:", error);
      throw error;
    }
  },

  deletePhotoAsync: async (photoId) => {
    const token = getAccessToken();
    if (!token) {
      console.error("Access token not found");
      return;
    }

    try {
      const response = await axios.delete(`${BASE_URL}/${photoId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.status === 200) {
        console.log("Photo has been deleted successfully");
        return response.data;
      } else {
        console.error("Request failed with status code: " + response.status);
        throw new Error("Failed to delete photo");
      }
    } catch (error) {
      console.error("Error deleting photo: ", error.message);
      throw error;
    }
  },
};

export default PhotoService;
