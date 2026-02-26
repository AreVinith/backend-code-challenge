using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageRepository repository, ILogger<MessagesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll(Guid organizationId)
    {
        var messages = await _repository.GetAllByOrganizationAsync(organizationId);
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
        var message = await _repository.GetByIdAsync(organizationId, id);

        if (message == null)
        {
            _logger.LogWarning("Message {MessageId} not found for Org {OrgId}", id, organizationId);
            return NotFound();
        }

        return Ok(message);
    }

    [HttpPost]
    public async Task<ActionResult<Message>> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        if (request == null)
            return BadRequest();

        var message = new Message
        {
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content,
            IsActive = true
        };

        // IMPORTANT: capture returned object
        var createdMessage = await _repository.CreateAsync(message);

        return CreatedAtAction(
            nameof(GetById),
            new { organizationId = organizationId, id = createdMessage.Id },
            createdMessage);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        var existing = await _repository.GetByIdAsync(organizationId, id);

        if (existing == null)
            return NotFound();

        existing.Title = request.Title;
        existing.Content = request.Content;

        // Call update and CAPTURE result
        var updatedMessage = await _repository.UpdateAsync(existing);

        if (updatedMessage == null)
            return NotFound();

        return Ok(updatedMessage);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid organizationId, Guid id)
    {
        var deleted = await _repository.DeleteAsync(organizationId, id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
