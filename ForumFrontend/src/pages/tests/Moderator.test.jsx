import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '../../utils/test-utils';
import userEvent from '@testing-library/user-event';
import Moderator from '../Moderator';
import { getAllUsers, banUser, unbanUser } from '../../services/userService';

vi.mock('../../services/userService');

describe('Moderator Component', () => {
    beforeEach(() => {
        window.prompt = vi.fn().mockReturnValue('Violation of rules');
    });

    it('loads and renders the user table', async () => {
        render(<Moderator />);
        
        await waitFor(() => {
            expect(getAllUsers).toHaveBeenCalled();
            expect(screen.getByText('GhostTester')).toBeInTheDocument();
            expect(screen.getByText('ModUser')).toBeInTheDocument();
        });
    });

    it('allows a moderator to ban a user', async () => {
        const user = userEvent.setup();
        render(<Moderator />);

        await waitFor(() => screen.getByText('GhostTester'));
        
        const banButtons = screen.getAllByRole('button', { name: /ban/i });
        await user.click(banButtons[0]);

        await waitFor(() => {
            expect(window.prompt).toHaveBeenCalled();
            expect(banUser).toHaveBeenCalledWith('user1', 'Violation of rules');
        });
    });
});