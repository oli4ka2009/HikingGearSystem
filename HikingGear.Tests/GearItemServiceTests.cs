using HikingGear.BLL.DTOs;
using HikingGear.BLL.Services;
using HikingGear.DAL.Repositories;
using HikingGear.Models.Entities;
using Moq;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace HikingGear.Tests
{
    public class GearItemServiceTests
    {
        private readonly Mock<IGearItemRepository> _gearItemRepoMock;
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<ITripRepository> _tripRepoMock;
        private readonly GearItemService _service;

        public GearItemServiceTests()
        {
            _gearItemRepoMock = new Mock<IGearItemRepository>();
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _tripRepoMock = new Mock<ITripRepository>();
            _service = new GearItemService(
                _gearItemRepoMock.Object,
                _categoryRepoMock.Object,
                _tripRepoMock.Object);
        }

        private void ValidateModel(object model)
        {
            var context = new ValidationContext(model, null, null);
            Validator.ValidateObject(model, context, true);
        }

        [Fact]
        public async Task AddCustomGearItemAsync_ValidData_ReturnsCreatedItem()
        {
            // Arrange
            var userId = 1;
            var dto = new GearItemCreateDto
            {
                CategoryId = 10,
                Name = "Tent",
                WeightInGrams = 2500,
                Quantity = 1,
                IsGroupGear = true,
                IsWearable = false
            };

            _categoryRepoMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(new GearCategory { Id = 10, TripId = 100 });
            _tripRepoMock.Setup(r => r.IsUserOwnerOfTripAsync(100, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AddCustomGearItemAsync(userId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.CategoryId, result.CategoryId);
            _gearItemRepoMock.Verify(r => r.AddItemsAsync(It.Is<IEnumerable<GearItem>>(items => items.First().Name == "Tent")), Times.Once);
        }

        [Fact]
        public async Task AddCustomGearItemAsync_CategoryNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GearCategory?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.AddCustomGearItemAsync(1, new GearItemCreateDto { CategoryId = 99 }));
        }

        [Fact]
        public async Task AddCustomGearItemAsync_UserNotOwner_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _categoryRepoMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(new GearCategory { Id = 10, TripId = 100 });
            _tripRepoMock.Setup(r => r.IsUserOwnerOfTripAsync(100, 1))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.AddCustomGearItemAsync(1, new GearItemCreateDto { CategoryId = 10 }));
        }

        [Fact]
        public void GearItemCreateDto_NegativeWeight_ValidationFails()
        {
            // Arrange
            var dto = new GearItemCreateDto
            {
                Name = "Backpack",
                WeightInGrams = -1,
                Quantity = 1
            };

            // Act & Assert
            Assert.Throws<ValidationException>(() => ValidateModel(dto));
        }

        [Fact]
        public void GearItemCreateDto_EmptyName_ValidationFails()
        {
            // Arrange
            var dto = new GearItemCreateDto
            {
                Name = "",
                WeightInGrams = 100,
                Quantity = 1
            };

            // Act & Assert
            Assert.Throws<ValidationException>(() => ValidateModel(dto));
        }

        [Fact]
        public async Task UpdateGearItemAsync_ItemNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _gearItemRepoMock.Setup(r => r.GetItemByIdAsync(It.IsAny<int>())).ReturnsAsync((GearItem?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.UpdateGearItemAsync(1, 1, new GearItemUpdateDto()));
        }

        [Fact]
        public async Task UpdateGearItemAsync_ValidUpdate_CallsRepositoryUpdate()
        {
            // Arrange
            var userId = 1;
            var itemId = 5;
            var existingItem = new GearItem { Id = itemId, CategoryId = 10, Name = "Old Name" };
            var dto = new GearItemUpdateDto
            {
                Name = "New Name",
                WeightInGrams = 500,
                Quantity = 2
            };

            _gearItemRepoMock.Setup(r => r.GetItemByIdAsync(itemId)).ReturnsAsync(existingItem);
            _categoryRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new GearCategory { Id = 10, TripId = 100 });
            _tripRepoMock.Setup(r => r.IsUserOwnerOfTripAsync(100, userId)).ReturnsAsync(true);

            // Act
            await _service.UpdateGearItemAsync(userId, itemId, dto);

            // Assert
            Assert.Equal("New Name", existingItem.Name);
            Assert.Equal(500, existingItem.WeightInGrams);
            _gearItemRepoMock.Verify(r => r.UpdateItemAsync(existingItem), Times.Once);
        }

        [Fact]
        public async Task GetPackingProgressAsync_TripNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _tripRepoMock.Setup(r => r.GetTripByIdAsync(It.IsAny<int>())).ReturnsAsync((Trip?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetPackingProgressAsync(1, 1));
        }

        [Fact]
        public async Task GetPackingProgressAsync_ValidData_CalculatesCorrectPercentages()
        {
            // Arrange
            var userId = 1;
            var tripId = 100;
            var trip = new Trip
            {
                Id = tripId,
                UserId = userId,
                Categories = new List<GearCategory>
                {
                    new GearCategory
                    {
                        GearItems = new List<GearItem>
                        {
                            new GearItem { IsPacked = true, IsGroupGear = false, WeightInGrams = 100, Quantity = 1 },
                            new GearItem { IsPacked = false, IsGroupGear = false, WeightInGrams = 200, Quantity = 1 },
                            new GearItem { IsPacked = true, IsGroupGear = true, WeightInGrams = 1000, Quantity = 1 }
                        }
                    }
                }
            };

            _tripRepoMock.Setup(r => r.GetTripByIdAsync(tripId)).ReturnsAsync(trip);

            // Act
            var result = await _service.GetPackingProgressAsync(userId, tripId);

            // Assert
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(2, result.PackedItems);
            Assert.Equal(66.67, result.ProgressPercentage);
            Assert.Equal(50, result.IndividualProgressPercentage);
            Assert.Equal(1000, result.GroupTotalWeightGrams);
        }
    }
}
