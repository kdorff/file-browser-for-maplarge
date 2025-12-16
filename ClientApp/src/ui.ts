import { FileSystemItem, FileSystemResult } from './api.js';

/**
 * Class representing the UI.
 */
export class UI {
    /**
     * The list of files and folders.
     */
    private listElement: HTMLElement;

    /**
     * The breadcrumbs element.
     */
    private breadcrumbsElement: HTMLElement;

    /**
     * The stats element.
     */
    private statsElement: HTMLElement;

    /**
     * The loading element.
     */
    private loadingElement: HTMLElement;

    /**
     * The file input element.
     */
    private fileInput: HTMLInputElement;

    /**
     * Initializes the UI.
     * @param onNavigate The callback to be called when the path changes.
     * @param onUpload The callback to be called when files are uploaded.
     */
    constructor(
        private onNavigate: (path: string) => void,
        private onUpload: (files: FileList) => void
    ) {
        // Get elements from the DOM
        this.listElement = document.getElementById('file-list')!;
        this.breadcrumbsElement = document.getElementById('breadcrumbs')!;
        this.statsElement = document.getElementById('stats')!;
        this.loadingElement = document.getElementById('loading')!;
        this.fileInput = document.getElementById('file-input') as HTMLInputElement;

        // Setup event listeners on the UI elements
        this.setupEventListeners();
    }

    /**
     * Sets up event listeners on the UI elements.
     */
    private setupEventListeners() {
        // File Input
        document.getElementById('btn-upload')!.addEventListener('click', () => {
            this.fileInput.click();
        });

        this.fileInput.addEventListener('change', () => {
            // Upload the files.
            if (this.fileInput.files?.length) {
                this.onUpload(this.fileInput.files);
                // Reset the file input.
                this.fileInput.value = '';
            }
        });
    }

    /**
     * Sets the loading state.
     * @param isLoading Whether the UI is in a loading state.
     */
    public setLoading(isLoading: boolean) {
        this.loadingElement.style.display = isLoading ? 'block' : 'none';
    }

    /**
     * Renders the list of files and folders.
     * @param result The result to render.
     * @param downloadUrlFn The function to generate download URLs.
     */
    public renderList(result: FileSystemResult, downloadUrlFn: (path: string) => string) {
        this.renderBreadcrumbs(result.currentPath);
        this.renderStats(result);

        // Clear the file list.
        this.listElement.innerHTML = '';

        // If no files are found, show a message.
        if (result.items.length === 0) {
            this.listElement.innerHTML = '<div style="grid-column: 1/-1; text-align: center; color: #888;">No files found</div>';
            return;
        }

        // Render each file and folder.
        result.items.forEach(item => {
            // Create the list item for the file or folder using a file or folder icon.
            const el = document.createElement('div');
            el.className = 'item';
            el.innerHTML = ` 
                <span class="icon">${item.isFolder ? '📁' : '📄'}</span>
                <div class="name">${item.name}</div>
                <div class="meta">${this.formatSize(item.size)}</div>
            `;

            // Add click handler for the file or folder
            el.addEventListener('click', () => {
                if (item.isFolder) {
                    this.onNavigate(item.path);
                } else {
                    window.location.href = downloadUrlFn(item.path);
                }
            });

            this.listElement.appendChild(el);
        });
    }

    /**
     * Renders the breadcrumbs.
     * @param path The path to render.
     */
    private renderBreadcrumbs(path: string) {
        // Split the path into parts.
        const parts = path ? path.split('/') : [];

        // Clear the breadcrumbs.
        this.breadcrumbsElement.innerHTML = '';

        // Add the Home breadcrumb.
        const home = document.createElement('span');
        home.innerText = 'Home';
        home.onclick = () => this.onNavigate('');
        this.breadcrumbsElement.appendChild(home);

        let currentPath = '';
        parts.forEach((part, index) => {
            // Add a separator.
            this.breadcrumbsElement.appendChild(document.createTextNode(' / '));

            // Update the current path for the breadcrumb.
            currentPath += (currentPath ? '/' : '') + part;

            // Create a span for the breadcrumb containing the part.
            const span = document.createElement('span');
            span.innerText = part;
            const p = currentPath;

            // Setup the click handler for the breadcrumb span
            span.onclick = () => this.onNavigate(p);

            // Add the span to the breadcrumbs.
            this.breadcrumbsElement.appendChild(span);
        });
    }

    /**
     * Renders the stats.
     * @param result The result to render.
     */
    private renderStats(result: FileSystemResult) {
        this.statsElement.innerText = `${result.folderCount} folders, ${result.fileCount} files, Total Size: ${this.formatSize(result.totalSize)}`;
    }

    /**
     * Formats a size in bytes to a human-readable string.
     * @param bytes The size in bytes.
     * @returns The formatted size.
     */
    private formatSize(bytes: number): string {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
    }
}
