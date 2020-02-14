using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LTPhotoAlbum.UnitTests
{
    public class UnitTests
    {
        private ConsoleApp _consoleApp;
        private Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
        private Mock<ILogger<ConsoleApp>> _mockLogger;
        private Mock<IConsoleProcessor> _mockConsoleProcessor;
        private Mock<IPhotoAlbumClient> _mockPhotoAlbumClient;

        public UnitTests()
        {
            _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();
            _mockLogger = new Mock<ILogger<ConsoleApp>>();
            _mockConsoleProcessor = new Mock<IConsoleProcessor>();
            _mockPhotoAlbumClient = new Mock<IPhotoAlbumClient>();
            _consoleApp = new ConsoleApp(_mockHostApplicationLifetime.Object, _mockLogger.Object, _mockConsoleProcessor.Object, _mockPhotoAlbumClient.Object);
        }

        [Fact]
        public async Task Process_HappyPath()
        {
            // Arrange
            _mockConsoleProcessor.Setup(x => x.RequestUserInput()).Returns("1");
            _mockPhotoAlbumClient.Setup(x => x.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Enumerable.Empty<Photo>()));

            // Act
            await _consoleApp.Process(new CancellationTokenSource().Token);

            // Assert
            // Everything should be called exactly once
            _mockConsoleProcessor.Verify(m => m.RequestUserInput(), Times.Once);
            _mockPhotoAlbumClient.Verify(m => m.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockConsoleProcessor.Verify(m => m.DisplayPhotoData(It.IsAny<IEnumerable<Photo>>()), Times.Once);
        }

        [Fact]
        public async Task Process_InvalidInput_AsksTwice()
        {
            // Arrange
            // Return a non-integer the first time, then an integer the second
            _mockConsoleProcessor.SetupSequence(x => x.RequestUserInput())
                .Returns("f")
                .Returns("1");
            _mockPhotoAlbumClient.Setup(x => x.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Enumerable.Empty<Photo>()));

            // Act
            await _consoleApp.Process(new CancellationTokenSource().Token);

            // Assert
            // Ensure that while RequestUserInput was called twice, the actual processing was only done once since the first input was invalid
            _mockConsoleProcessor.Verify(m => m.RequestUserInput(), Times.Exactly(2));
            _mockPhotoAlbumClient.Verify(m => m.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockConsoleProcessor.Verify(m => m.DisplayPhotoData(It.IsAny<IEnumerable<Photo>>()), Times.Once);
        }

        [Fact]
        public async Task Process_APIThrowsException_NoDisplay()
        {
            // Arrange
            _mockConsoleProcessor.Setup(x => x.RequestUserInput()).Returns("1");
            _mockPhotoAlbumClient.Setup(x => x.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>())).Throws(new ApiException(System.Net.HttpStatusCode.BadRequest, "Test exception"));

            // Act
            await _consoleApp.Process(new CancellationTokenSource().Token);

            // Assert
            // DisplayPhotoData should not be called since the API request failed
            _mockConsoleProcessor.Verify(m => m.RequestUserInput(), Times.Once);
            _mockPhotoAlbumClient.Verify(m => m.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockConsoleProcessor.Verify(m => m.DisplayPhotoData(It.IsAny<IEnumerable<Photo>>()), Times.Never);
        }
    }
}