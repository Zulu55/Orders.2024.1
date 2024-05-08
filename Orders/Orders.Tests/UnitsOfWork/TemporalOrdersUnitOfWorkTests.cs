using Moq;
using Orders.Backend.Repositories.Interfaces;
using Orders.Backend.UnitsOfWork.Implementations;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;
using Orders.Shared.Responses;

namespace Orders.Tests.UnitsOfWork
{
    [TestClass]
    public class TemporalOrdersUnitOfWorkTests
    {
        private TemporalOrdersUnitOfWork _unitOfWork = null!;
        private Mock<IGenericRepository<TemporalOrder>> _genericRepositoryMock = null!;
        private Mock<ITemporalOrdersRepository> _temporalOrdersRepositoryMock = null!;

        [TestInitialize]
        public void Initialize()
        {
            _genericRepositoryMock = new Mock<IGenericRepository<TemporalOrder>>();
            _temporalOrdersRepositoryMock = new Mock<ITemporalOrdersRepository>();
            _unitOfWork = new TemporalOrdersUnitOfWork(_genericRepositoryMock.Object, _temporalOrdersRepositoryMock.Object);
        }

        [TestMethod]
        public async Task AddFullAsync_CallsRepository_ReturnsResult()
        {
            var email = "test@example.com";
            var dto = new TemporalOrderDTO();
            var response = new ActionResponse<TemporalOrderDTO>();
            _temporalOrdersRepositoryMock.Setup(repo => repo.AddFullAsync(email, dto))
                .ReturnsAsync(response);

            var result = await _unitOfWork.AddFullAsync(email, dto);

            Assert.AreEqual(response, result);
            _temporalOrdersRepositoryMock.Verify(repo => repo.AddFullAsync(email, dto), Times.Once);
        }

        [TestMethod]
        public async Task GetAsync_CallsRepository_ReturnsResult()
        {
            var email = "test@example.com";
            var response = new ActionResponse<IEnumerable<TemporalOrder>>();
            _temporalOrdersRepositoryMock.Setup(repo => repo.GetAsync(email))
                .ReturnsAsync(response);

            var result = await _unitOfWork.GetAsync(email);

            Assert.AreEqual(response, result);
            _temporalOrdersRepositoryMock.Verify(repo => repo.GetAsync(email), Times.Once);
        }

        [TestMethod]
        public async Task GetCountAsync_CallsRepository_ReturnsResult()
        {
            var email = "test@example.com";
            var response = new ActionResponse<int>();
            _temporalOrdersRepositoryMock.Setup(repo => repo.GetCountAsync(email))
                .ReturnsAsync(response);

            var result = await _unitOfWork.GetCountAsync(email);

            Assert.AreEqual(response, result);
            _temporalOrdersRepositoryMock.Verify(repo => repo.GetCountAsync(email), Times.Once);
        }

        [TestMethod]
        public async Task PutFullAsync_CallsRepository_ReturnsResult()
        {
            var dto = new TemporalOrderDTO();
            var response = new ActionResponse<TemporalOrder>();
            _temporalOrdersRepositoryMock.Setup(repo => repo.PutFullAsync(dto))
                .ReturnsAsync(response);

            var result = await _unitOfWork.PutFullAsync(dto);

            Assert.AreEqual(response, result);
            _temporalOrdersRepositoryMock.Verify(repo => repo.PutFullAsync(dto), Times.Once);
        }

        [TestMethod]
        public async Task GetAsync_ById_CallsRepository_ReturnsResult()
        {
            int id = 1;
            var response = new ActionResponse<TemporalOrder>();
            _temporalOrdersRepositoryMock.Setup(repo => repo.GetAsync(id))
                .ReturnsAsync(response);

            var result = await _unitOfWork.GetAsync(id);

            Assert.AreEqual(response, result);
            _temporalOrdersRepositoryMock.Verify(repo => repo.GetAsync(id), Times.Once);
        }
    }
}