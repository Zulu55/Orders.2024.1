using Microsoft.Extensions.Configuration;
using MimeKit;
using Moq;
using Orders.Backend.Helpers;

namespace Orders.Tests.Helpers
{
    [TestClass]
    public class MailHelperTests
    {
        private Mock<IConfiguration> _configurationMock = null!;
        private Mock<ISmtpClient> _smtpClientMock = null!;
        private MailHelper _mailHelper = null!;

        [TestInitialize]
        public void Initialize()
        {
            _configurationMock = new Mock<IConfiguration>();
            _smtpClientMock = new Mock<ISmtpClient>();

            _configurationMock.SetupGet(x => x["Mail:From"]).Returns("From");
            _configurationMock.SetupGet(x => x["Mail:Name"]).Returns("Name");
            _configurationMock.SetupGet(x => x["Mail:Smtp"]).Returns("Smtp");
            _configurationMock.SetupGet(x => x["Mail:Port"]).Returns("123");
            _configurationMock.SetupGet(x => x["Mail:Password"]).Returns("Password");

            _mailHelper = new MailHelper(_configurationMock.Object, _smtpClientMock.Object);
        }

        [TestMethod]
        public void SendMail_ShouldReturnSuccessActionResponse()
        {
            // Arrange
            var toName = "John Doe";
            var toEmail = "john.doe@example.com";
            var subject = "Test Subject";
            var body = "Test Body";

            // Act
            var response = _mailHelper.SendMail(toName, toEmail, subject, body);

            // Assert
            Assert.IsTrue(response.WasSuccess);
            _smtpClientMock.Verify(x => x.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
            _smtpClientMock.Verify(x => x.Authenticate(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _smtpClientMock.Verify(x => x.Send(It.IsAny<MimeMessage>()), Times.Once);
            _smtpClientMock.Verify(x => x.Disconnect(It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        public void SendMail_ShouldReturnErrorActionResponse_WhenExceptionThrown()
        {
            // Arrange
            var toName = "John Doe";
            var toEmail = "john.doe@example.com";
            var subject = "Test Subject";
            var body = "Test Body";
            var exceptionMessage = "SMTP error";

            _smtpClientMock.Setup(x => x.Send(It.IsAny<MimeMessage>())).Throws(new Exception(exceptionMessage));

            // Act
            var response = _mailHelper.SendMail(toName, toEmail, subject, body);

            // Assert
            Assert.IsFalse(response.WasSuccess);
            Assert.AreEqual(exceptionMessage, response.Message);
        }
    }
}
