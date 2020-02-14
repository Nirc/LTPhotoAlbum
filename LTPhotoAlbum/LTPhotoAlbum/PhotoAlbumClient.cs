using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LTPhotoAlbum
{
    public interface IPhotoAlbumClient
    {
        Task<IEnumerable<Photo>> GetPhotos(int albumId, CancellationToken cancellationToken);
    }

    public class PhotoAlbumClient : IPhotoAlbumClient
    {
        private HttpClient _client;

        public PhotoAlbumClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<Photo>> GetPhotos(int albumId, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"?albumId={albumId}"))
            {
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return await JsonSerializer.DeserializeAsync<IEnumerable<Photo>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
                        }

                        throw new ApiException(response.StatusCode, "Photo Album API request failed");
                    }
                }
            }
        }
    }
}