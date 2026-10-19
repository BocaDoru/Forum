import { apiClient } from './apiClient';

export const getUserById = async (userId) => {
    return apiClient(`/users/user=${userId}`);
};

export const getUserByEmail = async (email) => {
    return apiClient(`/users/email=${email}`);
};

export const updateUsername = async (username) => {
    return apiClient('/users', {
        method: 'PATCH',
        body: username,
    });
};

export const changePassword = async (oldPassword, newPassword) => {
    return apiClient('/users/password', {
        method: 'PATCH',
        body: {
            OldPassword: oldPassword,
            NewPassword: newPassword
        },
    });
};

export const deleteUser = async () => {
    return apiClient('/users', {
        method: 'DELETE',
    });
};

export const getUserScore = async (userId) => {
    return apiClient(`/users/score/${userId}`, {
        method: 'GET',
    });
};

export const getAllUsers = async () => {
    return apiClient('/users/all');
};

export const banUser = async (userId, reason) => {
    return apiClient('/users/ban', {
        method: 'POST',
        body: {
            UserId: userId,
            Reason: reason
        },
    });
};

export const unbanUser = async (userId) => {
    return apiClient('/users/unban', {
        method: 'POST',
        body: userId,
    });
};
