using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.System;
using Windows.Media.Playback;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.ExtendedExecution.Foreground;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AutoLaunchWithAudio
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ExtendedExecutionForegroundSession session = null;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            MessageDialog dlg = new MessageDialog(args.ErrorMessage, "MediaFailed");
            await dlg.ShowAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            player.Source = MediaSource.CreateFromUri(new Uri("http://live-aacplus-64.kexp.org/kexp64.aac"));
            player.MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            player.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
        }

        private async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                session = new ExtendedExecutionForegroundSession();
                session.Reason = ExtendedExecutionForegroundReason.BackgroundAudio;
                var result = await session.RequestExtensionAsync();
                if (result != ExtendedExecutionForegroundResult.Allowed)
                    throw new Exception("EE denied");

                IList<AppDiagnosticInfo> infos = await AppDiagnosticInfo.RequestInfoForAppAsync();
                IList<AppResourceGroupInfo> resourceInfos = infos[0].GetResourceGroups();
                await resourceInfos[0].StartSuspendAsync();
            }
        }
    }
}
