// using Microsoft.HandsFree.ArcReactor;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ChildrensRehab;
using RehabKiosk.Model;



namespace RehabKiosk.Views
{
    public partial class SequenceHubPage : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer _handsOffTimer = new DispatcherTimer();
        public delegate void PrepareForPageActivationHandler(object sender);
        public event PrepareForPageActivationHandler PrepareForPageActivation = null;

        public enum PromptIdentifiers { CONFIRM_DELETE_ANIMATION, CONFIRM_DELETE_FAILURE, CLOUD_DIMENSION_LIMITATION };
        private enum SortByType { MOSTRECENT, NAMEASC, NAMEDESC };
        private enum BinaryExportTypes : int { BINARY = 1, MAKECODEBINARY = 2, JSON = 3 };
        private ObservableCollection<RehabAsset> _rehabAssets = new ObservableCollection<RehabAsset>();
        private FrameworkElement _currentAssetContainer = null;

        #region NotifyProperty
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                NotifyPropertyChanged(propertyName);
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion



        public SequenceHubPage()
        {
            InitializeComponent();
            DataContext = this;

            _handsOffTimer.Interval = TimeSpan.FromSeconds(2);
            _handsOffTimer.Tick += HandsOffTimer_Tick;
            _handsOffTimer.Start();
        }



        public FrameworkElement BurgerPanelMask
        {
            get
            {
                return App.ViewModel.BurgerPanelMask;
            }
        }



        public ObservableCollection<RehabAsset> RehabAssets
        {
            get
            {
                return _rehabAssets;
            }
        }



        public void FinalizeView()
        {
            
        }



        private void SequenceHubPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AssetPanel1.Width = AssetCanvas.ActualWidth * 0.9;
            AssetPanel1.Height = (AssetCanvas.ActualHeight- ButtonPanel.ActualHeight) * 0.9;
            AssetPanel2.Width = AssetCanvas.ActualWidth * 0.9;
            AssetPanel2.Height = (AssetCanvas.ActualHeight - ButtonPanel.ActualHeight) * 0.9;

