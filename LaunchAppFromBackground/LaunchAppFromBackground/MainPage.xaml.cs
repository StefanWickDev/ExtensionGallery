using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LaunchAppFromBackground
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string TimezoneTriggerTaskName = "timezoneTrigger";
        private string TimezoneTriggerTaskEntryPoint = "BackgroundTask.TimezoneChangedTrigger";

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UnregisterBackgroundTask();
            RegisterBackgroundTask();
        }

        private void RegisterBackgroundTask()
        {
            var requestTask = BackgroundExecutionManager.RequestAccessAsync();
            var builder = new BackgroundTaskBuilder();
            builder.Name = TimezoneTriggerTaskName;
            builder.TaskEntryPoint = TimezoneTriggerTaskEntryPoint;
            builder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));
            var task = builder.Register();

            status.Text = "Backgrond Task registered. Close app and change the timezone settings to see the app launched to foreground from a background task.";
        }

        private void UnregisterBackgroundTask()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == TimezoneTriggerTaskName)
                {
                    task.Value.Unregister(true);
                    break;
                }
            }
        }
    }
}
