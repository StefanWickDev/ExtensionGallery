using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel;
using Windows.UI.ViewManagement;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;

namespace Sketch_a_Window
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(800, 600);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Colors.Transparent;
            ApplicationView.GetForCurrentView().TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Black;
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Touch;
            inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        private async void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            Rect viewBounds = ApplicationView.GetForCurrentView().VisibleBounds;
            Rect inkBounds = args.Strokes[0].BoundingRect;
            ApplicationData.Current.LocalSettings.Values["X"] = viewBounds.X + inkBounds.Left;
            ApplicationData.Current.LocalSettings.Values["Y"] = viewBounds.Y + inkBounds.Top;
            ApplicationData.Current.LocalSettings.Values["Width"] = Window.Current.Bounds.Width;
            ApplicationData.Current.LocalSettings.Values["Height"] = Window.Current.Bounds.Height;
            
            var inkPoints = args.Strokes[0].GetInkPoints();
            var rawPoints = new double[inkPoints.Count * 2];
            for(int i=0; i<inkPoints.Count; i++)
            {
                rawPoints[2 * i]     = inkPoints[i].Position.X - inkBounds.Left;
                rawPoints[2 * i + 1] = inkPoints[i].Position.Y - inkBounds.Top;
            }
            SavePointsToSettings(rawPoints);
            

            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
            await Task.Delay(1000);
            sender.StrokeContainer.Clear();
        }

        private void SavePointsToSettings(double []points)
        {
            try
            {
                ApplicationData.Current.LocalSettings.Values["Points"] = points;
            }
            catch
            {
                double []reducedPoints = new double[points.Length / 2];
                for (int i=0; i<reducedPoints.Length-1; i+=2)
                {
                    reducedPoints[i] = points[2 * i];
                    reducedPoints[i+1] = points[2 * i +1];
                }
                SavePointsToSettings(reducedPoints);
            }
        }
    }
}
