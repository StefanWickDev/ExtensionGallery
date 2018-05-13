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

namespace HotkeyWindow
{
    class HotkeyAppContext : ApplicationContext
    {
        private HotKeyWindow hotkeyWindow = null;
        private AppServiceConnection connection = null;

        public HotkeyAppContext()
        {
            hotkeyWindow = new HotKeyWindow();
            hotkeyWindow.HotkeyPressed += new HotKeyWindow.HotkeyDelegate(hotkeys_HotkeyPressed);
            hotkeyWindow.RegisterCombo(1001, 1, (int)Keys.S); // stingray
            hotkeyWindow.RegisterCombo(1002, 1, (int)Keys.O); // octopus

            InitializeAppServiceConnection();
        }

        private async void InitializeAppServiceConnection()
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
            Application.Exit();
        }

        private async void hotkeys_HotkeyPressed(int ID)
        {
            // bring the UWP to the foreground (optional)
            IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
            await appListEntries.First().LaunchAsync();

            // send the key ID to the UWP
            ValueSet hotkeyPressed = new ValueSet();
            hotkeyPressed.Add("ID", ID);
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

        public void RegisterCombo(Int32 ID, int fsModifiers, int vlc)
        {
            if (RegisterHotKey(this.Handle, ID, fsModifiers, vlc))
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
