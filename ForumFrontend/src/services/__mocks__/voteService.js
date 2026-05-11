import { vi } from 'vitest';

export const castVote = vi.fn().mockResolvedValue({ Success: true });

export const getVoteCount = vi.fn().mockResolvedValue(12);

export const getUserVote = vi.fn().mockResolvedValue({ 
    Exists: true, 
    IsDownvote: false 
});