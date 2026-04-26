using System;
using System.Threading.Tasks;
using System.Net.Http;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    public static class InternetChecker
    {
        /// <summary>
        /// Checks internet availability by attempting HTTP requests to reliable endpoints.
        /// Retries multiple times before returning false to account for transient connectivity.
        /// </summary>
        /// <param name="attempts">Number of attempts (default 3).</param>
        /// <param name="timeoutSeconds">Per-request timeout in seconds (default 3).</param>
        /// <returns>True when any request succeeds.</returns>
        public static async Task<bool> HasInternetAsync(int attempts = 3, int timeoutSeconds = 3)
        {
            // A few reliable endpoints used for captive portal / connectivity checks.
            string[] urls = new[] {
                "https://clients3.google.com/generate_204",
                "https://www.google.com",
                "https://example.com"
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));

                    for (int attempt = 0; attempt < Math.Max(1, attempts); attempt++)
                    {
                        foreach (var url in urls)
                        {
                            try
                            {
                                using (var response = await client.GetAsync(url))
                                {
                                    if (response.IsSuccessStatusCode)
                                    {
                                        return true;
                                    }
                                }
                            }
                            catch { /* ignore and try next url */ }
                        }

                        if (attempt < attempts - 1)
                            await Task.Delay(500);
                    }
                }
            }
            catch { }

            return false;
        }
    }
}
