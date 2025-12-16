using FileBrowser.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileBrowser.Server.Controllers;

/**
 * Controller for file system operations

 * Endpoints:
 * * /api/fs/list
 * * /api/fs/download
 * * /api/fs/upload
 */
[ApiController]
[Route("api/fs")]
public class FileSystemController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FileSystemController> _logger;

    public FileSystemController(IFileService fileService, ILogger<FileSystemController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    /**
     * Lists the contents of a directory.
     * @param path Call with an empty string to list the root directory.
     * @returns json of FileSystemResult
     */
    [HttpGet("list")]
    public IActionResult List([FromQuery] string path = "")
    {
        try
        {
            var result = _fileService.GetContents(path);
            return Ok(result);
        }
        catch (DirectoryNotFoundException)
        {
            return NotFound("Directory not found");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files");
            return StatusCode(500, "Internal Server Error");
        }
    }

    /**
     * Downloads a file.
     * @param path The path to the file.
     * @returns file stream
     */
    [HttpGet("download")]
    public IActionResult Download([FromQuery] string path)
    {
        try
        {
            var result = _fileService.GetFile(path);
            if (result == null) return NotFound("File not found");

            var (stream, contentType, fileName) = result.Value;
            return File(stream, contentType, fileName);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return StatusCode(500, "Internal Server Error");
        }
    }

    /**
     * Uploads a file.
     * @param path The path to the file.
     * @param file The file to upload.
     * @returns Ok or error.
     */
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] string? path, [FromForm] IFormFile file)
    {
        Console.WriteLine($"Upload request received. Path: '{path}', File: {(file?.FileName ?? "null")}, Length: {file?.Length}");
        
        if (file == null || file.Length == 0)
        {
            Console.WriteLine("File is null or empty");
            return BadRequest("No file uploaded");
        }

        try
        {
            await _fileService.UploadFileAsync(path ?? string.Empty, file);
            return Ok();
        }
        catch (DirectoryNotFoundException)
        {
            return NotFound("Target directory not found");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, "Internal Server Error");
        }
    }
}
