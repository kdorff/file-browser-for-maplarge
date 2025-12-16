export class Router {
    /**
     * The callback to be called when the path changes.
     */
    private onPathChange: (path: string) => void;

    /**
     * Whether the router is currently navigating.
     */
    private isNavigating = false;

    /**
     * Initializes the router.
     * @param onPathChange The callback to be called when the path changes.
     */
    constructor(onPathChange: (path: string) => void) {
        this.onPathChange = onPathChange;
        window.addEventListener('popstate', () => this.handleUrlChange());
    }

    /**
     * Initializes the router.
     */
    public init() {
        this.handleUrlChange();
    }

    /**
     * Handle URL changes.
     */
    private handleUrlChange() {
        this.isNavigating = true;
        const params = new URLSearchParams(window.location.search);
        const path = params.get('path') || "";

        this.onPathChange(path);
        this.isNavigating = false;
    }

    /**
     * Navigate to a path.
     * @param path The path to navigate to.
     */
    public navigateToPath(path: string) {
        if (this.isNavigating) return;
        const url = new URL(window.location.href);
        if (path) {
            url.searchParams.set('path', path);
        } else {
            url.searchParams.delete('path');
        }
        window.history.pushState({}, '', url.toString());
        this.onPathChange(path);
    }
}
