import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getTagByName, createTag, askQuestion } from '../services/forumService';
import '../style/AskQuestion.css';

export default function AskQuestion() {
    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [image, setImage] = useState(null);
    
    const [currentTagInput, setCurrentTagInput] = useState('');
    const [tags, setTags] = useState([]);
    
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();

    const handleTagKeyDown = (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            const newTag = currentTagInput.trim().toLowerCase();
            
            if (newTag && !tags.includes(newTag)) {
                setTags([...tags, newTag]);
            }
            setCurrentTagInput('');
        }
    };

    const removeTag = (tagToRemove) => {
        setTags(tags.filter(tag => tag !== tagToRemove));
    };

    const handleFileChange = (e) => {
        if (e.target.files && e.target.files[0]) {
            setImage(e.target.files[0]);
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (tags.length === 0) {
            setError("You must add at least one tag.");
            return;
        }

        setIsLoading(true);

        try {
            const tagIds = [];
            
            for (const tagName of tags) {
                try {
                    const tagData = await getTagByName(tagName);
                    tagIds.push(tagData.Id); 
                } catch (err) {
                    const newTagData = await createTag(tagName);
                    tagIds.push(newTagData.Id); 
                }
            }

            await askQuestion(title, description, image, tagIds);
            
            navigate('/home');

        } catch (err) {
            setError(err.message || "An error occurred while posting your question.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="pageContainer">
            <header className="header">
                <h1 className="logo">
                    F<span style={{ color: '#ff9800' }}>&</span>rum
                </h1>
                <button onClick={() => navigate('/login')} className="logoutButton">LogOut</button>
            </header>

            <main className="askMainContent">
                <h2 className="pageTitle">Ask a Question</h2>

                <div className="askCard">
                    {error && <div className="errorText">{error}</div>}

                    <form onSubmit={handleSubmit} className="askForm">
                        <div className="inputGroup">
                            <label className="label">Title</label>
                            <input 
                                type="text" 
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                                className="input" 
                                required 
                            />
                        </div>

                        <div className="inputGroup">
                            <label className="label">Description</label>
                            <textarea 
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                                className="input textarea" 
                                rows="6"
                                required 
                            />
                        </div>

                        <div className="inputGroup">
                            <label className="label">Attach Image</label>
                            <div className="fileInputWrapper">
                                <input 
                                    type="file" 
                                    accept="image/*"
                                    onChange={handleFileChange}
                                    className="fileInput" 
                                />
                            </div>
                        </div>

                        <div className="inputGroup">
                            <label className="label">Tags</label>
                            <span className="tagHelpText">Add at least one tag. Press enter to confirm each tag.</span>
                            
                            <div className="tagInputContainer">
                                {tags.map(tag => (
                                    <span key={tag} className="tagBadge">
                                        {tag}
                                        <button type="button" onClick={() => removeTag(tag)} className="tagRemoveBtn">x</button>
                                    </span>
                                ))}
                                <input 
                                    type="text" 
                                    value={currentTagInput}
                                    onChange={(e) => setCurrentTagInput(e.target.value)}
                                    onKeyDown={handleTagKeyDown}
                                    className="tagInput"
                                    placeholder={tags.length === 0 ? "Type a tag..." : ""}
                                />
                            </div>
                        </div>

                        <div className="formActions">
                            <button type="button" onClick={() => navigate(-1)} className="cancelButton" disabled={isLoading}>
                                Cancel
                            </button>
                            <button type="submit" className="submitButton" disabled={isLoading}>
                                {isLoading ? 'Posting...' : 'Post'}
                            </button>
                        </div>
                    </form>
                </div>
                
                <div className="bottomActions">
                    <button onClick={() => navigate(-1)} className="backButton">Back</button>
                </div>
            </main>
        </div>
    );
}