import { apiClient } from './apiClient';

export const fetchImageBlob = async (imageId) => {
    const token = localStorage.getItem('forum_token');

    const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/images/${imageId}`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    if (!response.ok) {
        throw new Error('Failed to load image.');
    }

    const blob = await response.blob();
    return URL.createObjectURL(blob);
};

export const uploadImage = async (imageFile) => {
    const formData = new FormData();
    formData.append('Image', imageFile);

    return apiClient('/images/upload', {
        method: 'POST',
        body: formData
    });
};