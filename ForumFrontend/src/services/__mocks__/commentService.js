import { vi } from 'vitest';

export const getCommentsByQuestionId = vi.fn().mockResolvedValue([
    {
        Id: 'comment1',
        QuestionId: 'q1',
        AuthorId: 'user2',
        Text: 'This is a mocked comment.',
        CreatedAt: new Date().toISOString(),
        ImageId: null
    }
]);

export const editComment = vi.fn().mockResolvedValue({ Success: true });

export const deleteComment = vi.fn().mockResolvedValue({ Success: true });