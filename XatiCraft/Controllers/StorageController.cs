using Microsoft.AspNetCore.Mvc;
using XatiCraft.Handlers.Read;
using XatiCraft.Handlers.Upload;

namespace XatiCraft.Controllers;

[ApiController]
[Route("[controller]")]
public class StorageController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromServices] IUploader uploader, IFormFile file,
        CancellationToken cancellation)
    {
        await using Stream stream = file.OpenReadStream();
        UploadResult uploadResult = await uploader.UploadFileAsync(stream, file.FileName, cancellation);
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> Read([FromServices] IReader reader, string folder, CancellationToken cancellation)
    {
        return Ok(await reader.ListFilesAsync(folder, cancellation));
    }
}