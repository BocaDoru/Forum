import {apiClient} from './apiClient';

export const getCommentsByQuestionId = async (questionId) => {
    return apiClient(`/comments/question/${questionId}`);
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