            Canvas.SetLeft(AssetPanel1, (AssetCanvas.ActualWidth - AssetPanel1.ActualWidth) / 2);
            Canvas.SetTop(AssetPanel1, (AssetCanvas.ActualHeight - AssetPanel1.ActualHeight) / 2);
            Canvas.SetLeft(AssetPanel2, (AssetCanvas.ActualWidth - AssetPanel2.ActualWidth) / 2);
            Canvas.SetTop(AssetPanel2, (AssetCanvas.ActualHeight - AssetPanel2.ActualHeight) / 2);

        }



        private void OpenConfirmPopup(int popupIdentifier, object context, string msg, PopupMessageBox.ButtonTypes type, PopupMessageBox.IconTypes icon)
        {
            PopupMessageBox msgBox = new PopupMessageBox(ConfirmPopup, popupIdentifier, context, msg, type, icon);
            msgBox.PopupMessageBoxActionEvent += OnConfirmPopupActionEvent;
            ConfirmPopup.Child = msgBox;
            ConfirmPopup.IsOpen = true;
        }



        private void OnConfirmPopupActionEvent(object sender, PopupMessageBox.ActionEventArgs e)
        {
            switch (e.PromptIdentifier)
            {
                case (int)PromptIdentifiers.CONFIRM_DELETE_ANIMATION:
                    break;
            }
        }


        public void ApplicationActivated(bool active)
        {
        }




        private void SequenceWindow_FlyInContentCompleted(object sender, EventArgs e)
        {
        }



        private void SequenceWindow_OnCloseRequest(object sender, EventArgs e)
        {
            Storyboard hideContentFlyOut = FindResource("HubViewFlyinPanelHide") as Storyboard;
            DoubleAnimation contentFlyOutGridX = hideContentFlyOut.Children.FirstOrDefault(i => i.Name == "HubViewFlyinPanelHideGridX") as DoubleAnimation;
            contentFlyOutGridX.From = 0;
            contentFlyOutGridX.To = -HubViewBody.ActualWidth;
            hideContentFlyOut.Begin();
        }



        private void SequenceWindow_FlyOutContentCompleted(object sender, EventArgs e)
        {
            HubViewFlyinPanelViewbox.Child = null;
        }




        private void SequenceHubPage_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<RehabAsset> rehabAssets = new ObservableCollection<RehabAsset>();

            _rehabAssets.Add(new RehabAsset() { Title = "XBox AAP" });
            _rehabAssets.Add(new RehabAsset() { Title = "Table" });
            _rehabAssets.Add(new RehabAsset() { Title = "Chair" });
            _rehabAssets.Add(new RehabAsset() { Title = "Chair2" });
            _rehabAssets.Add(new RehabAsset() { Title = "Chai3" });

            for (int i = 0;i < 5;i++)
                rehabAssets.Add(new RehabAsset() { Title = "Item " + i.ToString() });

            _currentAssetContainer = AssetPanel1;
            _currentAssetContainer.DataContext = App.ViewModel.AssetManager.GetNextAsset();
            StartAssetEntry(_currentAssetContainer);
        }



        private void HandsOffTimer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            FrameworkElement nextAssetContainer = _currentAssetContainer == AssetPanel1 ? AssetPanel2 : AssetPanel1;

            nextAssetContainer.DataContext = App.ViewModel.AssetManager.GetNextAsset();
            StartAssetSwitch(nextAssetContainer, _currentAssetContainer);
            _currentAssetContainer = nextAssetContainer;
        }



        private void StartAssetEntry(FrameworkElement assetContainer)
        {
            _currentAssetContainer = assetContainer;
            DoubleAnimation canvasTranslateX = new DoubleAnimation();
            canvasTranslateX.Duration = TimeSpan.FromMilliseconds(500);
            Storyboard.SetTargetProperty(canvasTranslateX, new PropertyPath("(Canvas.Left)"));
            Storyboard.SetTarget(canvasTranslateX, assetContainer);
            canvasTranslateX.From = this.ActualWidth;
            canvasTranslateX.To = (this.ActualWidth - assetContainer.ActualWidth) / 2;

            SineEase easingFunction = new SineEase();
            easingFunction.EasingMode = EasingMode.EaseIn;
            canvasTranslateX.EasingFunction = easingFunction;

            ObjectAnimationUsingKeyFrames keyFrameAnimation = new ObjectAnimationUsingKeyFrames();
            keyFrameAnimation.Duration = canvasTranslateX.Duration;
            Storyboard.SetTarget(keyFrameAnimation, assetContainer);
            Storyboard.SetTargetProperty(keyFrameAnimation, new PropertyPath("Visibility"));
            DiscreteObjectKeyFrame visibilityAnimation = new DiscreteObjectKeyFrame(Visibility.Visible, new TimeSpan(0, 0, 0));
            keyFrameAnimation.KeyFrames.Add(visibilityAnimation);

            Storyboard sb = new Storyboard();
            sb.Children.Add(canvasTranslateX);
            sb.Children.Add(keyFrameAnimation);
            sb.Begin();
        }



        private void StartAssetExit(FrameworkElement assetContainer)
        {
            _currentAssetContainer = assetContainer;
            DoubleAnimation canvasTranslateX = new DoubleAnimation();
            canvasTranslateX.Duration = TimeSpan.FromMilliseconds(500);
            Storyboard.SetTargetProperty(canvasTranslateX, new PropertyPath("(Canvas.Left)"));
            Storyboard.SetTarget(canvasTranslateX, assetContainer);
            canvasTranslateX.To = 0 - AssetPanel1.ActualWidth;

            SineEase easingFunction = new SineEase();
            easingFunction.EasingMode = EasingMode.EaseIn;
            canvasTranslateX.EasingFunction = easingFunction;

            Storyboard sb = new Storyboard();
            sb.Children.Add(canvasTranslateX);
            sb.Begin();
        }



        private void StartAssetSwitch(FrameworkElement assetContainerEntry, FrameworkElement assetContainerExit)
        {
            DoubleAnimation canvasTranslateXExit = new DoubleAnimation();
            canvasTranslateXExit.Duration = TimeSpan.FromMilliseconds(500);
            Storyboard.SetTargetProperty(canvasTranslateXExit, new PropertyPath("(Canvas.Left)"));
            Storyboard.SetTarget(canvasTranslateXExit, assetContainerExit);
            canvasTranslateXExit.To = 0 - AssetPanel1.ActualWidth;

            SineEase easingFunction = new SineEase();
            easingFunction.EasingMode = EasingMode.EaseIn;
            canvasTranslateXExit.EasingFunction = easingFunction;

            ObjectAnimationUsingKeyFrames keyFrameAnimationExit = new ObjectAnimationUsingKeyFrames();
            keyFrameAnimationExit.Duration = canvasTranslateXExit.Duration;
            Storyboard.SetTarget(keyFrameAnimationExit, assetContainerExit);
            Storyboard.SetTargetProperty(keyFrameAnimationExit, new PropertyPath("Visibility"));
            DiscreteObjectKeyFrame visibilityAnimation = new DiscreteObjectKeyFrame(Visibility.Hidden, TimeSpan.FromMilliseconds(500));
            keyFrameAnimationExit.KeyFrames.Add(visibilityAnimation);

            DoubleAnimation canvasTranslateXEntry = new DoubleAnimation();
            canvasTranslateXEntry.Duration = TimeSpan.FromMilliseconds(500);
            Storyboard.SetTargetProperty(canvasTranslateXEntry, new PropertyPath("(Canvas.Left)"));
            Storyboard.SetTarget(canvasTranslateXEntry, assetContainerEntry);
            canvasTranslateXEntry.From = this.ActualWidth;
            canvasTranslateXEntry.To = (this.ActualWidth - assetContainerEntry.ActualWidth) / 2;

            canvasTranslateXEntry.EasingFunction = easingFunction;

            ObjectAnimationUsingKeyFrames keyFrameAnimationEntry = new ObjectAnimationUsingKeyFrames();
            keyFrameAnimationEntry.Duration = canvasTranslateXEntry.Duration;
            Storyboard.SetTarget(keyFrameAnimationEntry, assetContainerEntry);
            Storyboard.SetTargetProperty(keyFrameAnimationEntry, new PropertyPath("Visibility"));
            DiscreteObjectKeyFrame visibilityAnimationEntry = new DiscreteObjectKeyFrame(Visibility.Visible, new TimeSpan(0, 0, 0));
            keyFrameAnimationEntry.KeyFrames.Add(visibilityAnimationEntry);

            Storyboard sb = new Storyboard();
            sb.Children.Add(canvasTranslateXExit);
            sb.Children.Add(keyFrameAnimationExit);
            sb.Children.Add(canvasTranslateXEntry);
            sb.Children.Add(keyFrameAnimationEntry);            
            sb.Begin();
        }



    }

}