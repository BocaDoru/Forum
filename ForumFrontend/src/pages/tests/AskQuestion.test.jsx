import { render, screen, waitFor } from '../../utils/test-utils';
import AskQuestion from '../AskQuestion';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { askQuestion } from '../../services/questionService';

vi.mock('../../services/questionService');
vi.mock('../../services/imageService');

describe('AskQuestion Component', () => {
    
    it('renders the creation form correctly for a new question', () => {
        render(<AskQuestion />);

        expect(screen.getByRole('heading', { name: /ask a question/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /post/i })).toBeInTheDocument();
    });

    it('allows a user to fill out the form and submit a question', async () => {
        const user = userEvent.setup();
        render(<AskQuestion />);

        const titleInput = screen.getByLabelText(/title/i);
        const descInput = screen.getByLabelText(/description/i);
        const tagsInput = screen.getByLabelText(/tags/i);
        const postButton = screen.getByRole('button', { name: /post/i });

        await user.type(titleInput, 'How do I center a div?');
        await user.type(descInput, 'I have tried everything but it stays on the left.');
        
        await user.type(tagsInput, 'css{Enter}');
        
        await user.click(postButton);

        await waitFor(() => {
            expect(askQuestion).toHaveBeenCalledWith(
                'How do I center a div?', 
                'I have tried everything but it stays on the left.', 
                null,
                ['tag1']
            );
        });
    });
});