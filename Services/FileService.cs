using FileBrowser.Server.Models;
using Microsoft.Extensions.Options;

namespace FileBrowser.Server.Services;

/**
 * Options for the file browser service.
 * @param RootPath The root path for the file browser.
 */
public class FileBrowserOptions
{
    public string RootPath { get; set; } = "./FilePool";
}

/**
 * Interface for file system operations.
 * @param GetContents Gets the contents of a directory.
 * @param GetFile Gets a file.
 * @param UploadFileAsync Uploads a file.
 */
public interface IFileService
{
    FileSystemResult GetContents(string relativePath);
    (Stream Stream, string ContentType, string FileName)? GetFile(string relativePath);
    Task UploadFileAsync(string relativePath, IFormFile file);
}

/**
 * Service for file system operations.
 */
public class FileService : IFileService
{
    private readonly string _rootPath;

    public FileService(IOptions<FileBrowserOptions> options, IHostEnvironment env)
    {
        /**
         * Initialize the file service.
         * @param options The options for the file service.
         * @param env The host environment.
         */
        var configuredPath = options.Value.RootPath;
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            configuredPath = "./FilePool";
        }
        
        // Ensure root path is absolute
        _rootPath = Path.GetFullPath(configuredPath, env.ContentRootPath);
        
        if (!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
        }
    }

    /**
     * Get the absolute path for a relative path.
     * @param relativePath The relative path.
     * @returns The absolute path.
     */
    private string GetAbsolutePath(string relativePath)
    {
        // Sanitize path to prevent directory traversal
        if (string.IsNullOrWhiteSpace(relativePath)) return _rootPath;
        
        var normalized = relativePath.Replace('\\', '/').Trim('/');
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, normalized));
        
        if (!fullPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Access denied");
        }
        
        return fullPath;
    }

    /**
    * Get the relative path for an absolute path.
    * @param fullPath The absolute path.
    * @returns The relative path.
    */
    private string GetRelativePath(string fullPath)
    {
        var relative = Path.GetRelativePath(_rootPath, fullPath);
        return relative == "." ? "" : relative.Replace('\\', '/');
    }

    /**
     * Get the contents of a directory.
     * @param relativePath The relative path.
     * @returns The directory contents as FileSystemResult.
     */
    public FileSystemResult GetContents(string relativePath)
    {
        var fullPath = GetAbsolutePath(relativePath);
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {relativePath}");
        }

        var dirInfo = new DirectoryInfo(fullPath);
        var items = new List<FileSystemItem>();

        // Get Directories
        foreach (var dir in dirInfo.GetDirectories())
        {
            items.Add(new FileSystemItem
            {
                Name = dir.Name,
                Path = GetRelativePath(dir.FullName),
                IsFolder = true,
                LastModified = dir.LastWriteTime,
                ChildCount = dir.GetFileSystemInfos().Length // Shallow count
            });
        }

        // Get Files
        foreach (var file in dirInfo.GetFiles())
        {
            items.Add(new FileSystemItem
            {
                Name = file.Name,
                Path = GetRelativePath(file.FullName),
                IsFolder = false,
                Size = file.Length,
                LastModified = file.LastWriteTime
            });
        }

        return new FileSystemResult
        {
            CurrentPath = GetRelativePath(fullPath),
            ParentPath = GetRelativePath(dirInfo.Parent?.FullName ?? _rootPath) == "." ? "" : GetRelativePath(dirInfo.Parent?.FullName ?? _rootPath), // Simplified
            Items = items,
            FileCount = items.Count(i => !i.IsFolder),
            FolderCount = items.Count(i => i.IsFolder),
            TotalSize = items.Where(i => !i.IsFolder).Sum(i => i.Size)
        };
    }

    /**
     * Get a file.
     * @param relativePath The relative path to the file.
     * @returns stream for the file.
     */
    public (Stream Stream, string ContentType, string FileName)? GetFile(string relativePath)
    {
        var fullPath = GetAbsolutePath(relativePath);
        if (!File.Exists(fullPath)) return null;

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        var fileName = Path.GetFileName(fullPath);
        // Simple content type detection could go here, defaulting to octet-stream
        return (stream, "application/octet-stream", fileName);
    }

    /**
     * Upload a file.
     * @param relativePath The relative path to the file.
     * @param file The file to upload.
     */
    public async Task UploadFileAsync(string relativePath, IFormFile file)
    {
        var targetDir = GetAbsolutePath(relativePath);
        if (!Directory.Exists(targetDir))
        {
            throw new DirectoryNotFoundException("Target directory not found");
        }

        var fullPath = Path.Combine(targetDir, file.FileName);
        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
    }
}
