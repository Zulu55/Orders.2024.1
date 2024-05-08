using Moq;
using Orders.Backend.Repositories.Interfaces;
using Orders.Backend.UnitsOfWork.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Shared.Responses;

namespace Orders.Tests.UnitsOfWork
{
    [TestClass]
    public class StatesUnitOfWorkTests
    {
        private Mock<IGenericRepository<State>> _mockGenericRepository = null!;
        private Mock<IStatesRepository> _mockStatesRepository = null!;
        private StatesUnitOfWork _unitOfWork = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockGenericRepository = new Mock<IGenericRepository<State>>();
            _mockStatesRepository = new Mock<IStatesRepository>();
            _unitOfWork = new StatesUnitOfWork(_mockGenericRepository.Object, _mockStatesRepository.Object);
        }

        [TestMethod]
        public async Task GetAsync_Paginated_ShouldReturnStates()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var states = new List<State> { new State(), new State() };
            _mockStatesRepository.Setup(x => x.GetAsync(pagination))
                .ReturnsAsync(new ActionResponse<IEnumerable<State>>
                {
                    WasSuccess = true,
                    Result = states
                });

            // Act
            var result = await _unitOfWork.GetAsync(pagination);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(states, result.Result);
            _mockStatesRepository.Verify(x => x.GetAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnStates()
        {
            // Arrange
            var states = new List<State> { new State(), new State() };
            _mockStatesRepository.Setup(x => x.GetAsync())
                .ReturnsAsync(new ActionResponse<IEnumerable<State>>
                {
                    WasSuccess = true,
                    Result = states
                });

            // Act
            var result = await _unitOfWork.GetAsync();

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(states, result.Result);
            _mockStatesRepository.Verify(x => x.GetAsync(), Times.Once());
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_ShouldReturnTotalPages()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var totalPages = 5;
            _mockStatesRepository.Setup(x => x.GetTotalPagesAsync(pagination))
                .ReturnsAsync(new ActionResponse<int>
                {
                    WasSuccess = true,
                    Result = totalPages
                });

            // Act
            var result = await _unitOfWork.GetTotalPagesAsync(pagination);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(totalPages, result.Result);
            _mockStatesRepository.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_ById_ShouldReturnState()
        {
            // Arrange
            var stateId = 1;
            var state = new State();
            _mockStatesRepository.Setup(x => x.GetAsync(stateId))
                .ReturnsAsync(new ActionResponse<State>
                {
                    WasSuccess = true,
                    Result = state
                });

            // Act
            var result = await _unitOfWork.GetAsync(stateId);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(state, result.Result);
            _mockStatesRepository.Verify(x => x.GetAsync(stateId), Times.Once());
        }

        [TestMethod]
        public async Task GetComboAsync_ShouldReturnStates()
        {
            // Arrange
            var countryId = 1;
            var states = new List<State> { new State(), new State() };
            _mockStatesRepository.Setup(x => x.GetComboAsync(countryId))
                .ReturnsAsync(states);

            // Act
            var result = await _unitOfWork.GetComboAsync(countryId);

            // Assert
            Assert.AreEqual(states, result);
            _mockStatesRepository.Verify(x => x.GetComboAsync(countryId), Times.Once());
        }
    }
}