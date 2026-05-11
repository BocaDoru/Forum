import { render } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';

const AllTheProviders = ({ children }) => {
    return (
        <MemoryRouter>
            {children}
        </MemoryRouter>
    );
};

const customRender = (ui, options) =>
    render(ui, { wrapper: AllTheProviders, ...options });

export * from '@testing-library/react';
export { customRender as render };