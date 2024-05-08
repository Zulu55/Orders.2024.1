using Moq;
using Orders.Backend.Repositories.Interfaces;
using Orders.Backend.UnitsOfWork.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Shared.Responses;

namespace Orders.Tests.UnitsOfWork
{
    [TestClass]
    public class CitiesUnitOfWorkTests
    {
        private Mock<ICitiesRepository> _mockCitiesRepository = null!;
        private CitiesUnitOfWork _unitOfWork = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockCitiesRepository = new Mock<ICitiesRepository>();
            _unitOfWork = new CitiesUnitOfWork(null, _mockCitiesRepository.Object);
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnCities()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var expectedActionResponse = new ActionResponse<IEnumerable<City>> { WasSuccess = true, Result = new List<City>() };
            _mockCitiesRepository.Setup(x => x.GetAsync(pagination))
                .ReturnsAsync(expectedActionResponse);

            // Act
            var result = await _unitOfWork.GetAsync(pagination);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(expectedActionResponse.Result, result.Result);
            _mockCitiesRepository.Verify(x => x.GetAsync(pagination), Times.Once);
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_ShouldReturnTotalPages()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var expectedActionResponse = new ActionResponse<int> { WasSuccess = true, Result = 5 };
            _mockCitiesRepository.Setup(x => x.GetTotalPagesAsync(pagination))
                .ReturnsAsync(expectedActionResponse);

            // Act
            var result = await _unitOfWork.GetTotalPagesAsync(pagination);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(expectedActionResponse.Result, result.Result);
            _mockCitiesRepository.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once);
        }

        [TestMethod]
        public async Task GetComboAsync_ShouldReturnCities()
        {
            // Arrange
            var stateId = 1;
            var expectedCities = new List<City> { new City { Id = 1, Name = "City1" }, new City { Id = 2, Name = "City2" } };
            _mockCitiesRepository.Setup(x => x.GetComboAsync(stateId))
                .ReturnsAsync(expectedCities);

            // Act
            var result = await _unitOfWork.GetComboAsync(stateId);

            // Assert
            Assert.AreEqual(expectedCities, result);
            _mockCitiesRepository.Verify(x => x.GetComboAsync(stateId), Times.Once);
        }
    }
}