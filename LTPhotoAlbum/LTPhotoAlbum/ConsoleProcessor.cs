using System;
using System.Collections.Generic;

namespace LTPhotoAlbum
{
    public interface IConsoleProcessor
    {
        string ReadConsoleInput();
        int? ParseUserInput(string input);
        string GetPhotoDataString(IList<Photo> photos);
    }

    public class ConsoleProcessor : IConsoleProcessor
    {
        // Abstracts the console input so the unit tests do not depend on it
        public string ReadConsoleInput()
        {
            return Console.ReadLine();
        }

        public int? ParseUserInput(string input)
        {
            int result;
            bool success = Int32.TryParse(input, out result);

            if (!success) { return null; }
            else { return result; }
        }

        public string GetPhotoDataString(IList<Photo> photos)
        {
            if (photos == null) { throw new ArgumentNullException("photos"); }
            if (photos.Count < 1) { return "No photo data found."; }

            string dataString = "";
            foreach (var photo in photos)
            {
                dataString += $"[{photo.Id}] {photo.Title}\n\n";
            }

            return dataString;
        }
    }
}