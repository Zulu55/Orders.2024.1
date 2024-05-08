using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Orders.Backend.Controllers;
using Orders.Backend.UnitsOfWork.Interfaces;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Shared.Responses;

namespace Orders.Tests.Controllers
{
    [TestClass]
    public class TemporalOrdersControllerTests
    {
        private TemporalOrdersController _controller = null!;
        private Mock<ITemporalOrdersUnitOfWork> _temporalOrdersUnitOfWorkMock = null!;
        private Mock<IGenericUnitOfWork<TemporalOrder>> _unitOfWorkMock = null!;
        private DefaultHttpContext _httpContext = null!;

        [TestInitialize]
        public void Initialize()
        {
            _temporalOrdersUnitOfWorkMock = new Mock<ITemporalOrdersUnitOfWork>();
            _unitOfWorkMock = new Mock<IGenericUnitOfWork<TemporalOrder>>();
            _controller = new TemporalOrdersController(_unitOfWorkMock.Object, _temporalOrdersUnitOfWorkMock.Object);
            _httpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext = _httpContext;
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "testUser") }));
        }

        [TestMethod]
        public async Task PostAsync_Success_ReturnsOkObjectResult()
        {
            // Arrange
            var temporalOrderDTO = new TemporalOrderDTO();
            _temporalOrdersUnitOfWorkMock.Setup(x => x.AddFullAsync(It.IsAny<string>(), It.IsAny<TemporalOrderDTO>()))
                .ReturnsAsync(new ActionResponse<TemporalOrderDTO> { WasSuccess = true });

            // Act
            var result = await _controller.PostAsync(temporalOrderDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _temporalOrdersUnitOfWorkMock.Verify(x => x.AddFullAsync(It.IsAny<string>(), It.IsAny<TemporalOrderDTO>()), Times.Once());
        }

        [TestMethod]
        public async Task PostAsync_Failure_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var temporalOrderDTO = new TemporalOrderDTO();
            _temporalOrdersUnitOfWorkMock.Setup(x => x.AddFullAsync(It.IsAny<string>(), It.IsAny<TemporalOrderDTO>()))
                .ReturnsAsync(new ActionResponse<TemporalOrderDTO> { WasSuccess = false });

            // Act
            var result = await _controller.PostAsync(temporalOrderDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _temporalOrdersUnitOfWorkMock.Verify(x => x.AddFullAsync(It.IsAny<string>(), It.IsAny<TemporalOrderDTO>()), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_Success_ReturnsOkObjectResult()
        {
            // Arrange
            var userName = "testUser";
            _temporalOrdersUnitOfWorkMock.Setup(x => x.GetAsync(userName))
                .ReturnsAsync(new ActionResponse<IEnumerable<TemporalOrder>> { WasSuccess = true });

            // Act
            var result = await _controller.GetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _temporalOrdersUnitOfWorkMock.Verify(x => x.GetAsync(userName), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_Failure_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var userName = "testUser";
            _temporalOrdersUnitOfWorkMock.Setup(x => x.GetAsync(userName))
                .ReturnsAsync(new ActionResponse<IEnumerable<TemporalOrder>> { WasSuccess = false });

            // Act
            var result = await _controller.GetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _temporalOrdersUnitOfWorkMock.Verify(x => x.GetAsync(userName), Times.Once());
        }

        [TestMethod]
        public async Task GetCountAsync_Success_ReturnsOkObjectResult()
        {
            // Arrange
            var userName = "testUser";
            _temporalOrdersUnitOfWorkMock.Setup(x => x.GetCountAsync(userName))
                .ReturnsAsync(new ActionResponse<int> { WasSuccess = true, Result = 5 });

            // Act
            var result = await _controller.GetCountAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _temporalOrdersUnitOfWorkMock.Verify(x => x.GetCountAsync(userName), Times.Once());
        }

        [TestMethod]
        public async Task GetCountAsync_Failure_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var userName = "testUser";
            _temporalOrdersUnitOfWorkMock.Setup(x => x.GetCountAsync(userName))
                .ReturnsAsync(new ActionResponse<int> { WasSuccess = false, Message = "Failed" });

            // Act
            var result = await _controller.GetCountAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _temporalOrdersUnitOfWorkMock.Verify(x => x.GetCountAsync(userName), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ById_Success_ReturnsOkObjectResult()
        {
            // Arrange
            _temporalOrdersUnitOfWorkMock.Setup(x => x.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new ActionResponse<TemporalOrder> { WasSuccess = true, Result = new TemporalOrder() });

            // Act
            var result = await _controller.GetAsync(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _temporalOrdersUnitOfWorkMock.Verify(x => x.GetAsync(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ById_Failure_ReturnsNotFoundObjectResult()
        {
            // Arrange
            _temporalOrdersUnitOfWorkMock.Setup(x => x.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new ActionResponse<TemporalOrder> { WasSuccess = false, Message = "Not Found" });

            // Act
            var result = await _controller.GetAsync(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            _temporalOrdersUnitOfWorkMock.Verify(x => x.GetAsync(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public async Task PutFullAsync_Success_ReturnsOkObjectResult()
        {
            // Arrange
            var temporalOrderDTO = new TemporalOrderDTO();
            _temporalOrdersUnitOfWorkMock.Setup(x => x.PutFullAsync(temporalOrderDTO))
                .ReturnsAsync(new ActionResponse<TemporalOrder> { WasSuccess = true, Result = new TemporalOrder() });

            // Act
            var result = await _controller.PutFullAsync(temporalOrderDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _temporalOrdersUnitOfWorkMock.Verify(x => x.PutFullAsync(temporalOrderDTO), Times.Once());
        }

        [TestMethod]
        public async Task PutFullAsync_Failure_ReturnsNotFoundObjectResult()
        {
            // Arrange
            var temporalOrderDTO = new TemporalOrderDTO();
            _temporalOrdersUnitOfWorkMock.Setup(x => x.PutFullAsync(temporalOrderDTO))
                .ReturnsAsync(new ActionResponse<TemporalOrder> { WasSuccess = false, Message = "Not Found" });

            // Act
            var result = await _controller.PutFullAsync(temporalOrderDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            _temporalOrdersUnitOfWorkMock.Verify(x => x.PutFullAsync(temporalOrderDTO), Times.Once());
        }
    }
}