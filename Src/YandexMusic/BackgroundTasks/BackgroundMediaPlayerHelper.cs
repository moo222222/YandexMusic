using System;
using Windows.Media.Playback;
using YandexMusicUWP.Models;

namespace YandexMusicUWP.BackgroundTasks
{
    /// <summary>
    /// Класс-помощник для работы с фоновым воспроизведением музыки
    /// </summary>
    public static class BackgroundMediaPlayerHelper
    {
        private static MediaPlayer _mediaPlayer;
        private static Track _currentTrack;

        /// <summary>
        /// Инициализация фонового воспроизведения
        /// </summary>
        /// <param name="mediaPlayer">Экземпляр MediaPlayer</param>
        public static void Initialize(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
            
            // Настраиваем медиаплеер для фонового воспроизведения
            _mediaPlayer.CommandManager.IsEnabled = true;
            _mediaPlayer.SystemMediaTransportControls.IsEnabled = true;
            _mediaPlayer.SystemMediaTransportControls.IsPlayEnabled = true;
            _mediaPlayer.SystemMediaTransportControls.IsPauseEnabled = true;
            _mediaPlayer.SystemMediaTransportControls.IsNextEnabled = true;
            _mediaPlayer.SystemMediaTransportControls.IsPreviousEnabled = true;
            
            // Подписываемся на события системных элементов управления медиа
            _mediaPlayer.SystemMediaTransportControls.ButtonPressed += SystemMediaTransportControls_ButtonPressed;
        }

        /// <summary>
        /// Обновление информации о текущем треке
        /// </summary>
        /// <param name="track">Текущий трек</param>
        public static void UpdateTrackInfo(Track track)
        {
            _currentTrack = track;

            // Обновляем информацию о треке в системных элементах управления медиа
            Windows.Media.SystemMediaTransportControlsDisplayUpdater updater = _mediaPlayer.SystemMediaTransportControls.DisplayUpdater;

            if (updater != null)
            {
                updater.Type = Windows.Media.MediaPlaybackType.Music;

                // Очищаем предыдущие данные
                updater.MusicProperties.Title = track.Title;
                updater.MusicProperties.Artist = track.Artist;
                updater.MusicProperties.AlbumTitle = track.Album != null ? track.Album : "";

                // Если есть обложка, устанавливаем её
                if (!string.IsNullOrEmpty(track.CoverUrl))
                {
                    try
                    {
                        updater.Thumbnail = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri(track.CoverUrl));
                    }
                    catch (Exception)
                    {
                        // Игнорируем ошибки при установке обложки
                    }
                }

                // Применяем обновления
                updater.Update();
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопок системных элементов управления медиа
        /// </summary>
        private static void SystemMediaTransportControls_ButtonPressed
        (
            Windows.Media.SystemMediaTransportControls sender, 
            Windows.Media.SystemMediaTransportControlsButtonPressedEventArgs args
        )
        {
            switch (args.Button)
            {
                case Windows.Media.SystemMediaTransportControlsButton.Play:
                    _mediaPlayer.Play();
                    break;
                    
                case Windows.Media.SystemMediaTransportControlsButton.Pause:
                    _mediaPlayer.Pause();
                    break;
                    
                case Windows.Media.SystemMediaTransportControlsButton.Stop:
                    _mediaPlayer.Pause();
                    _mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
                    break;
                    
                case Windows.Media.SystemMediaTransportControlsButton.Next:
                    // Обработка перехода к следующему треку должна быть реализована в MainPage
                    // Здесь можно отправить сообщение или вызвать событие
                    break;
                    
                case Windows.Media.SystemMediaTransportControlsButton.Previous:
                    // Обработка перехода к предыдущему треку должна быть реализована в MainPage
                    // Здесь можно отправить сообщение или вызвать событие
                    break;
            }
        }
    }
}