using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic
{
    public class MessageLogic : IMessageLogic
    {
        private readonly IMessageRepository _repository;

        public MessageLogic(IMessageRepository repository)
        {
            _repository = repository;
        }

        // Get all messages
        public async Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
        {
            return await _repository.GetAllByOrganizationAsync(organizationId);
        }

        // Get single message
        public async Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
        {
            return await _repository.GetByIdAsync(organizationId, id);
        }

        // Create message
        public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
        {
            var validationErrors = Validate(request.Title, request.Content);

            if (validationErrors.Any())
                return new ValidationError(validationErrors);

            // Check unique title per organization
            var existingMessages = await _repository.GetAllByOrganizationAsync(organizationId);
            if (existingMessages.Any(x =>
                x.Title.Equals(request.Title, StringComparison.OrdinalIgnoreCase)))
            {
                return new Conflict("Title must be unique per organization.");
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Title = request.Title,
                Content = request.Content,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(message);

            return new Created<Message>(message);
        }

        // Update message
        public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
        {
            var existing = await _repository.GetByIdAsync(organizationId, id);

            if (existing == null)
                return new NotFound("Message not found.");

            // Rule: Only active messages can be updated
            if (!existing.IsActive)
            {
                return new ValidationError(new Dictionary<string, string[]>
        {
            { "IsActive", new[] { "Inactive message cannot be updated." } }
        });
            }

            // Validate Title & Content
            var validationErrors = Validate(request.Title, request.Content);
            if (validationErrors.Any())
                return new ValidationError(validationErrors);

            // Rule: Title must be unique per organization (excluding current message)
            var allMessages = await _repository.GetAllByOrganizationAsync(organizationId);

            if (allMessages.Any(x =>
                x.Id != id &&
                x.Title.Equals(request.Title, StringComparison.OrdinalIgnoreCase)))
            {
                return new Conflict("Title must be unique per organization.");
            }

            // Update fields
            existing.Title = request.Title;
            existing.Content = request.Content;

            // Auto set UpdatedAt
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);

            return new Updated();
        }

        // Delete message
        public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
        {
            var existing = await _repository.GetByIdAsync(organizationId, id);

            if (existing == null)
                return new NotFound("Message not found.");

            // Only active messages can be deleted
            if (!existing.IsActive)
            {
                return new ValidationError(new Dictionary<string, string[]>
                {
                    { "IsActive", new[] { "Inactive message cannot be deleted." } }
                });
            }

            await _repository.DeleteAsync(organizationId, id);

            return new Deleted();
        }

        // Common validation method
        private Dictionary<string, string[]> Validate(string title, string content)
        {
            var errors = new Dictionary<string, List<string>>();

            // Title validation
            if (string.IsNullOrWhiteSpace(title))
            {
                AddError(errors, "Title", "Title is required.");
            }
            else if (title.Length < 3 || title.Length > 200)
            {
                AddError(errors, "Title", "Title must be between 3 and 200 characters.");
            }

            // Content validation
            if (string.IsNullOrWhiteSpace(content))
            {
                AddError(errors, "Content", "Content is required.");
            }
            else if (content.Length < 10 || content.Length > 1000)
            {
                AddError(errors, "Content", "Content must be between 10 and 1000 characters.");
            }

            return errors.ToDictionary(k => k.Key, v => v.Value.ToArray());
        }

        private void AddError(Dictionary<string, List<string>> errors, string key, string message)
        {
            if (!errors.ContainsKey(key))
                errors[key] = new List<string>();

            errors[key].Add(message);
        }
    }
}