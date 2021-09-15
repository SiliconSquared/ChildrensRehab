using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Animation;


namespace RehabKiosk.Controls
{
    public class CanvasCarouselControlItem : ContentControl, INotifyPropertyChanged
    {
        #region NotifyProperty
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                NotifyPropertyChanged(propertyName);
        }



        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public CanvasCarouselControlItem()
        {
            
        }



        public bool WiredUp { get; set; }


        public Storyboard FillerStoryboard { get; set; }
    }
}
