using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.Models;
using EnterpriseOperations.Application.Services;
using EnterpriseOperations.Application.Settings;
using EnterpriseOperations.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;

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

        #region GetByIdAsync Tests

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

        #endregion

        #region CreateAsync Tests

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

        #endregion

        #region UpdateAsync Tests

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
                Description = "The report still requires additional data.",
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

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenDeleteSucceeds_InvalidatesCacheAndReturnsTrue()
        {
            // Arrange
            _repositoryMock
                .Setup(repository => repository.DeleteAsync(25))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(25);

            // Assert
            Assert.True(result);

            _repositoryMock.Verify(repository => repository.DeleteAsync(25), Times.Once);

            _cacheServiceMock.Verify(cache => cache.IncrementVersionAsync("operation-tasks:version"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenDeleteFails_DoesNotInvalidateCacheAndReturnsFalse()
        {
            // Arrange
            _repositoryMock
                .Setup(repository => repository.DeleteAsync(999))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(999);

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(repository => repository.DeleteAsync(999), Times.Once);

            _cacheServiceMock.Verify(cache => cache.IncrementVersionAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region GetPagedAsyncTests

        [Fact]
        public async Task GetPagedAsync_WhenCachedResultExists_ReturnsCachedResultWithoutCallingRepository()
        {
            // Arrange
            var queryParameters = new OperationTaskQueryParameters
            {
                PageNumber = 2,
                PageSize = 5,
                IsCompleted = true,
                SearchTerm = "report",
                SortBy = "createdAt",
                SortDirection = "desc"
            };

            var cachedResult = new PagedResult<OperationTaskDto>
            {
                Items =
                [
                    new OperationTaskDto
                    {
                        Id = 1,
                        Title = "Cached report task",
                        Description = "Returned from cache.",
                        IsCompleted = true,
                        CreatedAt = DateTime.UtcNow,
                        RowVersion = Convert.ToBase64String([1, 2, 3, 4])
                    }
                    ],
                PageNumber = 2,
                PageSize = 5,
                TotalCount = 6
            };

            const int cacheVersion = 3;

            var expectedCacheKey =
                "operation-tasks:paged:v3:" +
                "pageNumber=2:" +
                "pageSize=5:" +
                "isCompleted=True:" +
                "searchTerm=report:" +
                "sortBy=createdAt:" +
                "sortDirection=desc";

            _cacheServiceMock
                .Setup(cache => cache.GetVersionAsync("operation-tasks:version"))
                .ReturnsAsync(cacheVersion);

            _cacheServiceMock
                .Setup(cache => cache.GetAsync<PagedResult<OperationTaskDto>>(expectedCacheKey))
                .ReturnsAsync(cachedResult);

            // Act
            var result = await _service.GetPagedAsync(queryParameters);

            // Assert
            Assert.Same(cachedResult, result);

            _cacheServiceMock.Verify(cache => cache.GetVersionAsync("operation-tasks:version"), Times.Once);

            _cacheServiceMock.Verify(cache => cache.GetAsync<PagedResult<OperationTaskDto>>(expectedCacheKey), Times.Once);

            _cacheServiceMock.Verify(cache => cache.SetAsync(
                It.IsAny<string>(),
                It.IsAny<PagedResult<OperationTaskDto>>(),
                It.IsAny<TimeSpan>()),
                Times.Never);
        }

        [Fact]
        public async Task GetPagedAsync_WhenCacheMisses_ReturnsRepositoryResultAndStoresItInCache()
        {
            // Arrange
            var queryParameters = new OperationTaskQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                IsCompleted = false,
                SearchTerm = "supplier",
                SortBy = "title",
                SortDirection = "asc"
            };

            var createdAt = DateTime.UtcNow;

            var repositoryResult = new PagedResult<OperationTask>
            {
                Items =
                [
                    new OperationTask
                    {
                        Id = 11,
                        Title = "Review supplier contract",
                        Description = "Review the current supplier agreement.",
                        IsCompleted = false,
                        CreatedAt = createdAt,
                        CompletedAt = null,
                        RowVersion = [5, 6, 7, 8]
                    }
                ],
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1
            };

            const int cacheVersion = 4;

            var expectedCacheKey =
                "operation-tasks:paged:v4:" +
                "pageNumber=1:" +
                "pageSize=10:" +
                "isCompleted=False:" +
                "searchTerm=supplier:" +
                "sortBy=title:" +
                "sortDirection=asc";

            _cacheServiceMock
                .Setup(cache => cache.GetVersionAsync("operation-tasks:version"))
                .ReturnsAsync(cacheVersion);

            _cacheServiceMock
                .Setup(cache =>
                cache.GetAsync<PagedResult<OperationTaskDto>>(expectedCacheKey))
                .ReturnsAsync((PagedResult<OperationTaskDto>?)null);

            _repositoryMock
                .Setup(repository => repository.GetPagedAsync(queryParameters))
                .ReturnsAsync(repositoryResult);

            PagedResult<OperationTaskDto>? cachedResult = null;
            TimeSpan? capturedExpiration = null;

            _cacheServiceMock
                .Setup(cache => cache.SetAsync(
                    expectedCacheKey,
                    It.IsAny<PagedResult<OperationTaskDto>>(),
                    It.IsAny<TimeSpan>()))
                .Callback<string, PagedResult<OperationTaskDto>, TimeSpan>(
                (_, result, expiration) =>
                {
                    cachedResult = result;
                    capturedExpiration = expiration;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.GetPagedAsync(queryParameters);

            // Assert
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(19, result.PageSize);
            Assert.Equal(1, result.TotalCount);

            var item = Assert.Single(result.Items);

            Assert.Equal(11, item.Id);
            Assert.Equal("Review supplier contract", item.Title);
            Assert.Equal("Review the current supplier agreement.", item.Description);
            Assert.False(item.IsCompleted);
            Assert.Equal(createdAt, item.CreatedAt);
            Assert.Null(item.CompletedAt);
            Assert.Equal(Convert.ToBase64String([5, 6, 7, 8]), item.RowVersion);

            Assert.NotNull(cachedResult);
            Assert.Same(result, cachedResult);
            Assert.Equal(TimeSpan.FromMinutes(1), capturedExpiration);

            _cacheServiceMock.Verify(cache => cache.GetVersionAsync("operation-tasks:version"), Times.Once);

            _cacheServiceMock.Verify(cache => cache.GetAsync<PagedResult<OperationTaskDto>>(expectedCacheKey), Times.Once);

            _repositoryMock.Verify(repository => repository.GetPagedAsync(queryParameters), Times.Once);

            _cacheServiceMock.Verify(cache => cache.SetAsync(expectedCacheKey, result, TimeSpan.FromMinutes(1)), Times.Once);
        }

        [Fact]
        public async Task GetPagedAsync_WhenQueryParametersDiffer_UsesDifferentCacheKeys()
        {
            // Arrange
            var firstQuery = new OperationTaskQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                IsCompleted = false,
                SearchTerm = "report",
                SortBy = "createdAt",
                SortDirection = "desc"
            };

            var secondQuery = new OperationTaskQueryParameters
            {
                PageNumber = 2,
                PageSize = 5,
                IsCompleted = true,
                SearchTerm = "supplier",
                SortBy = "title",
                SortDirection = "asc"
            };

            _cacheServiceMock
                .Setup(cache => cache.GetVersionAsync("operation-tasks:version"))
                .ReturnsAsync(7);

            var capturedCacheKeys = new List<string>();

            _cacheServiceMock
                .Setup(cache => cache.GetAsync<PagedResult<OperationTaskDto>>(It.IsAny<string>()))
                .Callback<string>(key => capturedCacheKeys.Add(key))
                .ReturnsAsync((PagedResult<OperationTaskDto>?)null);

            _repositoryMock
                .Setup(repository => repository.GetPagedAsync(It.IsAny<OperationTaskQueryParameters>()))
                .ReturnsAsync((OperationTaskQueryParameters query) =>
                    new PagedResult<OperationTask>
                    {
                        Items = [],
                        PageNumber = query.PageNumber,
                        PageSize = query.PageSize,
                        TotalCount = 0
                    });

            // Act
            await _service.GetPagedAsync(firstQuery);
            await _service.GetPagedAsync(secondQuery);

            // Assert
            Assert.Equal(2, capturedCacheKeys.Count);
            Assert.NotEqual(capturedCacheKeys[0], capturedCacheKeys[1]);

            Assert.Contains("pageNumber=1", capturedCacheKeys[0]);
            Assert.Contains("pageSize=10", capturedCacheKeys[0]);
            Assert.Contains("isCompleted=False", capturedCacheKeys[0]);
            Assert.Contains("searchTerm=report", capturedCacheKeys[0]);
            Assert.Contains("sortBy=createdAt", capturedCacheKeys[0]);
            Assert.Contains("sortDirection=desc", capturedCacheKeys[0]);

            Assert.Contains("pageNumber=2", capturedCacheKeys[1]);
            Assert.Contains("pageSize=5", capturedCacheKeys[1]);
            Assert.Contains("isCompleted=True", capturedCacheKeys[1]);
            Assert.Contains("searhTerm=supplier", capturedCacheKeys[1]);
            Assert.Contains("sortBy=title", capturedCacheKeys[1]);
            Assert.Contains("sortDirection=asc", capturedCacheKeys[1]);

            _repositoryMock.Verify(repository => repository.GetPagedAsync(It.IsAny<OperationTaskQueryParameters>()), Times.Exactly(2));

            _cacheServiceMock.Verify(cache => cache.SetAsync(
                It.IsAny<string>(),
                It.IsAny<PagedResult<OperationTaskDto>>(),
                TimeSpan.FromMinutes(1)),
                Times.Exactly(2));
        }

        #endregion
    }
}
