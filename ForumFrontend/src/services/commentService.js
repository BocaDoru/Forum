import { apiClient, apiFormClient  } from './apiClient';

export const getCommentsForQuestion = async (questionId) => {
    return apiClient(`/comments/question/${questionId}`, {
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

export const editComment = async (commentId, text, imageId) => {
    return apiClient('/comments/edit', {
        method: 'PUT',
        body: {
            CommentId: commentId,
            Text: text,
            ImageId: imageId
        },
    });
};

export const deleteComment = async (commentId) => {
    return apiClient(`/comments/${commentId}`, {
        method: 'DELETE',
    });
};