using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Models.Account;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Landing;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;
using YandexMusicUWP.Models;

namespace YandexMusicUWP.Services
{
    /// <summary>
    /// Сервис для работы с API Яндекс.Музыки
    /// </summary>
    public class YandexMusicService
    {
        private readonly YandexMusicApi _api;
        private AuthStorage _auth;

        public bool IsAuthorized => _auth != null && !string.IsNullOrEmpty(_auth.Token);

        public YandexMusicService()
        {
            _api = new YandexMusicApi();
            _auth = new AuthStorage();
        }

        /// <summary>
        /// Авторизация по логину и паролю
        /// </summary>
        public async Task<bool> AuthorizeAsync(string login, string password)
        {
            try
            {
                AuthStorage authStorage = new AuthStorage
                {
                    //TODO
                    //User = login,
                    Token = password,
                    IsAuthorized = true,

                };

                // Fix: Explicitly await the task and handle the result properly
                await _api.User.AuthorizeAsync(authStorage, password);
                Yandex.Music.Api.Models.Common.YResponse<Yandex.Music.Api.Models.Account.YAccountResult> userInfo = await _api.User.GetUserAuthAsync(authStorage);

                _auth.User = default;//userInfo.Result;
                _auth.Token = default;//userInfo.Result.AccessToken;
                return true;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Ошибка авторизации: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Авторизация по токену
        /// </summary>
        public async Task<bool> AuthorizeByTokenAsync(string token)
        {
            try
            {
                //_auth.Token = token;

                Yandex.Music.Api.Common.AuthStorage _auth = new Yandex.Music.Api.Common.AuthStorage()
                {
                    IsAuthorized = true,
                    Token = token,
                };

                var userInfo = await _api.User.GetUserAuthAsync(_auth);  
                _auth.User = userInfo.Result.Account;
                return true;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Ошибка авторизации по токену: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получение популярных треков
        /// </summary>
        public async Task<ObservableCollection<Track>> GetPopularTracksAsync()
        {
            try
            {
                var landing = await _api.Landing.GetAsync(_auth);
                var blocks = landing.Result.Blocks;
                var chartBlock = blocks.FirstOrDefault(b => b.Type == YLandingBlockType.Chart); // chart

                if (chartBlock != null && chartBlock.Entities != null)
                {
                    var tracks = new ObservableCollection<Track>();
                    foreach (Yandex.Music.Api.Models.Landing.Entity.YLandingEntity entity in chartBlock.Entities)
                    {
                        if (entity.Data is YTrack yTrack)
                        {
                            tracks.Add(ConvertToTrack(yTrack));
                        }
                    }
                    return tracks;
                }

                return new ObservableCollection<Track>();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Ошибка получения популярных треков: {ex.Message}");
                return new ObservableCollection<Track>();
            }
        }

        /// <summary>
        /// Получение плейлистов пользователя
        /// </summary>
        public async Task<ObservableCollection<Playlist>> GetUserPlaylistsAsync()
        {
            try
            {
                ObservableCollection<YPlaylist> playlists = await _api.Playlist.GetFavoritesAsync(_auth);
                var result = new ObservableCollection<Playlist>();
                
                foreach (YPlaylist yPlaylist in playlists)
                {
                    result.Add(ConvertToPlaylist(yPlaylist));
                }

                return result;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Ошибка получения плейлистов: {ex.Message}");
                return new ObservableCollection<Playlist>();
            }
        }

        /// <summary>
        /// Получение треков плейлиста
        /// </summary>
        public async Task<ObservableCollection<Track>> GetPlaylistTracksAsync(string userId, string playlistId)
        {
            try
            {
                var playlist = await _api.Playlist.GetAsync(_auth, userId, playlistId);
                var tracks = new ObservableCollection<Track>();

                foreach (var yTrack in playlist.Result.Tracks)
                {
                    tracks.Add(ConvertToTrack(yTrack.Track));
                }

                return tracks;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Ошибка получения треков плейлиста: {ex.Message}");
                return new ObservableCollection<Track>();
            }
        }

        /// <summary>
        /// Поиск треков
        /// </summary>
        public async Task<ObservableCollection<Track>> SearchTracksAsync(string query)
        {
            try
            {
                var searchResult = await _api.Search.TrackAsync(_auth, query);
                var tracks = new ObservableCollection<Track>();

                if (searchResult.Result.Tracks != null)
                {
                    foreach (var yTrack in searchResult.Result.Tracks.Results)
                    {
                        tracks.Add(ConvertToTrack(yTrack));
                    }
                }

                return tracks;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Ошибка поиска треков: {ex.Message}");
                return new ObservableCollection<Track>();
            }
        }

        /// <summary>
        /// Получение URL для воспроизведения трека
        /// </summary>
        public async Task<string> GetTrackStreamUrlAsync(string trackId)
        {
            try
            {
                // Fetch metadata for the track download
                var metadataResponse = await _api.Track.GetMetadataForDownloadAsync(_auth, trackId);

                if (metadataResponse.Result != null && metadataResponse.Result.Count > 0)
                {
                    // Use the first available download info
                    var downloadInfo = metadataResponse.Result[0];

                    // Fetch the download file info using the download metadata
                    var downloadFileInfo = await _api.Track.GetDownloadFileInfoAsync(_auth, downloadInfo);

                    // Build the download URL using the file info
                    var downloadUrl = _api.Track.BuildLinkForDownload(downloadInfo, downloadFileInfo);

                    return downloadUrl;
                }

                return null;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Ошибка получения URL трека: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Конвертация трека из API в модель приложения
        /// </summary>
        private Track ConvertToTrack(YTrack yTrack)
        {
            var track = new Track
            {
                Id = yTrack.Id,
                Title = yTrack.Title,
                Artist = string.Join(", ", yTrack.Artists.Select(a => a.Name)),
                Duration = TimeSpan.FromMilliseconds(yTrack.DurationMs)
            };

            if (yTrack.Albums != null && yTrack.Albums.Count > 0)
            {
                track.Album = yTrack.Albums[0].Title;
                track.CoverUri = yTrack.Albums[0].CoverUri;
            }

            return track;
        }

        /// <summary>
        /// Конвертация плейлиста из API в модель приложения
        /// </summary>
        private Playlist ConvertToPlaylist(YPlaylist yPlaylist)
        {
            return new Playlist
            {
                Id = yPlaylist.Kind,
                Title = yPlaylist.Title,
                Description = yPlaylist.Description,
                Owner = yPlaylist.Owner.Login,
                CoverUri = yPlaylist.CoverUri,
                TrackCount = yPlaylist.TrackCount
            };
        }

        /// <summary>
        /// Показ сообщения об ошибке
        /// </summary>
        private async Task ShowErrorAsync(string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Ошибка",
                Content = message,
                CloseButtonText = "OK"
            };

            await errorDialog.ShowAsync();
        }

        /// <summary>
        /// Сохранение токена в локальные настройки
        /// </summary>
        public void SaveToken()
        {
            if (!string.IsNullOrEmpty(_auth.Token))
            {
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["YandexMusicToken"] = _auth.Token;
            }
        }

        /// <summary>
        /// Сохранение указанного токена в локальные настройки
        /// </summary>
        public void SaveToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["YandexMusicToken"] = token;
            }
        }

        /// <summary>
        /// Загрузка токена из локальных настроек
        /// </summary>
        public string LoadToken()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            return localSettings.Values["YandexMusicToken"] as string;
        }
    }
}