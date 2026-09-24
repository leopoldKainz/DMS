using DMS.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DMS.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    IDocumentService _documentService;
    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMultipleFiles(List<IFormFile> files)
    {
        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest("No files were uploaded.");
            }

            /* THIS PART WILL BE IMPLEMENTED LATER WHEN THE SERVICE IS READY
             
            return Ok(_documentService.)
            */
            return Ok();
        }
        catch(Exception ex)
        {
            return BadRequest(ex);
        }
    }
}