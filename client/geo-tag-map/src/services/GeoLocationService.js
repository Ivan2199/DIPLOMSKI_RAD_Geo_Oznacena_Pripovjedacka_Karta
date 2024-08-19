import axios from "axios";
const BASE_URL = "https://localhost:44395/api/geolocation";

const getAccessToken = () => {
  return localStorage.getItem("access_token");
};

const GeoLocationService = {
  fetchGeoLocations: async () => {
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
      console.error("Error while fetching GeoLocations:", error);
      throw error;
    }
  },

  getGeoLocationById: async (id) => {
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
      console.error("Error while fetching GeoLocation:", error);
      throw error;
    }
  },
  getGeoLocationByLocationId: async (id) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/locationid/${id}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching GeoLocation:", error);
      throw error;
    }
  },
};

export default GeoLocationService;
