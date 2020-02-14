using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LTPhotoAlbum
{
    public class ConsoleApp : IHostedService
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CancelIoEx(IntPtr handle, IntPtr lpOverlapped);

        private CancellationTokenSource _cts;
        private IHostApplicationLifetime _lifetime;
        private ILogger<ConsoleApp> _logger;
        private IConsoleProcessor _processor;
        private IPhotoAlbumClient _client;

        public ConsoleApp(IHostApplicationLifetime lifetime, ILogger<ConsoleApp> logger, IConsoleProcessor processor, IPhotoAlbumClient client)
        {
            _lifetime = lifetime;
            _logger = logger;
            _processor = processor;
            _client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _lifetime.ApplicationStopping.Register(OnStopping);
            Console.WriteLine("Welcome to the photo album service.\nTo view an album's information, enter in the id of an album.");

            while (!cancellationToken.IsCancellationRequested)
            {
                await Process(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnStopping()
        {
            _cts.Cancel();
            // Console.ReadLine blocks the thread
            var handle = GetStdHandle(-10);
            CancelIoEx(handle, IntPtr.Zero);
        }

        public async Task Process(CancellationToken cancellationToken)
        {
            int albumId;
            string input = _processor.RequestUserInput();
            if (input == null) { input = ""; }
            while (Int32.TryParse(input, out albumId) == false)
            {
                input = _processor.RequestUserInput();
            }

            Console.WriteLine("Fetching album data...");
            IEnumerable<Photo> photos = null;
            try
            {
                photos = await _client.GetPhotos(albumId, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"There was an error getting photo data.\n{e.Message}");
            }

            if (photos != null)
            {
                _processor.DisplayPhotoData(photos);
            }

            Console.WriteLine("Enter another album id:");
        }
    }
}