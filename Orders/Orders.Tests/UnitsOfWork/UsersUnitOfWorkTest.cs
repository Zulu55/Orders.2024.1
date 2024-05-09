using Microsoft.AspNetCore.Identity;
using Moq;
using Orders.Backend.Repositories.Interfaces;
using Orders.Backend.UnitsOfWork.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Shared.Responses;

namespace Orders.Tests.UnitsOfWork
{
    [TestClass]
    public class UsersUnitOfWorkTest
    {
        private readonly Mock<IUsersRepository> _mockUsersRepository = new Mock<IUsersRepository>();
        private readonly UsersUnitOfWork _usersUnitOfWork;

        public UsersUnitOfWorkTest()
        {
            _usersUnitOfWork = new UsersUnitOfWork(_mockUsersRepository.Object);
        }

        [TestMethod]
        public async Task AddUserAsync_ShouldReturnSuccess()
        {
            // Arrange
            var user = new User();
            var password = "TestPassword123";
            var expectedResult = IdentityResult.Success;
            _mockUsersRepository.Setup(repo => repo.AddUserAsync(user, password))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.AddUserAsync(user, password);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.AddUserAsync(user, password), Times.Once);
        }

        [TestMethod]
        public async Task AddUserAsync_ShouldReturnFailure()
        {
            // Arrange
            var user = new User();
            var password = "TestPassword123";
            var expectedResult = IdentityResult.Failed(new IdentityError());
            _mockUsersRepository.Setup(repo => repo.AddUserAsync(user, password))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.AddUserAsync(user, password);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.AddUserAsync(user, password), Times.Once);
        }

        [TestMethod]
        public async Task AddUserToRoleAsync_CallsRepositoryMethod()
        {
            // Arrange
            var user = new User();
            var roleName = "TestRole";
            _mockUsersRepository.Setup(repo => repo.AddUserToRoleAsync(user, roleName))
                                .Returns(Task.CompletedTask);

            // Act
            await _usersUnitOfWork.AddUserToRoleAsync(user, roleName);

            // Assert
            _mockUsersRepository.Verify(repo => repo.AddUserToRoleAsync(user, roleName), Times.Once);
        }

        [TestMethod]
        public async Task CheckRoleAsync_CallsRepositoryMethod()
        {
            // Arrange
            var roleName = "TestRole";
            _mockUsersRepository.Setup(repo => repo.CheckRoleAsync(roleName))
                                .Returns(Task.CompletedTask);

            // Act
            await _usersUnitOfWork.CheckRoleAsync(roleName);

            // Assert
            _mockUsersRepository.Verify(repo => repo.CheckRoleAsync(roleName), Times.Once);
        }

        [TestMethod]
        public async Task GetUserAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var email = "test@example.com";
            var expectedUser = new User { Email = email };
            _mockUsersRepository.Setup(repo => repo.GetUserAsync(email))
                                .ReturnsAsync(expectedUser);

            // Act
            var result = await _usersUnitOfWork.GetUserAsync(email);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUser, result);
            _mockUsersRepository.Verify(repo => repo.GetUserAsync(email), Times.Once);
        }

        [TestMethod]
        public async Task GetUserAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@example.com";

            // Act
            var result = await _usersUnitOfWork.GetUserAsync(email);

            // Assert
            Assert.IsNull(result);
            _mockUsersRepository.Verify(repo => repo.GetUserAsync(email), Times.Once);
        }

        [TestMethod]
        public async Task GetUserGuidAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedUser = new User { Id = userId.ToString() };
            _mockUsersRepository.Setup(repo => repo.GetUserAsync(userId))
                                .ReturnsAsync(expectedUser);

            // Act
            var result = await _usersUnitOfWork.GetUserAsync(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUser, result);
            _mockUsersRepository.Verify(repo => repo.GetUserAsync(userId), Times.Once);
        }

        [TestMethod]
        public async Task GetUserGuidAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            // Act
            var result = await _usersUnitOfWork.GetUserAsync(userId);

            // Assert
            Assert.IsNull(result);
            _mockUsersRepository.Verify(repo => repo.GetUserAsync(userId), Times.Once);
        }

        [TestMethod]
        public async Task ChangePasswordAsync_ReturnsSuccess_WhenPasswordChanged()
        {
            // Arrange
            var user = new User();
            var currentPassword = "CurrentPassword123";
            var newPassword = "NewPassword123";
            var expectedResult = IdentityResult.Success;
            _mockUsersRepository.Setup(repo => repo.ChangePasswordAsync(user, currentPassword, newPassword))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.ChangePasswordAsync(user, currentPassword, newPassword);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.ChangePasswordAsync(user, currentPassword, newPassword), Times.Once);
        }

        [TestMethod]
        public async Task ChangePasswordAsync_ReturnsFailure_WhenPasswordChangeFails()
        {
            // Arrange
            var user = new User();
            var currentPassword = "CurrentPassword123";
            var newPassword = "NewPassword123";
            var expectedResult = IdentityResult.Failed(new IdentityError { Description = "Password change failed." });
            _mockUsersRepository.Setup(repo => repo.ChangePasswordAsync(user, currentPassword, newPassword))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.ChangePasswordAsync(user, currentPassword, newPassword);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.ChangePasswordAsync(user, currentPassword, newPassword), Times.Once);
        }

        [TestMethod]
        public async Task UpdateUserAsync_ReturnsSuccess_WhenUpdateIsSuccessful()
        {
            // Arrange
            var user = new User();
            var expectedResult = IdentityResult.Success;
            _mockUsersRepository.Setup(repo => repo.UpdateUserAsync(user))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.UpdateUserAsync(user);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.UpdateUserAsync(user), Times.Once);
        }

        [TestMethod]
        public async Task UpdateUserAsync_ReturnsFailure_WhenUpdateFails()
        {
            // Arrange
            var user = new User();
            var expectedResult = IdentityResult.Failed(new IdentityError { Description = "Update failed." });
            _mockUsersRepository.Setup(repo => repo.UpdateUserAsync(user))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.UpdateUserAsync(user);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.UpdateUserAsync(user), Times.Once);
        }

        [TestMethod]
        public async Task IsUserInRoleAsync_ReturnsTrue_WhenUserIsInRole()
        {
            // Arrange
            var user = new User();
            var roleName = "TestRole";
            _mockUsersRepository.Setup(repo => repo.IsUserInRoleAsync(user, roleName))
                                .ReturnsAsync(true);

            // Act
            var result = await _usersUnitOfWork.IsUserInRoleAsync(user, roleName);

            // Assert
            Assert.IsTrue(result);
            _mockUsersRepository.Verify(repo => repo.IsUserInRoleAsync(user, roleName), Times.Once);
        }

        [TestMethod]
        public async Task IsUserInRoleAsync_ReturnsFalse_WhenUserIsNotInRole()
        {
            // Arrange
            var user = new User();
            var roleName = "TestRole";
            _mockUsersRepository.Setup(repo => repo.IsUserInRoleAsync(user, roleName))
                                .ReturnsAsync(false);

            // Act
            var result = await _usersUnitOfWork.IsUserInRoleAsync(user, roleName);

            // Assert
            Assert.IsFalse(result);
            _mockUsersRepository.Verify(repo => repo.IsUserInRoleAsync(user, roleName), Times.Once);
        }

        [TestMethod]
        public async Task LoginAsync_ReturnsSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            var loginModel = new LoginDTO();
            var expectedResult = SignInResult.Success;
            _mockUsersRepository.Setup(repo => repo.LoginAsync(loginModel))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.LoginAsync(loginModel);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.LoginAsync(loginModel), Times.Once);
        }

        [TestMethod]
        public async Task LoginAsync_ReturnsFailed_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginModel = new LoginDTO();
            var expectedResult = SignInResult.Failed;
            _mockUsersRepository.Setup(repo => repo.LoginAsync(loginModel))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.LoginAsync(loginModel);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.LoginAsync(loginModel), Times.Once);
        }

        [TestMethod]
        public async Task LogoutAsync_CallsRepositoryMethod()
        {
            // Arrange
            _mockUsersRepository.Setup(repo => repo.LogoutAsync())
                                .Returns(Task.CompletedTask);

            // Act
            await _usersUnitOfWork.LogoutAsync();

            // Assert
            _mockUsersRepository.Verify(repo => repo.LogoutAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GenerateEmailConfirmationTokenAsync_GeneratesTokenForUser()
        {
            // Arrange
            var user = new User();
            var expectedToken = "test-token";
            _mockUsersRepository.Setup(repo => repo.GenerateEmailConfirmationTokenAsync(user))
                                .ReturnsAsync(expectedToken);

            // Act
            var result = await _usersUnitOfWork.GenerateEmailConfirmationTokenAsync(user);

            // Assert
            Assert.AreEqual(expectedToken, result);
            _mockUsersRepository.Verify(repo => repo.GenerateEmailConfirmationTokenAsync(user), Times.Once);
        }

        [TestMethod]
        public async Task ConfirmEmailAsync_ReturnsSuccess_WhenEmailConfirmationIsSuccessful()
        {
            // Arrange
            var user = new User();
            var token = "confirmation-token";
            var expectedResult = IdentityResult.Success;
            _mockUsersRepository.Setup(repo => repo.ConfirmEmailAsync(user, token))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.ConfirmEmailAsync(user, token);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.ConfirmEmailAsync(user, token), Times.Once);
        }

        [TestMethod]
        public async Task ConfirmEmailAsync_ReturnsFailure_WhenEmailConfirmationFails()
        {
            // Arrange
            var user = new User();
            var token = "invalid-token";
            var expectedResult = IdentityResult.Failed(new IdentityError { Description = "Email confirmation failed." });
            _mockUsersRepository.Setup(repo => repo.ConfirmEmailAsync(user, token))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.ConfirmEmailAsync(user, token);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.ConfirmEmailAsync(user, token), Times.Once);
        }

        [TestMethod]
        public async Task GeneratePasswordResetTokenAsync_GeneratesTokenForUser()
        {
            // Arrange
            var user = new User();
            var expectedToken = "reset-token";
            _mockUsersRepository.Setup(repo => repo.GeneratePasswordResetTokenAsync(user))
                                .ReturnsAsync(expectedToken);

            // Act
            var result = await _usersUnitOfWork.GeneratePasswordResetTokenAsync(user);

            // Assert
            Assert.AreEqual(expectedToken, result);
            _mockUsersRepository.Verify(repo => repo.GeneratePasswordResetTokenAsync(user), Times.Once);
        }

        [TestMethod]
        public async Task ResetPasswordAsync_ReturnsSuccess_WhenPasswordResetIsSuccessful()
        {
            // Arrange
            var user = new User();
            var token = "valid-token";
            var newPassword = "NewPassword123";
            var expectedResult = IdentityResult.Success;
            _mockUsersRepository.Setup(repo => repo.ResetPasswordAsync(user, token, newPassword))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.ResetPasswordAsync(user, token, newPassword);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.ResetPasswordAsync(user, token, newPassword), Times.Once);
        }

        [TestMethod]
        public async Task ResetPasswordAsync_ReturnsFailure_WhenPasswordResetFails()
        {
            // Arrange
            var user = new User();
            var token = "invalid-token";
            var newPassword = "NewPassword123";
            var expectedResult = IdentityResult.Failed(new IdentityError { Description = "Password reset failed." });
            _mockUsersRepository.Setup(repo => repo.ResetPasswordAsync(user, token, newPassword))
                                .ReturnsAsync(expectedResult);

            // Act
            var result = await _usersUnitOfWork.ResetPasswordAsync(user, token, newPassword);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockUsersRepository.Verify(repo => repo.ResetPasswordAsync(user, token, newPassword), Times.Once);
        }

        [TestMethod]
        public async Task GetAsync_WithPagination_ReturnsUsers()
        {
            // Arrange
            var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
            var response = new ActionResponse<IEnumerable<User>> { WasSuccess = true };
            _mockUsersRepository.Setup(repo => repo.GetAsync(pagination))
                .ReturnsAsync(response);

            // Act
            var result = await _usersUnitOfWork.GetAsync(pagination);

            // Assert
            Assert.AreEqual(response, result);
            _mockUsersRepository.Verify(repo => repo.GetAsync(pagination), Times.Once);
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_WithPagination_ReturnsTotalPages()
        {
            // Arrange
            var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10 };
            var response = new ActionResponse<int> { WasSuccess = true, Result = 5 };
            _mockUsersRepository.Setup(repo => repo.GetTotalPagesAsync(pagination))
                .ReturnsAsync(response);

            // Act
            var result = await _usersUnitOfWork.GetTotalPagesAsync(pagination);

            // Assert
            Assert.AreEqual(response, result);
            _mockUsersRepository.Verify(repo => repo.GetTotalPagesAsync(pagination), Times.Once);
        }
    }
}