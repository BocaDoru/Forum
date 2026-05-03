import { apiClient } from './apiClient';

export const getUserById = async (userId) => {
    return apiClient(`/users/user=${userId}`); 
};

export const getUserByEmail = async (email) => {
    return apiClient(`/users/email=${email}`);
};