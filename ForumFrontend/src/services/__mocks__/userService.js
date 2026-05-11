import { vi } from 'vitest';

const mockUser = {
    Id: 'user1',
    Username: 'GhostTester',
    Email: 'ghost@tester.com',
    Score: 150,
    IsModerator: false,
    CreatedAt: new Date().toISOString()
};

export const getUserById = vi.fn().mockResolvedValue(mockUser);
export const getUserByEmail = vi.fn().mockResolvedValue(mockUser);
export const updateUsername = vi.fn().mockResolvedValue({ Success: true });
export const changePassword = vi.fn().mockResolvedValue({ Success: true });
export const deleteUser = vi.fn().mockResolvedValue({ Success: true });
export const getUserScore = vi.fn().mockResolvedValue(150);

export const getAllUsers = vi.fn().mockResolvedValue([
    mockUser,
    { ...mockUser, Id: 'user2', Username: 'ModUser', IsModerator: true }
]);

export const banUser = vi.fn().mockResolvedValue({ Success: true });
export const unbanUser = vi.fn().mockResolvedValue({ Success: true });