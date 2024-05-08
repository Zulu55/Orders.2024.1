using Microsoft.EntityFrameworkCore;
using Moq;
using Orders.Backend.Data;
using Orders.Backend.Repositories.Implementations;
using Orders.Backend.Repositories.Interfaces;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Shared.Enums;

namespace Orders.Tests.Repositories
{
    [TestClass]
    public class OrdersRepositoryTests
    {
        private DataContext _context = null!;
        private OrdersRepository _repository = null!;
        private Mock<IUsersRepository> _mockUserRepository = null!;

        [TestInitialize]
        public void Initialize()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
            _mockUserRepository = new Mock<IUsersRepository>();
            _repository = new OrdersRepository(_context, _mockUserRepository.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        [TestMethod]
        public async Task GetAsync_UserDoesNotExist_ReturnsFailedActionResponse()
        {
            // Act
            var response = await _repository.GetAsync("nonexistentuser@example.com", new PaginationDTO());

            // Assert
            Assert.IsFalse(response.WasSuccess);
            Assert.AreEqual("Usuario no válido", response.Message);
        }

        [TestMethod]
        public async Task GetAsync_ValidUserAndOrder_ReturnsOrders()
        {
            // Arrange
            var email = "test@example.com";
            var user = await CreateTestUser(email, UserType.User);
            await CreateTestOrder(user);
            _mockUserRepository.Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()))
                .ReturnsAsync(false);

            // Act
            var response = await _repository.GetAsync(email, new PaginationDTO());

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(1, response.Result.Count());
            _mockUserRepository.Verify(x => x.GetUserAsync(email), Times.Once());
            _mockUserRepository.Verify(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()), Times.Once());
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_UserDoesNotExist_ReturnsFailedActionResponse()
        {
            // Act
            var response = await _repository.GetTotalPagesAsync("nonexistentuser@example.com", new PaginationDTO());

            // Assert
            Assert.IsFalse(response.WasSuccess);
            Assert.AreEqual("Usuario no válido", response.Message);
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_ReturnsCorrectNumberOfPages()
        {
            // Arrange
            var email = "test@example.com";
            var user = await CreateTestUser(email, UserType.User);
            await CreateTestOrder(user);
            _mockUserRepository.Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()))
                .ReturnsAsync(false);
            var pagination = new PaginationDTO { RecordsNumber = 2, Page = 1 };

            // Act
            var response = await _repository.GetTotalPagesAsync(email, pagination);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.AreEqual(1, response.Result);
            _mockUserRepository.Verify(x => x.GetUserAsync(email), Times.Once());
            _mockUserRepository.Verify(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_OrderDoesNotExist_ReturnsFailedActionResponse()
        {
            // Act
            var response = await _repository.GetAsync(999);

            // Assert
            Assert.IsFalse(response.WasSuccess);
            Assert.AreEqual("Pedido no existe", response.Message);
        }

        [TestMethod]
        public async Task GetAsync_OrderExists_ReturnsOrder()
        {
            // Arrange
            var email = "test@example.com";
            var user = await CreateTestUser(email, UserType.User);
            var order = await CreateTestOrder(user);

            // Act
            var response = await _repository.GetAsync(order.Id);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.IsNotNull(response.Result);
            Assert.AreEqual(order.Id, response.Result.Id);
        }

        [TestMethod]
        public async Task UpdateFullAsync_UserDoesNotExist_ReturnsFailedActionResponse()
        {
            // Arrange
            var orderDTO = new OrderDTO { Id = 1, OrderStatus = OrderStatus.Sent };

            // Act
            var response = await _repository.UpdateFullAsync("nonexistentuser@example.com", orderDTO);

            // Assert
            Assert.IsFalse(response.WasSuccess);
            Assert.AreEqual("Usuario no existe", response.Message);
        }

        [TestMethod]
        public async Task UpdateFullAsync_OrderDoesNotExist_ReturnsFailedActionResponse()
        {
            // Arrange
            var email = "test@example.com";
            var user = await CreateTestUser(email, UserType.User);
            _mockUserRepository.Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()))
                .ReturnsAsync(true);
            var orderDTO = new OrderDTO { Id = 999, OrderStatus = OrderStatus.Sent };

            // Act
            var response = await _repository.UpdateFullAsync(email, orderDTO);

            // Assert
            Assert.IsFalse(response.WasSuccess);
            Assert.AreEqual("Pedido no existe", response.Message);
            _mockUserRepository.Verify(x => x.GetUserAsync(email), Times.Once());
            _mockUserRepository.Verify(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()), Times.Once());
        }

        [TestMethod]
        public async Task UpdateFullAsync_ValidData_UpdatesOrder()
        {
            // Arrange
            var email = "admin@example.com";
            var user = await CreateTestUser(email, UserType.Admin);
            var order = await CreateTestOrder(user);
            _mockUserRepository.Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()))
                .ReturnsAsync(true);
            var orderDTO = new OrderDTO { Id = order.Id, OrderStatus = OrderStatus.Sent };

            // Act
            var response = await _repository.UpdateFullAsync(email, orderDTO);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.AreEqual(OrderStatus.Sent, response.Result!.OrderStatus);
            _mockUserRepository.Verify(x => x.GetUserAsync(email), Times.Once());
            _mockUserRepository.Verify(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()), Times.Once());
        }

        [TestMethod]
        public async Task UpdateFullAsync_UserNoAdmin_ReturnError()
        {
            // Arrange
            var email = "user@example.com";
            var user = await CreateTestUser(email, UserType.User);
            var order = await CreateTestOrder(user);
            _mockUserRepository.Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()))
                .ReturnsAsync(false);
            var orderDTO = new OrderDTO { Id = order.Id, OrderStatus = OrderStatus.Sent };

            // Act
            var response = await _repository.UpdateFullAsync(email, orderDTO);

            // Assert
            Assert.IsFalse(response.WasSuccess);
            _mockUserRepository.Verify(x => x.GetUserAsync(email), Times.Once());
            _mockUserRepository.Verify(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()), Times.Once());
        }

        [TestMethod]
        public async Task UpdateFullAsync_CancelOrder_UpdatesOrderAndReturnInventory()
        {
            // Arrange
            var email = "admin@example.com";
            var user = await CreateTestUser(email, UserType.Admin);
            var order = await CreateTestOrderForCancel(user);
            _mockUserRepository.Setup(x => x.GetUserAsync(email))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()))
                .ReturnsAsync(true);
            var orderDTO = new OrderDTO { Id = order.Id, OrderStatus = OrderStatus.Cancelled };

            // Act
            var response = await _repository.UpdateFullAsync(email, orderDTO);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            Assert.AreEqual(OrderStatus.Cancelled, response.Result!.OrderStatus);
            _mockUserRepository.Verify(x => x.GetUserAsync(email), Times.Once());
            _mockUserRepository.Verify(x => x.IsUserInRoleAsync(user, UserType.Admin.ToString()), Times.Once());
        }

        private async Task<User> CreateTestUser(string email, UserType userType)
        {
            var user = new User { Email = email, UserType = userType, Address = "Any", Document = "Any", FirstName = "John", LastName = "Doe" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        private async Task<Order> CreateTestOrder(User user)
        {
            var order = new Order { User = user };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        private async Task<Order> CreateTestOrderForCancel(User user)
        {
            await _context.Products.AddAsync(new Product { Id = 1, Name = "Some", Description = "Some" });
            var order = new Order
            {
                User = user,
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail { Id = 1, ProductId = 1 }
                }
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }
    }
}