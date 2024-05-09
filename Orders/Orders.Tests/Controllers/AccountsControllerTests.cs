using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using Orders.Backend.Controllers;
using Orders.Backend.Helpers;
using Orders.Backend.Repositories.Interfaces;
using Orders.Backend.UnitsOfWork.Interfaces;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Shared.Enums;
using Orders.Shared.Responses;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Orders.Tests.Controllers
{
    [TestClass]
    public class AccountsControllerTests
    {
        private Mock<IUsersUnitOfWork> _mockUsersUnitOfWork = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private Mock<IFileStorage> _mockFileStorage = null!;
        private Mock<IMailHelper> _mockMailHelper = null!;
        private Mock<IUsersRepository> _mockUsersRepository = null!;
        private AccountsController _controller = null!;

        private const string _container = "userphotos";
        private const string _string64base = "U29tZVZhbGlkQmFzZTY0U3RyaW5n";

        [TestInitialize]
        public void Initialize()
        {
            _mockUsersUnitOfWork = new Mock<IUsersUnitOfWork>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockFileStorage = new Mock<IFileStorage>();
            _mockMailHelper = new Mock<IMailHelper>();
            _mockUsersRepository = new Mock<IUsersRepository>();

            _mockConfiguration
                .SetupGet(x => x["Url Frontend"])
                .Returns("http://frontend-url.com");
            _mockConfiguration
                .SetupGet(x => x["jwtKey"])
                .Returns("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns("http://generated-link.com");

            _controller = new AccountsController(
                _mockUsersUnitOfWork.Object,
                _mockConfiguration.Object,
                _mockFileStorage.Object,
                _mockMailHelper.Object,
                _mockUsersRepository.Object)
            {
                Url = mockUrlHelper.Object
            };

            var mockHttpContext = new Mock<HttpContext>();
            var mockHttpRequest = new Mock<HttpRequest>();

            mockHttpRequest.Setup(req => req.Scheme)
                .Returns("http");
            mockHttpContext.Setup(ctx => ctx.Request)
                .Returns(mockHttpRequest.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test@example.com"),
            }, "mock"));
            _controller.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnOk_WhenUsersAreFound()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var response = new ActionResponse<IEnumerable<User>> { WasSuccess = true };
            _mockUsersRepository.Setup(x => x.GetAsync(pagination))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAsync(pagination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockUsersRepository.Verify(x => x.GetAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnBadRequest_WhenUsersAreNotFound()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var response = new ActionResponse<IEnumerable<User>> { WasSuccess = false };
            _mockUsersRepository.Setup(x => x.GetAsync(pagination))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAsync(pagination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockUsersRepository.Verify(x => x.GetAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_ShouldReturnOk_WhenTotalPagesAreSuccessfullyRetrieved()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var response = new ActionResponse<int> { WasSuccess = true, Result = 5 };
            _mockUsersRepository.Setup(x => x.GetTotalPagesAsync(pagination))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetPagesAsync(pagination);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(5, okResult.Value);
            _mockUsersRepository.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_ShouldReturnBadRequest_WhenUnableToRetrieveTotalPages()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var response = new ActionResponse<int> { WasSuccess = false };
            _mockUsersRepository.Setup(x => x.GetTotalPagesAsync(pagination))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetPagesAsync(pagination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            _mockUsersRepository.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task CreateUser_ShouldReturnNoContent_WhenUserIsCreatedSuccessfully()
        {
            // Arrange
            var userDTO = new UserDTO
            {
                Password = "password123",
                Photo = _string64base,
                Address = "Some",
                CityId = 1,
                Document = "Any",
                Email = "Some",
                FirstName = "Test",
                Id = "123",
                LastName = "Test",
                PasswordConfirm = "password123",
                PhoneNumber = "Any",
                UserName = "Test",
                UserType = UserType.User
            };
            var user = new User();
            _mockFileStorage.Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", _container))
                .ReturnsAsync("photoUrl");
            _mockUsersUnitOfWork.Setup(x => x.AddUserAsync(It.IsAny<User>(), userDTO.Password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUsersUnitOfWork.Setup(x => x.AddUserToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockUsersUnitOfWork.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("token");

            var response = new ActionResponse<string> { WasSuccess = true };
            _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(response);

            // Act
            var result = await _controller.CreateUser(userDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockUsersUnitOfWork.Verify(x => x.AddUserAsync(It.IsAny<User>(), userDTO.Password), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.AddUserToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()), Times.Once());
            _mockMailHelper.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task CreateUser_ShouldReturnBadRequest_WhenUserCreationFails()
        {
            // Arrange
            var userDTO = new UserDTO();
            var identityErrors = new List<IdentityError> { new IdentityError { Description = "User creation failed" } };
            _mockUsersUnitOfWork.Setup(x => x.AddUserAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.CreateUser(userDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockUsersUnitOfWork.Verify(x => x.AddUserAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task CreateUser_ShouldReturnBadRequest_WhenEmailNotSent()
        {
            // Arrange
            var userDTO = new UserDTO { Password = "password123", Photo = _string64base };
            var user = new User();
            _mockFileStorage.Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", _container))
                .ReturnsAsync("photoUrl");
            _mockUsersUnitOfWork.Setup(x => x.AddUserAsync(It.IsAny<User>(), userDTO.Password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUsersUnitOfWork.Setup(x => x.AddUserToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockUsersUnitOfWork.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("token");

            var response = new ActionResponse<string> { WasSuccess = false };
            _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(response);

            // Act
            var result = await _controller.CreateUser(userDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockUsersUnitOfWork.Verify(x => x.AddUserAsync(It.IsAny<User>(), userDTO.Password), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.AddUserToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()), Times.Once());
            _mockMailHelper.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task ConfirmEmailAsync_UserNotFound_ReturnsNotFound()
        {
            // Act
            var result = await _controller.ConfirmEmailAsync(Guid.NewGuid().ToString(), "token");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ConfirmEmailAsync_InvalidToken_ReturnsBadRequest()
        {
            // Arrange
            var user = new User();
            var message = "Invalid token";
            var token = "token";
            var identityErrors = new List<IdentityError> { new IdentityError { Description = message } };

            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<Guid>()))
                .ReturnsAsync(user);
            _mockUsersUnitOfWork.Setup(x => x.ConfirmEmailAsync(user, token.Replace(" ", "+")))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.ConfirmEmailAsync(Guid.NewGuid().ToString(), token);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(It.IsAny<Guid>()), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.ConfirmEmailAsync(user, token.Replace(" ", "+")), Times.Once());
        }

        [TestMethod]
        public async Task ConfirmEmailAsync_ValidToken_ReturnsNoContent()
        {
            // Arrange
            var user = new User();
            var token = "token";
            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<Guid>()))
                .ReturnsAsync(user);
            _mockUsersUnitOfWork.Setup(x => x.ConfirmEmailAsync(user, token.Replace(" ", "+")))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ConfirmEmailAsync(Guid.NewGuid().ToString(), token);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(It.IsAny<Guid>()), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.ConfirmEmailAsync(user, token.Replace(" ", "+")), Times.Once());
        }

        [TestMethod]
        public async Task Login_Success_ReturnsOk()
        {
            // Arrange
            var user = new User
            {
                Email = "some@yopmail.com",
                UserType = UserType.User,
                Document = "123",
                FirstName = "John",
                LastName = "Doe",
                Address = "Any",
                Photo = _string64base,
                CityId = 1
            };
            var loginModel = new LoginDTO { Email = user.Email, Password = "123456" };

            _mockUsersUnitOfWork.Setup(x => x.LoginAsync(loginModel))
                .ReturnsAsync(SignInResult.Success);

            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(user.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.LoginAsync(loginModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockUsersUnitOfWork.Verify(x => x.LoginAsync(loginModel), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(user.Email), Times.Once());
        }

        [TestMethod]
        public async Task Login_LockedOut_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "test@test.com", Password = "Test1234!" };
            _mockUsersUnitOfWork.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(SignInResult.LockedOut);

            // Act
            var result = await _controller.LoginAsync(loginDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Ha superado el máximo número de intentos, su cuenta está bloqueada, intente de nuevo en 5 minutos.", badRequestResult.Value);
            _mockUsersUnitOfWork.Verify(x => x.LoginAsync(loginDto), Times.Once());
        }

        [TestMethod]
        public async Task Login_NotAllowed_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "test@test.com", Password = "Test1234!" };
            _mockUsersUnitOfWork.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(SignInResult.NotAllowed);

            // Act
            var result = await _controller.LoginAsync(loginDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("El usuario no ha sido habilitado, debes de seguir las instrucciones del correo enviado para poder habilitar el usuario.", badRequestResult.Value);
            _mockUsersUnitOfWork.Verify(x => x.LoginAsync(loginDto), Times.Once());
        }

        [TestMethod]
        public async Task Login_InvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "test@test.com", Password = "Test1234!" };
            _mockUsersUnitOfWork.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _controller.LoginAsync(loginDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Email o contraseña incorrectos.", badRequestResult.Value);
            _mockUsersUnitOfWork.Verify(x => x.LoginAsync(loginDto), Times.Once());
        }

        [TestMethod]
        public async Task PutAsync_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userName = "testuser";
            _controller.ControllerContext = GetControllerContext(userName);

            // Act
            var result = await _controller.PutAsync(new User());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutAsync_ExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var message = "Test exception";
            var userName = "testuser";
            _controller.ControllerContext = GetControllerContext(userName);
            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(userName))
                .Throws(new Exception(message));

            // Act
            var result = await _controller.PutAsync(new User());
            var badRequestResult = result as BadRequestObjectResult;

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual(message, badRequestResult!.Value);
        }

        [TestMethod]
        public async Task PutAsync_UserPhotoNotEmpty_UpdatesPhoto()
        {
            // Arrange
            var user = new User
            {
                Email = "some@yopmail.com",
                UserType = UserType.User,
                Document = "123",
                FirstName = "John",
                LastName = "Doe",
                Address = "Any",
                Photo = _string64base,
                CityId = 1
            };
            var currentUser = new User
            {
                Email = "some@yopmail.com",
                UserType = UserType.User,
                Document = "123",
                FirstName = "John",
                LastName = "Doe",
                Address = "Any",
                Photo = "oldPhoto",
                CityId = 1
            };
            var userName = "testuser";
            var newPhotoUrl = "newPhotoUrl";
            var mockIdentityResult = IdentityResult.Success;

            _controller.ControllerContext = GetControllerContext(userName);
            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(userName))
                .ReturnsAsync(currentUser);
            _mockFileStorage.Setup(fs => fs.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", _container))
                .ReturnsAsync(newPhotoUrl);
            _mockUsersUnitOfWork.Setup(x => x.UpdateUserAsync(currentUser))
                .ReturnsAsync(mockIdentityResult);

            // Act
            var result = await _controller.PutAsync(user);
            var okResult = result as OkObjectResult;
            var token = okResult?.Value as TokenDTO;

            // Assert
            Assert.IsNotNull(token!.Token);
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(userName), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.UpdateUserAsync(currentUser), Times.Once());
        }

        [TestMethod]
        public async Task PutAsync_PhotoUpdateException_ReturnsBadRequest()
        {
            // Arrange
            var user = new User { Photo = _string64base };
            var userName = "testuser";
            var message = "Photo upload failed";

            _controller.ControllerContext = GetControllerContext(userName);
            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(userName))
                .ReturnsAsync(new User());
            _mockFileStorage.Setup(fs => fs.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", _container))
                .Throws(new Exception(message));

            // Act
            var result = await _controller.PutAsync(user);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(userName), Times.Once());
        }

        [TestMethod]
        public async Task PutAsync_UpdateUserFails_ReturnsBadRequest()
        {
            // Arrange
            var user = new User();
            var currentUser = new User();
            var identityError = new IdentityError { Description = "Update failed" };
            var userName = "testuser";

            _controller.ControllerContext = GetControllerContext(userName);
            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(userName))
                .ReturnsAsync(currentUser);
            _mockUsersUnitOfWork.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            // Act
            var result = await _controller.PutAsync(user);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(userName), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Once());
        }

        [TestMethod]
        public async Task RecoverPassword_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userName = "test@example.com";

            // Act
            var result = await _controller.RecoverPasswordAsync(new EmailDTO { Email = userName });

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(userName), Times.Once());
        }

        [TestMethod]
        public async Task RecoverPassword_EmailSentSuccessfully_ReturnsNoContent()
        {
            // Arrange
            var user = new User { Email = "test@example.com" };
            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(user.Email))
                .ReturnsAsync(user);
            _mockUsersUnitOfWork.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("GeneratedToken");

            var response = new ActionResponse<string> { WasSuccess = true };
            _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(response);

            // Act
            var result = await _controller.RecoverPasswordAsync(new EmailDTO { Email = user.Email });

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(user.Email), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once());
            _mockMailHelper.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task RecoverPassword_EmailFailedWithMessage_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var user = new User { Email = "test@example.com" };
            var message = "Failed to send";
            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(user.Email))
                .ReturnsAsync(user);
            _mockUsersUnitOfWork.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("GeneratedToken");

            var response = new ActionResponse<string> { WasSuccess = false, Message = message };
            _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(response);

            // Act
            var result = await _controller.RecoverPasswordAsync(new EmailDTO { Email = user.Email });
            var badRequest = result as BadRequestObjectResult;

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual(message, badRequest!.Value);
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(user.Email), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once());
            _mockMailHelper.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_UserExists_ReturnsOkWithUser()
        {
            // Arrange
            var user = new User();
            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync("test@example.com")).ReturnsAsync(user);

            // Act
            var result = await _controller.GetAsync();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(user, okResult.Value);
        }

        [TestMethod]
        public async Task GetAsync_UserDoesNotExist_ReturnsOkWithNull()
        {
            // Act
            var result = await _controller.GetAsync();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
        }

        [TestMethod]
        public async Task ResetPassword_UserNotFound_ReturnsNotFound()
        {
            // Act
            var result = await _controller.ResetPasswordAsync(new ResetPasswordDTO());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task ResetPassword_ValidReset_ReturnsNoContent()
        {
            // Arrange
            var mockUser = new User();
            var mockIdentityResult = IdentityResult.Success;

            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<string>()))
                .ReturnsAsync(mockUser);
            _mockUsersUnitOfWork.Setup(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockIdentityResult);

            // Act
            var result = await _controller.ResetPasswordAsync(new ResetPasswordDTO());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(It.IsAny<string>()), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task ResetPassword_InvalidReset_ReturnsBadRequest()
        {
            // Arrange
            var description = "Test error";
            var mockUser = new User();
            var mockIdentityErrors = new List<IdentityError>
            {
                new IdentityError { Description = description }
            };
            var mockIdentityResult = IdentityResult.Failed(mockIdentityErrors.ToArray());

            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(It.IsAny<string>()))
                .ReturnsAsync(mockUser);
            _mockUsersUnitOfWork.Setup(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockIdentityResult);

            // Act
            var result = await _controller.ResetPasswordAsync(new ResetPasswordDTO());
            var badRequestResult = result as BadRequestObjectResult;

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual(description, badRequestResult!.Value);
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(It.IsAny<string>()), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task ChangePasswordAsync_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("TestError", "Test error message");

            // Act
            var result = await _controller.ChangePasswordAsync(new ChangePasswordDTO());

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task ChangePasswordAsync_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userName = "testuser";
            _controller.ControllerContext = GetControllerContext(userName);

            // Act
            var result = await _controller.ChangePasswordAsync(new ChangePasswordDTO());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task ChangePasswordAsync_ValidChange_ReturnsNoContent()
        {
            // Arrange
            var userName = "testuser";
            var mockUser = new User();
            var mockIdentityResult = IdentityResult.Success;

            _controller.ControllerContext = GetControllerContext(userName);
            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(userName))
                .ReturnsAsync(mockUser);
            _mockUsersUnitOfWork.Setup(x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockIdentityResult);

            // Act
            var result = await _controller.ChangePasswordAsync(new ChangePasswordDTO());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(userName), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task ResedToken_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var emailModel = new EmailDTO { Email = "test@example.com" };

            // Act
            var result = await _controller.ResedTokenAsync(emailModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(emailModel.Email), Times.Once());
        }

        [TestMethod]
        public async Task ResedToken_EmailSentSuccessfully_ReturnsNoContent()
        {
            // Arrange
            var emailModel = new EmailDTO
            {
                Email = "test@example.com"
            };
            var user = new User();

            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(emailModel.Email))
                .ReturnsAsync(user);

            _mockUsersUnitOfWork.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
               .ReturnsAsync("GeneratedToken");

            var response = new ActionResponse<string> { WasSuccess = true };
            _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(response);

            // Act
            var result = await _controller.ResedTokenAsync(emailModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(emailModel.Email), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
            _mockMailHelper.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task ResedToken_EmailFailedToSend_ReturnsBadRequest()
        {
            // Arrange
            var emailModel = new EmailDTO
            {
                Email = "test@example.com"
            };
            var user = new User();

            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(emailModel.Email))
                .ReturnsAsync(user);

            _mockUsersUnitOfWork.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
               .ReturnsAsync("GeneratedToken");

            var response = new ActionResponse<string> { WasSuccess = false, Message = "Email sending failed" };
            _mockMailHelper.Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(response);

            // Act
            var result = await _controller.ResedTokenAsync(emailModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(emailModel.Email), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
            _mockMailHelper.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task ChangePasswordAsync_InvalidChange_ReturnsBadRequest()
        {
            // Arrange
            var userName = "testuser";
            var description = "Test error";
            var mockUser = new User();
            var mockIdentityErrors = new List<IdentityError>
            {
                new IdentityError { Description = description }
            };
            var mockIdentityResult = IdentityResult.Failed(mockIdentityErrors.ToArray());

            _controller.ControllerContext = GetControllerContext(userName);
            _mockUsersUnitOfWork.Setup(x => x.GetUserAsync(userName))
                .ReturnsAsync(mockUser);
            _mockUsersUnitOfWork.Setup(x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockIdentityResult);

            // Act
            var result = await _controller.ChangePasswordAsync(new ChangePasswordDTO());
            var badRequestResult = result as BadRequestObjectResult;

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual(description, badRequestResult!.Value);
            _mockUsersUnitOfWork.Verify(x => x.GetUserAsync(userName), Times.Once());
            _mockUsersUnitOfWork.Verify(x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        private ControllerContext GetControllerContext(string userName)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName)
            };
            var identity = new ClaimsIdentity(claims, "test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            return new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}