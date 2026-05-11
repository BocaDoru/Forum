import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '../../utils/test-utils';
import QuestionDetail from '../QuestionDetail';
import { getQuestionById } from '../../services/questionService';
import { getCommentsByQuestionId } from '../../services/commentService';

vi.mock('../../services/questionService');
vi.mock('../../services/commentService');
vi.mock('../../services/userService');
vi.mock('../../services/voteService');
vi.mock('../../services/imageService');

vi.mock('react-router-dom', async () => {
    const actual = await vi.importActual('react-router-dom');
    return {
        ...actual,
        useParams: () => ({ id: 'q1' })
    };
});

describe('QuestionDetail Component', () => {
    beforeEach(() => {
        localStorage.clear();
        localStorage.setItem('forum_user', JSON.stringify({ Id: 'user1', Username: 'GhostTester' }));
    });

    it('loads the question and comments', async () => {
        render(<QuestionDetail />);

        await waitFor(() => {
            expect(getQuestionById).toHaveBeenCalledWith('q1');
            expect(getCommentsByQuestionId).toHaveBeenCalledWith('q1');
            
            expect(screen.getByText('Detailed Mock Question')).toBeInTheDocument();
            expect(screen.getByText('This is the full description of the question for the detail page.')).toBeInTheDocument();
            
            expect(screen.getByText('This is a mocked comment.')).toBeInTheDocument();
        });
    });
});