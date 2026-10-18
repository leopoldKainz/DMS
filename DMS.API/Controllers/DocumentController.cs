using DMS.API.Auth;
using DMS.Business.Interfaces;
using DMS.DAL.DTOs;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS.API.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    private const long MaxUploadBytes = 50L * 1024 * 1024;

    private readonly IDocumentService _documentService;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentController(IDocumentService documentService, IUnitOfWork unitOfWork)
    {
        _documentService = documentService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    public async Task<ActionResult<List<DocumentDto>>> UploadMultipleFiles(
        List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files were uploaded.");
        }

        Guid userId = User.GetUserId();
        var createdDocuments = new List<Document>();

        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                continue;
            }

            var document = new Document
            {
                Name = Path.GetFileNameWithoutExtension(file.FileName),
                Type = Path.GetExtension(file.FileName).TrimStart('.'),
                Size = file.Length
            };

            createdDocuments.Add(await _documentService.AddForOwnerAsync(userId, document));
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(createdDocuments.Adapt<List<DocumentDto>>());
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentDto>>> GetAll()
    {
        IReadOnlyList<Document> documents = await _documentService.GetByUserAsync(User.GetUserId());
        return Ok(documents.Adapt<List<DocumentDto>>());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> GetById(Guid id)
    {
        var document = await _documentService.GetByIdAsync(id);
        if (document == null || !await _documentService.HasAccessAsync(User.GetUserId(), id))
        {
            return NotFound($"Document with ID {id} not found.");
        }

        return Ok(document.Adapt<DocumentDto>());
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<DocumentDto>>> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAll();
        }

        IReadOnlyList<Document> documents = await _documentService.SearchByNameAsync(User.GetUserId(), query);
        return Ok(documents.Adapt<List<DocumentDto>>());
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        Guid userId = User.GetUserId();
        if (!await _documentService.HasAccessAsync(userId, id))
        {
            return NotFound($"Document with ID {id} not found.");
        }

        if (!await _documentService.HasPermissionAsync(userId, id, Permission.Owner))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Only the owner can delete this document.");
        }

        var deleted = await _documentService.RemoveByIdAsync(id);
        if (!deleted)
        {
            return NotFound($"Document with ID {id} not found.");
        }

        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}
