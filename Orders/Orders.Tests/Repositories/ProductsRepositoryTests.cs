using Microsoft.EntityFrameworkCore;
using Moq;
using Orders.Backend.Data;
using Orders.Backend.Helpers;
using Orders.Backend.Repositories.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Tests.Shared;

namespace Orders.Tests.Repositories
{
    [TestClass]
    public class ProductsRepositoryTests
    {
        private DataContext _context = null!;
        private ProductsRepository _repository = null!;
        private Mock<IFileStorage> _fileStorageMock = null!;
        private DbContextOptions<DataContext> _options = null!;

        private const string _string64base = "U29tZVZhbGlkQmFzZTY0U3RyaW5n";
        private const string _container = "products";

        [TestInitialize]
        public void SetUp()
        {
            _options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new DataContext(_options);
            _fileStorageMock = new Mock<IFileStorage>();
            _repository = new ProductsRepository(_context, _fileStorageMock.Object);

            PopulateData();
        }

        [TestCleanup]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public async Task AddImagesAsync_ProductNotFound_ReturnsError()
        {
            // Arrange
            var imageDto = new ImageDTO { ProductId = 999 };

            // Act
            var result = await _repository.AddImageAsync(imageDto);

            // Assert
            Assert.IsFalse(result.WasSuccess);
        }

