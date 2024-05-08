using Moq;
using Orders.Backend.Repositories.Interfaces;
using Orders.Backend.UnitsOfWork.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Shared.Responses;

namespace Orders.Tests.UnitsOfWork
{
    [TestClass]
    public class CountriesUnitOfWorkTests
    {
        private Mock<IGenericRepository<Country>> _mockGenericRepository = null!;
        private Mock<ICountriesRepository> _mockCountriesRepository = null!;
        private CountriesUnitOfWork _unitOfWork = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockGenericRepository = new Mock<IGenericRepository<Country>>();
            _mockCountriesRepository = new Mock<ICountriesRepository>();
            _unitOfWork = new CountriesUnitOfWork(_mockGenericRepository.Object, _mockCountriesRepository.Object);
        }

        [TestMethod]
        public async Task GetAsync_WithPagination_ShouldReturnData()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var expectedResponse = new ActionResponse<IEnumerable<Country>> { WasSuccess = true };
            _mockCountriesRepository.Setup(x => x.GetAsync(pagination))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _unitOfWork.GetAsync(pagination);

            // Assert
            Assert.AreEqual(expectedResponse, result);
            _mockCountriesRepository.Verify(x => x.GetAsync(pagination), Times.Once);
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnData()
        {
            // Arrange
            var expectedResponse = new ActionResponse<IEnumerable<Country>> { WasSuccess = true };
            _mockCountriesRepository.Setup(x => x.GetAsync())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _unitOfWork.GetAsync();

            // Assert
            Assert.AreEqual(expectedResponse, result);
            _mockCountriesRepository.Verify(x => x.GetAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_ShouldReturnTotalPages()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var expectedResponse = new ActionResponse<int> { WasSuccess = true };
            _mockCountriesRepository.Setup(x => x.GetTotalPagesAsync(pagination))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _unitOfWork.GetTotalPagesAsync(pagination);

            // Assert
            Assert.AreEqual(expectedResponse, result);
            _mockCountriesRepository.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once);
        }

        [TestMethod]
        public async Task GetAsync_WithId_ShouldReturnData()
        {
            // Arrange
            int id = 1;
            var expectedResponse = new ActionResponse<Country> { WasSuccess = true };
            _mockCountriesRepository.Setup(x => x.GetAsync(id))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _unitOfWork.GetAsync(id);

            // Assert
            Assert.AreEqual(expectedResponse, result);
            _mockCountriesRepository.Verify(x => x.GetAsync(id), Times.Once);
        }

        [TestMethod]
        public async Task GetComboAsync_ShouldReturnData()
        {
            // Arrange
            var expectedCountries = new List<Country> { new Country { Id = 1, Name = "Country1" } };
            _mockCountriesRepository.Setup(x => x.GetComboAsync())
                .ReturnsAsync(expectedCountries);

            // Act
            var result = await _unitOfWork.GetComboAsync();

            // Assert
            CollectionAssert.AreEqual(expectedCountries, new List<Country>(result));
            _mockCountriesRepository.Verify(x => x.GetComboAsync(), Times.Once);
        }
    }
}