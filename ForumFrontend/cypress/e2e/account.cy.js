// cypress/e2e/account.cy.js

describe('Account Settings', () => {
    const uniqueId = `${Date.now()}`;
    const testUsername = `User${uniqueId}`;
    const testEmail = `account${uniqueId}@example.com`;
    const testPassword = 'Password123!';

    before(() => {
        cy.request('POST', 'http://localhost:8080/api/users/register', { 
            Username: testUsername,
            Email: testEmail,
            Password: testPassword
        }).then(() => {
            cy.visit('http://localhost:5173/login');
            cy.get('#emailInput').type(testEmail);
            cy.get('#passwordInput').type(`${testPassword}{enter}`);
            cy.url().should('include', '/home');
        });
    });

    it('Updates the username successfully', () => {
        cy.intercept('PATCH', '**/users').as('updateUser');

        cy.visit('http://localhost:5173/account');

        const newUsername = `UpdatedName${uniqueId}`;
        
        cy.get('#newUsernameInput').clear().type(newUsername);
        cy.contains('button', 'Save').click();

        cy.wait('@updateUser').its('response.statusCode').should('eq', 200);
        cy.get('.successText').should('contain', 'Username updated successfully');
        
        cy.get('.profileName').should('contain', newUsername);
    });
});