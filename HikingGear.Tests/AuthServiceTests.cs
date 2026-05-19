using HikingGear.BLL.DTOs;
using HikingGear.BLL.Services;
using HikingGear.DAL.Repositories;
using HikingGear.Models.Entities;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace HikingGear.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _configMock = new Mock<IConfiguration>();

            // Mocking configuration for JWT
            _configMock.Setup(c => c["Jwt:Key"]).Returns("super-secret-key-that-is-at-least-32-characters-long");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
            _configMock.Setup(c => c["Jwt:Audience"]).Returns("test-audience");

            _service = new AuthService(_userRepoMock.Object, _configMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_EmailAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new UserRegisterDto { Email = "test@example.com", Password = "password123" };
            _userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(new User { Email = dto.Email });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAsync(dto));
            Assert.Equal("Користувач з таким Email вже зареєстрований.", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_ValidData_ReturnsSuccess()
        {
            // Arrange
            var dto = new UserRegisterDto { Email = "new@example.com", Password = "password123" };
            _userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((User?)null);

            // Act
            var result = await _service.RegisterAsync(dto);

            // Assert
            Assert.Contains("успішно", result.Message);
            _userRepoMock.Verify(r => r.AddUserAsync(It.Is<User>(u => u.Email == dto.Email)), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dto = new UserLoginDto { Email = "nonexistent@example.com", Password = "any" };
            _userRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((User?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(dto));
            Assert.Equal("Невірний емейл або пароль", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var email = "user@example.com";
            var correctPassword = "CorrectPassword123";
            var wrongPassword = "WrongPassword123";
            
            var user = new User 
            { 
                Email = email, 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword) 
            };

            _userRepoMock.Setup(r => r.GetUserByEmailAsync(email)).ReturnsAsync(user);

            var dto = new UserLoginDto { Email = email, Password = wrongPassword };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(dto));
            Assert.Equal("Невірний емейл або пароль", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var email = "user@example.com";
            var password = "CorrectPassword123";
            
            var user = new User 
            { 
                Id = 1,
                Email = email, 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password) 
            };

            _userRepoMock.Setup(r => r.GetUserByEmailAsync(email)).ReturnsAsync(user);

            var dto = new UserLoginDto { Email = email, Password = password };

            // Act
            var result = await _service.LoginAsync(dto);

            // Assert
            Assert.NotNull(result.Token);
            Assert.Contains("Успішний вхід", result.Message);
        }
    }
}
