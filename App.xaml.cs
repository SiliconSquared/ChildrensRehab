using System.Windows;
using RehabKiosk.ViewModels;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using RehabKiosk.Model;
using System.Reflection;
using System.Windows.Media;
using RehabKiosk.Controls;
using System.Windows.Interop;
using System.Windows.Input;
using RehabKiosk.Properties;
using System.Windows.Threading;
using System.Threading;
using System.Configuration;

namespace RehabKiosk
{
    public partial class App : System.Windows.Application
    {
        public enum CachedAssets { CURSOR };

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        private static Dictionary<string, object> _cachedResourceAssets = new Dictionary<string, object>();
        private const UInt32 StdOutputHandle = 0xFFFFFFF5;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(UInt32 nStdHandle);
        [DllImport("kernel32.dll")]
        private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
        private static Mutex _oneAppMutex = new Mutex(true, "RehabKioskAppMutex");


        public static Window AppWindow { get; set; }


        public App()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
        }



        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
            }
            catch
            {
            }
        }



        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;

            if (MessageBox.Show("An application exception occurred, do you wish to copy the details to the clipboard?", "RehabKiosk Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(e.Message);
                sb.AppendLine(e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    sb.AppendLine(e.InnerException.Message);
                    sb.AppendLine(e.InnerException.StackTrace.ToString());
                }
                Clipboard.SetText(sb.ToString());
            }

            Debug.WriteLine("MyHandler caught : " + e.Message);
            Debug.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        }

        public static MainViewModel ViewModel { get; set; }

        public static IAppCloseableSink CurrentAppCloseableSink { get; set; }

        public static double DPIScaling { get; set; }

        public static uint UTCTimestamp
        {
            get
            {
                return (uint)Math.Truncate((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            }
        }
        
        public static Version AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }


    /// <summary>
    /// Application Entry Point.
    /// </summary>
        [System.STAThreadAttribute()]
      //  [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {
            RehabKiosk.App app = new RehabKiosk.App();

            if (!_oneAppMutex.WaitOne(TimeSpan.Zero, true))
            {
                //there is already another instance running!
                Application.Current.Shutdown();
            }
            else
            {
                ViewModel = new MainViewModel();
                ViewModel.Initialize();
                app.InitializeComponent();
                app.Run();
            }
        }



        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            for (int i = 0; i < e.Args.Length; i++)
            {
                Console.WriteLine("Startup Command Line Parameter " + e.Args[i]);

                if (String.Compare(e.Args[i], "-C", true) == 0)
                    CreateConsole();
            }

            // Now start the app
            var mainWindow = new MainWindow();
            this.MainWindow = mainWindow;
            mainWindow.Show();

            //When WPF has fully loaded it force it to the foreground
            var wih = new WindowInteropHelper(mainWindow);
            IntPtr hWnd = wih.Handle;
            SetForegroundWindow(hWnd);
        }



        public static object GetCachedResourceAsset(CachedAssets assetType, string uri)
        {
            object asset = null;

            if (_cachedResourceAssets.TryGetValue(uri, out asset))
                return asset;
            switch(assetType)
            {
                case CachedAssets.CURSOR:
                    asset = new Cursor(Application.GetResourceStream(new Uri(uri)).Stream);
                    _cachedResourceAssets[uri] = asset;
                    break;
            }
            return asset;
        }



        public static void CreateConsole()
        {
            AllocConsole();
            IntPtr stdHandle = GetStdHandle(StdOutputHandle);
            SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
            StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }



        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }



    public class SharedResourceDictionary : ResourceDictionary
    {
        /// <summary>
        /// Internal cache of loaded dictionaries 
        /// </summary>
        public static Dictionary<Uri, ResourceDictionary> _sharedDictionaries = new Dictionary<Uri, ResourceDictionary>();

        /// <summary>
        /// Local member of the source uri
        /// </summary>
        private Uri _sourceUri;

        /// <summary>
        /// Gets or sets the uniform resource identifier (URI) to load resources from.
        /// </summary>
        public new Uri Source
        {
            get { return _sourceUri; }
            set
            {
                _sourceUri = value;

                if (!_sharedDictionaries.ContainsKey(value))
                {
                    // If the dictionary is not yet loaded, load it by setting
                    // the source of the base class
                    base.Source = value;

                    // add it to the cache
                    _sharedDictionaries.Add(value, this);
                }
                else
                {
                    // If the dictionary is already loaded, get it from the cache
                    MergedDictionaries.Add(_sharedDictionaries[value]);
                }
            }
        }
    }
}
    
