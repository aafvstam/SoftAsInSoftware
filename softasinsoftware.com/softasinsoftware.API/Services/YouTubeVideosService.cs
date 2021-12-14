﻿using Google.Apis.Services;
using Google.Apis.YouTube.v3;

using softasinsoftware.Shared.Models;

using System.Globalization;
using System.Text.Encodings.Web;

namespace softasinsoftware.API.Services
{
    public class YouTubeVideosService : IYouTubeVideosService
    {
        public IConfiguration Configuration { get; private set; }

        public YouTubeVideosService(IConfiguration configuration)
        {
           this.Configuration = configuration;
        }

        public async Task<YouTubeVideoList> GetYouTubePlayListVideosAsync(int numberOfShows)
        {
            string playlist = this.Configuration["YouTube:PlayListID"];

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = this.Configuration["YouTube:APIKey"],
                ApplicationName = "SoftAsInSoftware"                // _appSettings.YouTubeApplicationName,
            });

            var listRequest = youtubeService.PlaylistItems.List("snippet");
            listRequest.PlaylistId = playlist;
            listRequest.MaxResults = numberOfShows;

            var playlistItems = await listRequest.ExecuteAsync();

            var result = new YouTubeVideoList
            {
                YouTubeVideos = playlistItems.Items.Select(item => new YouTubeVideo
                {
                    Provider = "YouTube",
                    ProviderId = item.Snippet.ResourceId.VideoId,
                    Title = item.Snippet.Title, // GetUsefulBitsFromTitle(item.Snippet.Title),
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url,
                    Url = GetVideoUrl(item.Snippet.ResourceId.VideoId, item.Snippet.PlaylistId, item.Snippet.Position ?? 0)
                }).ToList()
            };

            foreach (var show in result.YouTubeVideos)
            {
                show.ShowDate = await GetVideoPublishDate(youtubeService, show.ProviderId);
                show.LiveBroadcastContent = await GetVideoLiveBroadcastContent(youtubeService, show.ProviderId);
            }

            if (!string.IsNullOrEmpty(playlistItems.NextPageToken))
            {
                result.MoreVideosUrl = GetPlaylistUrl(playlist);
            }

            return result;
        }

        private static async Task<DateTimeOffset> GetVideoPublishDate(YouTubeService client, string videoId)
        {
            var videoRequest = client.Videos.List("snippet");
            videoRequest.Id = videoId;
            videoRequest.MaxResults = 1;

            var video = await videoRequest.ExecuteAsync();
            var rawDate = video.Items[0].Snippet.PublishedAtRaw;

            return DateTimeOffset.Parse(rawDate, null, DateTimeStyles.RoundtripKind);
        }

        private static async Task<string> GetVideoLiveBroadcastContent(YouTubeService client, string videoId)
        {
            var videoRequest = client.Videos.List("snippet");
            videoRequest.Id = videoId;
            videoRequest.MaxResults = 1;

            var video = await videoRequest.ExecuteAsync();
            var liveBroadcastContent = video.Items[0].Snippet.LiveBroadcastContent;

            return liveBroadcastContent;
        }

        private static string GetUsefulBitsFromTitle(string title)
        {
            if (string.IsNullOrEmpty(title)) return string.Empty;

            if (!title.Any(c => c == '-'))
            {
                return title;
            }

            var lastHyphen = title.IndexOf('-');
            if (lastHyphen >= 0)
            {
                string? result = title[(lastHyphen + 1)..].Trim();
                return result;
            }

            return string.Empty;
        }

        private static string GetVideoUrl(string id, string playlistId, long itemIndex)
        {
            var encodedId = UrlEncoder.Default.Encode(id);
            var encodedPlaylistId = UrlEncoder.Default.Encode(playlistId);
            var encodedItemIndex = UrlEncoder.Default.Encode(itemIndex.ToString());

            return $"https://www.youtube.com/watch?v={encodedId}&list={encodedPlaylistId}&index={encodedItemIndex}";
        }

        private static string GetPlaylistUrl(string playlistId)
        {
            var encodedPlaylistId = UrlEncoder.Default.Encode(playlistId);

            return $"https://www.youtube.com/playlist?list={encodedPlaylistId}";
        }


        private static class DesignData
        {
            public static readonly List<YouTubeVideo> Videos = new()
            {
                new YouTubeVideo
                {
                    ShowDate = new DateTime(2015, 7, 21, 9, 30, 0),
                    Title = "Soft as in Software - July 21st 2015",
                    Provider = "YouTube",
                    ProviderId = "7O81CAjmOXk",
                    ThumbnailUrl = "http://img.youtube.com/vi/7O81CAjmOXk/mqdefault.jpg",
                    Url = "https://www.youtube.com/watch?v=7O81CAjmOXk&index=1&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF"
                },
                new YouTubeVideo
                {
                    ShowDate = new DateTime(2015, 7, 14, 15, 30, 0),
                    Title = "Soft as in Software - July 14th 2015",
                    Provider = "YouTube",
                    ProviderId = "bFXseBPGAyQ",
                    ThumbnailUrl = "http://img.youtube.com/vi/bFXseBPGAyQ/mqdefault.jpg",
                    Url = "https://www.youtube.com/watch?v=bFXseBPGAyQ&index=2&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF"
                },

                new YouTubeVideo
                {
                    ShowDate = new DateTime(2015, 7, 7, 15, 30, 0),
                    Title = "Soft as in Software - July 7th 2015",
                    Provider = "YouTube",
                    ProviderId = "APagQ1CIVGA",
                    ThumbnailUrl = "http://img.youtube.com/vi/APagQ1CIVGA/mqdefault.jpg",
                    Url = "https://www.youtube.com/watch?v=APagQ1CIVGA&index=3&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF"
                },
                new YouTubeVideo
                {
                    ShowDate = DateTime.Now.AddDays(-28),
                    Title = "Soft as in Software - July 21st 2015",
                    Provider = "YouTube",
                    ProviderId = "7O81CAjmOXk",
                    ThumbnailUrl = "http://img.youtube.com/vi/7O81CAjmOXk/mqdefault.jpg",
                    Url = "https://www.youtube.com/watch?v=7O81CAjmOXk&index=1&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF"
                },
            };
        }
    }
}
