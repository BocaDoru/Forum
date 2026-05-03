import { apiClient } from './apiClient';

export const getCommentsByQuestionId = async (questionId) => {
    return apiClient(`/comments/question/${questionId}`);
};