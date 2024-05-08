using Moq;
using Orders.Backend.Repositories.Interfaces;
using Orders.Backend.UnitsOfWork.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Shared.Responses;

namespace Orders.Tests.UnitsOfWork
{
    [TestClass]
    public class ProductsUnitOfWorkTests
    {
        private Mock<IGenericRepository<Product>> _repositoryMock = null!;
        private Mock<IProductsRepository> _productsRepositoryMock = null!;
        private ProductsUnitOfWork _unitOfWork = null!;

        [TestInitialize]
        public void SetUp()
        {
            _repositoryMock = new Mock<IGenericRepository<Product>>();
            _productsRepositoryMock = new Mock<IProductsRepository>();
            _unitOfWork = new ProductsUnitOfWork(_repositoryMock.Object, _productsRepositoryMock.Object);
        }

        [TestMethod]
        public async Task GetAsync_WithPagination_ReturnsProducts()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var expectedActionResponse = new ActionResponse<IEnumerable<Product>> { WasSuccess = true };
            _productsRepositoryMock.Setup(x => x.GetAsync(pagination))
                .ReturnsAsync(expectedActionResponse);

            // Act
            var result = await _unitOfWork.GetAsync(pagination);

            // Assert
            Assert.AreEqual(expectedActionResponse, result);
            _productsRepositoryMock.Verify(x => x.GetAsync(pagination), Times.Once);
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_ReturnsTotalPages()
        {
            // Arrange
            var pagination = new PaginationDTO();
            var expectedActionResponse = new ActionResponse<int> { WasSuccess = true };
            _productsRepositoryMock.Setup(x => x.GetTotalPagesAsync(pagination))
                .ReturnsAsync(expectedActionResponse);

            // Act
            var result = await _unitOfWork.GetTotalPagesAsync(pagination);

            // Assert
            Assert.AreEqual(expectedActionResponse, result);
            _productsRepositoryMock.Verify(x => x.GetTotalPagesAsync(pagination), Times.Once);
        }

        [TestMethod]
        public async Task GetAsync_ById_ReturnsProduct()
        {
            // Arrange
            var productId = 1;
            var expectedActionResponse = new ActionResponse<Product> { WasSuccess = true };
            _productsRepositoryMock.Setup(x => x.GetAsync(productId))
                .ReturnsAsync(expectedActionResponse);

            // Act
            var result = await _unitOfWork.GetAsync(productId);

            // Assert
            Assert.AreEqual(expectedActionResponse, result);
            _productsRepositoryMock.Verify(x => x.GetAsync(productId), Times.Once);
        }

        [TestMethod]
        public async Task AddFullAsync_ReturnsProduct()
        {
            // Arrange
            var productDTO = new ProductDTO();
            var expectedActionResponse = new ActionResponse<Product> { WasSuccess = true };
            _productsRepositoryMock.Setup(x => x.AddFullAsync(productDTO))
                .ReturnsAsync(expectedActionResponse);

            // Act
            var result = await _unitOfWork.AddFullAsync(productDTO);

            // Assert
            Assert.AreEqual(expectedActionResponse, result);
            _productsRepositoryMock.Verify(x => x.AddFullAsync(productDTO), Times.Once);
        }

        [TestMethod]
        public async Task UpdateFullAsync_ReturnsProduct()
        {
            // Arrange
            var productDTO = new ProductDTO();
            var expectedActionResponse = new ActionResponse<Product> { WasSuccess = true };
            _productsRepositoryMock.Setup(x => x.UpdateFullAsync(productDTO))
                .ReturnsAsync(expectedActionResponse);

            // Act
            var result = await _unitOfWork.UpdateFullAsync(productDTO);

            // Assert
            Assert.AreEqual(expectedActionResponse, result);
            _productsRepositoryMock.Verify(x => x.UpdateFullAsync(productDTO), Times.Once);
        }

        [TestMethod]
        public async Task AddImageAsync_ReturnsImage()
        {
            // Arrange
            var imageDTO = new ImageDTO();
            var expectedActionResponse = new ActionResponse<ImageDTO> { WasSuccess = true };
            _productsRepositoryMock.Setup(x => x.AddImageAsync(imageDTO))
                .ReturnsAsync(expectedActionResponse);

            // Act
            var result = await _unitOfWork.AddImageAsync(imageDTO);

            // Assert
            Assert.AreEqual(expectedActionResponse, result);
            _productsRepositoryMock.Verify(x => x.AddImageAsync(imageDTO), Times.Once);
        }

        [TestMethod]
        public async Task RemoveLastImageAsync_ReturnsImage()
        {
            // Arrange
            var imageDTO = new ImageDTO();
            var expectedActionResponse = new ActionResponse<ImageDTO> { WasSuccess = true };
            _productsRepositoryMock.Setup(x => x.RemoveLastImageAsync(imageDTO))
                .ReturnsAsync(expectedActionResponse);

            // Act
            var result = await _unitOfWork.RemoveLastImageAsync(imageDTO);

            // Assert
            Assert.AreEqual(expectedActionResponse, result);
            _productsRepositoryMock.Verify(x => x.RemoveLastImageAsync(imageDTO), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_ExistingItem_ReturnsSuccessResponse()
        {
            // Arrange
            int id = 1;
            _productsRepositoryMock.Setup(x => x.DeleteAsync(id))
                .ReturnsAsync(new ActionResponse<Product> { WasSuccess = true });

            // Act
            var response = await _unitOfWork.DeleteAsync(id);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            _productsRepositoryMock.Verify(x => x.DeleteAsync(id), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_NonExistingItem_ReturnsFailureResponse()
        {
            // Arrange
            int id = 999; // Make sure this ID does not exist in your test data
            _productsRepositoryMock.Setup(x => x.DeleteAsync(id))
                .ReturnsAsync(new ActionResponse<Product> { WasSuccess = false });

            // Act
            var response = await _unitOfWork.DeleteAsync(id);

            // Assert
            Assert.IsFalse(response.WasSuccess);
            _productsRepositoryMock.Verify(x => x.DeleteAsync(id), Times.Once);
        }
    }
}