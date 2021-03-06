﻿using Microsoft.Extensions.Hosting;
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
            _mockConsoleProcessor.Setup(x => x.ParseUserInput(It.IsAny<string>())).Returns(1);
            _mockPhotoAlbumClient.Setup(x => x.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult((IList<Photo>)new List<Photo>()));

            // Act
            await _consoleApp.Process(new CancellationTokenSource().Token);

            // Assert
            // Each of these should be called exactly once for a happy path
            _mockConsoleProcessor.Verify(m => m.ParseUserInput(It.IsAny<string>()), Times.Once);
            _mockPhotoAlbumClient.Verify(m => m.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockConsoleProcessor.Verify(m => m.GetPhotoDataString(It.IsAny<IList<Photo>>()), Times.Once);
        }

        [Fact]
        public async Task Process_InvalidInput_AsksUntilValid()
        {
            // Arrange
            // Return a non-integer the first time, then null, then an integer
            _mockConsoleProcessor.SetupSequence(x => x.ParseUserInput(It.IsAny<string>()))
                .Returns((int?)null)
                .Returns((int?)null)
                .Returns(1);
            _mockPhotoAlbumClient.Setup(x => x.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult((IList<Photo>)new List<Photo>()));

            // Act
            await _consoleApp.Process(new CancellationTokenSource().Token);

            // Assert
            // Ensure that while RequestUserInput was called three times, the actual processing was only done once since the first inputs were invalid
            // This ensures that the input is being validated
            _mockConsoleProcessor.Verify(m => m.ParseUserInput(It.IsAny<string>()), Times.Exactly(3));
            _mockPhotoAlbumClient.Verify(m => m.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockConsoleProcessor.Verify(m => m.GetPhotoDataString(It.IsAny<IList<Photo>>()), Times.Once);
        }

        [Fact]
        public async Task Process_APIRequestFails_DoesNotDisplay()
        {
            // Arrange
            _mockConsoleProcessor.Setup(x => x.ParseUserInput(It.IsAny<string>())).Returns(1);
            _mockPhotoAlbumClient.Setup(x => x.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>())).Throws(new ApiException(System.Net.HttpStatusCode.BadRequest, "Test exception"));

            // Act
            await _consoleApp.Process(new CancellationTokenSource().Token);

            // Assert
            // DisplayPhotoData should not be called since the API request failed
            _mockConsoleProcessor.Verify(m => m.ParseUserInput(It.IsAny<string>()), Times.Once);
            _mockPhotoAlbumClient.Verify(m => m.GetPhotos(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockConsoleProcessor.Verify(m => m.GetPhotoDataString(It.IsAny<IList<Photo>>()), Times.Never);
        }
    }
}