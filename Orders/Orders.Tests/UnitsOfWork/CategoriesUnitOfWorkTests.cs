using Moq;
using Orders.Backend.Repositories.Interfaces;
using Orders.Backend.UnitsOfWork.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Shared.Responses;

namespace Orders.Tests.UnitsOfWork
{
    [TestClass]
    public class CategoriesUnitOfWorkTests
    {
        private Mock<IGenericRepository<Category>> _mockGenericRepository = null!;
        private Mock<ICategoriesRepository> _mockCategoriesRepository = null!;
        private CategoriesUnitOfWork _unitOfWork = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockGenericRepository = new Mock<IGenericRepository<Category>>();
            _mockCategoriesRepository = new Mock<ICategoriesRepository>();
            _unitOfWork = new CategoriesUnitOfWork(_mockGenericRepository.Object, _mockCategoriesRepository.Object);
        }

        [TestMethod]
        public async Task GetAsync_CallsRepositoryAndReturnsResult()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var expectedActionResponse = new ActionResponse<IEnumerable<Category>> { Result = new List<Category>() };
            _mockCategoriesRepository.Setup(x => x.GetAsync(pagination)).ReturnsAsync(expectedActionResponse);

            // Act
            var result = await _unitOfWork.GetAsync(pagination);

            // Assert
            Assert.AreEqual(expectedActionResponse, result);
            _mockCategoriesRepository.Verify(x => x.GetAsync(pagination), Times.Once);
        }

        [TestMethod]
        public async Task GetComboAsync_CallsRepositoryAndReturnsResult()
        {
            // Arrange
            var expectedCategories = new List<Category> { new Category() };
            _mockCategoriesRepository.Setup(x => x.GetComboAsync()).ReturnsAsync(expectedCategories);

            // Act
            var result = await _unitOfWork.GetComboAsync();

            // Assert
            Assert.AreEqual(expectedCategories, result);
            _mockCategoriesRepository.Verify(x => x.GetComboAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_CallsRepositoryAndReturnsResult()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var expectedActionResponse = new ActionResponse<int> { Result = 5 };
            _mockCategoriesRepository.Setup(x => x.GetTotalPagesAsync(pagination)).ReturnsAsync(expectedActionResponse);

            // Act
            var result = await _unitOfWork.GetTotalPagesAsync(pagination);

            // Assert
            Assert.AreEqual(expectedActionResponse, result);
            _mockCategoriesRepository.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once);
        }
    }
}