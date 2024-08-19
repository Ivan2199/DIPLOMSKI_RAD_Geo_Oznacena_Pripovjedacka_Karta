export const getUserRole = () => {
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
  return null;
};
