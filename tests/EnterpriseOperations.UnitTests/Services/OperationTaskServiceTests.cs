using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.Services;
using EnterpriseOperations.Application.Settings;
using EnterpriseOperations.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.UnitTests.Services
{
    public class OperationTaskServiceTests
    {
        private readonly Mock<IOperationTaskRepository> _repositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly OperationTaskService _service;

        public OperationTaskServiceTests()
        {
            _repositoryMock = new Mock<IOperationTaskRepository>();
            _cacheServiceMock = new Mock<ICacheService>();

            var cacheSettings = Options.Create(
                new CacheSettings
                {
                    OperationTasksPagedExpirationMinutes = 1,
                    ExternalSystemStatusExpirationMinutes = 10
                });

            _service = new OperationTaskService(
                _repositoryMock.Object,
                _cacheServiceMock.Object,
                cacheSettings);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTaskExists_ReturnsTaskDto()
        {
            // Arrange
            var operationTask = new OperationTask
            {
                Id = 1,
                Title = "Prepare monthly report",
                Description = "Prepare the operations report.",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                RowVersion = [1, 2, 3, 4]
            };

            _repositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(operationTask);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(operationTask.Id, result.Id);
            Assert.Equal(operationTask.Title, result.Title);
            Assert.Equal(operationTask.Description, result.Description);
            Assert.Equal(operationTask.IsCompleted, result.IsCompleted);
            Assert.Equal(operationTask.CreatedAt, result.CreatedAt);
            Assert.Equal(Convert.ToBase64String(operationTask.RowVersion), result.RowVersion);

            _repositoryMock.Verify(repository => repository.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTaskDoesNotExist_ReturnsNull()
        {
            // Arrange
            _repositoryMock
                .Setup(repository => repository.GetByIdAsync(999))
                .ReturnsAsync((OperationTask?)null);

            // Act
            var result = await _service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);

            _repositoryMock.Verify(repository => repository.GetByIdAsync(999), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenDataIsValid_CreatesTaskAndInvalidatesCache() 
        {
            // Arrange
            var createDto = new CreateOperationTaskDto
            {
                Title = "Review supplier contracts",
                Description = "Review contracts before renewal."
            };

            OperationTask? capturedTask = null;

            _repositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<OperationTask>()))
                .Callback<OperationTask>(task => capturedTask = task)
                .ReturnsAsync((OperationTask task) =>
                {
                    task.Id = 10;
                    task.RowVersion = [5, 6, 7, 8];

                    return task;
                });

            var beforeCreate = DateTime.UtcNow;

            // Act
            var result = await _service.CreateAsync(createDto);

            var afterCreate = DateTime.UtcNow;

            //Assert
            Assert.NotNull(capturedTask);
            Assert.Equal(createDto.Title, capturedTask.Title);
            Assert.Equal(createDto.Description, capturedTask.Description);
            Assert.False(capturedTask.IsCompleted);
            Assert.Null(capturedTask.CompletedAt);
            Assert.InRange(capturedTask.CreatedAt, beforeCreate, afterCreate);

            Assert.Equal(10, result.Id);
            Assert.Equal(createDto.Title, result.Title);
            Assert.Equal(createDto.Description, result.Description);
            Assert.False(result.IsCompleted);
            Assert.Equal(Convert.ToBase64String([5, 6, 7, 8]), result.RowVersion);

            _repositoryMock.Verify(repository => repository.AddAsync(It.IsAny<OperationTask>()), Times.Once);

            _cacheServiceMock.Verify(cache => cache.IncrementVersionAsync("operation-tasks:version"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenUpdateSucceeds_UpdatesTaskAndInvalidatesCache() 
        {
            // Arrange
            var rowVersionBytes = new byte[] { 10, 20, 30, 40 };

            var updateDto = new UpdateOperationTaskDto
            {
                Title = "Complete monthly report",
                Description = "Finalize and submit the monthly report.",
                IsCompleted = true,
                RowVersion = Convert.ToBase64String(rowVersionBytes)
            };

            OperationTask? capturedTask = null;

            _repositoryMock
                .Setup(repository => repository.UpdateAsync(It.IsAny<OperationTask>()))
                .Callback<OperationTask>(task => capturedTask = task)
                .ReturnsAsync(true);

            var beforeUpdate = DateTime.UtcNow;

            // Act
            var result = await _service.UpdateAsync(15, updateDto);

            var afterUpdate = DateTime.UtcNow;

            // Assert
            Assert.True(result);

            Assert.NotNull(capturedTask);
            Assert.Equal(15, capturedTask.Id);
            Assert.Equal(updateDto.Title, capturedTask.Title);
            Assert.Equal(updateDto.Description, capturedTask.Description);
            Assert.True(capturedTask.IsCompleted);

            Assert.NotNull(capturedTask.CompletedAt);
            Assert.InRange(capturedTask.CompletedAt.Value, beforeUpdate, afterUpdate);

            Assert.Equal(rowVersionBytes, capturedTask.RowVersion);

            _repositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<OperationTask>()), Times.Once);

            _cacheServiceMock.Verify(cache => cache.IncrementVersionAsync("operation-tasks:version"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenTaskIsNotCompleted_SetsCompletedAtToNull()
        {
            // Arrange
            var updateDto = new UpdateOperationTaskDto
            {
                Title = "Continue monthly report",
                Description = "The report still requires addtional data.",
                IsCompleted = false,
                RowVersion = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 })
            };

            OperationTask? capturedTask = null;

            _repositoryMock
                .Setup(repository => repository.UpdateAsync(It.IsAny<OperationTask>()))
                .Callback<OperationTask>(task => capturedTask = task)
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(20, updateDto);

            // Assert
            Assert.True(result);

            Assert.NotNull(capturedTask);
            Assert.False(capturedTask.IsCompleted);
            Assert.Null(capturedTask.CompletedAt);

            _cacheServiceMock.Verify(cache => cache.IncrementVersionAsync("operation-tasks:version"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenUpdateFails_DoesNotInvalidateCache()
        {
            // Arrange
            var updateDto = new UpdateOperationTaskDto
            {
                Title = "Missing task",
                Description = "This task does not exist.",
                IsCompleted = false,
                RowVersion = Convert.ToBase64String(new byte[] { 5, 6, 7, 8 })
            };

            _repositoryMock
                .Setup(repository => repository.UpdateAsync(It.IsAny<OperationTask>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(999, updateDto);

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<OperationTask>()), Times.Once);

            _cacheServiceMock.Verify(cache => cache.IncrementVersionAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenRowVersionIsInvalid_ThrowsFormatException()
        {
            // Arrange
            var updateDto = new UpdateOperationTaskDto
            {
                Title = "Invalid concurrency token",
                Description = "RowVersion is not valid Base64.",
                IsCompleted = false,
                RowVersion = "not-valid-base64"
            };

            // Act
            var action = async () => await _service.UpdateAsync(1, updateDto);

            // Assert
            await Assert.ThrowsAsync<FormatException>(action);

            _repositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<OperationTask>()), Times.Never);

            _cacheServiceMock.Verify(cache => cache.IncrementVersionAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
