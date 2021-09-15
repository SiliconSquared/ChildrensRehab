using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RehabKiosk.Model
{
    public class ActiveOperationState : INotifyPropertyChanged
    {
        private delegate void NoArgDelegate();
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


        private bool _active = false;
        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                NotifyPropertyChanged("Active");
            }
        }



        private bool _asyncActive = false;
        public bool AsyncActive
        {
            get
            {
                return _asyncActive;
            }
            set
            {
                _asyncActive = value;
                NotifyPropertyChanged("AsyncActive");
            }
        }



        private bool _progressActive = false;
        public bool ProgressActive
        {
            get
            {
                return _progressActive;
            }
            set
            {
                _progressActive = value;
                NotifyPropertyChanged("ProgressActive");
            }
        }



        private string _messageText = null;
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


        private ControlTemplate _dropInControlContent = null;
        public ControlTemplate DropInControlContent
        {
            get
            {
                return _dropInControlContent;
            }
            set
            {
                _dropInControlContent = value;
                NotifyPropertyChanged("DropInControlContent");
            }
        }



        public void Activate(FrameworkElement resourceOwner, string messageResource, bool progress = false)
        {
            DropInControlContent = null;
            if(messageResource != null)
                MessageText = (string)resourceOwner.Resources[messageResource];
            else
                MessageText = null;

            if (progress)
                ProgressActive = true;
            else
                AsyncActive = true;
            Active = true;
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, (NoArgDelegate)delegate { });
        }



        public async Task ShowConfirmation(FrameworkElement resourceOwner, string messageResourceOrText, string contentResource, int dragOnBusyWait, int hangOnConfirmationWait)
        {
            await Task.Delay(dragOnBusyWait);
            ProgressActive = AsyncActive = false;
            MessageText = (string)resourceOwner.Resources[messageResourceOrText];
            if (MessageText == null)
                MessageText = messageResourceOrText;
            DropInControlContent = App.Current.Resources[contentResource] as ControlTemplate;
            await Task.Delay(hangOnConfirmationWait);
            Done();
        }



        public void Done()
        {
            Active = false;
        }

    }
}
