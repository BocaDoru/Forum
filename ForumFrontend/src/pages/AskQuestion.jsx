import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { 
    getTagByName, 
    getTagById,
    createTag, 
    askQuestion, 
    getQuestionById, 
    editQuestion 
} from '../services/questionService.js';
import { fetchImageBlob, uploadImage } from '../services/imageService';
import '../style/AskQuestion.css';
import Header from "../components/Header.jsx";

export default function AskQuestion() {
    const { id } = useParams(); 
    const isEditMode = Boolean(id);

    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    
    const [image, setImage] = useState(null);
    const [existingImageId, setExistingImageId] = useState('');
    const [imagePreviewUrl, setImagePreviewUrl] = useState('');
    
    const [existingStatus, setExistingStatus] = useState('received');
    const [currentTagInput, setCurrentTagInput] = useState('');
    const [tags, setTags] = useState([]);
    
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        if (isEditMode) {
            const loadQuestionData = async () => {
                setIsLoading(true);
                try {
                    const qData = await getQuestionById(id);
                    setTitle(qData.Title);
                    setDescription(qData.Text);
                    if (qData.ImageId) setExistingImageId(qData.ImageId);
                    
                    const statusString = qData.Status === 2 ? 'solved' : qData.Status === 1 ? 'in_progress' : 'received';
                    setExistingStatus(statusString);

                    if (qData.TagNames && qData.TagNames.length > 0) {
                        setTags(qData.TagNames);
                    } else if (qData.TagIds && qData.TagIds.length > 0) {
                        const resolvedTagNames = await Promise.all(
                            qData.TagIds.map(async (tagId) => {
                                try {
                                    const tagData = await getTagById(tagId);
                                    return tagData.Name; 
                                } catch (err) {
                                    console.error(`Failed to load tag: ${tagId}`);
                                    return null; 
                                }
                            })
                        );
                        setTags(resolvedTagNames.filter(name => name !== null));
                    }

                    if (qData.ImageId) {
                        setExistingImageId(qData.ImageId);
                        const blobUrl = await fetchImageBlob(qData.ImageId);
                        setImagePreviewUrl(blobUrl);
                    }
                } catch (err) {
                    setError("Failed to load question for editing.");
                } finally {
                    setIsLoading(false);
                }
            };
            loadQuestionData();
        }
    }, [id, isEditMode]);

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
            const file = e.target.files[0];
            setImage(file);
            setExistingImageId('');
            setImagePreviewUrl(URL.createObjectURL(file));
        }
    };

    const handleRemoveImage = () => {
        setImage(null);
        setExistingImageId('');
        setImagePreviewUrl('');
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

            if (isEditMode) {
                let finalImageId = existingImageId; 

                if (image) {
                    const uploadRes = await uploadImage(image);
                    finalImageId = uploadRes?.Id || '';
                }

                await editQuestion(id, title, description, tagIds, existingStatus, finalImageId);
                navigate(`/question/${id}`);
            } else {
                await askQuestion(title, description, image, tagIds);
                navigate('/home'); 
            }

        } catch (err) {
            setError(err.message || "An error occurred while saving your question.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="pageContainer">
            <Header />

            <main className="askMainContent">
                <h2 className="pageTitle">{isEditMode ? 'Edit Question' : 'Ask a Question'}</h2>

                <div className="askCard">
                    {error && <div className="errorText">{error}</div>}

                    <form onSubmit={handleSubmit} className="askForm">
                        <div className="inputGroup">
                            <label className="label" htmlFor="titleInput">Title</label>
                            <input id="titleInput" type="text" value={title} onChange={(e) => setTitle(e.target.value)} className="input" required />
                        </div>

                        <div className="inputGroup">
                            <label className="label" htmlFor="descriptionInput">Description</label>
                            <textarea id="descriptionInput" value={description} onChange={(e) => setDescription(e.target.value)} className="input textarea" rows="6" required />
                        </div>

                        <div className="inputGroup">
                            <label className="label" htmlFor="imageInput">Attach Image</label>
                            
                            {imagePreviewUrl ? (
                                <div className="imagePreviewContainer">
                                    <img src={imagePreviewUrl} alt="Preview" className="imagePreview" />
                                    <button type="button" onClick={handleRemoveImage} className="removeImageBtn">X Remove Image</button>
                                </div>
                            ) : (
                                <div className="fileInputWrapper">
                                    <input id="imageInput" type="file" accept="image/*" onChange={handleFileChange} className="fileInput" />
                                </div>
                            )}
                        </div>

                        <div className="inputGroup">
                            <label className="label" htmlFor="tagsInput">Tags</label>
                            <span className="tagHelpText">Press enter to confirm each tag.</span>
                            
                            <div className="tagInputContainer">
                                {tags.map(tag => (
                                    <span key={tag} className="tagBadge">
                                        {tag}
                                        <button type="button" onClick={() => removeTag(tag)} className="tagRemoveBtn">x</button>
                                    </span>
                                ))}
                                <input 
                                    id="tagsInput"
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
                            <button type="button" onClick={() => navigate(-1)} className="cancelButton" disabled={isLoading}>Cancel</button>
                            <button type="submit" className="submitButton" disabled={isLoading}>{isLoading ? 'Saving...' : (isEditMode ? 'Save Changes' : 'Post')}</button>
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