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
    public class CitiesControllerTests
    {
        private Mock<IGenericUnitOfWork<City>> _mockGenericUnitOfWork = null!;
        private Mock<ICitiesUnitOfWork> _mockCitiesUnitOfWork = null!;
        private CitiesController _controller = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockGenericUnitOfWork = new Mock<IGenericUnitOfWork<City>>();
            _mockCitiesUnitOfWork = new Mock<ICitiesUnitOfWork>();
            _controller = new CitiesController(_mockGenericUnitOfWork.Object, _mockCitiesUnitOfWork.Object);
        }

        [TestMethod]
        public async Task GetComboAsync_ShouldReturnOkResult()
        {
            // Arrange
            var stateId = 1;
            var cities = new List<City> { new City { Id = 1, Name = "City1" }, new City { Id = 2, Name = "City2" } };
            _mockCitiesUnitOfWork.Setup(x => x.GetComboAsync(stateId)).ReturnsAsync(cities);

            // Act
            var result = await _controller.GetComboAsync(stateId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var resultValue = okResult.Value as IEnumerable<City>;
            Assert.IsNotNull(resultValue);
            Assert.AreEqual(2, resultValue.Count());
            new List<City> { new City { Id = 1, Name = "City1" }, new City { Id = 2, Name = "City2" } };
            _mockCitiesUnitOfWork.Verify(x => x.GetComboAsync(stateId), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnOkResult_WhenActionResponseIsSuccess()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var response = new ActionResponse<IEnumerable<City>> { WasSuccess = true, Result = new List<City>() };
            _mockCitiesUnitOfWork.Setup(x => x.GetAsync(pagination)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetAsync(pagination);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            _mockCitiesUnitOfWork.Verify(x => x.GetAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnBadRequest_WhenActionResponseIsNotSuccess()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var response = new ActionResponse<IEnumerable<City>> { WasSuccess = false };
            _mockCitiesUnitOfWork.Setup(x => x.GetAsync(pagination)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetAsync(pagination);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult);
            _mockCitiesUnitOfWork.Verify(x => x.GetAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_ShouldReturnOkResult_WhenActionResponseIsSuccess()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var response = new ActionResponse<int> { WasSuccess = true, Result = 1 };
            _mockCitiesUnitOfWork.Setup(x => x.GetTotalPagesAsync(pagination)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetPagesAsync(pagination);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(1, okResult.Value);
            _mockCitiesUnitOfWork.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_ShouldReturnBadRequest_WhenActionResponseIsNotSuccess()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var response = new ActionResponse<int> { WasSuccess = false };
            _mockCitiesUnitOfWork.Setup(x => x.GetTotalPagesAsync(pagination)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetPagesAsync(pagination);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult);
            _mockCitiesUnitOfWork.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once());
        }
    }
}