using LTPhotoAlbum;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace LTPhotoAlbum2.Tests
{
    public class ConsoleProcessorTests
    {
        private ConsoleProcessor _processor;

        public ConsoleProcessorTests()
        {
            _processor = new ConsoleProcessor();
        }

        [Fact]
        public void AskForInput_IntegerInput_ValidInput()
        {
            string input = "1";
            int? result = _processor.ParseUserInput(input);
            Assert.NotNull(result);
        }

        [Fact]
        public void AskForInput_NonIntegerInputs_InvalidInput()
        {
            int? result;
            List<string> inputs = new List<string> { "1.0", "A", " ", null };
            foreach (var input in inputs)
            {
                result = _processor.ParseUserInput(input);
                Assert.Null(result);
            }
        }

        [Fact]
        public void GetDataString_PopulatedList_ReturnsDataString()
        {
            var photos = new List<Photo> { new Photo { AlbumId = 1, Id = 1, ThumbnailUrl = "thumb", Title = "title", Url = "url" } };
            string dataString = _processor.GetPhotoDataString(photos);

            // Don't necessarily care how we've formatted it, just that it has been formatted (i.e. not null or empty)
            Assert.NotNull(dataString);
            Assert.NotEqual(string.Empty, dataString);
        }

        [Fact]
        public void GetDataString_EmptyList_ReturnsStaticString()
        {
            var photos = new List<Photo>();
            string dataString = _processor.GetPhotoDataString(photos);
            Assert.Equal("No photo data found.", dataString);
        }

        [Fact]
        public void GetDataString_NullList_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() => _processor.GetPhotoDataString(null));
        }
    }
}