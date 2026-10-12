using DMS.Business.Interfaces;
using DMS.DAL.DTOs;
using DMS.DAL.Entities;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace DMS.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<List<DocumentDto>>> UploadMultipleFiles(
        List<IFormFile> files,
        [FromQuery] Guid userId)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files were uploaded.");
        }

        var createdDocuments = new List<DocumentDto>();

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

            Document savedDocument;
            savedDocument = await _documentService.AddForOwnerAsync(userId, document);

            createdDocuments.Add(savedDocument.Adapt<DocumentDto>());
        }

        return Ok(createdDocuments);
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentDto>>> GetAll([FromQuery] Guid? userId = null)
    {
        IReadOnlyList<Document> documents;
        if (userId.HasValue)
        {
            documents = await _documentService.GetByUserAsync(userId.Value);
        }
        else
        {
            documents = await _documentService.GetAllAsync();
        }

        return Ok(documents.Adapt<List<DocumentDto>>());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> GetById(Guid id)
    {
        var document = await _documentService.GetByIdAsync(id);
        if (document == null)
        {
            return NotFound($"Document with ID {id} not found.");
        }

        return Ok(document.Adapt<DocumentDto>());
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<DocumentDto>>> Search([FromQuery] string query, [FromQuery] Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAll(userId);
        }

        IReadOnlyList<Document> documents;
        if (userId.HasValue)
        {
            documents = await _documentService.SearchByNameAsync(userId.Value, query);
        }
        else
        {
            var allDocs = await _documentService.GetAllAsync();
            documents = allDocs.Where(d => d.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Ok(documents.Adapt<List<DocumentDto>>());
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _documentService.RemoveByIdAsync(id);
        if (!deleted)
        {
            return NotFound($"Document with ID {id} not found.");
        }

        return NoContent();
    }
}
