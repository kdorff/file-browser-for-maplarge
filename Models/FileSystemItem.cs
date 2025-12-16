namespace FileBrowser.Server.Models;

/**
 * Model for a filesystem item.
 */
public class FileSystemItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsFolder { get; set; }
    public long Size { get; set; }
    public DateTime? LastModified { get; set; }
    public int? ChildCount { get; set; } // Only for folders
}

/**
 * Model for a filesystem list operation.
 */
public class FileSystemResult
{
    public string CurrentPath { get; set; } = string.Empty;
    public string ParentPath { get; set; } = string.Empty;
    public List<FileSystemItem> Items { get; set; } = new();
    public int FileCount { get; set; }
    public int FolderCount { get; set; }
    public long TotalSize { get; set; }
}
