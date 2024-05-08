using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Orders.Backend.Controllers;
using Orders.Backend.Helpers;
using Orders.Backend.UnitsOfWork.Interfaces;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Shared.Responses;

namespace Orders.Tests.Controllers
{
    [TestClass]
    public class OrdersControllerTests
    {
        private Mock<IOrdersHelper> _mockOrdersHelper = null!;
        private Mock<IOrdersUnitOfWork> _mockOrdersUnitOfWork = null!;
        private OrdersController _controller = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockOrdersHelper = new Mock<IOrdersHelper>();
            _mockOrdersUnitOfWork = new Mock<IOrdersUnitOfWork>();
            _controller = new OrdersController(_mockOrdersHelper.Object, _mockOrdersUnitOfWork.Object);
        }

        private void SetupUser(string username)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, username)
            }, "mock"));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [TestMethod]
        public async Task PostAsync_ShouldReturnBadRequest_WhenOrderIsNotProcessed()
        {
            // Arrange
            SetupUser("testuser");
            var orderDto = new OrderDTO();
            _mockOrdersHelper.Setup(x => x.ProcessOrderAsync("testuser", It.IsAny<string>()))
                .ReturnsAsync(new ActionResponse<bool> { WasSuccess = false });

            // Act
            var result = await _controller.PostAsync(orderDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockOrdersHelper.Verify(x => x.ProcessOrderAsync("testuser", It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task PostAsync_ShouldReturnNoContent_WhenOrderIsProcessed()
        {
            // Arrange
            SetupUser("testuser");
            var orderDto = new OrderDTO();
            _mockOrdersHelper.Setup(x => x.ProcessOrderAsync("testuser", It.IsAny<string>()))
                .ReturnsAsync(new ActionResponse<bool> { WasSuccess = true });

            // Act
            var result = await _controller.PostAsync(orderDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockOrdersHelper.Verify(x => x.ProcessOrderAsync("testuser", It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnOk_WhenOrdersAreRetrievedSuccessfully()
        {
            // Arrange
            SetupUser("testuser");
            var paginationDto = new PaginationDTO();
            _mockOrdersUnitOfWork.Setup(x => x.GetAsync("testuser", paginationDto))
                .ReturnsAsync(new ActionResponse<IEnumerable<Order>> { WasSuccess = true, Result = new List<Order>() });

            // Act
            var result = await _controller.GetAsync(paginationDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockOrdersUnitOfWork.Verify(x => x.GetAsync("testuser", paginationDto), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnBadRequest_WhenOrdersRetrievalFails()
        {
            // Arrange
            SetupUser("testuser");
            var paginationDto = new PaginationDTO();
            _mockOrdersUnitOfWork.Setup(x => x.GetAsync("testuser", paginationDto))
                .ReturnsAsync(new ActionResponse<IEnumerable<Order>> { WasSuccess = false });

            // Act
            var result = await _controller.GetAsync(paginationDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockOrdersUnitOfWork.Verify(x => x.GetAsync("testuser", paginationDto), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_ShouldReturnOk_WhenTotalPagesAreRetrievedSuccessfully()
        {
            // Arrange
            SetupUser("testuser");
            var paginationDto = new PaginationDTO();
            _mockOrdersUnitOfWork.Setup(x => x.GetTotalPagesAsync("testuser", paginationDto))
                .ReturnsAsync(new ActionResponse<int> { WasSuccess = true, Result = 5 });

            // Act
            var result = await _controller.GetPagesAsync(paginationDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(5, okResult!.Value);
            _mockOrdersUnitOfWork.Verify(x => x.GetTotalPagesAsync("testuser", paginationDto), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_ShouldReturnBadRequest_WhenRetrievalFails()
        {
            // Arrange
            SetupUser("testuser");
            var paginationDto = new PaginationDTO();
            _mockOrdersUnitOfWork.Setup(x => x.GetTotalPagesAsync("testuser", paginationDto))
                .ReturnsAsync(new ActionResponse<int> { WasSuccess = false });

            // Act
            var result = await _controller.GetPagesAsync(paginationDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockOrdersUnitOfWork.Verify(x => x.GetTotalPagesAsync("testuser", paginationDto), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_WithId_ShouldReturnOk_WhenOrderIsRetrievedSuccessfully()
        {
            // Arrange
            SetupUser("testuser");
            int orderId = 1;
            _mockOrdersUnitOfWork.Setup(x => x.GetAsync(orderId))
                .ReturnsAsync(new ActionResponse<Order> { WasSuccess = true, Result = new Order() });

            // Act
            var result = await _controller.GetAsync(orderId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockOrdersUnitOfWork.Verify(x => x.GetAsync(orderId), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_WithId_ShouldReturnNotFound_WhenOrderIsNotFound()
        {
            // Arrange
            SetupUser("testuser");
            int orderId = 1;
            _mockOrdersUnitOfWork.Setup(x => x.GetAsync(orderId))
                .ReturnsAsync(new ActionResponse<Order> { WasSuccess = false, Message = "Order not found" });

            // Act
            var result = await _controller.GetAsync(orderId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Order not found", notFoundResult!.Value);
            _mockOrdersUnitOfWork.Verify(x => x.GetAsync(orderId), Times.Once());
        }

        [TestMethod]
        public async Task PutAsync_ShouldReturnOk_WhenOrderIsUpdatedSuccessfully()
        {
            // Arrange
            SetupUser("testuser");
            var orderDto = new OrderDTO();
            _mockOrdersUnitOfWork.Setup(x => x.UpdateFullAsync("testuser", orderDto))
                .ReturnsAsync(new ActionResponse<Order> { WasSuccess = true });

            // Act
            var result = await _controller.PutAsync(orderDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockOrdersUnitOfWork.Verify(x => x.UpdateFullAsync("testuser", orderDto), Times.Once());
        }

        [TestMethod]
        public async Task PutAsync_ShouldReturnBadRequest_WhenUpdateFails()
        {
            // Arrange
            SetupUser("testuser");
            var orderDto = new OrderDTO();
            _mockOrdersUnitOfWork.Setup(x => x.UpdateFullAsync("testuser", orderDto))
                .ReturnsAsync(new ActionResponse<Order> { WasSuccess = false, Message = "Update failed" });

            // Act
            var result = await _controller.PutAsync(orderDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Update failed", badRequestResult!.Value);
            _mockOrdersUnitOfWork.Verify(x => x.UpdateFullAsync("testuser", orderDto), Times.Once());
        }
    }
}