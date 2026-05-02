import { apiClient } from './apiClient';

export const getQuestions = async (page = 1) => {
    return apiClient(`/questions/page=${page}`);
};

export const getQuestionsByTitle = async (title, page = 1) => {
    return apiClient(`/questions/title=${title}/page=${page}`);
};

export const getQuestionsByTag = async (tag, page = 1) => {
    return apiClient(`/questions/tag=${tag}/page=${page}`);
};

export const getQuestionsByAuthor = async (authorId, page = 1) => {
    return apiClient(`/questions/author=${authorId}/page=${page}`);
};

export const getQuestionVoteCount = async (questionId) => {
    return apiClient(`/questions/${questionId}/votecount`);
};

export const getTagByName = async (tagName) => {
    return apiClient(`/questions/tag=${tagName}`);
};

export const createTag = async (tagName) => {
    return apiClient('/questions/tag', {
        method: 'POST',
        body: tagName, 
        headers: { 'Content-Type': 'application/json' }
    });
};

export const askQuestion = async (title, text, imageFile, tagIds) => {
    const formData = new FormData();
    formData.append('Title', title);
    formData.append('Text', text);
    
    tagIds.forEach(id => {
        formData.append('TagIds', id);
    });

    if (imageFile) {
        formData.append('Image', imageFile);
    }

    return apiClient('/questions/create', {
        method: 'POST',
        body: formData
    });
};