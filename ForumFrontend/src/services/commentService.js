import { apiClient } from './apiClient';

export const getCommentsByQuestionId = async (questionId) => {
    return apiClient(`/comments/question/${questionId}`);
};

export const createComment = async (questionId, text, imageFile) => {
    const formData = new FormData();
    formData.append('QuestionId', questionId);
    formData.append('Text', text);
    if (imageFile) {
        formData.append('Image', imageFile);
    }

    return apiClient('/comments/create', {
        method: 'POST',
        body: formData
    });
};

export const deleteComment = async (commentId) => {
    return apiClient(`/comments/${commentId}`, {
        method: 'DELETE'
    });
};

export const editComment = async (commentId, text, imageId = "") => {
    return apiClient('/comments/edit', {
        method: 'PUT',
        body: {
            CommentId: commentId,
            Text: text,
            ImageId: imageId
        }
    });
};