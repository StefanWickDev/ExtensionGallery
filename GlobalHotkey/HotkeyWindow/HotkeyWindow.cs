using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace HotkeyWindow
{
    public enum Modifiers
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeast = 16384
    }

    class HotkeyAppContext : ApplicationContext
    {
        private HotKeyWindow hotkeyWindow = null;
        private AppServiceConnection connection = null;
        private Process process = null;

        public HotkeyAppContext()
        {
            int processId = (int)ApplicationData.Current.LocalSettings.Values["processId"];
            process = Process.GetProcessById(processId);
            process.EnableRaisingEvents = true;
            process.Exited += HotkeyAppContext_Exited;
            hotkeyWindow = new HotKeyWindow();
            hotkeyWindow.HotkeyPressed += new HotKeyWindow.HotkeyDelegate(hotkeys_HotkeyPressed);
            hotkeyWindow.RegisterCombo(1001, Modifiers.Alt, Keys.S); // Alt-S = stingray
            hotkeyWindow.RegisterCombo(1002, Modifiers.Alt, Keys.O); // Alt-O = octopus
        }

        private void HotkeyAppContext_Exited(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async Task InitializeAppServiceConnection()
        {
            connection = new AppServiceConnection();
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.AppServiceName = "HotkeyConnection";
            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                Debug.WriteLine(status);
                Application.Exit();
            }

            connection.ServiceClosed += Connection_ServiceClosed;
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Debug.WriteLine("Connection_ServiceClosed");
            connection = null;
        }

        private async void hotkeys_HotkeyPressed(int ID)
        {
            // bring the UWP to the foreground (optional)
            IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
            await appListEntries.First().LaunchAsync();

            // send the key ID to the UWP
            ValueSet hotkeyPressed = new ValueSet();
            hotkeyPressed.Add("ID", ID);
            if (connection == null)
            {
                await InitializeAppServiceConnection();
            }
            AppServiceResponse response = await connection.SendMessageAsync(hotkeyPressed);
        }
    }

    public class HotKeyWindow : NativeWindow
    {
        private const int WM_HOTKEY = 0x0312;
        private const int WM_DESTROY = 0x0002;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private List<Int32> IDs = new List<int>();
        public delegate void HotkeyDelegate(int ID);
        public event HotkeyDelegate HotkeyPressed;

        // creates a headless Window to register for and handle WM_HOTKEY
        public HotKeyWindow()
        {
            this.CreateHandle(new CreateParams());
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        public void RegisterCombo(Int32 ID, Modifiers fsModifiers, Keys vlc)
        {
            if (RegisterHotKey(this.Handle, ID, (int)fsModifiers, (int)vlc))
            {
                IDs.Add(ID);
            }
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            this.DestroyHandle();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_HOTKEY: //raise the HotkeyPressed event
                    HotkeyPressed?.Invoke(m.WParam.ToInt32());
                    break;

                case WM_DESTROY: //unregister all hot keys
                    foreach (int ID in IDs)
                    {
                        UnregisterHotKey(this.Handle, ID);
                    }
                    break;
            }
            base.WndProc(ref m);
        }
    }
}
