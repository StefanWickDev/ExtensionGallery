using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace GlobalHotkey
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                Process process = Process.GetCurrentProcess();
                ApplicationData.Current.LocalSettings.Values["processID"] = process.Id;

                App.AppServiceConnected += AppServiceConnected;
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();

                MessageDialog dlg = new MessageDialog("Alt-S: Stingray\r\nAlt-O: Octopus", "Global Hotkeys Registered");
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await dlg.ShowAsync();
                });
            }
        }

        private void AppServiceConnected(object sender, Windows.ApplicationModel.AppService.AppServiceTriggerDetails e)
        {
            e.AppServiceConnection.RequestReceived += AppServiceConnection_RequestReceived;
        }

        private async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            int id = (int)args.Request.Message["ID"];
            switch(id)
            {
                case 1001://stingray
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        stingrayMove.Begin();
                    });
                    break;
                case 1002://octopus
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()=>
                    {
                        octopusMove.Begin();
                    });
                    break;
                default:
                    break;
            }
            await args.Request.SendResponseAsync(new ValueSet());
            App.AppServiceDeferral.Complete();
        }
    }
}
