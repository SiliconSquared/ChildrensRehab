using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Windows.Interop;
using RehabKiosk.Views;
using System.Windows.Forms;
using System.Drawing;
using RehabKiosk.Controls;
using System.Runtime.InteropServices;
using RehabKiosk.ViewModels;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Threading.Tasks;
using ChildrensRehab;

namespace RehabKiosk
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private const double APPWINDOW_ASPECTRATIO = 870.0 / 855.0; 

        public enum ActiveSlideInPanels { NONE, SETTINGS, DEVICES, ONLINE };
        public enum ActiveHubPages { NONE, SEQUENCES, DEVICEASSETS, DEVICECONSOLE, CLOUDGALLERY, PROGRAMMER, HELP };

        private bool _burgerPanelTransitioning = false;
        private ActiveSlideInPanels _activeSlideInPanel = ActiveSlideInPanels.NONE;
        private BurgerPanelControl _activeBurgerPanelControl = null;
        private SequenceHubPage _sequenceHubPage = null;
        private SettingsPanel _settingsPanel = null;
        private WindowInteropHelper _windowInteropHelper;

        #region NotifyProperty
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                NotifyPropertyChanged(propertyName);
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public MainWindow()
        {
            InitializeComponent();
            // this.MaxHeight = System.Windows.SystemParameters.WorkArea.Height + SystemParameters.SmallCaptionHeight;

            DataContext = this;

            App.AppWindow = this;
            App.ViewModel.MainWindow = this;
            App.ViewModel.AssetManager = new RehabAssetManager();
            App.ViewModel.AssetManager.Initialize();

            App.ViewModel.BurgerPanelMask = null;
        }



        public static readonly DependencyProperty IsApplicationActiveProperty = DependencyProperty.Register("IsApplicationActive", typeof(bool), typeof(MainWindow), new PropertyMetadata(false, (d, e) => ((MainWindow)d).NotifyPropertyChanged("IsApplicationActive")));
        public bool IsApplicationActive
        {
            get { return (bool)GetValue(IsApplicationActiveProperty); }
            set { SetValue(IsApplicationActiveProperty, value); }
        }


        
        public bool IsDeviceSelectionPanelActive
        {
            get
            {
                return _activeSlideInPanel == ActiveSlideInPanels.DEVICES;
            }
        }



        private bool _disableAppbar = false;
        public bool DisableAppbar
        {
            get
            {
                return _disableAppbar;
            }
            set
            {
                _disableAppbar = value;
                NotifyPropertyChanged("DisableAppbar");
            }
        }



        private bool _resizingWindow = false;
        public bool ResizingWindow
        {
            get
            {
                return _resizingWindow;
            }
            set
            {
                _resizingWindow = value;
                NotifyPropertyChanged("ResizingWindow");
            }
        }



        private double _currentBurgerPanelWidth = 0;
        public double CurrentBurgerPanelWidth
        {
            get
            {
                return _currentBurgerPanelWidth;
            }
            set
            {
                _currentBurgerPanelWidth = value;
                NotifyPropertyChanged("CurrentBurgerPanelWidth");
            }
        }


        private ActiveHubPages _activeHubPage = ActiveHubPages.SEQUENCES;
        public ActiveHubPages ActiveHubPage
        {
            get
            {
                return _activeSlideInPanel == ActiveSlideInPanels.NONE ? _activeHubPage : ActiveHubPages.NONE;
            }
            set
            {
                _activeHubPage = value;
                NotifyPropertyChanged("ActiveHubPage");
            }
        }



        public ActiveSlideInPanels ActiveSlideInPanel
        {
            get
            {
                return _activeSlideInPanel;
            }
            set
            {
                _activeSlideInPanel = value;
                NotifyPropertyChanged("ActiveSlideInPanel", "ActiveHubPage");
            }
        }



        public bool IsSettingPanelActive
        {
            get
            {
                return _activeSlideInPanel == ActiveSlideInPanels.SETTINGS;
            }
        }



        public Thickness FrameThickness
        {
            get
            {
                if(this.WindowState == WindowState.Maximized)
                    return new Thickness(7,7,7,10);
                else
                    return new Thickness(0);
            }
        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _windowInteropHelper = new WindowInteropHelper(this);
            _hookID = SetHook(_lowLevelKeyboardProc);

            // Determine app WPF DPI scaling
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(new WindowInteropHelper(this).Handle);
            System.Windows.Forms.Screen screen = ((RehabKiosk.MainWindow)System.Windows.Application.Current.MainWindow).GetAppScreen();
            App.DPIScaling = (96.0 / graphics.DpiY);

            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            // Add and initialize the default pages
            _sequenceHubPage = new SequenceHubPage();
            _sequenceHubPage.PrepareForPageActivation += PrepareForPageActivation;
            HubFrame.AddPage(ActiveHubPages.SEQUENCES, _sequenceHubPage);

            // Show My Animations Hub
            HubFrame.Display(ActiveHubPages.SEQUENCES);

            PositionInitialWindow();
        }



        public void OpenInfoPopup(FrameworkElement ele, double overrideWidth = 0)
        {
            if (_activeBurgerPanelControl != null)
                _activeBurgerPanelControl.IsEnabled = false;
            App.ViewModel.MainWindow.DisableAppbar = true;


            IInfoPopupChild infoPopupImpl = ele as IInfoPopupChild;
            infoPopupImpl.CloseRequestEvent += InfoPopupImpl_CloseRequestEvent;

            if (overrideWidth > 0)
                PopupContentControl.Width = overrideWidth;
            else
                PopupContentControl.Width = 360;
            PopupContentControl.Content = ele;
            AppPopupFrame.Visibility = Visibility.Visible;
        }



        private void InfoPopupImpl_CloseRequestEvent(object sender, EventArgs e)
        {
            AppPopupFrame.Visibility = Visibility.Collapsed;
            PopupContentControl.Content = null;
            if (_activeBurgerPanelControl != null)
                _activeBurgerPanelControl.IsEnabled = true;
            App.ViewModel.MainWindow.DisableAppbar = false;
        }



        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }



        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NotifyPropertyChanged("FrameThickness");
        }


        private void PositionInitialWindow()
        {
            // Get screen application window is being created on
            Screen activeScreen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            Graphics graphics = Graphics.FromHwnd(new WindowInteropHelper(this).Handle);

            Rectangle rcArea = activeScreen.WorkingArea;
            //rcArea.Inflate(new System.Drawing.Size(-100, -100));

            this.Left = rcArea.Left * (96.0 / graphics.DpiY);
            this.Top = rcArea.Top * (96.0 / graphics.DpiY);
            this.Width = rcArea.Width * (96.0 / graphics.DpiY);
            this.Height = rcArea.Height * (96.0 / graphics.DpiY);

            // Need to show My Animations hub view after the main app window has been positioned and sized as the MeasureItemOverride
            // gets confused by Recycling items when it shouldn't as the container size finally settles
            _sequenceHubPage.FinalizeView();
        }



        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_ENTERSIZEMOVE = 0x0231;
            const int WM_EXITSIZEMOVE = 0x0232;

            if (msg == WM_ENTERSIZEMOVE)
                ResizingWindow = true;
            else if (msg == WM_EXITSIZEMOVE)
                ResizingWindow = false;
            return IntPtr.Zero;
        }



        private delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);


        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    if(IsApplicationActive && wParam.ToInt32() > 0)
                    {
                        App.ViewModel.HookKeyboardHandler?.Invoke(this, new MainViewModel.HotKeyEvent() { Modifier = LOWORD16(lParam), Key = HIWORD16(lParam)  });                            
                        handled = true;
                    }
                    break;
            }
            return IntPtr.Zero;
        }



        private void AppWindow_Activated(object sender, EventArgs e)
        {
            IsApplicationActive = true;
            if (_sequenceHubPage != null)
                _sequenceHubPage.ApplicationActivated(IsApplicationActive);

            /*
            if (InfoPopup.Child != null)
            {
                DispatcherTimer postDisplayTimer = new DispatcherTimer();
                postDisplayTimer.Interval = TimeSpan.FromSeconds(0.2);
                postDisplayTimer.Tick += (tsender, te) =>
                {
                    (tsender as DispatcherTimer).Stop();
                    InfoPopup.IsOpen = true;
                };
                postDisplayTimer.Start();
            }*/
        }



        private void AppWindow_Deactivated(object sender, EventArgs e)
        {
            IsApplicationActive = false;
            if (_sequenceHubPage != null)
                _sequenceHubPage.ApplicationActivated(IsApplicationActive);
            GC.Collect();
        }



        // Allows application window to be dragged
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }



        bool _burgerBarExpanded = false;
        private void BurgerButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb;

            if (_burgerBarExpanded)
                sb = FindResource("CollapseBurgerBar") as Storyboard;
            else
                sb = FindResource("ExpandBurgerBar") as Storyboard;
            _burgerBarExpanded = !_burgerBarExpanded;
            sb.Begin();
        }



        private void PrepareForPageActivation(object sender)
        {
            CollapseBurgerBar();
            HideBurgerPanel();            
        }



        private void CollapseBurgerBar()
        {
            if (_burgerBarExpanded)
            {
                Storyboard sb = FindResource("CollapseBurgerBar") as Storyboard;
                sb.Begin();
                _burgerBarExpanded = false;
            }
        }



        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_burgerPanelTransitioning)
            {
                if (_activeBurgerPanelControl != null)
                    HideBurgerPanel();
                else
                    ShowBurgerPanel(ActiveSlideInPanels.SETTINGS);
            }
        }



        private void ShowDevicePanel_Click(object sender, RoutedEventArgs e)
        {
            if (!_burgerPanelTransitioning)
            {
                if (_activeBurgerPanelControl != null)
                    HideBurgerPanel();
                else
                    ShowBurgerPanel(ActiveSlideInPanels.DEVICES);
            }
        }



        private void ShowAccount_Click(object sender, RoutedEventArgs e)
        {
            if (!_burgerPanelTransitioning)
            {
                if (_activeBurgerPanelControl != null)
                    HideBurgerPanel();
                else
                    ShowBurgerPanel(ActiveSlideInPanels.ONLINE);
            }
        }



        private void SettingsPanel_OnCloseRequest(object sender, EventArgs e)
        {
            HideBurgerPanel();
        }




        private void BurgerPanel_OnCloseRequest(object sender, EventArgs e)
        {
            HideBurgerPanel();
        }



        private void ShowBurgerPanel(ActiveSlideInPanels newPanel)
        {
            if (_activeSlideInPanel != newPanel)
            {
                CollapseBurgerBar();
                ActiveSlideInPanel = newPanel;

                switch (_activeSlideInPanel)
                {
                    case ActiveSlideInPanels.SETTINGS:
                        _settingsPanel = new SettingsPanel();
                        _settingsPanel.DataContext = App.ViewModel;
                        _activeBurgerPanelControl = _settingsPanel;
                        break;
                }

                if (_activeBurgerPanelControl != null)
                {
                    CurrentBurgerPanelWidth = _activeBurgerPanelControl.PanelWidth;
                    _activeBurgerPanelControl.PanelCloseRequest += BurgerPanel_OnCloseRequest;
                    SettingsGrid.Children.Add(_activeBurgerPanelControl);
                    _activeBurgerPanelControl.InvalidateMeasure();
                    _activeBurgerPanelControl.InvalidateVisual();

                    Storyboard showBurgerPanelFlyIn = FindResource("BurgerPanelFlyIn") as Storyboard;
                    showBurgerPanelFlyIn.Completed += delegate
                    {
                        _burgerPanelTransitioning = false;
                    };

                    DoubleAnimation burgerPanelFlyInX = showBurgerPanelFlyIn.Children.FirstOrDefault(i => i.Name == "BurgerPanelFlyInX") as DoubleAnimation;
                    burgerPanelFlyInX.From = -CurrentBurgerPanelWidth;
                    burgerPanelFlyInX.To = 0;
                    showBurgerPanelFlyIn.Begin();
                    _burgerPanelTransitioning = true;
                    NotifyPropertyChanged("IsDeviceSelectionPanelActive");
                }
            }
        }



        private void HideBurgerPanel()
        {
            if (_activeBurgerPanelControl != null)
            {
                Storyboard hideBurgerPanelFlyOut = FindResource("BurgerPanelFlyOut") as Storyboard;
                DoubleAnimation burgerPanelFlyOutX = hideBurgerPanelFlyOut.Children.FirstOrDefault(i => i.Name == "BurgerPanelFlyOutX") as DoubleAnimation;
                burgerPanelFlyOutX.From = 0;
                burgerPanelFlyOutX.To = -CurrentBurgerPanelWidth;
                hideBurgerPanelFlyOut.Completed += delegate
                {
                    _burgerPanelTransitioning = false;
                    SettingsGrid.Children.Clear();
                    if (_activeBurgerPanelControl != null)
                    {
                        _activeBurgerPanelControl.PanelCloseRequest -= BurgerPanel_OnCloseRequest;
                        _activeBurgerPanelControl = null;
                    }
                };
                hideBurgerPanelFlyOut.Begin();
                _burgerPanelTransitioning = true;
                ActiveSlideInPanel = ActiveSlideInPanels.NONE;
                NotifyPropertyChanged("IsDeviceSelectionPanelActive");
            }
        }


        
        private void ShowSequences_Click(object sender, RoutedEventArgs e)
        {
            HideBurgerPanel();
            if (ActiveHubPage != ActiveHubPages.SEQUENCES)
            {
                ActiveHubPage = ActiveHubPages.SEQUENCES;
                HubFrame.Display(ActiveHubPage);
            }
        }


        

        private void WebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            HideBurgerPanel();
            System.Diagnostics.Process.Start("https://aka.ms/AA9amqa");
        }


        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HideBurgerPanel();

            /*
            if (ActiveHubPage != ActiveHubPages.HELP)
            {
                if (_helpPage == null)
                {
                    _helpPage = new HelpPage();
                    _helpPage.PrepareForPageActivation += PrepareForPageActivation;
                    HubFrame.AddPage(ActiveHubPages.HELP, _helpPage);
                }
                ActiveHubPage = ActiveHubPages.HELP;
                HubFrame.Display(ActiveHubPage);
            }*/
            System.Diagnostics.Process.Start("https://aka.ms/AA9amqd");
        }


        private void AppContentMask_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HideBurgerPanel();
        }



        private void SysMenuCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }



        private void SysMenuMinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        
        private void SysMenuRestoreButton_Click(object sender, RoutedEventArgs e)
        {
           // this.MaxHeight = System.Windows.SystemParameters.WorkArea.Height + SystemParameters.SmallCaptionHeight;
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }



        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if(App.CurrentAppCloseableSink != null)
            {
                if(!App.CurrentAppCloseableSink.AppCloseRequest())
                {
                    e.Cancel = true;
                    return;
                }
            }
        }



        private void MainWindow_Closed(object sender, EventArgs e)
        {
        }



        public Screen GetAppScreen()
        {
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(this);
            return System.Windows.Forms.Screen.FromHandle(windowInteropHelper.Handle);
        }



        public bool RegisterHotKey(int hotkeyid, uint modifier, uint vkkey)
        {
            return RegisterHotKey(_windowInteropHelper.Handle, hotkeyid, modifier, vkkey);
        }



        public void UnregisterHotKey(int hotkeyid)
        {
            UnregisterHotKey(_windowInteropHelper.Handle, hotkeyid);
        }



        int GetIntUnchecked(IntPtr value)
        {
            return IntPtr.Size == 8 ? unchecked((int)value.ToInt64()) : value.ToInt32();
        }



        int LOWORD16(IntPtr value)
        {
            return unchecked((short)GetIntUnchecked(value));
        }



        int HIWORD16(IntPtr value)
        {
            return unchecked((short)(((uint)GetIntUnchecked(value)) >> 16));
        }


        public const int VK_LCONTROL = 0xA2;
        public const int VK_RCONTROL = 0xA3;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _lowLevelKeyboardProc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);



        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }



        public bool _ctrlKeyDown = false;
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int vkCode = Marshal.ReadInt32(lParam);

            if (nCode >= 0 && App.ViewModel.MainWindow.IsApplicationActive)
            {
                if (wParam == (IntPtr)WM_KEYDOWN && (vkCode == VK_LCONTROL || vkCode == VK_RCONTROL))
                    App.ViewModel.MainWindow._ctrlKeyDown = true;
                else if (wParam == (IntPtr) WM_KEYUP && (vkCode == VK_LCONTROL || vkCode == VK_RCONTROL))
                    App.ViewModel.MainWindow._ctrlKeyDown = false;

                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    int modifier = 0;


                    if (App.ViewModel.MainWindow._ctrlKeyDown)
                        modifier = (int)MainViewModel.HotKeyEvent.KeyModifiers.MOD_CTRL;
                    App.ViewModel.HookKeyboardHandler?.Invoke(App.AppWindow, new MainViewModel.HotKeyEvent() { Modifier = modifier, Key = vkCode });
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }



        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetAsyncKeyState(int keyCode);
        


        private static KeyStates DetermineKeyState(int key)
        {
            KeyStates state = KeyStates.None;

            short retVal = GetAsyncKeyState((int)key);


            // Debug.WriteLine("KEYSTATE " + retVal);

            //If the high-order bit is 1, the key is down
            //otherwise, it is up.
            if ((retVal & 0x8000) == 0x8000)
                state |= KeyStates.Down;

            //If the low-order bit is 1, the key is toggled.
            if ((retVal & 1) == 1)
                state |= KeyStates.Toggled;

            return state;
        }

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

      
    }
}

