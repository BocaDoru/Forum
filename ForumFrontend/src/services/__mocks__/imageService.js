import { vi } from 'vitest';

export const fetchImageBlob = vi.fn().mockResolvedValue('blob:http://localhost/fake-image-blob-url');

export const uploadImage = vi.fn().mockResolvedValue({ 
    Id: 'fake-image-id-123',
    FileName: 'test-image.png',
    FilePath: 'FileStorage/Images/test-image.png'
});