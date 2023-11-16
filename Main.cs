using Flow.Launcher.Plugin.ObsidianOmnisearch.View;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.ObsidianOmnisearch
{
    /// <summary>
    /// A word match and where it starts on the note
    /// </summary>
    class OmniMatch
    {
        public string Match { get; set; }
        public int Offset { get; set; }
    }

    /// <summary>
    /// A result comming from the Omnisearch Plugin on Obsidian
    /// </summary>
    class OmniResult
    {
        public float Score { get; set; }
        public string Vault { get; set; }
        public string Path { get; set; }
        public string Basename { get; set; }
        public List<string> Foundwords { get; set; }
        public List<OmniMatch> Matches { get; set; }
        public string Excerpt { get; set; }
    }

    // Probably doing this plugin work asyncronously isn't helping very much at all on performance or anything.
    // But since I started like this I'll keep this way for now.
    public class ObsidianOmnisearch : IAsyncPlugin, ISettingProvider
    {
        private PluginInitContext _context;
        private Settings _settings;

        public Control CreateSettingPanel()
        {
            return new SettingsView(_settings);
        }

        public Task InitAsync(PluginInitContext context)
        {
            _context = context;
            _settings = _context.API.LoadSettingJsonStorage<Settings>();

            if (_settings.IsFirstTimeUser)
            {
                _context.API.ShowMsg("First time using Omnisearch?", "To use it, follow instructions in the plugin home page on Github.\nhttps://github.com/UmBarril/Flow.Launcher.Plugin.ObsidianOmnisearch");
            }
            return Task.CompletedTask;
        }
        
        public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            List<Result> results = new();
            if (query.IsReQuery || query.Search.Length == 0)
            {
                return results;
            }
            try
            {
                var content = await ObsidianQueryAsync(query.Search, token);
                if (content.Count > 0)
                {
                    _settings.IsFirstTimeUser = false;
                    foreach (OmniResult result in content)
                    {
                        string description = HttpUtility.HtmlDecode(result.Excerpt.Replace("<br>", "\n"));
                     
                        Lazy<UserControl> previewPanel = null;

                        if (_settings.IsCustomPreviewActive)
                        {
                            //dispaching to the main thread since it won't work otherwise
                            previewPanel = Application.Current.Dispatcher.Invoke(delegate
                            {
                                return new Lazy<UserControl>(new PreviewPanel(description, _settings.PreviewFontSize));
                            });
                        }
                        results.Add(new()
                        {
                            Title = $"{result.Basename}",
                            SubTitle = $"{result.Vault} / {result.Matches.Count} matches",
                            IcoPath = @"Images/favicon.ico",
                            TitleHighlightData = _context.API.FuzzySearch(query.Search, result.Basename).MatchData,
                            PreviewPanel = previewPanel, // if null, it will show the default preview panel
                            Action = _ =>
                            {
                                _context.API.OpenAppUri($"obsidian://open?vault={result.Vault}&file={result.Path}");
                                return true;
                            },
                            Score = (int)result.Score
                        });
                    }
                }
                else
                {
                    results.Add(new()
                    {
                        Title = "Nothing Found",
                        IcoPath = @"Images\obsidian-icon.png",
                    });
                }
            }
            catch(TaskCanceledException) { /* ignore */ }
            catch (HttpRequestException ex)
            {
                if (!_settings.IsFirstTimeUser)
                {
                    results.Add(new()
                    {
                        Title = "Error: Obsidian is not running or the Omnisearch server is not enabled.",
                        SubTitle = "Click To See How To Fix This",
                        IcoPath = @"Images\obsidian-icon.png",
                        Action = actionContext =>
                        {
                            if (actionContext.SpecialKeyState.CtrlPressed)
                            {
                                _context.API.CopyToClipboard(ex.Message);
                                return true;
                            }
                            _context.API.OpenUrl("https://github.com/UmBarril/Flow.Launcher.Plugin.ObsidianOmnisearch/blob/master/README.md");
                            return true;
                        }
                    });
                }
                else
                {
                    results.Add(new()
                    {
                        Title = "Error: Obsidian is not running or the Omnisearch server is not enabled.",
                        SubTitle = "Click to Open Obsidian",
                        IcoPath = @"Images\obsidian-icon.png",
                        Action = actionContext =>
                        {
                            if (actionContext.SpecialKeyState.CtrlPressed)
                            {
                                _context.API.CopyToClipboard(ex.Message);
                                return true;
                            }
                            _context.API.OpenAppUri("obsidian://open");
                            return true;
                        }
                    });
                }
            }
            
            return results;
        }

        private async Task<List<OmniResult>> ObsidianQueryAsync(string searchQuery, CancellationToken token)
        {
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(5); // change default timeout
            client.BaseAddress = new Uri($"http://localhost:{_settings.Port}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = await client.GetAsync($"search?q={searchQuery}", token);
            var data = await json.Content.ReadFromJsonAsync<List<OmniResult>>(cancellationToken: token);
            return data;
        }
    }
}