using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Orders.Backend.Controllers;
using Orders.Backend.UnitsOfWork.Interfaces;
using Orders.Shared.DTOs;
using Orders.Shared.Responses;

namespace Orders.Tests.Controllers
{
    [TestClass]
    public class GenericControllerTests
    {
        private Mock<IGenericUnitOfWork<object>> _mockUnitOfWork = null!;
        private PaginationDTO _paginationDTO = null!;
        private object _testModel = null!;
        private int _testId;

        [TestInitialize]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IGenericUnitOfWork<object>>();
            _paginationDTO = new PaginationDTO();
            _testModel = new object();
            _testId = 1;
        }

        [TestMethod]
        public async Task GetAsync_Pagination_Success()
        {
            // Arrange
            var response = new ActionResponse<IEnumerable<object>> { WasSuccess = true };
            _mockUnitOfWork.Setup(x => x.GetAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.GetAsync(_paginationDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockUnitOfWork.Verify(x => x.GetAsync(It.IsAny<PaginationDTO>()), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_Pagination_Failure()
        {
            // Arrange
            var response = new ActionResponse<IEnumerable<object>> { WasSuccess = false };
            _mockUnitOfWork.Setup(x => x.GetAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.GetAsync(_paginationDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockUnitOfWork.Verify(x => x.GetAsync(It.IsAny<PaginationDTO>()), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_Success()
        {
            // Arrange
            var response = new ActionResponse<int> { WasSuccess = true, Result = 5 };
            _mockUnitOfWork.Setup(x => x.GetTotalPagesAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.GetPagesAsync(_paginationDTO);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(5, okResult.Value);
            _mockUnitOfWork.Verify(x => x.GetTotalPagesAsync(It.IsAny<PaginationDTO>()), Times.Once());
        }

        [TestMethod]
        public async Task GetPagesAsync_Failure()
        {
            // Arrange
            var response = new ActionResponse<int> { WasSuccess = false };
            _mockUnitOfWork.Setup(x => x.GetTotalPagesAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.GetPagesAsync(_paginationDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockUnitOfWork.Verify(x => x.GetTotalPagesAsync(It.IsAny<PaginationDTO>()), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_Id_Success()
        {
            // Arrange
            var response = new ActionResponse<object> { WasSuccess = true, Result = _testModel };
            _mockUnitOfWork.Setup(x => x.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.GetAsync(_testId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(_testModel, okResult.Value);
            _mockUnitOfWork.Verify(x => x.GetAsync(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_Id_NotFound()
        {
            // Arrange
            var response = new ActionResponse<object> { WasSuccess = false };
            _mockUnitOfWork.Setup(x => x.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.GetAsync(_testId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            _mockUnitOfWork.Verify(x => x.GetAsync(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public async Task PostAsync_Success()
        {
            // Arrange
            var response = new ActionResponse<object> { WasSuccess = true, Result = _testModel };
            _mockUnitOfWork.Setup(x => x.AddAsync(It.IsAny<object>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.PostAsync(_testModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(_testModel, okResult.Value);
            _mockUnitOfWork.Verify(x => x.AddAsync(It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public async Task PostAsync_Failure()
        {
            // Arrange
            var response = new ActionResponse<object> { WasSuccess = false, Message = "Error" };
            _mockUnitOfWork.Setup(x => x.AddAsync(It.IsAny<object>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.PostAsync(_testModel);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Error", badRequestResult.Value);
            _mockUnitOfWork.Verify(x => x.AddAsync(It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public async Task PutAsync_Success()
        {
            // Arrange
            var response = new ActionResponse<object> { WasSuccess = true, Result = _testModel };
            _mockUnitOfWork.Setup(x => x.UpdateAsync(It.IsAny<object>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.PutAsync(_testModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(_testModel, okResult.Value);
            _mockUnitOfWork.Verify(x => x.UpdateAsync(It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public async Task PutAsync_Failure()
        {
            // Arrange
            var response = new ActionResponse<object> { WasSuccess = false, Message = "Error" };
            _mockUnitOfWork.Setup(x => x.UpdateAsync(It.IsAny<object>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.PutAsync(_testModel);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Error", badRequestResult.Value);
            _mockUnitOfWork.Verify(x => x.UpdateAsync(It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public async Task DeleteAsync_Success()
        {
            // Arrange
            var response = new ActionResponse<object> { WasSuccess = true };
            _mockUnitOfWork.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.DeleteAsync(_testId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockUnitOfWork.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public async Task DeleteAsync_GetFailed()
        {
            // Arrange
            var response = new ActionResponse<object> { WasSuccess = false, Message = "Error." };
            _mockUnitOfWork.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.DeleteAsync(_testId);

            // Assert                                                                                                                                                               
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _mockUnitOfWork.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_Success()
        {
            // Arrange
            var response = new ActionResponse<IEnumerable<object>> { WasSuccess = true };
            _mockUnitOfWork.Setup(x => x.GetAsync())
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.GetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockUnitOfWork.Verify(x => x.GetAsync(), Times.Once());
        }

        [TestMethod]
        public async Task GetAsync_BadRequest()
        {
            // Arrange
            var response = new ActionResponse<IEnumerable<object>> { WasSuccess = false };
            _mockUnitOfWork.Setup(x => x.GetAsync())
                .ReturnsAsync(response);

            var controller = new GenericController<object>(_mockUnitOfWork.Object);

            // Act
            var result = await controller.GetAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
            _mockUnitOfWork.Verify(x => x.GetAsync(), Times.Once());
        }
    }
}