using Microsoft.AspNetCore.Mvc;
using XatiCraft.Data;
using XatiCraft.Guards;
using XatiCraft.Handlers.Read;
using XatiCraft.Handlers.Upload;
using XatiCraft.Model;

namespace XatiCraft.Controllers;

[ApiController]
[Route("[controller]")]
[IpSessionGuard]
public class StorageController : ControllerBase
{
    [HttpPost("upload")]
    [IpAddressGuard]
    public async Task<IActionResult> Upload([FromServices] IUploader uploader, [FromServices] ApplicationContext db,
        IFormFile file,
        CancellationToken cancellation)
    {
        await using Stream stream = file.OpenReadStream();
        UploadResult uploadResult = await uploader.UploadFileAsync(stream, file.FileName, cancellation);
        Product product = new Product
        {
            Title = "hi", Price = 1, Description = "DD"
        };
        await db.Products.AddAsync(product, cancellation);
        await db.SaveChangesAsync(cancellation);
        ProductMetadata productMetadata = new ProductMetadata
        {
            FileKey = uploadResult.Key,
            Location = uploadResult.Location,
            OriginalFile = file.FileName,
            Product = product
        };
        await db.ProductMetadata.AddAsync(productMetadata, cancellation);
        await db.SaveChangesAsync(cancellation);
        return NoContent();
    }

    [HttpGet]
    [ApiKeyGuard]
    [IpAddressGuard]
    public async Task<IActionResult> Read([FromServices] IReader reader, string folder, CancellationToken cancellation)
    {
        return Ok(await reader.ListFilesAsync(folder, cancellation));
    }
}