using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YandexMusicUWP.Models
{
    /// <summary>
    /// Модель трека Яндекс.Музыки
    /// </summary>
    public class Track : INotifyPropertyChanged
    {
        private string _id;
        private string _title;
        private string _artist;
        private string _album;
        private string _coverUri;
        private TimeSpan _duration;
        private bool _isPlaying;

        /// <summary>
        /// Идентификатор трека
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Название трека
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Исполнитель трека
        /// </summary>
        public string Artist
        {
            get => _artist;
            set
            {
                if (_artist != value)
                {
                    _artist = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Альбом трека
        /// </summary>
        public string Album
        {
            get => _album;
            set
            {
                if (_album != value)
                {
                    _album = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// URI обложки трека
        /// </summary>
        public string CoverUri
        {
            get => _coverUri;
            set
            {
                if (_coverUri != value)
                {
                    _coverUri = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Длительность трека
        /// </summary>
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Флаг, указывающий, воспроизводится ли трек в данный момент
        /// </summary>
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Форматированная длительность трека (мм:сс)
        /// </summary>
        public string FormattedDuration => $"{(int)Duration.TotalMinutes}:{Duration.Seconds:D2}";

        /// <summary>
        /// URL обложки трека
        /// </summary>
        public string CoverUrl => !string.IsNullOrEmpty(CoverUri) ? $"https://{CoverUri.Replace("%%", "200x200")}" : null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}