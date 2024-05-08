using Microsoft.EntityFrameworkCore;
using Orders.Backend.Data;
using Orders.Backend.Repositories.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;

namespace Orders.Tests.Repositories
{
    [TestClass]
    public class CitiesRepositoryTests
    {
        private DataContext _context = null!;
        private CitiesRepository _repository = null!;

        [TestInitialize]
        public void Initialize()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;

            _context = new DataContext(options);
            _repository = new CitiesRepository(_context);

            _context.Countries.Add(new Country { Id = 1, Name = "Country" });
            _context.States.AddRange(
                new State { Id = 1, Name = "State1", CountryId = 1 },
                new State { Id = 2, Name = "State2", CountryId = 1 });
            _context.Cities.AddRange(
                new City { Id = 1, Name = "City1", StateId = 1 },
                new City { Id = 2, Name = "City2", StateId = 1 },
                new City { Id = 3, Name = "City3", StateId = 2 }
            );
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnAllCitiesInStateWithPagination()
        {
            // Arrange
            var pagination = new PaginationDTO { Id = 1, RecordsNumber = 2, Page = 1, Filter = "City" };

            // Act
            var response = await _repository.GetAsync(pagination);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.AreEqual(2, response.Result!.Count());
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnFilteredCities()
        {
            // Arrange
            var pagination = new PaginationDTO { Id = 1, Filter = "City1", RecordsNumber = 10, Page = 1 };

            // Act
            var response = await _repository.GetAsync(pagination);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.AreEqual(1, response.Result!.Count());
            Assert.AreEqual("City1", response.Result!.First().Name);
        }

        [TestMethod]
        public async Task GetComboAsync_ShouldReturnAllCitiesInState()
        {
            // Arrange
            var stateId = 1;

            // Act
            var cities = await _repository.GetComboAsync(stateId);

            // Assert
            Assert.AreEqual(2, cities.Count());
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_ShouldReturnTotalPages()
        {
            // Arrange
            var pagination = new PaginationDTO { Id = 1, RecordsNumber = 1, Page = 1, Filter = "City" };

            // Act
            var response = await _repository.GetTotalPagesAsync(pagination);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.AreEqual(2, response.Result);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}