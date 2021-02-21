using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectReinforced.Clients
{
    public class ClientManager
    {
        [DllImport("kernel32")]
        public static extern void CloseHandle(IntPtr hObject);
        [DllImport("user32")]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);
        [DllImport("user32")]
        public static extern IntPtr GetForegroundWindow();

        public LolClient Lol { get; }
        public int R6 { get; }

        public ClientManager()
        {
            this.Lol = new LolClient();
            this.R6 = 0;
        }

        public async Task Initialize()
        {
            if (Lol.IsRunning)
            {
                await Lol.InitializeClientApi();
            }
        }

        public static bool IsRunning(string name)
        {
            var processes = Process.GetProcessesByName(name);
            return processes.Length > 0;
        }

        public static bool IsActive(string name)
        {
            IntPtr mwHandle = FindWindow(null, name);
            IntPtr cwHandle = GetForegroundWindow();

            bool isActive = mwHandle == cwHandle;

            CloseHandle(mwHandle);
            CloseHandle(cwHandle);

            return isActive;
        }
    }
}
