using Microsoft.EntityFrameworkCore;
using Orders.Backend.Data;
using Orders.Backend.Repositories.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;

namespace Orders.Tests.Repositories
{
    [TestClass]
    public class CategoriesRepositoryTests
    {
        private DataContext _context = null!;
        private CategoriesRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
            _repository = new CategoriesRepository(_context);

            _context.Categories.AddRange(new List<Category>
            {
                new Category { Id = 1, Name = "Electronics" },
                new Category { Id = 2, Name = "Books" },
                new Category { Id = 3, Name = "Clothing" },
            });

            _context.SaveChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public async Task GetAsync_ReturnsFilteredCategories()
        {
            // Arrange
            var pagination = new PaginationDTO { Filter = "Book", RecordsNumber = 10, Page = 1 };

            // Act
            var response = await _repository.GetAsync(pagination);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            var categories = response.Result!.ToList();
            Assert.AreEqual(1, categories.Count);
            Assert.AreEqual("Books", categories.First().Name);
        }

        [TestMethod]
        public async Task GetAsync_ReturnsAllCategories_WhenNoFilterIsProvided()
        {
            // Arrange
            var pagination = new PaginationDTO { RecordsNumber = 10, Page = 1 };

            // Act
            var response = await _repository.GetAsync(pagination);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            var categories = response.Result!.ToList();
            Assert.AreEqual(3, categories.Count);
        }

        [TestMethod]
        public async Task GetComboAsync_ReturnsAllCategories()
        {
            // Act
            var categories = await _repository.GetComboAsync();

            // Assert
            Assert.AreEqual(3, categories.Count());
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_ReturnsCorrectNumberOfPages()
        {
            // Arrange
            var pagination = new PaginationDTO { RecordsNumber = 2, Page = 1 };

            // Act
            var response = await _repository.GetTotalPagesAsync(pagination);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.AreEqual(2, response.Result);
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_WithFilter_ReturnsCorrectNumberOfPages()
        {
            // Arrange
            var pagination = new PaginationDTO { RecordsNumber = 2, Page = 1, Filter = "Bo" };

            // Act
            var response = await _repository.GetTotalPagesAsync(pagination);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.AreEqual(1, response.Result);
        }
    }
}