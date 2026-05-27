import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '../../utils/test-utils';
import userEvent from '@testing-library/user-event';
import Account from '../Account';
import { updateUsername, changePassword, deleteUser } from '../../services/userService';

vi.mock('../../services/userService');

describe('Account Component', () => {
    beforeEach(() => {
        sessionStorage.clear();
        sessionStorage.setItem('forum_user', JSON.stringify({ Id: 'user1', Username: 'GhostTester', Email: 'ghost@tester.com' }));
        window.confirm = vi.fn().mockReturnValue(true); 
    });

    it('renders user details from sessionStorage', () => {
        render(<Account />);
        expect(screen.getByText('GhostTester')).toBeInTheDocument();
        expect(screen.getByText('ghost@tester.com')).toBeInTheDocument();
    });

    it('submits a username update', async () => {
        const user = userEvent.setup();
        render(<Account />);

        const usernameInput = screen.getByLabelText(/new username/i);
        await user.clear(usernameInput);
        await user.type(usernameInput, 'NewGhostName');
        
        const saveButtons = screen.getAllByRole('button', { name: /(save|update)/i });
        await user.click(saveButtons[0]);

        await waitFor(() => {
            expect(updateUsername).toHaveBeenCalledWith('NewGhostName');
        });
    });

    it('calls delete user API when confirmed', async () => {
        const user = userEvent.setup();
        render(<Account />);

        await user.click(screen.getByRole('button', { name: /delete account/i }));

        await waitFor(() => {
            expect(window.confirm).toHaveBeenCalled();
            expect(deleteUser).toHaveBeenCalled();
            expect(sessionStorage.getItem('forum_token')).toBeNull();
        });
    });
});