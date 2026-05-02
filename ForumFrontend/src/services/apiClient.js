const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const apiClient = async (endpoint, { method = 'GET', body, ...customConfig } = {}) => {
    const token = localStorage.getItem('forum_token');

    const headers = {
        'Content-Type': 'application/json',
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const config = {
        method,
        headers: {
            ...headers,
            ...customConfig.headers,
        },
    };

    if (body) {
        config.body = JSON.stringify(body);
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`, config);

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        
        let errorMessage = "An unexpected error occurred.";

        if (errorData.Errors && Object.keys(errorData.Errors).length > 0) {
            const firstErrorCategory = Object.keys(errorData.Errors)[0];
            errorMessage = errorData.Errors[firstErrorCategory][0];
        } 
        else if (errorData.Message) {
            errorMessage = errorData.Message;
        } 
        else {
            errorMessage = `Request failed with status ${response.status}`;
        }

        throw new Error(errorMessage);
    }

    return response.json();
};

export const apiFormClient = async (endpoint, formData) => {
    const token = localStorage.getItem('forum_token');

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
        },
        body: formData,
    });

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        
        let errorMessage = "An unexpected error occurred.";
        if (errorData.Errors && Object.keys(errorData.Errors).length > 0) {
            const firstErrorCategory = Object.keys(errorData.Errors)[0];
            errorMessage = errorData.Errors[firstErrorCategory][0];
        } else if (errorData.Message) {
            errorMessage = errorData.Message;
        } else {
            errorMessage = `Request failed with status ${response.status}`;
        }

        throw new Error(errorMessage);
    }

    return response.json();
};