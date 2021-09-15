using System;
using System.ComponentModel;
using System.Windows;
using ChildrensRehab;
using RehabKiosk;

namespace RehabKiosk.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public class HotKeyEvent
        {
            public enum KeyModifiers { MOD_CTRL = 0x0002 }
            public enum Keys { VK_Z = 0x5A, VK_Q = 0x51 }


            public int Modifier { get; set; }
            public int Key { get; set; }
        }




        #region NotifyProperty      
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                NotifyPropertyChanged(propertyName);
        }



        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public MainViewModel()
        {
        }



        public void Initialize()
        {

        }


        public string ApplicationDocumentsFolder { get; private set; }


        public string ApplicationPrivateFolder { get; private set; }

        public RehabAssetManager AssetManager { get; set; }


        public MainWindow MainWindow { get; set; }



        public EventHandler<HotKeyEvent> HookKeyboardHandler;


        public string AppPublishVersion
        {
            get
            {
                return RehabKiosk.App.AssemblyVersion.Major + "." + RehabKiosk.App.AssemblyVersion.Minor;
            }
        }
        

        public FrameworkElement BurgerPanelMask { get; set; }


    }
}
