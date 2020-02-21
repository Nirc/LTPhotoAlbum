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
            _logger.LogInformation("Application started");
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
            // Won't see log from this in the console since it closes so quickly but in the real world it could go to a log file
            _logger.LogInformation("Application stopped");
            return Task.CompletedTask;
        }

        private void OnStopping()
        {
            _logger.LogInformation("Application stopping");

            _cts.Cancel();
            // Console.ReadLine blocks the thread
            var handle = GetStdHandle(-10);
            CancelIoEx(handle, IntPtr.Zero);
        }

        public async Task Process(CancellationToken cancellationToken)
        {
            int? albumId = null;
            while (albumId == null)
            {
                Console.WriteLine("Please enter an integer for an album id:");
                var input = _processor.ReadConsoleInput();
                albumId = _processor.ParseUserInput(input);
            }

            Console.WriteLine("Fetching album data...\n");
            IList<Photo> photos = null;
            try
            {
                photos = await _client.GetPhotos(albumId.Value, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There was an error getting photo data.");
            }

            if (photos != null)
            {
                var dataString = _processor.GetPhotoDataString(photos);
                Console.WriteLine(dataString);
            }
        }
    }
}