import { apiClient } from './apiClient';

export const castVote = async (postId, isQuestion, isDownvote) => {
    return apiClient('/votes/cast', {
        method: 'POST',
        body: {
            PostId: postId,
            IsQuestion: isQuestion,
            IsDownvote: isDownvote
        },
    });
};

export const getVoteCount = async (postId, isQuestion) => {
    return apiClient(`/votes/${postId}/${isQuestion}`, {
        method: 'GET',
    });
};