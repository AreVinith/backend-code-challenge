using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace CodeChallenge.Tests
{
    public class MessageLogicTests
    {
        private readonly Mock<IMessageRepository> _mockRepo;
        private readonly MessageLogic _logic;

        public MessageLogicTests()
        {
            _mockRepo = new Mock<IMessageRepository>();
            _logic = new MessageLogic(_mockRepo.Object);
        }

        [Fact]
        public async Task CreateMessageAsync_ShouldReturnCreated_WhenValidRequest()
        {
            // Arrange
            var organizationId = Guid.NewGuid();

            var request = new CreateMessageRequest
            {
                Title = "Test Title",
                Content = "This is valid content for testing"
            };

            _mockRepo.Setup(r => r.GetAllByOrganizationAsync(organizationId))
                     .ReturnsAsync(new List<Message>());

            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                     .ReturnsAsync((Message m) => m);

            // Act
            var result = await _logic.CreateMessageAsync(organizationId, request);

            // Assert
            result.Should().BeOfType<Created<Message>>();
        }
        [Fact]
        public async Task CreateMessageAsync_ShouldReturnConflict_WhenTitleAlreadyExists()
        {
            // Arrange
            var orgId = Guid.NewGuid();
            var request = new CreateMessageRequest { Title = "Duplicate", Content = "Valid content" };
            var existingMessages = new List<Message> { new Message { Title = "Duplicate" } };

            _mockRepo.Setup(r => r.GetAllByOrganizationAsync(orgId))
                .ReturnsAsync(existingMessages);

            // Act
            var result = await _logic.CreateMessageAsync(orgId, request);

            // Assert
            result.Should().BeOfType<Conflict>();
        }
        [Fact]
        public async Task CreateMessageAsync_ShouldReturnValidationError_WhenContentTooShort()
        {
            // Arrange
            var orgId = Guid.NewGuid();
            var request = new CreateMessageRequest { Title = "Valid Title", Content = "short" };

            // Act
            var result = await _logic.CreateMessageAsync(orgId, request);

            // Assert
            result.Should().BeOfType<ValidationError>();
        }
        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            var orgId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var request = new UpdateMessageRequest { Title = "Title", Content = "Content" };

            _mockRepo.Setup(r => r.GetByIdAsync(orgId, messageId))
                .ReturnsAsync((Message?)null);

            var result = await _logic.UpdateMessageAsync(orgId, messageId, request);

            result.Should().BeOfType<NotFound>();
        }
        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnValidationError_WhenMessageIsInactive()
        {
            var orgId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var request = new UpdateMessageRequest { Title = "Title", Content = "Valid content" };

            _mockRepo.Setup(r => r.GetByIdAsync(orgId, messageId))
                .ReturnsAsync(new Message { IsActive = false });

            var result = await _logic.UpdateMessageAsync(orgId, messageId, request);

            result.Should().BeOfType<ValidationError>();
        }
        [Fact]
        public async Task DeleteMessageAsync_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            var orgId = Guid.NewGuid();
            var messageId = Guid.NewGuid();

            _mockRepo.Setup(r => r.GetByIdAsync(orgId, messageId))
                .ReturnsAsync((Message?)null);

            var result = await _logic.DeleteMessageAsync(orgId, messageId);

            result.Should().BeOfType<NotFound>();
        }
    }
}