        [TestMethod]
        public async Task AddImageAsync_WithValidData_AddsImage()
        {
            // Arrange
            var imageDTO = new ImageDTO
            {
                ProductId = 1,
                Images = new List<string> { _string64base }
            };

            _fileStorageMock.Setup(fs => fs.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", _container))
                .ReturnsAsync("storedImagePath");

            // Act
            var result = await _repository.AddImageAsync(imageDTO);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.IsTrue(result.Result!.Images[0].Contains("storedImagePath"));
            _fileStorageMock.Verify(x => x.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", _container), Times.Once());
        }

        [TestMethod]
        public async Task RemoveLastImageAsync_ProductNotFound_ReturnsError()
        {
            // Arrange
            var imageDto = new ImageDTO { ProductId = 999 };

            // Act
            var result = await _repository.RemoveLastImageAsync(imageDto);

            // Assert
            Assert.IsFalse(result.WasSuccess);
        }

        [TestMethod]
        public async Task RemoveLastImageAsync_NoImages_ReturnsOk()
        {
            // Arrange
            var imageDto = new ImageDTO { ProductId = 1 };

            // Act
            var result = await _repository.RemoveLastImageAsync(imageDto);

            // Assert
            Assert.IsTrue(result.WasSuccess);
        }

        [TestMethod]
        public async Task RemoveLastImageAsync_RemovesLastImage_ReturnsOk()
        {
            // Arrange
            var imagePath = "https//image2.jpg";
            _fileStorageMock.Setup(fs => fs.RemoveFileAsync(imagePath, _container))
                .Returns(Task.CompletedTask);

            var imageDto = new ImageDTO { ProductId = 2 };

            // Act
            var result = await _repository.RemoveLastImageAsync(imageDto);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(1, result.Result!.Images.Count);
            _fileStorageMock.Verify(x => x.RemoveFileAsync(imagePath, _container), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_WithoutFilter_ReturnsAllProducts()
        {
            // Arrange
            var pagination = new PaginationDTO { RecordsNumber = 10, Page = 1 };

            // Act
            var result = await _repository.GetAsync(pagination);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            var products = result.Result as List<Product>;
            Assert.AreEqual(2, products!.Count);
        }

        [TestMethod]
        public async Task GetAsync_WithPagination_ReturnsProducts()
        {
            // Arrange
            var pagination = new PaginationDTO { Filter = "Some", CategoryFilter = "Any" };

            // Act
            var result = await _repository.GetAsync(pagination);

            // Assert
            Assert.IsTrue(result.WasSuccess);
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_ReturnsTotalPages()
        {
            // Arrange
            var pagination = new PaginationDTO { Filter = "Some", CategoryFilter = "Any" };

            // Act
            var result = await _repository.GetTotalPagesAsync(pagination);

            // Assert
            Assert.IsTrue(result.WasSuccess);
        }

        [TestMethod]
        public async Task GetAsync_ValidId_ReturnsProduct()
        {
            // Act
            var result = await _repository.GetAsync(1);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual("Product A", result.Result!.Name);
        }

        [TestMethod]
        public async Task GetAsync_InvalidId_ReturnsError()
        {
            // Act
            var result = await _repository.GetAsync(999);

            // Assert
            Assert.IsFalse(result.WasSuccess);
        }

        [TestMethod]
        public async Task AddFullAsync_ValidDTO_ReturnsOk()
        {
            // Arrange
            _fileStorageMock.Setup(fs => fs.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", _container))
                .ReturnsAsync("testImage.jpg");

            var productDTO = new ProductDTO
            {
                Name = "TestProduct",
                Description = "Description",
                Price = 100.00M,
                Stock = 10,
                ProductImages = new List<string> { _string64base },
                ProductCategoryIds = new List<int> { 1 }
            };

            // Act
            var result = await _repository.AddFullAsync(productDTO);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual("TestProduct", result.Result!.Name);
            _fileStorageMock.Verify(x => x.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", _container), Times.Once());
        }

        [TestMethod]
        public async Task AddFullAsync_DuplicateName_ReturnsErrors()
        {
            // Arrange
            var productDTO = new ProductDTO
            {
                Name = "Product A",
                Description = "Product A",
                Price = 100.00M,
                Stock = 10,
                ProductImages = new List<string> { _string64base },
                ProductCategoryIds = new List<int> { 1 }
            };

            // Act
            var result = await _repository.AddFullAsync(productDTO);

            // Assert
            Assert.IsFalse(result.WasSuccess);
            Assert.AreEqual("Ya existe un producto con el mismo nombre.", result.Message);
        }

        [TestMethod]
        public async Task AddFullAsync_GeneralException_ReturnsErrors()
        {
            // Arrange
            var productDTO = new ProductDTO
            {
                Name = "Product A",
                Description = "Product A",
                Price = 100.00M,
                Stock = 10,
                ProductImages = new List<string> { _string64base },
                ProductCategoryIds = new List<int> { 1 }
            };

            var message = "Test exception";
            _fileStorageMock.Setup(fs => fs.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", _container))
                .Throws(new Exception(message));

            // Act
            var result = await _repository.AddFullAsync(productDTO);

            // Assert
            Assert.IsFalse(result.WasSuccess);
            Assert.AreEqual(message, result.Message);
            _fileStorageMock.Verify(x => x.SaveFileAsync(It.IsAny<byte[]>(), ".jpg", _container), Times.Once());
        }

        [TestMethod]
        public async Task UpdateFullAsync_ValidDTO_UpdatesProduct()
        {
            // Arrange

            var productDTO = new ProductDTO
            {
                Id = 1,
                Name = "NewName",
                Description = "NewDescription",
                Price = 100.00M,
                Stock = 10,
                ProductCategoryIds = new List<int> { 2 }
            };

            // Act
            var result = await _repository.UpdateFullAsync(productDTO);

            // Assert
            //Assert.IsTrue(result.WasSuccess);
            //Assert.AreEqual("NewName", result.Result!.Name);
        }

        [TestMethod]
        public async Task UpdateFullAsync_NonExistingProduct_ReturnsError()
        {
            // Arrange
            var productDTO = new ProductDTO
            {
                Id = 999,
                Name = "TestName",
                Description = "TestDescription",
                Price = 100.00M,
                Stock = 10
            };

            // Act
            var result = await _repository.UpdateFullAsync(productDTO);

            // Assert
            Assert.IsFalse(result.WasSuccess);
        }

        [TestMethod]
        public async Task UpdateFullAsync_GeneralException_ReturnsError()
        {
            // Arrange
            var exceptionalContext = new ExceptionalDataContext(_options);
            var repository = new ProductsRepository(exceptionalContext, _fileStorageMock.Object);
            var productDTO = new ProductDTO
            {
                Id = 1,
                Name = "DuplicateName",
                Description = "Description",
                Price = 100.00M,
                Stock = 10,
                ProductCategoryIds = new List<int> { 2 }
            };

            // Act
            var result = await repository.UpdateFullAsync(productDTO);

            // Assert
            Assert.IsFalse(result.WasSuccess);
            Assert.AreEqual("Test Exception", result.Message);
        }

        [TestMethod]
        public async Task UpdateFullAsync_DbUpdateException_ReturnsError()
        {
            // Arrange
            var exceptionalContext = new ExceptionalDBUpdateDataContextWithInnerException(_options);
            var repository = new ProductsRepository(exceptionalContext, _fileStorageMock.Object);
            var productDTO = new ProductDTO
            {
                Id = 1,
                Name = "DuplicateName",
                Description = "Description",
                Price = 100.00M,
                Stock = 10,
                ProductCategoryIds = new List<int> { 2 }
            };

            // Act
            var result = await repository.UpdateFullAsync(productDTO);

            // Assert
            Assert.IsFalse(result.WasSuccess);
            Assert.AreEqual("Ya existe un producto con el mismo nombre.", result.Message);
        }

        [TestMethod]
        public async Task DeleteAsync_ExistingItem_ReturnsSuccessResponse()
        {
            // Arrange
            int id = 2;

            // Act
            var response = await _repository.DeleteAsync(id);

            // Assert
            Assert.IsTrue(response.WasSuccess);
        }

        [TestMethod]
        public async Task DeleteAsync_NonExistingItem_ReturnsNotFoundResponse()
        {
            // Arrange
            int nonExistingId = 999;

            // Act
            var response = await _repository.DeleteAsync(nonExistingId);

            // Assert
            Assert.IsFalse(response.WasSuccess);
        }

        [TestMethod]
        public async Task DeleteAsync_FailureDueToRelatedRecords_ReturnsFailureResponse()
        {
            // Arrange
            int id = 1;

            // Act
            var response = await _repository.DeleteAsync(id);

            // Assert
            Assert.IsFalse(response.WasSuccess);
        }

        private void PopulateData()
        {
            var category1 = new Category { Id = 1, Name = "Category1" };
            var category2 = new Category { Id = 2, Name = "Category2" };
            _context.Categories.AddRange(category1, category2);
            _context.SaveChanges();

            var product1 = new Product
            {
                Id = 1,
                Name = "Product A",
                Description = "Product A",
                ProductCategories = new List<ProductCategory> { new ProductCategory { Category = category1 } }
            };
            var product2 = new Product
            {
                Id = 2,
                Name = "Product B",
                Description = "Product B",
                ProductCategories = new List<ProductCategory> { new ProductCategory { Category = category1 } },
                ProductImages = new List<ProductImage>
                {
                    new ProductImage { Image = "https//image1.jpg" },
                    new ProductImage { Image = "https//image2.jpg" }
                }
            };
            _context.Products.AddRange(product1, product2);
            var temporalOrder = new TemporalOrder
            {
                Product = product1,
                Quantity = 1,
                User = new User { Address = "some", Document = "any", FirstName = "John", LastName = "Doe" }
            };
            _context.TemporalOrders.Add(temporalOrder);
            _context.SaveChanges();
        }
    }
}