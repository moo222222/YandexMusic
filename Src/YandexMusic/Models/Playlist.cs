using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YandexMusicUWP.Models
{
    /// <summary>
    /// Модель плейлиста Яндекс.Музыки
    /// </summary>
    public class Playlist : INotifyPropertyChanged
    {
        private string _id;
        private string _title;
        private string _description;
        private string _owner;
        private string _coverUri;
        private int _trackCount;
        private ObservableCollection<Track> _tracks;

        /// <summary>
        /// Идентификатор плейлиста
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
        /// Название плейлиста
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
        /// Описание плейлиста
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Владелец плейлиста
        /// </summary>
        public string Owner
        {
            get => _owner;
            set
            {
                if (_owner != value)
                {
                    _owner = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// URI обложки плейлиста
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
        /// Количество треков в плейлисте
        /// </summary>
        public int TrackCount
        {
            get => _trackCount;
            set
            {
                if (_trackCount != value)
                {
                    _trackCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Треки в плейлисте
        /// </summary>
        public ObservableCollection<Track> Tracks
        {
            get => _tracks ?? (_tracks = new ObservableCollection<Track>());
            set
            {
                if (_tracks != value)
                {
                    _tracks = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// URL обложки плейлиста
        /// </summary>
        public string CoverUrl => !string.IsNullOrEmpty(CoverUri) ? $"https://{CoverUri.Replace("%%", "200x200")}" : null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}