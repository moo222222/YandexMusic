using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using YandexMusicUWP.BackgroundTasks;

namespace YandexMusicUWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user. Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Регистрируем фоновую задачу для воспроизведения музыки
            await RegisterBackgroundAudioTaskAsync();
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Регистрация фоновой задачи для воспроизведения музыки
        /// </summary>
        private async System.Threading.Tasks.Task RegisterBackgroundAudioTaskAsync()
        {
            // Проверяем, зарегистрирована ли уже задача
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == "YandexMusicBackgroundAudioTask")
                {
                    // Задача уже зарегистрирована
                    return;
                }
            }

            // Запрашиваем доступ к фоновым задачам
            var requestStatus = await BackgroundExecutionManager.RequestAccessAsync();

            if (requestStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity ||
                requestStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
            {
                // Создаем билдер для фоновой задачи
                var builder = new BackgroundTaskBuilder();
                builder.Name = "YandexMusicBackgroundAudioTask";
                builder.TaskEntryPoint = "YandexMusicUWP.BackgroundTasks.BackgroundAudioTask";

                // Устанавливаем триггер для аудио
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));

                // Регистрируем задачу
                BackgroundTaskRegistration task = builder.Register();

                Debug.WriteLine("Фоновая задача для воспроизведения музыки зарегистрирована");
            }
            else
            {
                Debug.WriteLine($"Не удалось получить доступ к фоновым задачам: {requestStatus}");
            }
        }

        /// <summary>
        /// Invoked when application execution is being suspended. Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}