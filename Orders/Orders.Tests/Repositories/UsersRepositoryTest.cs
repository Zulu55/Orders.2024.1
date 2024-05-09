using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orders.Backend.Data;
using Orders.Backend.Repositories.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;

namespace Orders.Tests.Repositories;

[TestClass]
public class UsersRepositoryTests
{
    private DataContext _context = null!;
    private UsersRepository _usersRepository = null!;
    private Mock<UserManager<User>> _mockUserManager = null!;
    private Mock<RoleManager<IdentityRole>> _mockRoleManager = null!;
    private Mock<SignInManager<User>> _mockSignInManager = null!;
    private readonly Guid _guid = Guid.NewGuid();

    [TestInitialize]
    public void SetUp()
    {
        // Initialize the in-memory database
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new DataContext(options);

        // Mock the UserManager, RoleManager, SignInManager
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
        _mockRoleManager = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null, null, null, null);
        var optionsAccessorMock = new Mock<IOptions<IdentityOptions>>();
        var loggerMock = new Mock<ILogger<SignInManager<User>>>();
        var authenticationSchemeProviderMock = new Mock<IAuthenticationSchemeProvider>();
        var userConfirmationMock = new Mock<IUserConfirmation<User>>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
        _mockSignInManager = new Mock<SignInManager<User>>(
            _mockUserManager.Object,
            httpContextAccessorMock.Object,
            claimsFactoryMock.Object,
            optionsAccessorMock.Object,
            loggerMock.Object,
            authenticationSchemeProviderMock.Object,
            userConfirmationMock.Object);
        _usersRepository = new UsersRepository(_context, _mockUserManager.Object, _mockRoleManager.Object, _mockSignInManager.Object);

