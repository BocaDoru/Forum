import { apiClient } from './apiClient';

export const loginUser = async (email, password) => {
    return apiClient('/users/auth', {
        method: 'POST',
        body: { email, password },
    });
};

export const registerUser = async (username, email, password, isModerator) => {
    return apiClient('/users/register', {
        method: 'POST',
        body: { 
            Email: email, 
            Username: username, 
            Password: password, 
            IsModerator: isModerator
        },
    });
};