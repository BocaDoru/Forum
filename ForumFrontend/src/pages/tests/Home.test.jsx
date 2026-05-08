import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '../../utils/test-utils';
import userEvent from '@testing-library/user-event';
import Home from '../Home';
import { getQuestions, getQuestionsByTitle } from '../../services/questionService';
import { getUserById } from '../../services/userService';

vi.mock('../../services/questionService');
vi.mock('../../services/userService');

describe('Home Component', () => {
    beforeEach(() => {
        localStorage.clear();
        localStorage.setItem('forum_user', JSON.stringify({ Id: 'user1', Username: 'GhostTester' }));
    });

    it('renders default questions on load', async () => {
        render(<Home />);
        
        await waitFor(() => {
            expect(getQuestions).toHaveBeenCalledWith(1);
            expect(screen.getByText('First Mock Question')).toBeInTheDocument();
            expect(screen.getByText('Second Mock Question')).toBeInTheDocument();
        });
    });

    it('applies the search filter', async () => {
        const user = userEvent.setup();
        render(<Home />);

        const searchInput = screen.getByPlaceholderText(/type to search/i);
        await user.type(searchInput, 'First');
        
        await user.click(screen.getByRole('button', { name: /apply/i }));

        await waitFor(() => {
            expect(getQuestionsByTitle).toHaveBeenCalledWith('First', 1);
        });
    });
});