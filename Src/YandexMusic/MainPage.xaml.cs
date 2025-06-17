using System;
using System.Collections.ObjectModel;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Models.Account;
using YandexMusicUWP.BackgroundTasks;
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
        
        //private string _authToken1;
        //private static Yandex.Music.Api.Common.AuthStorage _authstorage1;

        private YandexMusicService _musicService;

        private ObservableCollection<Track> _tracks = new ObservableCollection<Track>();
        private Track _selectedTrack;

        // Медиаплеер для воспроизведения музыки
        private MediaPlayer _mediaPlayer;
        private TimeSpan _totalDuration;
        private bool _isTimelineSliderChanging = false;
        private bool _isPlaying = false;

        public MainPage()
        {
            this.InitializeComponent();


            // Подгружаем сохраненный токен
            /*_authToken1 = _musicService.LoadToken();

            // Формируем auth storage
            if (!string.IsNullOrEmpty(_authToken1))
            {
                _authstorage1 = new Yandex.Music.Api.Common.AuthStorage()
                {
                    IsAuthorized = true,
                    Token = _authToken1,
                };
            }*/


            InitializeYandexMusicService();
            InitializeMediaPlayer();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Подгружаем сохраненный токен
            string _authToken1 = _musicService.LoadToken();

            // Проверяем, есть ли сохраненный токен

            if (!string.IsNullOrEmpty(_authToken1))
            {

                // Обновляем  auth storage на всяк. случай, если токен изменился    
                //_authstorage1 = new Yandex.Music.Api.Common.AuthStorage()
                //{
                //    IsAuthorized = true,
                //    Token = _authToken1,
                //};
                // Пытаемся авторизоваться по токену
                bool success = await _musicService.AuthorizeByTokenAsync(_authToken1);
                if (success)
                {
                    // Если авторизация успешна, изменяем надпись на кнопке "Войти"
                    LoginButton.Content = "...";

                    // Загружаем популярные треки
                    await LoadPopularTracksAsync();
                }
            }
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
        /// Инициализация медиаплеера
        /// </summary>
        private void InitializeMediaPlayer()
        {
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
            _mediaPlayer.CommandManager.IsEnabled = true;

            // Подписываемся на события медиаплеера
            _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            _mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            _mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;

            // Устанавливаем громкость
            _mediaPlayer.Volume = VolumeSlider.Value / 100.0;

            // Инициализируем фоновое воспроизведение
            BackgroundMediaPlayerHelper.Initialize(_mediaPlayer);
        }

        /// <summary>
        /// Вызывается при загрузке страницы
        /// </summary>
       
        /// <summary>
        /// Обработчик нажатия на кнопку "Войти"
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }



        /// <summary>
        /// Обработчик поиска треков
        /// </summary>
        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string query = args.QueryText;
            if (!string.IsNullOrEmpty(query))
            {
                var tracks = await _musicService.SearchTracksAsync(query);
                _tracks = tracks;
                // Обновляем список треков в UI
                TracksListView.ItemsSource = _tracks;

                // Закрываем панель меню в узком режиме после поиска
                if (SplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                {
                    SplitView.IsPaneOpen = false;
                }
            }
        }





        /// <summary>
        /// Обработчик нажатия на кнопку воспроизведения/паузы
        /// </summary>
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTrack != null)
            {
                if (_isPlaying)
                {
                    // Если трек уже воспроизводится, ставим на паузу
                    _mediaPlayer.Pause();
                    _isPlaying = false;
                    PlayPauseIcon.Glyph = "\uE768"; // Иконка воспроизведения
                }
                else
                {
                    // Если трек на паузе, продолжаем воспроизведение
                    if (_mediaPlayer.Source != null)
                    {
                        _mediaPlayer.Play();
                        _isPlaying = true;
                        PlayPauseIcon.Glyph = "\uE769"; // Иконка паузы
                    }
                    else
                    {
                        // Если источник не установлен, получаем URL и начинаем воспроизведение
                        await PlayCurrentTrackAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Воспроизведение текущего трека
        /// </summary>
        private async System.Threading.Tasks.Task PlayCurrentTrackAsync()
        {
            if (_selectedTrack != null)
            {
                // Получаем URL для воспроизведения трека
                string streamUrl = await _musicService.GetTrackStreamUrlAsync(_selectedTrack.Id);
                if (!string.IsNullOrEmpty(streamUrl))
                {
                    // Устанавливаем источник и начинаем воспроизведение
                    _mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(streamUrl));
                    _mediaPlayer.Play();
                    _isPlaying = true;
                    PlayPauseIcon.Glyph = "\uE769"; // Иконка паузы

                    // Обновляем информацию о текущем треке в фоновом задании
                    BackgroundMediaPlayerHelper.UpdateTrackInfo(_selectedTrack);
                }
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку предыдущего трека
        /// </summary>
        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            // Находим индекс текущего трека в списке
            int currentIndex = -1;
            for (int i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Id == _selectedTrack?.Id)
                {
                    currentIndex = i;
                    break;
                }
            }

            // Если текущий трек найден и есть предыдущий трек
            if (currentIndex > 0)
            {
                _selectedTrack = _tracks[currentIndex - 1];
                UpdateCurrentTrackInfo();
                await PlayCurrentTrackAsync();
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку следующего трека
        /// </summary>
        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Находим индекс текущего трека в списке
            int currentIndex = -1;
            for (int i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Id == _selectedTrack?.Id)
                {
                    currentIndex = i;
                    break;
                }
            }

            // Если текущий трек найден и есть следующий трек
            if (currentIndex >= 0 && currentIndex < _tracks.Count - 1)
            {
                _selectedTrack = _tracks[currentIndex + 1];
                UpdateCurrentTrackInfo();
                await PlayCurrentTrackAsync();
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку остановки
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer.Source != null)
            {
                _mediaPlayer.Pause();
                _mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
                _isPlaying = false;
                PlayPauseIcon.Glyph = "\uE768"; // Иконка воспроизведения

                // Обновляем шкалу времени
                TimelineSlider.Value = 0;
                CurrentTimeTextBlock.Text = "0:00";
            }
        }

        /// <summary>
        /// Обработчик нажатия на трек в списке
        /// </summary>
        private async void TrackItem_Click(object sender, ItemClickEventArgs e)
        {
            _selectedTrack = e.ClickedItem as Track;
            if (_selectedTrack != null)
            {
                // Обновляем информацию о текущем треке в плеере
                UpdateCurrentTrackInfo();

                // Начинаем воспроизведение выбранного трека
                await PlayCurrentTrackAsync();
            }
        }

        /// <summary>
        /// Обновление информации о текущем треке в плеере
        /// </summary>
        private void UpdateCurrentTrackInfo()
        {
            // Устанавливаем данные трека в элементы интерфейса
            DataContext = _selectedTrack;
            TotalTimeTextBlock.Text = _selectedTrack.FormattedDuration;
        }

        /// <summary>
        /// Обработчик события открытия медиа
        /// </summary>
        private async void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Получаем длительность трека
                _totalDuration = sender.PlaybackSession.NaturalDuration;

                // Устанавливаем максимальное значение для шкалы времени
                TimelineSlider.Maximum = _totalDuration.TotalSeconds;

                // Обновляем текст общей длительности
                TotalTimeTextBlock.Text = _selectedTrack.FormattedDuration;
            });
        }

        /// <summary>
        /// Обработчик события завершения воспроизведения
        /// </summary>
        private async void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Автоматически переходим к следующему треку
                NextButton_Click(null, null);
            });
        }

        /// <summary>
        /// Обработчик события ошибки воспроизведения
        /// </summary>
        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Показываем сообщение об ошибке
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Ошибка воспроизведения",
                    Content = $"Не удалось воспроизвести трек: {args.ErrorMessage}",
                    CloseButtonText = "OK"
                };

                errorDialog.ShowAsync();

                // Сбрасываем состояние воспроизведения
                _isPlaying = false;
                PlayPauseIcon.Glyph = "\uE768"; // Иконка воспроизведения
            });
        }

        /// <summary>
        /// Обработчик события изменения позиции воспроизведения
        /// </summary>
        private async void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (!_isTimelineSliderChanging && sender.Position.TotalSeconds > 0)
                {
                    // Обновляем положение шкалы времени
                    TimelineSlider.Value = sender.Position.TotalSeconds;

                    // Обновляем текст текущего времени
                    CurrentTimeTextBlock.Text = $"{(int)sender.Position.TotalMinutes}:{sender.Position.Seconds:D2}";
                }
            });
        }

        /// <summary>
        /// Обработчик изменения значения шкалы времени
        /// </summary>
        private void TimelineSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_mediaPlayer.Source != null && Math.Abs(e.NewValue - e.OldValue) > 1)
            {
                _isTimelineSliderChanging = true;

                // Устанавливаем новую позицию воспроизведения
                _mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);

                // Обновляем текст текущего времени
                TimeSpan position = TimeSpan.FromSeconds(e.NewValue);
                CurrentTimeTextBlock.Text = $"{(int)position.TotalMinutes}:{position.Seconds:D2}";

                _isTimelineSliderChanging = false;
            }
        }

        /// <summary>
        /// Обработчик изменения значения громкости
        /// </summary>
        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Volume = e.NewValue / 100.0;
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку гамбургер-меню
        /// </summary>
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
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
        
    }
}