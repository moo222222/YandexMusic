using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Playback;

namespace YandexMusicUWP.BackgroundTasks
{
    /// <summary>
    /// Фоновая задача для воспроизведения аудио
    /// </summary>
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private MediaPlayer _mediaPlayer;

        /// <summary>
        /// Метод, вызываемый при запуске фоновой задачи
        /// </summary>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Получаем отсрочку завершения задачи, чтобы она продолжала работать в фоне
            _deferral = taskInstance.GetDeferral();

            // Подписываемся на событие отмены задачи
            taskInstance.Canceled += TaskInstance_Canceled;

            try
            {
                // Получаем экземпляр медиаплеера из BackgroundMediaPlayer
                _mediaPlayer = BackgroundMediaPlayer.Current;

                // Подписываемся на события медиаплеера
                _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
                _mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;

                // Настраиваем системные элементы управления медиа
                var smtc = _mediaPlayer.SystemMediaTransportControls;
                smtc.ButtonPressed += SystemMediaTransportControls_ButtonPressed;
                smtc.PropertyChanged += SystemMediaTransportControls_PropertyChanged;

                // Включаем системные элементы управления медиа
                smtc.IsEnabled = true;
                smtc.IsPlayEnabled = true;
                smtc.IsPauseEnabled = true;
                smtc.IsNextEnabled = true;
                smtc.IsPreviousEnabled = true;
                smtc.IsStopEnabled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации фоновой задачи: {ex.Message}");
                _deferral.Complete();
            }
        }

        /// <summary>
        /// Обработчик события отмены задачи
        /// </summary>
        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                // Отписываемся от событий
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
                    _mediaPlayer.MediaFailed -= MediaPlayer_MediaFailed;

                    var smtc = _mediaPlayer.SystemMediaTransportControls;
                    smtc.ButtonPressed -= SystemMediaTransportControls_ButtonPressed;
                    smtc.PropertyChanged -= SystemMediaTransportControls_PropertyChanged;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при отмене фоновой задачи: {ex.Message}");
            }
            finally
            {
                // Завершаем отсрочку
                _deferral.Complete();
            }
        }

        /// <summary>
        /// Обработчик события завершения воспроизведения
        /// </summary>
        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            // Здесь можно реализовать автоматический переход к следующему треку
            // или отправить сообщение в основное приложение
        }

        /// <summary>
        /// Обработчик события ошибки воспроизведения
        /// </summary>
        private void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Debug.WriteLine($"Ошибка воспроизведения: {args.ErrorMessage}");
        }

        /// <summary>
        /// Обработчик нажатия кнопок системных элементов управления медиа
        /// </summary>
        private void SystemMediaTransportControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    _mediaPlayer.Play();
                    break;

                case SystemMediaTransportControlsButton.Pause:
                    _mediaPlayer.Pause();
                    break;

                case SystemMediaTransportControlsButton.Stop:
                    _mediaPlayer.Pause();
                    _mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
                    break;

                case SystemMediaTransportControlsButton.Next:
                    // Здесь можно реализовать переход к следующему треку
                    // или отправить сообщение в основное приложение
                    break;

                case SystemMediaTransportControlsButton.Previous:
                    // Здесь можно реализовать переход к предыдущему треку
                    // или отправить сообщение в основное приложение
                    break;
            }
        }

        /// <summary>
        /// Обработчик изменения свойств системных элементов управления медиа
        /// </summary>
        private void SystemMediaTransportControls_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            // Обработка изменений свойств системных элементов управления медиа
        }
    }
}