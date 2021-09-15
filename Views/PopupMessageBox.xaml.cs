using RehabKiosk.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RehabKiosk
{
    /// <summary>
    /// Interaction logic for PopupMessageBox.xaml
    /// </summary>
    public partial class PopupMessageBox : UserControl, INotifyPropertyChanged, IInfoPopupChild
    {
        public class ActionEventArgs
        {
            public object Context { get; set; }
            public Results Result { get; set; }
            public int PromptIdentifier { get; set; }
        }



        public enum ButtonTypes { OK, YESNO };
        public enum IconTypes { NONE, QUESTION, EXCLAMATION, INFO };
        public enum Results { OK, YES, NO };

        public event EventHandler<ActionEventArgs> PopupMessageBoxActionEvent;
        public event EventHandler CloseRequestEvent;

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



        public PopupMessageBox(Popup popupHost, int promptIdentifier, object context, string msg, ButtonTypes type, IconTypes icon)
        {
            InitializeComponent();
            DataContext = this;

            FontSize = IconFontSize = 20;
            PopupHost = popupHost;
            PromptIdentifier = promptIdentifier;
            Context = context;
            MessageText = msg;
            ButtonType = type;
            IconType = icon;
        }



        private void PopupMessageBox_Loaded(object sender, RoutedEventArgs e)
        {
            this.PreviewKeyDown += new KeyEventHandler(HandleEscapeKey);
            if (ButtonType == ButtonTypes.OK)
                OKButton.Focus();
            else
                YesButton.Focus();
        }



        private void PopupMessageBox_Unloaded(object sender, RoutedEventArgs e)
        {
            this.PreviewKeyDown -= new KeyEventHandler(HandleEscapeKey);
        }

        

        private string _messageText;
        public string MessageText
        {
            get
            {
                return _messageText;
            }
            set
            {
                _messageText = value;
                NotifyPropertyChanged("MessageText");
            }
        }



        private IconTypes _iconType = IconTypes.NONE;
        public IconTypes IconType
        {
            get
            {
                return _iconType;
            }
            set
            {
                _iconType = value;
                NotifyPropertyChanged("IconType");
            }
        }



        private double _iconFontSize = 12;
        public double IconFontSize
        {
            get
            {
                return _iconFontSize;
            }
            set
            {
                _iconFontSize = value;
                NotifyPropertyChanged("IconFontSize");
            }
        }



        private ButtonTypes _buttonType = ButtonTypes.OK;
        public ButtonTypes ButtonType
        {
            get
            {
                return _buttonType;
            }
            set
            {
                _buttonType = value;
                NotifyPropertyChanged("ButtonType");
            }
        }



        public Popup PopupHost { get; set; }

        public int PromptIdentifier { get; set; }

        public object Context { get; set; }


        private void HandleEscapeKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (ButtonType == ButtonTypes.OK)
                    OkButton_Click(null, null);
                else
                    NoButton_Click(null, null);
            }
            else
            {
                if (ButtonType == ButtonTypes.OK)
                {
                    if (e.Key == Key.O)
                        OkButton_Click(null, null);
                }
                else
                {
                    if (e.Key == Key.Y)
                        YesButton_Click(null, null);
                    else if (e.Key == Key.N)
                        NoButton_Click(null, null);
                }
            }
        }



        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if(PopupHost != null)
                PopupHost.IsOpen = false;
            else
                CloseRequestEvent?.Invoke(this, null);
            PopupMessageBoxActionEvent?.Invoke(this, new ActionEventArgs() { Result = Results.OK, PromptIdentifier = this.PromptIdentifier, Context = this.Context });
        }



        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (PopupHost != null)
                PopupHost.IsOpen = false;
            else
                CloseRequestEvent?.Invoke(this, null);
            PopupMessageBoxActionEvent?.Invoke(this, new ActionEventArgs() { Result = Results.YES, PromptIdentifier = this.PromptIdentifier, Context = this.Context });
        }



        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PopupHost != null)
                PopupHost.IsOpen = false;
            else
                CloseRequestEvent?.Invoke(this, null);
            PopupMessageBoxActionEvent?.Invoke(this, new ActionEventArgs() { Result = Results.NO, PromptIdentifier = this.PromptIdentifier, Context = this.Context });
        }


    }
}
