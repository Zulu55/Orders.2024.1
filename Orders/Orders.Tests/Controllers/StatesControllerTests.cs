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
    public class StatesControllerTests
    {
        private Mock<IGenericUnitOfWork<State>> _mockUnitOfWork = null!;
        private Mock<IStatesUnitOfWork> _mockStatesUnitOfWork = null!;
        private StatesController _controller = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockUnitOfWork = new Mock<IGenericUnitOfWork<State>>();
            _mockStatesUnitOfWork = new Mock<IStatesUnitOfWork>();
            _controller = new StatesController(_mockUnitOfWork.Object, _mockStatesUnitOfWork.Object);
        }

        [TestMethod]
        public async Task GetComboAsync_ShouldReturnOk()
        {
            // Arrange
            var countryId = 1;
            var states = new List<State> { new State(), new State() };
            _mockStatesUnitOfWork.Setup(x => x.GetComboAsync(countryId)).ReturnsAsync(states);

            // Act
            var result = await _controller.GetComboAsync(countryId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(states, okResult.Value);
            _mockStatesUnitOfWork.Verify(x => x.GetComboAsync(countryId), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_Paginated_ShouldReturnOk()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var states = new List<State> { new State(), new State() };
            _mockStatesUnitOfWork.Setup(x => x.GetAsync(pagination))
                .ReturnsAsync(new ActionResponse<IEnumerable<State>>
                {
                    WasSuccess = true,
                    Result = states
                });

            // Act
            var result = await _controller.GetAsync(pagination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(states, okResult.Value);
            _mockStatesUnitOfWork.Verify(x => x.GetAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnOk()
        {
            // Arrange
            var states = new List<State> { new State(), new State() };
            _mockStatesUnitOfWork.Setup(x => x.GetAsync())
                .ReturnsAsync(new ActionResponse<IEnumerable<State>>
                {
                    WasSuccess = true,
                    Result = states
                });

            // Act
            var result = await _controller.GetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(states, okResult.Value);
            _mockStatesUnitOfWork.Verify(x => x.GetAsync(), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnError()
        {
            // Arrange
            _mockStatesUnitOfWork.Setup(x => x.GetAsync())
                .ReturnsAsync(new ActionResponse<IEnumerable<State>>
                {
                    WasSuccess = false,
                });

            // Act
            var result = await _controller.GetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockStatesUnitOfWork.Verify(x => x.GetAsync(), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnBadRequest()
        {
            // Arrange
            var pagination = new PaginationDTO();
            _mockStatesUnitOfWork.Setup(x => x.GetAsync(pagination))
                .ReturnsAsync(new ActionResponse<IEnumerable<State>> { WasSuccess = false });

            // Act
            var result = await _controller.GetAsync(pagination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockStatesUnitOfWork.Verify(x => x.GetAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_ShouldReturnOk()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var totalPages = 5;
            _mockStatesUnitOfWork.Setup(x => x.GetTotalPagesAsync(pagination))
                .ReturnsAsync(new ActionResponse<int>
                {
                    WasSuccess = true,
                    Result = totalPages
                });

            // Act
            var result = await _controller.GetPagesAsync(pagination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(totalPages, okResult.Value);
            _mockStatesUnitOfWork.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_ShouldReturnBadRequest()
        {
            // Arrange
            var pagination = new PaginationDTO();
            _mockStatesUnitOfWork.Setup(x => x.GetTotalPagesAsync(pagination))
                .ReturnsAsync(new ActionResponse<int> { WasSuccess = false });

            // Act
            var result = await _controller.GetPagesAsync(pagination);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockStatesUnitOfWork.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ById_ShouldReturnOk()
        {
            // Arrange
            var stateId = 1;
            var state = new State();
            _mockStatesUnitOfWork.Setup(x => x.GetAsync(stateId))
                .ReturnsAsync(new ActionResponse<State>
                {
                    WasSuccess = true,
                    Result = state
                });

            // Act
            var result = await _controller.GetAsync(stateId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(state, okResult.Value);
            _mockStatesUnitOfWork.Verify(x => x.GetAsync(stateId), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ById_ShouldReturnNotFound()
        {
            // Arrange
            var stateId = 1;
            var message = "State not found";
            _mockStatesUnitOfWork.Setup(x => x.GetAsync(stateId))
                .ReturnsAsync(new ActionResponse<State>
                {
                    WasSuccess = false,
                    Message = message
                });

            // Act
            var result = await _controller.GetAsync(stateId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.AreEqual(message, notFoundResult.Value);
            _mockStatesUnitOfWork.Verify(x => x.GetAsync(stateId), Times.Once());
        }
    }
}