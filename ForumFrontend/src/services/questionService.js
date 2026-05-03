import { apiClient, apiFormClient } from './apiClient';

export const getQuestions = async (page) => {
    return apiClient(`/questions/page=${page}`, {
        method: 'GET',
    });
};

export const getQuestionById = async (questionId) => {
    return apiClient(`/questions/${questionId}`, {
        method: 'GET',
    });
};

export const getQuestionsByAuthor = async (authorId, page) => {
    return apiClient(`/questions/author/page=${page}`, {
        method: 'GET',
        body: authorId,
    });
};

export const getQuestionsByTag = async (tag, page) => {
    return apiClient(`/questions/tag/page=${page}`, {
        method: 'GET',
        body: tag,
    });
};

export const getQuestionsByTitle = async (title, page) => {
    return apiClient(`/questions/title/page=${page}`, {
        method: 'GET',
        body: title,
    });
};

export const getQuestionVoteCount = async (questionId) => {
    return apiClient(`/questions/${questionId}/votecount`, {
        method: 'GET',
    });
};

export const getTagByName = async (tag) => {
    return apiClient(`/questions/tag=${tag}`, {
        method: 'GET',
    });
};

export const createComment = async (questionId, text, image) => {
    const formData = new FormData();
    formData.append('QuestionId', questionId);
    formData.append('Text', text);
    if (image) formData.append('Image', image);

    return apiFormClient('/comments/create', formData);
};

export const editQuestion = async (questionId, title, text, imageId, tagIds, status) => {
    return apiClient(`/questions/${questionId}`, {
        method: 'PUT',
        body: {
            Title: title,
            Text: text,
            ImageId: imageId,
            TagIds: tagIds,
            Status: status
        },
    });
};

export const deleteQuestion = async (questionId) => {
    return apiClient(`/questions/${questionId}`, {
        method: 'DELETE',
    });
};

export const createTag = async (name) => {
    return apiClient('/questions/tag', {
        method: 'POST',
        body: name,
    });
};

export const solveQuestion = async (questionId) => {
    return apiClient('/questions/solve', {
        method: 'POST',
        body: questionId,
    });
};