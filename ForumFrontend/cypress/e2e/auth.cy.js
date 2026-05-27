describe('Authentication Flow', () => {
    const uniqueId = `${Date.now()}-${Math.floor(Math.random() * 10000)}`;
    const testUsername = `TestUser${uniqueId}`;
    const testEmail = `test${uniqueId}@example.com`;
    const testPassword = 'SecurePassword123!';

    it('Registers a new user successfully', () => {
        cy.intercept('POST', '**/users/register').as('registerApi');

        cy.visit('http://localhost:5173/register');
        
        cy.get('#usernameInput').type(testUsername);
        cy.get('#emailInput').type(testEmail);
        cy.get('#passwordInput').type(`${testPassword}{enter}`);

        cy.wait('@registerApi').its('response.statusCode').should('eq', 200);
        cy.url().should('include', '/home');
    });

    it('Logs in the user successfully', () => {
        cy.intercept('POST', '**/users/auth').as('loginApi');
        cy.intercept('GET', '**/questions/page=*').as('getQuestions');

        cy.visit('http://localhost:5173/login');
        
        cy.get('#emailInput').type(testEmail);
        cy.get('#passwordInput').type(`${testPassword}{enter}`);

        cy.wait('@loginApi').its('response.statusCode').should('eq', 200);
        cy.url().should('include', '/home');
        cy.wait('@getQuestions');
        cy.contains('+ Ask Question').should('be.visible');
    });

    it('Shows an error on invalid login', () => {
        cy.intercept('POST', '**/users/auth').as('loginApi');

        cy.visit('http://localhost:5173/login');
        cy.get('#emailInput').type('wrong@email.com');
        cy.get('#passwordInput').type('WrongPassword!{enter}');

        cy.wait('@loginApi').its('response.statusCode').should('not.eq', 200);
        cy.get('.errorText').should('be.visible');
        cy.url().should('include', '/login');
    });
});