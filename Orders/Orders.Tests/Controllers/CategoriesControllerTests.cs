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
    public class CategoriesControllerTests
    {
        private Mock<IGenericUnitOfWork<Category>> _mockGenericUnitOfWork = null!;
        private Mock<ICategoriesUnitOfWork> _mockCategoriesUnitOfWork = null!;
        private CategoriesController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockGenericUnitOfWork = new Mock<IGenericUnitOfWork<Category>>();
            _mockCategoriesUnitOfWork = new Mock<ICategoriesUnitOfWork>();
            _controller = new CategoriesController(_mockGenericUnitOfWork.Object, _mockCategoriesUnitOfWork.Object);
        }

        [TestMethod]
        public async Task GetComboAsync_ReturnsOkObjectResult()
        {
            // Arrange
            var comboData = new List<Category> { new Category() };
            _mockCategoriesUnitOfWork.Setup(x => x.GetComboAsync()).ReturnsAsync(comboData);

            // Act
            var result = await _controller.GetComboAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(comboData, okResult!.Value);
            _mockCategoriesUnitOfWork.Verify(x => x.GetComboAsync(), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ReturnsOkObjectResult_WhenWasSuccessIsTrue()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var response = new ActionResponse<IEnumerable<Category>> { WasSuccess = true };
            _mockCategoriesUnitOfWork.Setup(x => x.GetAsync(pagination)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetAsync(pagination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(response.Result, okResult!.Value);
            _mockCategoriesUnitOfWork.Verify(x => x.GetAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ReturnsBadRequestResult_WhenWasSuccessIsFalse()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var response = new ActionResponse<IEnumerable<Category>> { WasSuccess = false };
            _mockCategoriesUnitOfWork.Setup(x => x.GetAsync(pagination)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetAsync(pagination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockCategoriesUnitOfWork.Verify(x => x.GetAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_ReturnsOkObjectResult_WhenWasSuccessIsTrue()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var action = new ActionResponse<int> { WasSuccess = true, Result = 5 };
            _mockCategoriesUnitOfWork.Setup(x => x.GetTotalPagesAsync(pagination)).ReturnsAsync(action);

            // Act
            var result = await _controller.GetPagesAsync(pagination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(action.Result, okResult!.Value);
            _mockCategoriesUnitOfWork.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_ReturnsBadRequestResult_WhenWasSuccessIsFalse()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var action = new ActionResponse<int> { WasSuccess = false };
            _mockCategoriesUnitOfWork.Setup(x => x.GetTotalPagesAsync(pagination)).ReturnsAsync(action);

            // Act
            var result = await _controller.GetPagesAsync(pagination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockCategoriesUnitOfWork.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once());
        }
    }
}