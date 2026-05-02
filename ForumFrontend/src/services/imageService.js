import { apiClient } from './apiClient';

export const getImageUrl = (imageId) => {
    return `${import.meta.env.VITE_API_BASE_URL}/images/${imageId}`;
};

export const uploadImage = async (file) => {
    const formData = new FormData();
    formData.append('file', file);

    return apiFormClient('/images/upload', formData);
};