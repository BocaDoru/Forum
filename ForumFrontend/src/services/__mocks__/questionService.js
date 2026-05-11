import { vi } from 'vitest';

const mockQuestionList = [
    { Id: 'q1', Title: 'First Mock Question', Text: 'Content 1', AuthorId: 'user1', Status: 0, VoteCount: 5, CreatedAt: new Date().toISOString() },
    { Id: 'q2', Title: 'Second Mock Question', Text: 'Content 2', AuthorId: 'user2', Status: 1, VoteCount: 10, CreatedAt: new Date().toISOString() }
];

export const getQuestions = vi.fn().mockResolvedValue(mockQuestionList);
export const getQuestionsByTitle = vi.fn().mockResolvedValue([mockQuestionList[0]]);
export const getQuestionsByTag = vi.fn().mockResolvedValue(mockQuestionList);
export const getQuestionsByAuthor = vi.fn().mockResolvedValue(mockQuestionList);

export const getQuestionVoteCount = vi.fn().mockResolvedValue(42);

export const getTagByName = vi.fn().mockResolvedValue({ Id: 'tag1', Name: 'react' });
export const getTagById = vi.fn().mockResolvedValue({ Id: 'tag1', Name: 'react' });
export const createTag = vi.fn().mockResolvedValue({ Id: 'tag2', Name: 'new-tag' });

export const askQuestion = vi.fn().mockResolvedValue({ Success: true, Id: 'new-q-id' });

export const getQuestionById = vi.fn().mockResolvedValue({
    Id: 'q1',
    Title: 'Detailed Mock Question',
    Text: 'This is the full description of the question for the detail page.',
    AuthorId: 'user1',
    Status: 0,
    TagIds: ['tag1'],
    ImageId: null,
    CreatedAt: new Date().toISOString()
});

export const deleteQuestion = vi.fn().mockResolvedValue({ Success: true });
export const editQuestion = vi.fn().mockResolvedValue({ Success: true });