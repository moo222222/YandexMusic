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
        /// Обработчик нажатия на кнопку "Войти"
        /// </summary>
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = LoginTextBox.Text;
                string password = PasswordBox.Password;

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    ShowError("Введите логин и пароль");
                    return;
                }

                // Показываем индикатор загрузки
                LoginButton.IsEnabled = false;
                LoginButton.Content = "Выполняется вход...";

                // Авторизация через сервис
                bool success = await _musicService.AuthorizeAsync(login, password);
                
                if (!success)
                {
                    ShowError("Не удалось авторизоваться. Проверьте логин и пароль.");
                    return;
                }

                // Сохраняем токен, если пользователь выбрал "Запомнить меня"
                if (true)//(RememberMeCheckBox.IsChecked == true)
                {
                    _musicService.SaveToken();
                }

                // Переходим на главную страницу
                Frame.Navigate(typeof(MainPage));
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка авторизации: {ex.Message}");
            }
            finally
            {
                // Восстанавливаем кнопку
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Войти";
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Войти по токену"
        /// </summary>
        private async void TokenLoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем диалог для ввода токена
            ContentDialog tokenDialog = new ContentDialog
            {
                Title = "Вход по токену",
                Content = new TextBox { PlaceholderText = "Введите токен Яндекс.Музыки" },
                PrimaryButtonText = "Войти",
                CloseButtonText = "Отмена"
            };

            ContentDialogResult result = await tokenDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string token = (tokenDialog.Content as TextBox).Text;

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
            }
        }
    }
}