import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '../../utils/test-utils';
import userEvent from '@testing-library/user-event';
import Login from '../Login';
import { loginUser } from '../../services/authService';

vi.mock('../../services/authService');

describe('Login Component', () => {
    beforeEach(() => {
        localStorage.clear();
    });

    it('renders the login form correctly', () => {
        render(<Login />);
        expect(screen.getByRole('heading', { name: /login/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument();
    });

    it('submits the form and saves tokens on success', async () => {
        const user = userEvent.setup();
        render(<Login />);

        await user.type(screen.getByLabelText(/email/i), 'ghost@tester.com');
        await user.type(screen.getByLabelText(/password/i), 'password123');
        await user.click(screen.getByRole('button', { name: /login/i }));

        await waitFor(() => {
            expect(loginUser).toHaveBeenCalledWith('ghost@tester.com', 'password123');
            expect(localStorage.getItem('forum_token')).toBe('fake-jwt-token');
        });
    });
});