        PopulateDatabase();
    }

    [TestCleanup]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [TestMethod]
    public async Task GetAsync_WithEmail_UserExists_ReturnsUser()
    {
        // Arrange
        var email = "john.doe@example.com";

        // Act
        var user = await _usersRepository.GetUserAsync(email);

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual("John", user.FirstName);
    }

    [TestMethod]
    public async Task GetAsync_WithEmail_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        // Act
        var user = await _usersRepository.GetUserAsync(email);

        // Assert
        Assert.IsNull(user);
    }

    [TestMethod]
    public async Task GetAsync_WithUserId_UserExists_ReturnsUser()
    {
        // Act
        var user = await _usersRepository.GetUserAsync(_guid);

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual("Jane", user.FirstName);
    }

    [TestMethod]
    public async Task GetAsync_WithUserId_UserDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var user = await _usersRepository.GetUserAsync(userId);

        // Assert
        Assert.IsNull(user);
    }

    [TestMethod]
    public async Task GetAsync_WithPagination_ReturnsUsers()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10, Filter = "J" };

        // Act
        var result = await _usersRepository.GetAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(2, result.Result.Count());
    }

    [TestMethod]
    public async Task GetTotalPagesAsync_WithPagination_ReturnsTotalPages()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 1, Filter = "J" };

        // Act
        var result = await _usersRepository.GetTotalPagesAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(2, result.Result);
    }

    [TestMethod]
    public async Task GetTotalPagesAsync_WithFilter_ReturnsFilteredTotalPages()
    {
        // Arrange
        var pagination = new PaginationDTO { Page = 1, RecordsNumber = 10, Filter = "John" };

        // Act
        var result = await _usersRepository.GetTotalPagesAsync(pagination);

        // Assert
        Assert.IsTrue(result.WasSuccess);
        Assert.AreEqual(1, result.Result);
    }

    [TestMethod]
    public async Task GeneratePasswordResetTokenAsync_ReturnsToken()
    {
        // Arrange
        var user = new User();
        var expectedToken = "fake-reset-token";
        _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _usersRepository.GeneratePasswordResetTokenAsync(user);

        // Assert
        Assert.AreEqual(expectedToken, result);
        _mockUserManager.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once());
    }

    [TestMethod]
    public async Task ResetPasswordAsync_ReturnsIdentityResult()
    {
        // Arrange
        var user = new User();
        var token = "valid-token";
        var newPassword = "newPassword123!";
        var expectedResult = IdentityResult.Success;

        _mockUserManager.Setup(x => x.ResetPasswordAsync(user, token, newPassword))
                    .ReturnsAsync(expectedResult);

        // Act
        var result = await _usersRepository.ResetPasswordAsync(user, token, newPassword);

        // Assert
        Assert.AreEqual(expectedResult, result);
        _mockUserManager.Verify(x => x.ResetPasswordAsync(user, token, newPassword), Times.Once());
    }

    [TestMethod]
    public async Task GenerateEmailConfirmationTokenAsync_ReturnsToken()
    {
        // Arrange
        var user = new User();
        var expectedToken = "email-confirm-token";

        _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _usersRepository.GenerateEmailConfirmationTokenAsync(user);

        // Assert
        Assert.AreEqual(expectedToken, result);
        _mockUserManager.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
    }

    [TestMethod]
    public async Task ConfirmEmailAsync_ReturnsIdentityResult()
    {
        // Arrange
        var user = new User();
        var token = "valid-token";
        var expectedResult = IdentityResult.Success;

        _mockUserManager.Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _usersRepository.ConfirmEmailAsync(user, token);

        // Assert
        Assert.AreEqual(expectedResult, result);
        _mockUserManager.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once());
    }

    [TestMethod]
    public async Task AddUserAsync_ReturnsIdentityResult()
    {
        // Arrange
        var user = new User();
        var password = "StrongPassword123!";
        var expectedResult = IdentityResult.Success;

        _mockUserManager.Setup(x => x.CreateAsync(user, password))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _usersRepository.AddUserAsync(user, password);

        // Assert
        Assert.AreEqual(expectedResult, result);
        _mockUserManager.Verify(x => x.CreateAsync(user, password), Times.Once());
    }

    [TestMethod]
    public async Task AddUserToRoleAsync_CallsAddToRoleAsync()
    {
        // Arrange
        var user = new User();
        var roleName = "Admin";
        var expectedResult = IdentityResult.Success;

        _mockUserManager.Setup(x => x.AddToRoleAsync(user, roleName))
            .ReturnsAsync(expectedResult);

        // Act
        await _usersRepository.AddUserToRoleAsync(user, roleName);

        // Assert
        _mockUserManager.Verify(x => x.AddToRoleAsync(user, roleName), Times.Once());
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ReturnsIdentityResult()
    {
        // Arrange
        var user = new User();
        var currentPassword = "CurrentPassword123!";
        var newPassword = "NewPassword123!";

        var expectedResult = IdentityResult.Success;
        _mockUserManager.Setup(x => x.ChangePasswordAsync(user, currentPassword, newPassword))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _usersRepository.ChangePasswordAsync(user, currentPassword, newPassword);

        // Assert
        Assert.AreEqual(expectedResult, result);
        _mockUserManager.Verify(x => x.ChangePasswordAsync(user, currentPassword, newPassword), Times.Once());
    }

    [TestMethod]
    public async Task CheckRoleAsync_RoleExists_DoesNothing()
    {
        // Arrange
        var roleName = "Admin";
        _mockRoleManager.Setup(x => x.RoleExistsAsync(roleName))
            .ReturnsAsync(true);

        // Act
        await _usersRepository.CheckRoleAsync(roleName);

        // Assert
        _mockRoleManager.Verify(x => x.RoleExistsAsync(roleName), Times.Once());
        _mockRoleManager.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Never());
    }

    [TestMethod]
    public async Task CheckRoleAsync_RoleDoesNotExist_CreatesRole()
    {
        // Arrange
        var roleName = "Admin";
        _mockRoleManager.Setup(x => x.RoleExistsAsync(roleName))
            .ReturnsAsync(false);
        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _usersRepository.CheckRoleAsync(roleName);

        // Assert
        _mockRoleManager.Verify(x => x.RoleExistsAsync(roleName), Times.Once());
        _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == roleName)), Times.Once());
    }

    [TestMethod]
    public async Task IsUserInRoleAsync_UserIsInRole_ReturnsTrue()
    {
        // Arrange
        var user = new User();
        var roleName = "Admin";
        _mockUserManager.Setup(x => x.IsInRoleAsync(user, roleName))
            .ReturnsAsync(true);

        // Act
        var result = await _usersRepository.IsUserInRoleAsync(user, roleName);

        // Assert
        Assert.IsTrue(result);
        _mockUserManager.Verify(x => x.IsInRoleAsync(user, roleName), Times.Once());
    }

    [TestMethod]
    public async Task IsUserInRoleAsync_UserIsNotInRole_ReturnsFalse()
    {
        // Arrange
        var user = new User();
        var roleName = "Admin";
        _mockUserManager.Setup(x => x.IsInRoleAsync(user, roleName)).ReturnsAsync(false);

        // Act
        var result = await _usersRepository.IsUserInRoleAsync(user, roleName);

        // Assert
        Assert.IsFalse(result);
        _mockUserManager.Verify(x => x.IsInRoleAsync(user, roleName), Times.Once());
    }

    [TestMethod]
    public async Task LoginAsync_ValidCredentials_ReturnsSignInResultSuccess()
    {
        // Arrange
        var model = new LoginDTO { Email = "user@example.com", Password = "password123" };
        _mockSignInManager.Setup(x => x.PasswordSignInAsync(model.Email, model.Password, false, true))
            .ReturnsAsync(SignInResult.Success);

        // Act
        var result = await _usersRepository.LoginAsync(model);

        // Assert
        Assert.IsTrue(result.Succeeded);
        _mockSignInManager.Verify(x => x.PasswordSignInAsync(model.Email, model.Password, false, true), Times.Once());
    }

    [TestMethod]
    public async Task LoginAsync_InvalidCredentials_ReturnsSignInResultFailed()
    {
        // Arrange
        var model = new LoginDTO { Email = "user@example.com", Password = "wrongPassword" };
        _mockSignInManager.Setup(x => x.PasswordSignInAsync(model.Email, model.Password, false, true))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _usersRepository.LoginAsync(model);

        // Assert
        Assert.IsFalse(result.Succeeded);
        _mockSignInManager.Verify(x => x.PasswordSignInAsync(model.Email, model.Password, false, true), Times.Once());
    }

    [TestMethod]
    public async Task LogoutAsync_CallsSignOutAsync()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.SignOutAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _usersRepository.LogoutAsync();

        // Assert
        _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once());
    }

    [TestMethod]
    public async Task UpdateUserAsync_UserUpdated_ReturnsIdentityResultSuccess()
    {
        // Arrange
        var user = new User();
        var expectedResult = IdentityResult.Success;
        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _usersRepository.UpdateUserAsync(user);

        // Assert
        Assert.AreEqual(expectedResult, result);
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once());
    }

    [TestMethod]
    public async Task UpdateUserAsync_UserUpdateFailed_ReturnsIdentityResultFailed()
    {
        // Arrange
        var user = new User();
        var expectedResult = IdentityResult.Failed();
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(expectedResult);

        // Act
        var result = await _usersRepository.UpdateUserAsync(user);

        // Assert
        Assert.AreEqual(expectedResult, result);
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once());
    }

    private void PopulateDatabase()
    {
        var country = new Country
        {
            Name = "Country",
            States = new List<State>
            {
                new State
                {
                    Name = "State",
                    Cities = new List<City>
                    {
                        new City { Name = "City" }
                    }
                }
            }
        };
        _context.Countries.Add(country);
        _context.SaveChanges();

        var user1 = new User { Id = "1", FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Address = "Some", Document = "Any", CityId = 1 };
        var user2 = new User { Id = _guid.ToString(), FirstName = "Jane", LastName = "Doe", Email = "jane.doe@example.com", Address = "Some", Document = "Any", CityId = 1 };
        _context.Users.AddRange(user1, user2);
        _context.SaveChanges();
    }
}