using Microsoft.EntityFrameworkCore;
using Orders.Backend.Data;
using Orders.Backend.Repositories.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;

namespace Orders.Tests.Repositories
{
    [TestClass]
    public class CountriesRepositoryTests
    {
        private DataContext _context = null!;
        private CountriesRepository _repository = null!;

        [TestInitialize]
        public void Initialize()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
            _repository = new CountriesRepository(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var countries = new[]
            {
                new Country { Id = 1, Name = "USA" },
                new Country { Id = 2, Name = "Canada" },
                new Country { Id = 3, Name = "Mexico" },
            };

            _context.Countries.AddRange(countries);
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task GetAsync_Pagination_ShouldReturnPaginatedCountries()
        {
            // Arrange
            var pagination = new PaginationDTO { Page = 1, RecordsNumber = 2, Filter = "USA" };

            // Act
            var response = await _repository.GetAsync(pagination);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.AreEqual(1, response!.Result!.Count());
        }

        [TestMethod]
        public async Task GetAsync__ShouldReturnCountries()
        {
            // Act
            var response = await _repository.GetAsync();

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.AreEqual(3, response!.Result!.Count());
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_ShouldReturnTotalPages()
        {
            // Arrange
            var pagination = new PaginationDTO { RecordsNumber = 2, Filter = "Mexico" };

            // Act
            var response = await _repository.GetTotalPagesAsync(pagination);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.AreEqual(1, response.Result);
        }

        [TestMethod]
        public async Task GetAsync_ById_ShouldReturnCountry()
        {
            // Arrange
            var countryId = 1;

            // Act
            var response = await _repository.GetAsync(countryId);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual("USA", response.Result.Name);
        }

        [TestMethod]
        public async Task GetAsync_ById_ShouldReturnNotFoundForInvalidId()
        {
            // Arrange
            var countryId = 10;

            // Act
            var response = await _repository.GetAsync(countryId);

            // Assert
            Assert.IsFalse(response.WasSuccess);
            Assert.IsNull(response.Result);
            Assert.AreEqual("País no existe", response.Message);
        }

        [TestMethod]
        public async Task GetComboAsync_ShouldReturnAllCountries()
        {
            // Act
            var countries = await _repository.GetComboAsync();

            // Assert
            Assert.AreEqual(3, countries.Count());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
