using Moq;
using Orders.Backend.Repositories.Interfaces;
using Orders.Backend.UnitsOfWork.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Responses;

namespace Orders.Tests.UnitsOfWork
{
    [TestClass]
    public class GenericUnitOfWorkTests
    {
        private Mock<IGenericRepository<object>> _mockRepository = null!;
        private GenericUnitOfWork<object> _unitOfWork = null!;
        private object _testModel = null!;
        private int _testId;
        private PaginationDTO _paginationDTO = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockRepository = new Mock<IGenericRepository<object>>();
            _unitOfWork = new GenericUnitOfWork<object>(_mockRepository.Object);
            _testModel = new object();
            _testId = 1;
            _paginationDTO = new PaginationDTO();
        }

        [TestMethod]
        public async Task AddAsync_Success()
        {
            _mockRepository.Setup(x => x.AddAsync(It.IsAny<object>()))
                .ReturnsAsync(new ActionResponse<object> { Result = _testModel });

            var result = await _unitOfWork.AddAsync(_testModel);

            Assert.IsNotNull(result);
            Assert.AreEqual(_testModel, result.Result);
        }

        [TestMethod]
        public async Task DeleteAsync_Success()
        {
            _mockRepository.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(new ActionResponse<object> { Result = _testModel });

            var result = await _unitOfWork.DeleteAsync(_testId);

            Assert.IsNotNull(result);
            Assert.AreEqual(_testModel, result.Result);
        }

        [TestMethod]
        public async Task GetAsync_Pagination_Success()
        {
            _mockRepository.Setup(x => x.GetAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(new ActionResponse<IEnumerable<object>> { Result = new List<object> { _testModel } });

            var result = await _unitOfWork.GetAsync(_paginationDTO);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Result!.Count());
        }

        [TestMethod]
        public async Task GetTotalPagesAsync_Success()
        {
            _mockRepository.Setup(x => x.GetTotalPagesAsync(It.IsAny<PaginationDTO>()))
                .ReturnsAsync(new ActionResponse<int> { Result = 5 });

            var result = await _unitOfWork.GetTotalPagesAsync(_paginationDTO);

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Result);
        }

        [TestMethod]
        public async Task GetAsync_Id_Success()
        {
            _mockRepository.Setup(x => x.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new ActionResponse<object> { Result = _testModel });

            var result = await _unitOfWork.GetAsync(_testId);

            Assert.IsNotNull(result);
            Assert.AreEqual(_testModel, result.Result);
        }

        [TestMethod]
        public async Task GetAsync_Success()
        {
            _mockRepository.Setup(x => x.GetAsync())
                .ReturnsAsync(new ActionResponse<IEnumerable<object>> { Result = new List<object> { _testModel } });

            var result = await _unitOfWork.GetAsync();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task UpdateAsync_Success()
        {
            _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<object>()))
                .ReturnsAsync(new ActionResponse<object> { Result = _testModel });

            var result = await _unitOfWork.UpdateAsync(_testModel);

            Assert.IsNotNull(result);
            Assert.AreEqual(_testModel, result.Result);
        }
    }
}