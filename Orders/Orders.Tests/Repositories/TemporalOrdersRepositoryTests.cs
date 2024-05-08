using Microsoft.EntityFrameworkCore;
using Moq;
using Orders.Backend.Data;
using Orders.Backend.Repositories.Implementations;
using Orders.Backend.Repositories.Interfaces;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Tests.Shared;

namespace Orders.Tests.Repositories
{
    [TestClass]
    public class TemporalOrdersRepositoryTests
    {
        private TemporalOrdersRepository _repository = null!;
        private DataContext _context = null!;
        private Mock<IUsersRepository> _userRepositoryMock = null!;
        private DbContextOptions<DataContext> _options = null!;

        [TestInitialize]
        public void Initialize()
        {
            _options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(_options);
            _userRepositoryMock = new Mock<IUsersRepository>();
            _repository = new TemporalOrdersRepository(_context, _userRepositoryMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public async Task AddFullAsync_ValidData_AddsTemporalOrder()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User { Email = email, Address = "Any", Document = "Any", FirstName = "John", LastName = "Doe" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var product = new Product { Id = 1, Name = "Some", Description = "Some" };
            _context.Products.Add(product);
            _context.SaveChanges();

            var dto = new TemporalOrderDTO
            {
                ProductId = product.Id,
                Quantity = 1
            };

            _userRepositoryMock.Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _repository.AddFullAsync(email, dto);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(1, _context.TemporalOrders.Count());
            var temporalOrder = _context.TemporalOrders.First();
            Assert.AreEqual(product.Id, temporalOrder.ProductId);
            Assert.AreEqual(1, temporalOrder.Quantity);
        }

        [TestMethod]
        public async Task AddFullAsync_WithException_ReturnsError()
        {
            // Arrange
            var exceptionalContext = new ExceptionalDataContext(_options);
            var email = "test@example.com";
            var user = new User { Email = email, Address = "Any", Document = "Any", FirstName = "John", LastName = "Doe" };
            exceptionalContext.Users.Add(user);
            exceptionalContext.SaveChanges();

            var product = new Product { Id = 1, Name = "Some", Description = "Some" };
            exceptionalContext.Products.Add(product);
            exceptionalContext.SaveChanges();

            var dto = new TemporalOrderDTO
            {
                ProductId = product.Id,
                Quantity = 1
            };

            _userRepositoryMock.Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);

            var repository = new TemporalOrdersRepository(exceptionalContext, _userRepositoryMock.Object);

            // Act
            var result = await repository.AddFullAsync(email, dto);

            // Assert
            Assert.IsFalse(result.WasSuccess);
        }

        [TestMethod]
        public async Task AddFullAsync_ValidUser_ReturnsError()
        {
            // Arrange
            var email = "test@example.com";
            var product = new Product { Id = 1, Name = "Some", Description = "Some" };
            _context.Products.Add(product);
            _context.SaveChanges();

            var dto = new TemporalOrderDTO
            {
                ProductId = product.Id,
                Quantity = 1
            };

            // Act
            var result = await _repository.AddFullAsync(email, dto);

            // Assert
            Assert.IsFalse(result.WasSuccess);
            Assert.AreEqual("Usuario no existe", result.Message);
        }

        [TestMethod]
        public async Task AddFullAsync_InvalidProduct_ReturnsError()
        {
            // Arrange
            var email = "test@example.com";
            var dto = new TemporalOrderDTO
            {
                ProductId = 999,
                Quantity = 1
            };

            // Act
            var result = await _repository.AddFullAsync(email, dto);

            // Assert
            Assert.IsFalse(result.WasSuccess);
            Assert.AreEqual("Producto no existe", result.Message);
        }

        [TestMethod]
        public async Task GetAsync_UserExists_ReturnsTemporalOrders()
        {
            // Arrange
            var email = "test@example.com";
            var product = new Product { Id = 1, Name = "Some", Description = "Some" };
            _context.Products.Add(product);
            var user = new User { Email = email, Address = "Any", Document = "Any", FirstName = "John", LastName = "Doe" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var temporalOrders = new List<TemporalOrder>
            {
                new TemporalOrder { User = user, Product = product, Quantity = 1 },
                new TemporalOrder { User = user, Product = product, Quantity = 2 }
            };

            _context.TemporalOrders.AddRange(temporalOrders);
            _context.SaveChanges();

            // Act
            var result = await _repository.GetAsync(email);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(2, result.Result!.Count());
        }

        [TestMethod]
        public async Task GetCountAsync_UserWithNoOrders_ReturnsZero()
        {
            // Arrange
            var email = "test@example.com";

            // Act
            var result = await _repository.GetCountAsync(email);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(0, result.Result);
        }

        [TestMethod]
        public async Task GetCountAsync_UserDoesNotExist_ReturnsZero()
        {
            // Arrange
            var email = "nonexistent@example.com";

            // Act
            var result = await _repository.GetCountAsync(email);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(0, result.Result);
        }

        [TestMethod]
        public async Task PutFullAsync_OrderExists_UpdatesOrder()
        {
            // Arrange
            var temporalOrder = new TemporalOrder { Id = 1, Remarks = "Old Remarks", Quantity = 5 };
            _context.TemporalOrders.Add(temporalOrder);
            await _context.SaveChangesAsync();

            var updateDTO = new TemporalOrderDTO { Id = 1, Remarks = "New Remarks", Quantity = 10 };

            // Act
            var result = await _repository.PutFullAsync(updateDTO);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.AreEqual(updateDTO.Remarks, result.Result!.Remarks);
            Assert.AreEqual(updateDTO.Quantity, result.Result.Quantity);
        }

        [TestMethod]
        public async Task PutFullAsync_OrderDoesNotExist_ReturnsErrorActionResponse()
        {
            // Arrange
            var updateDTO = new TemporalOrderDTO { Id = 99, Remarks = "New Remarks", Quantity = 10 };

            // Act
            var result = await _repository.PutFullAsync(updateDTO);

            // Assert
            Assert.IsFalse(result.WasSuccess);
            Assert.AreEqual("Registro no encontrado", result.Message);
        }

        [TestMethod]
        public async Task GetAsync_OrderExists_ReturnsOrder()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User { Email = email, Address = "Any", Document = "Any", FirstName = "John", LastName = "Doe" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var product = new Product { Id = 1, Name = "Some", Description = "Some" };
            _context.Products.Add(product);
            _context.SaveChanges();

            var temporalOrder = new TemporalOrder { Id = 1, User = user, Product = product };
            _context.TemporalOrders.Add(temporalOrder);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(1);

            // Assert
            Assert.IsTrue(result.WasSuccess);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(1, result.Result.Id);
        }

        [TestMethod]
        public async Task GetAsync_OrderDoesNotExist_ReturnsErrorActionResponse()
        {
            // Act
            var result = await _repository.GetAsync(99);

            // Assert
            Assert.IsFalse(result.WasSuccess);
            Assert.AreEqual("Registro no encontrado", result.Message);
        }
    }
}