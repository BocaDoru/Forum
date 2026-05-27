import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '../../utils/test-utils';
import userEvent from '@testing-library/user-event';
import Register from '../Register';
import { registerUser } from '../../services/authService';

vi.mock('../../services/authService');

describe('Register Component', () => {
    beforeEach(() => {
        sessionStorage.clear();
    });

    it('renders the registration form correctly', () => {
        render(<Register />);
        expect(screen.getByRole('heading', { name: /create an account/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    });

    it('submits the form successfully', async () => {
        const user = userEvent.setup();
        render(<Register />);

        await user.type(screen.getByLabelText(/username/i), 'NewUser');
        await user.type(screen.getByLabelText(/email/i), 'new@user.com');
        await user.type(screen.getByLabelText(/password/i), 'securePass');
        
        
        await user.click(screen.getByRole('button', { name: /register/i }));

        await waitFor(() => {
            expect(registerUser).toHaveBeenCalledWith('NewUser', 'new@user.com', 'securePass');
        });
    });
});