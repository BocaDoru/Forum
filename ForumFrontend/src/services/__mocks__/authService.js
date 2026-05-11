import { vi } from 'vitest';

export const loginUser = vi.fn().mockResolvedValue({ 
    JwtToken: 'fake-jwt-token',
    User: { Id: 'user1', Username: 'GhostTester', Email: 'ghost@tester.com', IsModerator: false } 
});

export const registerUser = vi.fn().mockResolvedValue({ 
    JwtToken: 'fake-jwt-token',
    User: { Id: 'user2', Username: 'NewUser', Email: 'new@user.com', IsModerator: false }
});