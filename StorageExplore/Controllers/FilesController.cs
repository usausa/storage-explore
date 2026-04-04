namespace StorageExplore.Controllers;

using Microsoft.AspNetCore.Mvc;

using StorageExplore.Services;

[ApiController]
[Route("api/files")]
[IgnoreAntiforgeryToken]
public class FilesController : ControllerBase
{
    private readonly FileStorageService storage;

    public FilesController(FileStorageService storage)
    {
        this.storage = storage;
    }

    [HttpGet("download/{bucket}/{**path}")]
    public IActionResult Download(string bucket, string path)
    {
        var fileInfo = storage.GetFileInfo(bucket, path);
        if (fileInfo is null)
        {
            return NotFound();
        }

        var stream = storage.OpenRead(bucket, path);
        if (stream is null)
        {
            return NotFound();
        }

        return File(stream, "application/octet-stream", fileInfo.Name);
    }

    [HttpGet("preview/{bucket}/{**path}")]
    public IActionResult Preview(string bucket, string path)
    {
        var fileInfo = storage.GetFileInfo(bucket, path);
        if (fileInfo is null)
        {
            return NotFound();
        }

#pragma warning disable CA2000
        var stream = storage.OpenRead(bucket, path);
#pragma warning restore CA2000
        if (stream is null)
        {
            return NotFound();
        }

        return File(stream, fileInfo.ContentType, enableRangeProcessing: true);
    }

    [HttpGet("thumbnail/{bucket}/{**path}")]
    public IActionResult Thumbnail(string bucket, string path)
    {
        var fileInfo = storage.GetFileInfo(bucket, path);
        if (fileInfo is null)
        {
            return NotFound();
        }

#pragma warning disable CA2000
        var stream = storage.OpenRead(bucket, path);
#pragma warning restore CA2000
        if (stream is null)
        {
            return NotFound();
        }

        if (fileInfo.Extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg" or ".ico")
        {
            return File(stream, fileInfo.ContentType, enableRangeProcessing: true);
        }

        stream.Dispose();
        return NoContent();
    }

    [HttpPost("upload/{bucket}/{**path}")]
    [RequestSizeLimit(10L * 1024 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024 * 1024 * 1024)]
    public async Task<IActionResult> Upload(string bucket, string path = "")
    {
        if (Request.Form.Files.Count == 0)
        {
            return BadRequest("No files uploaded.");
        }

        var results = new List<object>();
        foreach (var file in Request.Form.Files)
        {
            var targetPath = string.IsNullOrEmpty(path)
                ? file.FileName
                : $"{path}/{file.FileName}";

            await using var stream = file.OpenReadStream();
            await storage.SaveFileAsync(bucket, targetPath, stream);

            results.Add(new { name = file.FileName, size = file.Length, path = targetPath });
        }

        return Ok(new { uploaded = results.Count, files = results });
    }

    [HttpPost("create-folder/{bucket}/{**path}")]
    public IActionResult CreateFolder(string bucket, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("Path is required.");
        }

        if (storage.Exists(bucket, path))
        {
            return Conflict("Already exists.");
        }

        storage.CreateDirectory(bucket, path);
        return Ok();
    }

    [HttpPost("rename/{bucket}/{**path}")]
    public IActionResult Rename(string bucket, string path, [FromQuery] string newName)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("Path is required.");
        }
        if (string.IsNullOrWhiteSpace(newName))
        {
            return BadRequest("New name is required.");
        }

        if (!storage.Exists(bucket, path))
        {
            return NotFound();
        }

        var newPath = storage.Rename(bucket, path, newName);
        if (newPath is null)
        {
            return Conflict("Rename failed. The target name may already exist or be invalid.");
        }

        return Ok(new { newPath });
    }

    [HttpDelete("{bucket}/{**path}")]
    public IActionResult Delete(string bucket, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("Path is required.");
        }

        if (!storage.Exists(bucket, path))
        {
            return NotFound();
        }

        storage.Delete(bucket, path);
        return Ok();
    }
}
