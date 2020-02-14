using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LTPhotoAlbum
{
    public interface IConsoleProcessor
    {
        string RequestUserInput();
        void DisplayPhotoData(IEnumerable<Photo> photos);
    }

    public class ConsoleProcessor : IConsoleProcessor
    {
        private IPhotoAlbumClient _client;

        public ConsoleProcessor(IPhotoAlbumClient client)
        {
            _client = client;
        }

        public string RequestUserInput()
        {
            Console.WriteLine("Please enter an integer for the album id:");
            return Console.ReadLine();
        }

        public void DisplayPhotoData(IEnumerable<Photo> photos)
        {
            foreach (var photo in photos)
            {
                Console.WriteLine($"[{photo.Id}] {photo.Title}\n");
            }
        }
    }
}