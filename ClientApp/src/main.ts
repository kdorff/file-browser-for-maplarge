import { ApiClient } from './api.js';
import { Router } from './router.js';
import { UI } from './ui.js';

/**
 * The API client.
 */
const api = new ApiClient();

/**
 * The current path.
 */
let currentPath = '';

/**
 * Handles the change of path.
 * @param path The path to change to.
 */
const handlePathChange = async (path: string) => {
    currentPath = path;
    ui.setLoading(true);
    try {
        const result = await api.list(path);
        ui.renderList(result, (p) => api.getDownloadUrl(p));
    } catch (e) {
        console.error(e);
        alert('Error loading files');
    } finally {
        ui.setLoading(false);
    }
};

/**
 * Handles the upload of files.
 * @param files The files to upload.
 */
const handleUpload = async (files: FileList) => {
    ui.setLoading(true);
    try {
        // Upload sequentially for simplicity
        for (let i = 0; i < files.length; i++) {
            await api.upload(currentPath, files[i]);
        }
        // Refresh
        await handlePathChange(currentPath);
    } catch (e) {
        console.error(e);
        alert('Error uploading files');
        ui.setLoading(false);
    }
};

/**
 * Initializes the router and UI.
 */
const router = new Router(handlePathChange);
const ui = new UI(
    (path) => router.navigateToPath(path),
    (files) => handleUpload(files)
);

/**
 * Initializes the router.
 */
router.init();

/**
 * Handles the up button click.
 */
document.getElementById('btn-up')!.addEventListener('click', () => {
    if (!currentPath) return; // already at root
    // Simple parent logic: strip last segment
    const parts = currentPath.split('/');
    parts.pop();
    router.navigateToPath(parts.join('/'));
});
