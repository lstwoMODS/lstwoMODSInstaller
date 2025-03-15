using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Newtonsoft.Json;

namespace lstwoMODSInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Windows;

    public static class GitHubDeviceFlowHelper
    {
        private const string ClientId = "Ov23li2Dx7C5y0yx0YUG";
        private const string DeviceCodeUrl = "https://github.com/login/device/code";
        private const string AccessTokenUrl = "https://github.com/login/oauth/access_token";

        public static async Task<string> GetPATAsync()
        {
            HttpClient client = new HttpClient();

            try
            {
                var deviceCodeResponse = await client.PostAsync(DeviceCodeUrl, new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("scope", "repo")
                }));

                deviceCodeResponse.EnsureSuccessStatusCode();
                var deviceCodeContent = await deviceCodeResponse.Content.ReadAsStringAsync();

                var deviceData = deviceCodeContent.Split('&')
                    .ToDictionary(c => c.Split('=')[0], c => Uri.UnescapeDataString(c.Split('=')[1]));

                var verificationUri = deviceData["verification_uri"];
                var userCode = deviceData["user_code"];
                var deviceCode = deviceData["device_code"];
                var interval = deviceData["interval"];

                var messageBox = new TaskCompletionSource<bool>();

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(verificationUri) { UseShellExecute = true });

                while (true)
                {
                    MessageBox.Show($"The verification page will open automatically. Please log in and enter the code: {userCode}\n\n{verificationUri}",
                        "Login Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK);

                    await Task.Delay(int.Parse(interval) * 1000);

                    var tokenResponse = await client.PostAsync(AccessTokenUrl, new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("client_id", ClientId),
                        new KeyValuePair<string, string>("device_code", deviceCode),
                        new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:device_code")
                    }));

                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

                    if (tokenResponse.IsSuccessStatusCode)
                    {
                        Dictionary<string, string> tokenData = tokenContent.Split('&').ToDictionary(c => c.Split('=')[0], c => Uri.UnescapeDataString(c.Split('=')[1]));

                        if (tokenData.TryGetValue("access_token", out var accessToken))
                        {
                            messageBox.SetResult(true);
                            MessageBox.Show("PAT initialized successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            return accessToken;
                        }
                    }
                    else
                    {
                        var errorData = tokenContent.Split('&').ToDictionary(c => c.Split('=')[0], c => Uri.UnescapeDataString(c.Split('=')[1]));
                        if (errorData.TryGetValue("error", out var value) && value == "authorization_pending")
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            finally
            {
                client.Dispose();
            }
        }
    }

}
