using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Shared.Connectivity
{
    public static class InternetChecker
    {
        private static readonly string[] ProbeUrls =
        {
            "https://clients3.google.com/generate_204",
            "https://www.google.com",
            "https://example.com"
        };

        public static async Task<bool> HasInternetAsync(int attempts = 3, int timeoutSeconds = 3)
        {
            int attemptCount = Math.Max(1, attempts);
            using (var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds))
            })
            {
                for (int attempt = 0; attempt < attemptCount; attempt++)
                {
                    foreach (string url in ProbeUrls)
                    {
                        try
                        {
                            using (HttpResponseMessage response = await client.GetAsync(url))
                                if (response.IsSuccessStatusCode)
                                    return true;
                        }
                        catch (HttpRequestException)
                        {
                        }
                        catch (TaskCanceledException)
                        {
                        }
                    }

                    if (attempt + 1 < attemptCount)
                        await Task.Delay(500);
                }
            }

            return false;
        }
    }
}
