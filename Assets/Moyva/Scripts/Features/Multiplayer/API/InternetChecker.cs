using System;
using System.Threading.Tasks;
using System.Net.Http;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    public static class InternetChecker
    {
        private const string Prefix = "[InternetChecker]";
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
                Debug.Log($"{Prefix} HasInternetAsync start (attempts={attempts}, timeoutSeconds={timeoutSeconds})");
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));

                    for (int attempt = 0; attempt < Math.Max(1, attempts); attempt++)
                    {
                        Debug.Log($"{Prefix} Attempt {attempt + 1}/{Math.Max(1, attempts)}");
                        foreach (var url in urls)
                        {
                            try
                            {
                                Debug.Log($"{Prefix} Trying {url}");
                                using (var response = await client.GetAsync(url))
                                {
                                    if (response.IsSuccessStatusCode)
                                    {
                                        Debug.Log($"{Prefix} Success {url} status={(int)response.StatusCode}");
                                        return true;
                                    }
                                    else
                                    {
                                        Debug.Log($"{Prefix} Non-success status from {url}: {(int)response.StatusCode}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.Log($"{Prefix} Request to {url} failed: {ex.Message}");
                            }
                        }

                        if (attempt < attempts - 1)
                        {
                            Debug.Log($"{Prefix} Attempt {attempt + 1} failed, retrying after delay...");
                            await Task.Delay(500);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Prefix} Probe failed: {ex.Message}");
            }

            Debug.Log($"{Prefix} HasInternetAsync finished: no successful responses");
            return false;
        }
    }
}
