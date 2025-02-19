using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using Narod.SteamGameFinder;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json;
using System.Xml.Linq;
using Windows.Media.Protection.PlayReady;
using Microsoft.Windows.Themes;
using Windows.Gaming.Input;
using System.Diagnostics;

namespace lstwoMODSInstaller.ModManagement
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class DataManager
    {
        public static readonly string dataFolder = @$"{Path.GetTempPath()}\lstwoMODSInstaller_DATA";

        public const string REGISTRY_KEY = @"Software\lstwoMODSInstaller";

        public static Dictionary<string, Game> games = new();
        public static Dictionary<string, string> processBatchFiles = new();

        public static Core coreData;

        public static string savedRepoCommitSHA;
        public static string latestRepoCommitSHA;

        public static async Task UpdateData(bool forceRedownload = false)
        {
            if(!await CheckForNewCommit() && Directory.Exists(dataFolder) && !forceRedownload)
            {
                await LoadDataFolder();
                return;
            }

            await GithubManager.TryDownloadFolderAsync("lstwoMODS", "lstwoMODSInstaller", "main", "data", dataFolder);

            UpdateSavedRepoCommitSHA();

            await LoadDataFolder();
        }

        public static async Task DownloadOverridesFolderAsync(string targetDirectory)
        {
            try
            {
                string owner = "lstwoMODS";
                string repo = "lstwoMODSInstaller";
                string branch = "main";
                string path = "overrides";

                Directory.CreateDirectory(targetDirectory);

                await GithubManager.TryDownloadFolderAsync(owner, repo, branch, path, targetDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        private static async Task LoadDataFolder()
        {
            games = new();
            processBatchFiles = new();

            var coreJson = await File.ReadAllTextAsync(@$"{dataFolder}\core.json");
            coreData = JsonConvert.DeserializeObject<Core>(coreJson);

            var subDirectories = Directory.GetDirectories(dataFolder);

            foreach(var dir in subDirectories)
            {
                if(dir.EndsWith(@"\batch"))
                {
                    LoadBatchFolder(dir);
                }
                else if(dir.EndsWith(@"\games"))
                {
                    LoadGamesFolder(dir);
                }
            }
        }

        private static void LoadBatchFolder(string path)
        {
            var files = Directory.GetFiles(path);

            foreach(var file in files)
            {
                if(Path.GetExtension(file) != ".bat")
                {
                    continue;
                }

                processBatchFiles.Add(Path.GetFileNameWithoutExtension(file), file);
            }
        }

        private static void LoadGamesFolder(string path)
        {
            var subDirectories = Directory.GetDirectories(Path.GetFullPath(path));

            foreach (var dir in subDirectories)
            {
                var id = dir.Split('\\').Last();
                var json = File.ReadAllText($@"{dir}\data.json");
                var game = JsonConvert.DeserializeObject<Game>(json);

                game.LoadMods(dir);

                games.Add(id, game);
            }
        }

        private static async Task<bool> CheckForNewCommit()
        {
            using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY);

            savedRepoCommitSHA = key?.GetValue("savedRepoCommitSHA") as string;
            latestRepoCommitSHA = (await GithubManager.GetNewestCommitData("lstwoMODS", "lstwoMODSInstaller")).GetProperty("sha").GetString();

            return savedRepoCommitSHA != latestRepoCommitSHA;
        }

        private static async void UpdateSavedRepoCommitSHA()
        {
            using var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY);

            latestRepoCommitSHA ??= (await GithubManager.GetNewestCommitData("lstwoMODS", "lstwoMODSInstaller")).GetProperty("sha").GetString();
            key.SetValue("savedRepoCommitSHA", latestRepoCommitSHA);

            savedRepoCommitSHA = latestRepoCommitSHA;
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class GithubManager
    {
        private static readonly HttpClient HttpClient = new HttpClient
        {
            DefaultRequestHeaders = { { "User-Agent", "lstwoMODSInstaller" } }
        };

        static GithubManager()
        {
            var pat = PATManager.GetPAT();

            if (!string.IsNullOrEmpty(pat))
            {
                HttpClient.DefaultRequestHeaders.Authorization = new("token", pat);
            }
        }

        public static async Task RefreshPATAsync()
        {
            PATManager.DeletePAT();
            await PATManager.EnsurePATExistsAsync();
            var pat = PATManager.GetPAT();
            HttpClient.DefaultRequestHeaders.Authorization = new("token", pat);
        }

        public static async Task<JsonElement> GetNewestCommitData(string owner, string repo)
        {
            try
            {
                var response = await HttpClient.GetStringAsync($"https://api.github.com/repos/{owner}/{repo}/commits");

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                return root[0].Clone();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while getting newest commit: " + ex.Message, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                return new();
            }
        }

        public static async Task<bool> TryDownloadFolderAsync(string owner, string repo, string branch, string path, string targetDirectory)
        {
            try
            {
                var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/contents/{path}?ref={branch}";

                var response = await HttpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                var items = JsonConvert.DeserializeObject<JArray>(await response.Content.ReadAsStringAsync());

                foreach (var item in items)
                {
                    var itemType = item["type"]?.ToString();
                    var itemPath = item["path"]?.ToString();
                    var itemName = item["name"]?.ToString();

                    if (itemType == "file")
                    {
                        var downloadUrl = item["download_url"]?.ToString();

                        if (string.IsNullOrEmpty(downloadUrl))
                        {
                            continue;
                        }

                        var targetFilePath = Path.Combine(targetDirectory, itemName);

                        using var fileResponse = await HttpClient.GetAsync(downloadUrl);
                        fileResponse.EnsureSuccessStatusCode();
                        await using var fs = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write);
                        await fileResponse.Content.CopyToAsync(fs);
                    }
                    else if (itemType == "dir")
                    {
                        string subfolderTargetDirectory = Path.Combine(targetDirectory, itemName);
                        Directory.CreateDirectory(subfolderTargetDirectory);

                        var subFolderResult = await TryDownloadFolderAsync(owner, repo, branch, itemPath, subfolderTargetDirectory);

                        if (!subFolderResult)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while getting newest commit: " + ex.Message, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static async Task<GitHubRelease> GetLatestReleaseInfo(string owner, string repo, bool includePreReleases = false)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/releases";

            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                JArray releases = JArray.Parse(json);
                JObject latestRelease = includePreReleases ? (JObject)releases[0] : (JObject)releases.First(r => (bool)r["prerelease"] == false);

                return ParseRelease(latestRelease);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<GitHubRelease> GetReleaseByTag(string owner, string repo, string tag)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/releases/tags/{tag}";

            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                JObject release = JObject.Parse(json);
                return ParseRelease(release);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string[]> DownloadModAsync(Mod mod, Game game, Action<int> progressCallback)
        {
            await mod.UpdateLatestRelease();

            var release = mod.latestRelease;
            var downloadedFiles = new List<string>();

            foreach(var asset in release.Assets)
            {
                var shouldDownload = true;

                foreach (var filter in mod.file_filter)
                {
                    if(filter.StartsWith("?") && asset.Name.Contains(filter.Replace("?", "")))
                    {
                        shouldDownload = false;
                        break;
                    }
                    else if(!filter.StartsWith("?") && !asset.Name.Contains(filter))
                    {
                        shouldDownload = false;
                        break;
                    }
                }

                if(!shouldDownload)
                {
                    continue;
                }

                var filePath = await DownloadFileAsync(asset.BrowserDownloadUrl, progressCallback);
                ProcessFile(mod, game, filePath);

                downloadedFiles.Add(filePath);
            }

            return downloadedFiles.ToArray();
        }

        private static void ProcessFile(Mod mod, Game game, string filePath)
        {
            var gamePath = game.GetGamePath();

            if (DataManager.processBatchFiles.TryGetValue(mod.process_file, out var batchFilePath) && !string.IsNullOrEmpty(batchFilePath) && !string.IsNullOrEmpty(filePath) && 
                !string.IsNullOrEmpty(gamePath))
            {
                var startInfo = new ProcessStartInfo(batchFilePath, $"\"{filePath}\" \"{gamePath}\"")
                {
                    CreateNoWindow = false,
                    UseShellExecute = false
                };

                using var process = new Process();

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
        }

        private static async Task<string> DownloadFileAsync(string fileUrl, Action<int> progressCallback)
        {
            using var client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var path = Path.Combine(Path.GetTempPath(), "lstwoMODSInstaller", fileUrl.Split('/').Last());
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var totalReadBytes = 0L;

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using var contentStream = await response.Content.ReadAsStreamAsync();

            var buffer = new byte[81920];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fs.WriteAsync(buffer, 0, bytesRead);
                totalReadBytes += bytesRead;

                if (totalBytes > 0)
                {
                    int progress = (int)((double)totalReadBytes / totalBytes * 100);
                    progressCallback?.Invoke(progress);
                }
            }

            return path;
        }

        private static GitHubRelease ParseRelease(JObject release)
        {
            return release.ToObject<GitHubRelease>();
            var r = new GitHubRelease
            {
                Id = release["id"].ToString(),
                Name = release["name"].ToString(),
                TagName = release["tag_name"].ToString(),
                HtmlUrl = release["html_url"].ToString(),
                Body = release["body"].ToString(),
                CreatedAt = release["created_at"].ToString(),
                PublishedAt = release["published_at"].ToString(),
                Draft = (bool)release["draft"],
                PreRelease = (bool)release["prerelease"],
                TargetCommitish = release["target_commitish"].ToString(),
                Author = new GitHubUser
                {
                    Login = release["author"]["login"].ToString(),
                    Id = release["author"]["id"].ToString(),
                    HtmlUrl = release["author"]["html_url"].ToString()
                },
                Assets = release["assets"].ToObject<List<GitHubAsset>>()
            };
        }

        public class GitHubRelease
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("tag_name")]
            public string TagName { get; set; }

            [JsonProperty("html_url")]
            public string HtmlUrl { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }

            [JsonProperty("published_at")]
            public string PublishedAt { get; set; }

            [JsonProperty("draft")]
            public bool Draft { get; set; }

            [JsonProperty("prerelease")]
            public bool PreRelease { get; set; }

            [JsonProperty("target_commitish")]
            public string TargetCommitish { get; set; }

            [JsonProperty("author")]
            public GitHubUser Author { get; set; }

            [JsonProperty("assets")]
            public List<GitHubAsset> Assets { get; set; }
        }

        public class GitHubUser
        {
            [JsonProperty("login")]
            public string Login { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("html_url")]
            public string HtmlUrl { get; set; }
        }

        public class GitHubAsset
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("browser_download_url")]
            public string BrowserDownloadUrl { get; set; }

            [JsonProperty("size")]
            public long Size { get; set; }

            [JsonProperty("download_count")]
            public int DownloadCount { get; set; }

            [JsonProperty("content_type")]
            public string ContentType { get; set; }
        }
    }

    public class Core
    {
        public Mod lstwomods_core;
    }

    public class Game
    {
        public string game_name;
        public Mod mod_pack;
        public LocatingMethod locating_method;
        public Mod bepinex;

        public Dictionary<string, Mod> mods;
        public string id;

        private string gamePath;

        public void LoadMods(string dir)
        {
            var modsDir = $@"{dir}\mods\";
            mods = new();

            if(!Directory.Exists(modsDir))
            {
                return;
            }

            var modFiles = Directory.GetFiles(modsDir);

            foreach (var file in modFiles)
            {
                var mod = Mod.LoadFromPath(file, false, this);

                if (mod != null)
                {
                    mods.Add(mod.id, mod);
                }
            }

            var hiddenModsDir = $@"{modsDir}\hidden\";

            if (!Directory.Exists(hiddenModsDir))
            {
                return;
            }

            var hiddenModFiles = Directory.GetFiles(hiddenModsDir);

            foreach (var file in hiddenModFiles)
            {
                var mod = Mod.LoadFromPath(file, true, this);

                if (mod != null)
                {
                    mods.Add(mod.id, mod);
                }
            }
        }

        public string GetGamePath()
        {
            if (gamePath == null)
            {
                switch (locating_method.method_type)
                {
                    case "steam":
                        gamePath = GetGamePath_Steam();
                        break;
                    case "manual":
                        gamePath = GetGamePath_Manual();
                        break;
                }

                gamePath ??= GetGamePath_Manual();
            }

            return gamePath;
        }

        public override string ToString()
        {
            return game_name;
        }

        private string GetGamePath_Steam()
        {
            var steamGameLocator = new SteamGameLocator();

            if (!steamGameLocator.getIsSteamInstalled())
            {
                throw new Exception("Steam is not installed");
            }

            return steamGameLocator.getGameInfoByFolder(locating_method.data[0]).steamGameLocation;
        }

        private string GetGamePath_Manual()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Executable Files (*.exe)|*.exe",
                Title = "Select an Executable File"
            };

            while (true)
            {
                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    string selectedFileName = Path.GetFileNameWithoutExtension(selectedFilePath);
                    string selectedFolderPath = Path.GetDirectoryName(selectedFilePath);

                    if (string.Equals(selectedFileName, locating_method.data[0], StringComparison.OrdinalIgnoreCase))
                    {
                        return selectedFolderPath;
                    }
                    else
                    {
                        MessageBoxResult retry = MessageBox.Show(
                            $"The selected file '{selectedFileName}' does not match '{locating_method.data[0]}'.\n",
                            "File Mismatch",
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Warning);

                        if (retry == MessageBoxResult.Cancel)
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class Mod
    {
        public string mod_name;
        public string repo_owner;
        public string repo_name;
        public string[] file_filter;
        public string process_file;
        public string[] dependencies;
        public string specific_tag;

        public bool isHidden;
        public string id;
        public GithubManager.GitHubRelease latestRelease;
        public Game parentGame;

        public static Mod LoadFromPath(string path, bool hidden, Game parent)
        {
            var extension = Path.GetExtension(path);

            if (extension != ".json")
            {
                return null;
            }

            var json = File.ReadAllText(path);
            var mod = JsonConvert.DeserializeObject<Mod>(json);

            mod.isHidden = hidden;
            mod.id = Path.GetFileNameWithoutExtension(path);
            mod.parentGame = parent;

            return mod;
        }

        public async Task<bool> DownloadMod(Action<int> progressCallback, bool showFinishMessageBox = true)
        {
            return await DownloadMod(progressCallback, parentGame, showFinishMessageBox);
        }

        public async Task<bool> DownloadMod(Action<int> progressCallback, Game game, bool showFinishMessageBox = true)
        {
            try
            {
                foreach (var dependency in dependencies)
                {
                    Mod mod = null;

                    if (game.mods.ContainsKey(dependency))
                    {
                        mod = game.mods[dependency];
                    }
                    else if (DataManager.games["all"].mods.ContainsKey(dependency))
                    {
                        mod = DataManager.games["all"].mods[dependency];
                    }

                    if(!await mod?.DownloadMod(progressCallback, game, false))
                    {
                        return false;
                    }
                }

                await UpdateLatestRelease();
                await GithubManager.DownloadModAsync(this, game, progressCallback);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading mod \"{mod_name}\": {ex.Message}", "Error downloading mod", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (showFinishMessageBox)
            {
                MessageBox.Show($"Finished downloading mod \"{mod_name}\"!", "Finished downloading mod", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return true;
        }

        public async Task UpdateLatestRelease()
        {
            if (latestRelease == null)
            {
                if (string.IsNullOrEmpty(specific_tag))
                {
                    latestRelease = await GithubManager.GetLatestReleaseInfo(repo_owner, repo_name, true);
                }
                else
                {
                    latestRelease = await GithubManager.GetReleaseByTag(repo_owner, repo_name, specific_tag);
                }
            }
        }

        public override string ToString()
        {
            return mod_name;
        }
    }

    public class LocatingMethod
    {
        public string method_type;
        public string[] data;
    }
}
