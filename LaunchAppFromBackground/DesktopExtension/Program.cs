using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace DesktopExtension
{
    class Program
    {
        static private AutoResetEvent resetEvent;
        static void Main(string[] args)
        {
            resetEvent = new AutoResetEvent(false);
            InvokeForegroundApp();
            resetEvent.WaitOne();
        }

        static private async void InvokeForegroundApp()
        {
            var appListEntries = await Package.Current.GetAppListEntriesAsync();
            await appListEntries.First().LaunchAsync();
            resetEvent.Set();
        }
    }
}
