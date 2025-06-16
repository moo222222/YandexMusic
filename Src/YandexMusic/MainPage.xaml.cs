using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Yandex.Music.Api.Models.Track;
using YandexMusicUWP.Models;
using YandexMusicUWP.Services;

namespace YandexMusicUWP
{
    /// <summary>
    /// Главная страница приложения
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Сервис для работы с API Яндекс.Музыки
        private YandexMusicService _musicService;
        private Track _currentTrack;
        private ObservableCollection<Track> _tracks;
        public string savedToken;

        public MainPage()
        {
            this.InitializeComponent();
            InitializeYandexMusicService();
        }

        /// <summary>
        /// Инициализация сервиса Яндекс.Музыки
        /// </summary>
        private void InitializeYandexMusicService()
        {
            _musicService = new YandexMusicService();
            _tracks = new ObservableCollection<Track>();
        }

        /// <summary>
        /// Вызывается при загрузке страницы
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Проверяем, есть ли сохраненный токен
            /*string*/savedToken = _musicService.LoadToken();
            if (!string.IsNullOrEmpty(savedToken))
            {
                // Пытаемся авторизоваться по токену
                bool success = await _musicService.AuthorizeByTokenAsync(savedToken);
                if (success == true)
                {
                    // Загружаем популярные треки
                    await LoadPopularTracksAsync();
                }
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Войти"
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        /// <summary>
        /// Обработчик нажатия на кнопку воспроизведения
        /// </summary>
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTrack != null)
            {
                // Получаем URL для воспроизведения трека
                string streamUrl = await _musicService.GetTrackStreamUrlAsync(_currentTrack.Id);
                if (!string.IsNullOrEmpty(streamUrl))
                {
                    // код для воспроизведения трека
                    MediaPlayer player = BackgroundMediaPlayer.Current;
                    player.SetUriSource(new Uri(streamUrl));
                    player.Play();

                }
            }
        }

        /// <summary>
        /// Обработчик нажатия на трек в списке
        /// </summary>
        private void TrackItem_Click(object sender, ItemClickEventArgs e)
        {
            _currentTrack = e.ClickedItem as Track;
            if (_currentTrack != null)
            {
                // Обновляем информацию о текущем треке в плеере
                UpdateCurrentTrackInfo();
            }
        }

        /// <summary>
        /// Обновление информации о текущем треке в плеере
        /// </summary>
        private void UpdateCurrentTrackInfo()
        {
            // Здесь будет код для обновления информации о треке в плеере
        }

        /// <summary>
        /// Метод для загрузки популярных треков
        /// </summary>
        private async System.Threading.Tasks.Task LoadPopularTracksAsync()
        {
            var tracks = await _musicService.GetPopularTracksAsync();
            _tracks = tracks;
            // Обновляем список треков в UI
            TracksListView.ItemsSource = _tracks;
        }

        /// <summary>
        /// Обработчик поиска треков
        /// </summary>
        /* private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
         {
             string query = args.QueryText;
             if (!string.IsNullOrEmpty(query))
             {
                 // Fetch the tracks from the service
                 var tracks = await _musicService.SearchTracksAsync(query);

                 // Convert List<YTrack> to ObservableCollection<Track>
                 _tracks = new ObservableCollection<Track>(
                     tracks.Select(track => new Track
                     {
                         Id = track.Id,
                         Title = track.Title,
                         Artist = track.Artist, // //Artists = track.Artists?.Select(artist => new YArtist { Name = artist.Name }).ToList(),
                         Album = track.Album,
                         DurationMs = track.DurationMs,
                         CoverUri = track.CoverUri
                     })
                 );

                 // Update the UI with the converted tracks
                 TracksListView.ItemsSource = _tracks.Select(t => new
                 {
                     t.Title,
                     t.Artist,//Artist = string.Join(", ", t.Artist?.Select(a => a.Name) ?? Enumerable.Empty<string>()),
                     t.Id
                 });
             }
         }*/

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string query = args.QueryText;
            if (!string.IsNullOrEmpty(query))
            {
                var tracks = await _musicService.SearchTracksAsync(query);
                _tracks = tracks;
                // Обновляем список треков в UI
                TracksListView.ItemsSource = _tracks;
            }
        }



    }
}