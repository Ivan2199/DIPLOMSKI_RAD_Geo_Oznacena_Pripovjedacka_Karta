import React, { useEffect, useState } from "react";
import UserService from "../services/UserService";
import "../styles/UserManagement.css";

function UserManagement() {
  const [users, setUsers] = useState([]);
  const [pageSize, setPageSize] = useState(20);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalNumberOfUsers, setTotalNumberOfUsers] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [orderBy, setOrderBy] = useState("Username");
  const [sortOrder, setSortOrder] = useState("ASC");
  const [updatedUser, setUpdatedUser] = useState({});

  const getUsers = async () => {
    setLoading(true);
    setError(null);
    try {
      const options = { orderBy, sortOrder };
      const response = await UserService.getUserListAsync(
        pageSize,
        pageNumber,
        options
      );
      setUsers(response.List);
      setTotalNumberOfUsers(response.TotalSize);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    getUsers();
  }, [pageSize, pageNumber, orderBy, sortOrder]);

  const handleSetAsAdminUser = async (userId) => {
    try {
      console.log(userId);
      await UserService.updateUserToAdminAsync(userId, true, updatedUser);
      getUsers();
    } catch (error) {
      console.log("Error has occurred while updating an user", error);
    }
  };

  const handleDeleteUser = async (userId) => {
    try {
      await UserService.deleteUserAsync(userId);
      getUsers();
    } catch (error) {
      console.log("Error has occurred while deleting an user", error);
    }
  };

  return (
    <div className="user-management">
      <h1>User Management</h1>
      {error && <p className="error">{error}</p>}
      <div className="controls">
        <label>
          Order By:
          <select value={orderBy} onChange={(e) => setOrderBy(e.target.value)}>
            <option value="Username">Username</option>
            <option value="FirstName">FirstName</option>
            <option value="LastName">LastName</option>
          </select>
        </label>
        <label>
          Sort Order:
          <select
            value={sortOrder}
            onChange={(e) => setSortOrder(e.target.value)}
          >
            <option value="ASC">Ascending</option>
            <option value="DESC">Descending</option>
          </select>
        </label>
      </div>
      {loading ? (
        <p>Loading...</p>
      ) : (
        <>
          <table className="user-table">
            <thead>
              <tr>
                <th>Username</th>
                <th>FirstName</th>
                <th>LastName</th>
                <th>UserRole</th>
                <th>Email</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.Username}</td>
                  <td>{user.FirstName}</td>
                  <td>{user.LastName}</td>
                  <td>{user.UserRole.Type}</td>
                  <td>{user.Email}</td>
                  <td>
                    <div className="admin-user-action-buttons">
                      {user.UserRole.Type !== "Admin" && (
                        <div
                          className="set-as-admin-action-button"
                          onClick={() => handleSetAsAdminUser(user.Id)}
                        >
                          Set As Admin
                        </div>
                      )}
                      <div
                        className="delete-user-action-button"
                        onClick={() => handleDeleteUser(user.Id)}
                      >
                        Delete {user.UserRole.Type}
                      </div>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <div className="pagination">
            <button
              onClick={() => setPageNumber((prev) => Math.max(prev - 1, 1))}
              disabled={pageNumber === 1}
            >
              Previous
            </button>
            <span>
              Page {pageNumber} of {Math.ceil(totalNumberOfUsers / pageSize)}
            </span>
            <button
              onClick={() => setPageNumber((prev) => prev + 1)}
              disabled={pageNumber >= Math.ceil(totalNumberOfUsers / pageSize)}
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}

export default UserManagement;
