/**
 * Model or a filesystem item.
 */
export interface FileSystemItem {
    name: string;
    path: string;
    isFolder: boolean;
    size: number;
    lastModified: string;
    childCount?: number;
}

/**
 * Model for the result of a filesystem list operation.
 */
export interface FileSystemResult {
    currentPath: string;
    parentPath: string;
    items: FileSystemItem[];
    fileCount: number;
    folderCount: number;
    totalSize: number;
}

/**
 * API Client for interacting with the FileBrowser API.
 */
export class ApiClient {
    private baseUrl = '/api/fs';

    /**
     * Lists the contents of a directory.
     * @param path Call with an empty string to list the root directory.
     * @returns json of FileSystemResult
     */
    async list(path: string = ""): Promise<FileSystemResult> {
        const response = await fetch(`${this.baseUrl}/list?path=${encodeURIComponent(path)}`);
        if (!response.ok) throw new Error('Failed to load file list');
        return response.json();
    }

    /**
     * Uploads a file.
     * @param path The path to the file.
     * @param file The file to upload.
     */
    async upload(path: string, file: File): Promise<void> {
        const formData = new FormData();
        formData.append('path', path);
        formData.append('file', file);

        const response = await fetch(`${this.baseUrl}/upload`, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) throw new Error('Upload failed');
    }

    /**
     * Gets the download URL for a file.
     * @param path The path to the file.
     * @returns The download URL.
     */
    getDownloadUrl(path: string): string {
        return `${this.baseUrl}/download?path=${encodeURIComponent(path)}`;
    }
}
