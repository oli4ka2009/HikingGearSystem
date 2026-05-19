using HikingGear.BLL.DTOs;
using HikingGear.BLL.Services;
using HikingGear.DAL.Repositories;
using HikingGear.Models.Entities;
using Moq;
using Xunit;

namespace HikingGear.Tests
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<ITripRepository> _tripRepoMock;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _tripRepoMock = new Mock<ITripRepository>();
            _service = new CategoryService(_categoryRepoMock.Object, _tripRepoMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidData_ReturnsCreatedCategory()
        {
            // Arrange
            var userId = 1;
            var dto = new CreateCategoryDto { Name = "Food", TripId = 100 };
            _tripRepoMock.Setup(r => r.IsUserOwnerOfTripAsync(100, userId)).ReturnsAsync(true);
            _categoryRepoMock.Setup(r => r.GetByTripAndNameAsync(100, "Food")).ReturnsAsync((GearCategory?)null);

            // Act
            var result = await _service.CreateAsync(userId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Food", result.Name);
            _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<GearCategory>()), Times.Once);
            _categoryRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_DuplicateName_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var dto = new CreateCategoryDto { Name = "Food", TripId = 100 };
            _tripRepoMock.Setup(r => r.IsUserOwnerOfTripAsync(100, userId)).ReturnsAsync(true);
            _categoryRepoMock.Setup(r => r.GetByTripAndNameAsync(100, "Food"))
                .ReturnsAsync(new GearCategory { Name = "Food" });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(userId, dto));
            Assert.Contains("вже існує", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_UserNotOwner_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = 1;
            var dto = new CreateCategoryDto { Name = "Food", TripId = 100 };
            _tripRepoMock.Setup(r => r.IsUserOwnerOfTripAsync(100, userId)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateAsync(userId, dto));
        }

        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            var userId = 1;
            var categoryId = 10;
            var category = new GearCategory { Id = categoryId, TripId = 100 };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
            _tripRepoMock.Setup(r => r.IsUserOwnerOfTripAsync(100, userId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(userId, categoryId);

            // Assert
            Assert.True(result);
            _categoryRepoMock.Verify(r => r.DeleteAsync(category), Times.Once);
            _categoryRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ReturnsFalse()
        {
            // Arrange
            _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GearCategory?)null);

            // Act
            var result = await _service.DeleteAsync(1, 999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_UserNotOwner_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = 1;
            var categoryId = 10;
            var category = new GearCategory { Id = categoryId, TripId = 100 };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(category);
            _tripRepoMock.Setup(r => r.IsUserOwnerOfTripAsync(100, userId)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteAsync(userId, categoryId));
        }

        [Fact]
        public async Task UpdateAsync_DuplicateName_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var categoryId = 10;
            var existingCategory = new GearCategory { Id = categoryId, Name = "Old Name", TripId = 100 };
            var dto = new UpdateCategoryDto { Name = "New Name" };

            _categoryRepoMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
            _tripRepoMock.Setup(r => r.IsUserOwnerOfTripAsync(100, userId)).ReturnsAsync(true);
            _categoryRepoMock.Setup(r => r.GetByTripAndNameAsync(100, "New Name"))
                .ReturnsAsync(new GearCategory { Name = "New Name" });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(userId, categoryId, dto));
        }
    }
}
