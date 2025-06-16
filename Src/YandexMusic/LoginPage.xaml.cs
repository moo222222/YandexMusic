using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using YandexMusicUWP.Services;

namespace YandexMusicUWP
{
    /// <summary>
    /// Страница авторизации в Яндекс.Музыке
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private YandexMusicService _musicService;

        public LoginPage()
        {
            this.InitializeComponent();
            _musicService = new YandexMusicService();
            
        }

      
        /// <summary>
        /// Обработчик нажатия на кнопку "Войти по токену"
        /// </summary>
        //private async void TokenLoginButton_Click(object sender, RoutedEventArgs e)
        private async void SaveToken_Click(object sender, RoutedEventArgs e)
        {
                       
            string token = TokenBox.Text;

            if (string.IsNullOrEmpty(token))
            {
                ShowError("Введите токен");
                return;
            }

            try
            {
                // Авторизация по токену через сервис
                bool success = await _musicService.AuthorizeByTokenAsync(token);
                    
                if (!success)
                {
                    ShowError("Не удалось авторизоваться. Проверьте токен.");
                    return;
                }

                // Сохраняем токен, если пользователь выбрал "Запомнить меня"
                if (true)//(RememberMeCheckBox.IsChecked == true)
                {
                    _musicService.SaveToken(token);
                }

                // Переходим на главную страницу
                Frame.Navigate(typeof(MainPage));
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка авторизации по токену: {ex.Message}");
            }
            
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Назад"
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        /// <summary>
        /// Показывает сообщение об ошибке
        /// </summary>
        private async void ShowError(string message)
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
        /// Вызывается при загрузке страницы
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Проверяем, есть ли сохраненный токен
            string savedToken = _musicService.LoadToken();
            if (!string.IsNullOrEmpty(savedToken))
            {
                //RememberMeCheckBox.IsChecked = true;
                TokenBox.Text = savedToken;
            }
        }
    }
}