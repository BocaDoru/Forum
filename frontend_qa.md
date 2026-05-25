# Frontend Questions and Answers

This document compiles the recent questions you asked about the frontend and their respective explanations.

---

### Q1: Walk me through the frontend and explain how everything comes together (specifically how questions/comments are posted, edited, and how votes are shown).
*(This was initially covered in the `frontend_architecture.md` artifact, but here is the summary).*
The frontend is built in React and uses an `apiClient.js` to communicate with the backend. 
- **Posting/Editing:** Uses `FormData` to bundle text, tags, and images to send POST/PUT requests to the backend. Comments use inline editing where text flips to a `<textarea>`, while questions use a dedicated page (`AskQuestion.jsx`).
- **Voting:** The frontend fetches the user's specific vote (`getUserVote`), maps it to a number (1, -1, 0), and uses that number to dynamically add a CSS class (`active-upvote`) to the arrow buttons, turning them orange.

---

### Q2: How are all the comments deleted from a question if I delete that question?
Because you wanted to handle cascading deletes without changing the backend code, the React frontend handles it manually:
1. It checks if there are any comments loaded on the screen.
2. It loops through every comment and fires off a `deleteComment(id)` request for each one simultaneously (using `Promise.all`).
3. Only after all comments are successfully deleted does it send the request to delete the question itself.

---

### Q3: What are the `.jsx` files?
JSX stands for **JavaScript XML**. In React, `.jsx` files are the visual building blocks (components and pages) where you write HTML-like code directly inside JavaScript. 
Examples: `Home.jsx` (the feed), `QuestionDetail.jsx` (single question page), and `AskQuestion.jsx` (the posting form).

---

### Q4: And what are the `.js` files?
The `.js` files contain pure logic, data processing, and background tasks without any visual UI (HTML).
In your project, these are mostly in the `src/services/` folder. They act as "messengers" to the backend (e.g., `questionService.js`, `apiClient.js`), keeping your visual `.jsx` files clean.

---

### Q5: How do I call this `.js` logic in the `.jsx` files?
It's a two-step process:
1. **Export:** The `.js` file must use the `export` keyword before its functions.
2. **Import:** The `.jsx` file must `import` that function at the very top (e.g., `import { getQuestions } from "../services/questionService.js";`).
Since these functions talk to the backend, they take time, so you must use `await` when calling them to wait for the backend's response.

---

### Q6: How is a question posted?
1. The user fills out the form in `AskQuestion.jsx`.
2. Upon clicking "Post", the frontend processes the tags. It checks if they exist in the DB; if not, it creates them instantly to get their Database IDs.
3. It bundles the Title, Text, Image file, and Tag IDs into a digital box called a `FormData` object.
4. It sends this `FormData` box to the backend via `apiClient`, which automatically attaches your secret login token (`Authorization: Bearer <token>`).

---

### Q7: Walk me through an example of the entire flow.
1. Alice types "How to reverse an array?", adds tags "javascript" and "arrays", and clicks "Post".
2. The frontend asks the backend for the ID of "javascript" (gets ID 12). It finds "arrays" doesn't exist, tells the backend to create it, and gets ID 45.
3. The frontend creates a `FormData` object, packing Title, Text, and TagIds (12 and 45).
4. `apiClient.js` stamps Alice's secret token onto the package.
5. The request is sent to `/questions/create`. 
6. The backend verifies the token, saves the data to the database, and replies with "Success".
7. The frontend immediately redirects Alice to the `/home` page.

---

### Q8: How is a question edited?
Editing re-uses the exact same `AskQuestion.jsx` page in a special "Edit Mode".
1. `useEffect` fetches the existing question data from the backend and pre-fills the text boxes and tags.
2. The user makes changes and clicks "Save Changes".
3. The frontend processes any new tags just like before. If a *new* image was attached, it uploads the image first to get a new `ImageId`.
4. It sends a `PUT` request with standard JSON (Title, Text, TagIds, Status, ImageId) to `/questions/{id}`.
5. On success, it redirects back to the question detail page.

---

### Q9: The router navigates to `/edit-question/123`. How does it know that the page that needs to be displayed is the AskQuestion page?
This mapping is handled by a library called `react-router-dom` inside your `App.jsx` file. 
It contains a list of routes, specifically: `<Route path="/edit-question/:id" element={<AskQuestion />} />`. 
When the URL matches that pattern, React Router instantly loads the `<AskQuestion />` component.

---

### Q10: How does the deletion of a question work?
1. **Permissions:** The "Delete" button only renders if you are the author or a Moderator.
2. **Safety:** Clicking it shows a browser confirmation popup (`window.confirm`).
3. **Cleanup:** If confirmed, the frontend deletes all associated comments one-by-one.
4. **Execution:** It then calls `deleteQuestion(id)` which sends a `DELETE` request to the backend.
5. **Redirect:** The user is sent back to `/home`.

---

### Q11: How does the coloring of the vote work?
1. The backend returns your specific vote status (e.g., `{ IsDownvote: false }`).
2. The helper function `mapVoteToState` converts this to `1` (Upvote), `-1` (Downvote), or `0` (Neutral).
3. This number is stored in React State (e.g., `comment.UserVote`).
4. The HTML/JSX button uses a dynamic class: ``className={`voteArrow ${comment.UserVote === 1 ? 'active-upvote' : ''}`}``.
5. Your CSS file says `.active-upvote { color: #ff8b60; }`. So when the state is `1`, the class is applied, and the arrow turns orange!
