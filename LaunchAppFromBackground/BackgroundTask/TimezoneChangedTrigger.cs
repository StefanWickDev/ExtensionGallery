using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Metadata;

namespace BackgroundTask 
{
    public sealed class TimezoneChangedTrigger : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                deferral.Complete();
            }
        }
    }
}
