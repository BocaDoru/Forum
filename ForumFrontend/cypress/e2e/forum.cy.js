// cypress/e2e/forum.cy.js

describe('Forum Interactions', () => {
    const uniqueId = `${Date.now()}`;
    const testEmail = `forum${uniqueId}@example.com`;
    const testPassword = 'Password123!';

    before(() => {
        cy.request('POST', 'http://localhost:8080/api/users/register', {
            Username: `ForumTester${uniqueId}`,
            Email: testEmail,
            Password: testPassword
        }).then(() => {
            cy.visit('http://localhost:5173/login');
            cy.get('#emailInput').type(testEmail);
            cy.get('#passwordInput').type(`${testPassword}{enter}`);
            cy.url().should('include', '/home');
        });
    });

    beforeEach(() => {
        cy.intercept('GET', '**/questions/page=*').as('getQuestions');
        cy.intercept('POST', '**/questions/create').as('createQuestion');
        cy.intercept('GET', '**/questions/*').as('getQuestionDetail');
        
        Cypress.session.clearAllSavedSessions(); 
    });

    it('Loads the dashboard and displays questions', () => {
        cy.visit('http://localhost:5173/home');
        cy.wait('@getQuestions');
        cy.get('.sectionTitle').should('contain', 'All Questions');
    });

    it('Creates a new question successfully', () => {
        cy.visit('http://localhost:5173/home');
        cy.wait('@getQuestions');
        
        cy.contains('button', '+ Ask Question').click();
        cy.url().should('include', '/ask');

        cy.get('#titleInput').type('How does Cypress work?');
        cy.get('#descriptionInput').type('I am writing E2E tests and need to understand Cypress.');
        
        cy.get('#tagsInput').type('testing{enter}');
        
        cy.contains('button', 'Post').click();
        
        cy.wait('@createQuestion').its('response.statusCode').should('eq', 200);
        cy.url().should('include', '/home');
    });

    it('Can search and filter questions', () => {
        cy.visit('http://localhost:5173/home');
        cy.wait('@getQuestions');

        cy.get('.filterDropdown').select('Search by Title');
        cy.get('.searchInput').type('How does Cypress work?');
        cy.contains('button', 'Apply').click();

        cy.wait('@getQuestions');
        cy.get('.questionList').contains('How does Cypress work?').should('be.visible');
    });

    it('Can view question details and post a comment', () => {
        cy.visit('http://localhost:5173/home');
        cy.wait('@getQuestions');

        cy.get('.questionCard').first().click();
        cy.wait('@getQuestionDetail');

        cy.intercept('POST', '**/comments/create').as('postComment');
        cy.get('#commentInput').type('This is an automated test comment.');
        cy.contains('button', 'Post Comment').click();

        cy.wait('@postComment').its('response.statusCode').should('eq', 200);
        cy.get('.commentsList').contains('This is an automated test comment.').should('be.visible');
    });
});