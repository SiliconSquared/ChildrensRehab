using RehabKiosk.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace RehabKiosk.Controls
{
    public class DisplayItemMetaData
    {
        public bool Behind { get; set; }

        public int ZIndex { get; set; }

        public double Opacity { get; set; }

        public double Top { get; set; }

        public double Left { get; set; }

        public double ScaleX { get; set; }

        public double ScaleY { get; set; }

        public double CenterX { get; set; }

        public double CenterY { get; set; }

        public int BinaryLevel { get; set; }
    }


    public class PreCalcDisplaySet : IComparable<PreCalcDisplaySet>
    {
        public int Index { get; set; }

        public int ZOrder { get; set; }

        public double Coefficient { get; set; }

        public int BinaryLevel { get; set; }

        public int CompareTo(PreCalcDisplaySet item)
        {
            // return item.ZOrder.CompareTo(this.ZOrder);
            return item.Coefficient.CompareTo(this.Coefficient);
        }

    }


    public partial class CanvasCarouselControl : ItemsControl
    {
        private const double FULLROTATIONDEGREES = 360.0;
        private const double HALFROTATIONDEGREES = 180.0;
        protected double _currentRotation = 0;
        protected double X_SCALE = 0;
        protected double Y_SCALE = 0;
        private double _cachedRotationSpeed = DEFAULT_ROTATION_SPEED;
        private bool _spinning = false;
        private bool _pendingAnimationSnap = false;
        private bool _animationSnapRunning = false;
        private object _animationSnapLock = new object();
        private UInt32 _storyboardRunningRefCount = 0;
        private bool _flagSpinToStop = false;
        private const double INTERNAL_SCALE_COEFFICIENT = 1.0;
        //private const double INTERNAL_SCALE_COEFFICIENT = 1;
        private DispatcherTimer _rotationTimer = new DispatcherTimer();
        private DispatcherTimer _idleTimer = new DispatcherTimer();
        private DateTime _previousTime;
        private DateTime _currentTime;
        private DateTime _startRotationTime;
        private DateTime _previousInternalEventTime = DateTime.MinValue;
        private double _predictedRotationSeconds;
        private double _rotationToGo = 0;
        private double _degreesToRotate;
        protected double _targetRotation = 0;
        private object _selectedItem = null;
        private FrameworkElement _selectedContainer = null;
        private FrameworkElement _pendingSelectedContainer = null;
       // private GestureRecognizer _gestureRecognizer = new GestureRecognizer();
        private UInt32 _rotationControl = (UInt32) RotationControl.NORMAL;
        private Dictionary<object, DisplayItemMetaData> _displayMetaDataCollection = new Dictionary<object, DisplayItemMetaData>();
        private Dictionary<Storyboard, CanvasCarouselControlItem> _storyboardToItemMap = new Dictionary<Storyboard, CanvasCarouselControlItem>();
        private int _debugCount = 0;

        public delegate void ElementSelectedHandler(object sender, object item);
        public event ElementSelectedHandler ElementSelected = null;
        public delegate void InteractingHandler(object sender);
        public event InteractingHandler Interacting = null;
        public delegate void IdleHandler(object sender);
        public event IdleHandler Idle = null;
        

        public enum OrientationAlignments { Auto, Horizontal, Vertical }
        public enum RotateDirection { Left, Right };

        enum RotationControl { NORMAL= 0x0, NOFINALBOUNCE = 0x1 }


        public CanvasCarouselControl()
        {
            this.InitializeComponent();
            this.SizeChanged += CanvasCarouselControl_SizeChanged;
            this.LayoutUpdated += CanvasCarouselControl_LayoutUpdated;

            RotationSpeedOneOff = 0;
            OrientationAlignment = OrientationAlignments.Horizontal;
            
            _rotationTimer.Interval = TimeSpan.FromMilliseconds(10);
            _rotationTimer.Tick += RotationTimer_Tick;

            _idleTimer.Interval = TimeSpan.FromSeconds(1);
            // _idleTimer.Tick += IdleTimer_Tick;

        }



        void CanvasCarouselControl_LayoutUpdated(object sender, object e)
        {            
            if (_selectedContainer == null && Items.Count > 0)
            {
                WireUpItemContainers();
                SetElementPositions();
                
                SelectElement(ItemContainerGenerator.ContainerFromItem(Items[0]) as FrameworkElement);
            }
            
        }


        void IdleTimer_Tick(object sender, object e)
        {
            if (_previousInternalEventTime != DateTime.MinValue)
            {
                if ((DateTime.Now - _previousInternalEventTime).TotalSeconds > 5)
                {
                    if (Idle != null)
                        Idle(this);
                    _previousInternalEventTime = DateTime.MinValue;
                }
            }
        }



        void CanvasCarouselControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            X_SCALE = CenterX * INTERNAL_SCALE_COEFFICIENT;
            Y_SCALE = CenterY * INTERNAL_SCALE_COEFFICIENT;                      
        }


        
        public Point SelectedItemCanvasOrigin
        {
            get
            {
                Point pt = new Point();
                pt.X = Canvas.GetLeft(_selectedContainer);
                pt.Y = Canvas.GetTop(_selectedContainer);
                return pt;
            }
        }



        public object SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                CanvasCarouselControlItem itemContainer = ItemContainerGenerator.ContainerFromItem(value) as CanvasCarouselControlItem;
                if (_rotationTimer.IsEnabled)
                {
                    _pendingSelectedContainer = itemContainer;
                    Debug.WriteLine("PENDING");
                }
                else
                    RotateToElement(itemContainer);
            }
        }



        protected virtual void RotationTimer_Tick(object sender, object e)
        {
            _currentTime = DateTime.Now;

            // Debug.WriteLine("RotationTimer_Tick " + RotationSpeed.ToString());

            if ((_rotationToGo < RotationAmount) && (_rotationToGo > -RotationAmount))
            {
                _rotationToGo = 0;
                if (_currentRotation != _targetRotation)
                    _currentRotation = _targetRotation;
                else
                {
                    Debug.WriteLine("ROTATION STOP");
                    _rotationTimer.Stop();

                    lock (_animationSnapLock)
                    {
                        RotationSpeedOneOff = 0;
                        _animationSnapRunning = false;
                    }


                    // Restore assigned (not bounced) rotation speed
                    RotationSpeed = _cachedRotationSpeed;
                    RotationSpeedOneOff = 0;
                    
                    // Restore rotation characteristics
                    _rotationControl = (UInt32)RotationControl.NORMAL;

                    // Fire selected event if required
                    if (SelectOnRotate && ElementSelected != null)
                    {
                        Debug.WriteLine("ElementSelected FIRED");
                        ElementSelected(this, _selectedItem);
                    }

                    if (_pendingSelectedContainer != null)
                    {
                        Debug.WriteLine("PENDING RELEASED");
                        RotateToElement(_pendingSelectedContainer);
                        _pendingSelectedContainer = null;
                    }
                    return;
                }
            }
            else if (_rotationToGo < 0)
            {
                _rotationToGo += RotationAmount;
                _currentRotation = ClampDegrees(_currentRotation + RotationAmount);
            }
            else
            {
                _rotationToGo -= RotationAmount;
                _currentRotation = ClampDegrees(_currentRotation - RotationAmount);
            }

            SetElementPositions();

            if ((_rotationControl & (UInt32)RotationControl.NOFINALBOUNCE) == 0)
            {
                // Slow down near the end
                if (_predictedRotationSeconds - (DateTime.Now - _startRotationTime).TotalSeconds < 0.1)
                    RotationSpeed = 50;
                else if (_predictedRotationSeconds - (DateTime.Now - _startRotationTime).TotalSeconds < 0.2)
                    RotationSpeed = 100;
                else if (_predictedRotationSeconds - (DateTime.Now - _startRotationTime).TotalSeconds < 0.3)
                    RotationSpeed = 125;
                else if (_predictedRotationSeconds - (DateTime.Now - _startRotationTime).TotalSeconds < 0.4)
                    RotationSpeed = 150;
            }

            _previousTime = _currentTime;
            _previousInternalEventTime = DateTime.Now;
        }



        protected override DependencyObject GetContainerForItemOverride()
        {
            return (DependencyObject)new CanvasCarouselControlItem();
        }



        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is CanvasCarouselControlItem;
        }



        private void WireUpItemContainers()
        {
            for (int index = 0; index < Items.Count; index++)
            {
                CanvasCarouselControlItem contentControl = ItemContainerGenerator.ContainerFromIndex(index) as CanvasCarouselControlItem;
                if(contentControl != null)
                {
                    if (!contentControl.WiredUp)
                    {
                        WireUpContainer(contentControl, true);
                        contentControl.WiredUp = true;
                    }
                }            
            }
        }



        private void WireUpContainer(CanvasCarouselControlItem contentControl, bool wire)
        {
            /*
            if (wire)
            {
                contentControl.Tapped += contentControl_Tapped;
                contentControl.PointerPressed += contentControl_PointerPressed;
                contentControl.ManipulationMode = ManipulationModes.All;
                contentControl.ManipulationCompleted += contentControl_ManipulationCompleted;
                contentControl.ManipulationDelta += contentControl_ManipulationDelta;
                contentControl.ManipulationInertiaStarting += contentControl_ManipulationInertiaStarting;
            }
            else
            {
                contentControl.Tapped -= contentControl_Tapped;
                contentControl.PointerPressed -= contentControl_PointerPressed;
                contentControl.ManipulationMode = ManipulationModes.None;
                contentControl.ManipulationCompleted -= contentControl_ManipulationCompleted;
                contentControl.ManipulationDelta -= contentControl_ManipulationDelta;
                contentControl.ManipulationInertiaStarting -= contentControl_ManipulationInertiaStarting;
            }*/
        }


        /*
        void contentControl_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            //App.MainPage.AddVisualConsoleRow("MANIP COMPLETED " + _storyboardRunningRefCount.ToString());
            _spinning = false;
            if (_storyboardRunningRefCount == 0)
                SnapToHighestZElement();
            else
            {
                lock (_animationSnapLock)
                {
                    if (!_animationSnapRunning)
                        _pendingAnimationSnap = true;
                }
            }
        }
        */

        /*
        void contentControl_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            //App.MainPage.AddVisualConsoleRow((_debugCount++) + "MANIPULATION STARTING");
            //Debug.WriteLine("VELOCITY " + e.Velocities.Linear.Y.ToString());
            double maxVelocity = Math.Abs(e.Velocities.Linear.Y);

            double spinTime = 1500;
            if (maxVelocity < 0.3)
                spinTime = 500;
            else if (maxVelocity < 0.7)
                spinTime = 700;
            else if (maxVelocity < 1.5)
                spinTime = 1000;
            else if (maxVelocity < 2.5)
                spinTime = 1300;

            e.TranslationBehavior.DesiredDeceleration = maxVelocity / spinTime;
            prevPulse = DateTime.Now;
            _spinning = true;
        }
        */

        /*
        DateTime prevPulse;
        void contentControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Interacting != null)
                Interacting(this);

            if (_flagSpinToStop)
            {
                //App.MainPage.AddVisualConsoleRow((_debugCount++) + "FLAG SPIN TO STOP PROCESSED");
                e.Complete();
                _flagSpinToStop = false;
                _spinning = false;
                return;
            }

            lock (_animationSnapLock)
            {
                if (_animationSnapRunning)
                {
                    //App.MainPage.AddVisualConsoleRow((_debugCount++) + "ANIMATION SNAP HURRY UP");
                    RotationSpeed = MAXIMUM_ROTATION_SPEED;
                    Debug.WriteLine("ANIMATION SNAP HURRY UP");
                    return;
                }
            }

            //Debug.WriteLine("DELTA Translate Y " + e.Delta.Translation.Y.ToString() + " " + (DateTime.Now - prevPulse).TotalMilliseconds);
            // App.MainPage.AddVisualConsoleRow((_debugCount++) + "DELTA " + e.Delta.Translation.Y.ToString());
            // Limit the speed of the spin
            double maxInertia;
            if(e.Delta.Translation.Y > 0)            
                maxInertia = Math.Min(3, e.Delta.Translation.Y);
            else
                maxInertia = Math.Max(-3, e.Delta.Translation.Y);

            prevPulse = DateTime.Now;
            if (e.IsInertial)
            {
                _currentRotation = ClampDegrees(_currentRotation + (-maxInertia / 2)); // 4.5
                SetElementPositions();
            }
            else
            {
                _currentRotation = ClampDegrees(_currentRotation + (-maxInertia / 2));
                SetElementPositions();
            }
        }
        */

        /*
        void contentControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //App.MainPage.AddVisualConsoleRow((_debugCount++) + "PRESSED" + (_spinning ? " SPINNING" : ""));
            if (_spinning)
                _flagSpinToStop = true;
            _rotationTimer.Stop();
            if (Interacting != null)
                Interacting(null);
            _previousInternalEventTime = DateTime.Now;
        }
        */


        /*
        void contentControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //App.MainPage.AddVisualConsoleRow((_debugCount++) + "TAPPED");
            FrameworkElement element = (FrameworkElement)sender;
            SelectElement(element);
            _previousInternalEventTime = DateTime.Now;
        }

        */

        public void SelectElement(FrameworkElement element, bool forceOnEvent = false)
        {
            if (element != null)
            {
                SelectRotation(element);

                if (forceOnEvent || (!SelectOnRotate && ElementSelected != null))
                {
                    Debug.WriteLine("ElementSelected FIRED");
                    ElementSelected(this, ItemContainerGenerator.ItemFromContainer(element));
                }
            }
        }



        private void StartRotation(double numberOfDegrees)
        {
            _rotationToGo = numberOfDegrees;
            if (!_rotationTimer.IsEnabled)
            {
                _rotationTimer.Start();
                Debug.WriteLine("**** ROTATION START ****" + numberOfDegrees.ToString());
            }
        }



        public void SelectRotation(FrameworkElement element)
        {
            if (element != null)
            {
                RotateToElement(element);
            }
        }



        private void RotateToElement(FrameworkElement element)
        {
            if (element != null && element != _selectedContainer)
            {
                _selectedContainer = element;
                _selectedItem = ItemContainerGenerator.ItemFromContainer(_selectedContainer);
                int targetIndex = ItemContainerGenerator.IndexFromContainer(element);

                if (RotationSpeedOneOff != 0)
                    _rotationControl = (UInt32) RotationControl.NOFINALBOUNCE;

                _degreesToRotate = GetDegreesNeededToPlaceElementInFront(_currentRotation, targetIndex, Items.Count);
                _targetRotation = ClampDegrees(_currentRotation - _degreesToRotate);
                _previousTime = _startRotationTime = DateTime.Now;
                _predictedRotationSeconds = Math.Abs(_degreesToRotate) / RotationSpeed;
                StartRotation(_degreesToRotate);
            }
        }



        private bool SnapToHighestZElement()
        {
            List<PreCalcDisplaySet> preDisplaySet = GetVisibleDisplaySet();
            CanvasCarouselControlItem contentControlHighestZ = ItemContainerGenerator.ContainerFromIndex(preDisplaySet[0].Index) as CanvasCarouselControlItem;

            if (_selectedContainer != contentControlHighestZ)
            {
                // App.MainPage.AddVisualConsoleRow("Snap Back");

                // Give a nice smooth snap to position effect
                RotationSpeedOneOff = 75;
                SelectElement(contentControlHighestZ);
                return true;
            }
            return false;
        }



        internal static double GetDegreesNeededToPlaceElementInFront(double currentRotation, int targetIndex, int totalNumberOfElements)
        {
            double rawDegrees = -(HALFROTATIONDEGREES - (currentRotation + FULLROTATIONDEGREES * ((double)targetIndex / (double)totalNumberOfElements)));

            if (rawDegrees > HALFROTATIONDEGREES)
                return -(FULLROTATIONDEGREES - rawDegrees);
            return rawDegrees;
        }




        private List<PreCalcDisplaySet> GetVisibleDisplaySet()
        {
            // Calculate the ZIndexes for the entire set of items
            List<PreCalcDisplaySet> preDisplaySet = new List<PreCalcDisplaySet>();
            for (int preindex = 0; preindex < Items.Count; preindex++)
            {
                double degrees = FULLROTATIONDEGREES * ((double)preindex / (double)Items.Count) + _currentRotation;

                PreCalcDisplaySet item = new PreCalcDisplaySet();
                item.Index = preindex;
                item.Coefficient = GetCoefficient(degrees);
                item.ZOrder = GetZValue(degrees);
                preDisplaySet.Add(item);
            }

            // Sort highest first
            preDisplaySet.Sort();

            // Clamp to the top N items for display
            preDisplaySet = preDisplaySet.GetRange(0, Math.Min(preDisplaySet.Count, 15));

            return preDisplaySet;
        }




        public void SetElementPositions()
        {
            if (Items.Count == 0)
                return;

            List<PreCalcDisplaySet> preDisplaySet = GetVisibleDisplaySet();


            // Now create quick access dictionary for display items
            Dictionary<CanvasCarouselControlItem, PreCalcDisplaySet> displayItemMap = new Dictionary<CanvasCarouselControlItem, PreCalcDisplaySet>();

            // Figure out z pairings
            bool paired = true;
            int binaryLevel = 0;
            foreach(PreCalcDisplaySet setItem in preDisplaySet)
            {
                CanvasCarouselControlItem indexedContainer = ItemContainerGenerator.ContainerFromIndex(setItem.Index) as CanvasCarouselControlItem;
                if(indexedContainer != null)
                {
                    displayItemMap[indexedContainer] = setItem;
                    setItem.BinaryLevel = binaryLevel;

                    if(paired)
                    {
                        binaryLevel++;
                        paired = false;
                    }
                    else
                        paired = !paired;
                }
            }


            Debug.WriteLine("CURRENTROTATION " + _currentRotation.ToString());
            int itemsVisible = 0;
            for (int index = 0; index < Items.Count; index++)
            {
                CanvasCarouselControlItem contentControl = ItemContainerGenerator.ContainerFromIndex(index) as CanvasCarouselControlItem;
                if (contentControl == null)
                    continue;
                DisplayItemMetaData displayMetaData;
                RehabAsset contrib = contentControl.DataContext as RehabAsset;

                if (contentControl != null)
                {
                    double degrees = FULLROTATIONDEGREES * ((double)index / (double)Items.Count) + _currentRotation;
                    double elementWidthCenter = GetElementCenter(contentControl.Width, contentControl.ActualWidth);
                    double elementHeightCenter = GetElementCenter(contentControl.Height, contentControl.ActualHeight);
                    double x = -X_SCALE * Math.Sin(ConvertToRads(degrees)) - (double.IsNaN(Y_SCALE) ? 0.0 : Y_SCALE / 100.0) * (Math.Cos(ConvertToRads(degrees)) * LookDownOffset);
                    if (OrientationAlignment == OrientationAlignments.Vertical)
                        x = 0;
                    double y = Y_SCALE * Math.Sin(ConvertToRads(degrees)) - (double.IsNaN(X_SCALE) ? 0.0 : X_SCALE / 100.0) * (Math.Cos(ConvertToRads(degrees)) * LookDownOffset);
                    if (OrientationAlignment == OrientationAlignments.Horizontal)
                        y = 0;
                    int zValue = GetZValue(degrees);
                    double scaleX = (1.0 - PerspectiveScale) + PerspectiveScale * GetCoefficient(degrees);
                    double scaleY = (1.0 - PerspectiveScale) + PerspectiveScale * GetCoefficient(degrees);

                    if (!_displayMetaDataCollection.TryGetValue(contentControl, out displayMetaData))
                    {
                        displayMetaData = new DisplayItemMetaData();
                        _displayMetaDataCollection[contentControl] = displayMetaData;
                    }
                    
                    if (zValue < preDisplaySet.Last().ZOrder)
                    {
                        if (!displayMetaData.Behind)
                        {
                            if (contentControl.FillerStoryboard != null)
                                contentControl.FillerStoryboard.SkipToFill();
                        }
                        
                        displayMetaData.Behind = true;
                        contentControl.Opacity = 0;
                        Debug.WriteLine(contrib.Title + " HIDDEN");                                                
                    }
                    else
                    {
                        if (displayMetaData.Behind)
                        {
                            displayMetaData.Behind = false;

                            Debug.WriteLine(contrib.Title + " SHOWN");
                            Canvas.SetTop(contentControl, displayMetaData.Top);
                            Canvas.SetZIndex(contentControl, 1);

                            ScaleTransform scale = contentControl.RenderTransform as ScaleTransform;
                            if (scale == null)
                            {
                                scale = new ScaleTransform();
                                contentControl.RenderTransform = scale;
                            }
                            scale.CenterX = displayMetaData.CenterX;
                            scale.CenterY = displayMetaData.CenterY;
                            scale.ScaleX = displayMetaData.ScaleX;
                            scale.ScaleY = displayMetaData.ScaleY; 
                            contentControl.Opacity = 0.001;
                        }

                  //      scaleX = scalings[displayItemMap[contentControl].BinaryLevel];

                        itemsVisible++;
                        
                        DoubleAnimation canvasTranslateY;
                        DoubleAnimation opacityAnimation;
                        DoubleAnimation scaleYAnimation, scaleXAnimation;
                        ObjectAnimationUsingKeyFrames zindexAnimation;
                        if (contentControl.FillerStoryboard == null)
                        {
                            ScaleTransform scale = contentControl.RenderTransform as ScaleTransform;
                            if (scale == null)
                            {
                                scale = new ScaleTransform();
                                contentControl.RenderTransform = scale;
                            }
                            scale.CenterX = elementWidthCenter;
                            scale.CenterY = elementHeightCenter;

                            contentControl.FillerStoryboard = new Storyboard();
                            _storyboardToItemMap.Add(contentControl.FillerStoryboard, contentControl);
                            contentControl.FillerStoryboard.Completed += FillerStoryboard_Completed;

                            canvasTranslateY = new DoubleAnimation();
                            canvasTranslateY.Duration = TimeSpan.FromMilliseconds(33);
                            Storyboard.SetTargetProperty(canvasTranslateY, new PropertyPath("(Canvas.Top)"));
                            Storyboard.SetTarget(canvasTranslateY, contentControl);
                            contentControl.FillerStoryboard.Children.Add(canvasTranslateY);

                            opacityAnimation = new DoubleAnimation();
                            opacityAnimation.Duration = TimeSpan.FromMilliseconds(33);
                            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
                            Storyboard.SetTarget(opacityAnimation, contentControl);
                            contentControl.FillerStoryboard.Children.Add(opacityAnimation);

                            scaleYAnimation = new DoubleAnimation();
                            scaleYAnimation.Duration = TimeSpan.FromMilliseconds(33);
                            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
                            Storyboard.SetTarget(scaleYAnimation, contentControl);
                            contentControl.FillerStoryboard.Children.Add(scaleYAnimation);

                            scaleXAnimation = new DoubleAnimation();
                            scaleXAnimation.Duration = TimeSpan.FromMilliseconds(33);
                            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
                            Storyboard.SetTarget(scaleXAnimation, contentControl);
                            contentControl.FillerStoryboard.Children.Add(scaleXAnimation);

                            zindexAnimation = new ObjectAnimationUsingKeyFrames();
                            zindexAnimation.Duration = TimeSpan.FromMilliseconds(33);
                            DiscreteObjectKeyFrame keyFrame = new DiscreteObjectKeyFrame();
                            keyFrame.KeyTime = TimeSpan.FromMilliseconds(33);
                            zindexAnimation.KeyFrames.Add(keyFrame);
                            Storyboard.SetTargetProperty(zindexAnimation, new PropertyPath("(Canvas.ZIndex)"));
                            Storyboard.SetTarget(zindexAnimation, contentControl);
                            contentControl.FillerStoryboard.Children.Add(zindexAnimation);
                        }
                        else
                        {
                            contentControl.FillerStoryboard.SkipToFill();
                            canvasTranslateY = contentControl.FillerStoryboard.Children[0] as DoubleAnimation;
                            opacityAnimation = contentControl.FillerStoryboard.Children[1] as DoubleAnimation;
                            scaleYAnimation = contentControl.FillerStoryboard.Children[2] as DoubleAnimation;
                            scaleXAnimation = contentControl.FillerStoryboard.Children[3] as DoubleAnimation;
                            zindexAnimation = contentControl.FillerStoryboard.Children[4] as ObjectAnimationUsingKeyFrames;
                        }

                        canvasTranslateY.To = y + CenterY - elementHeightCenter;
                        opacityAnimation.To = (1.0 - Fade) + Fade * GetCoefficient(degrees);
                        scaleXAnimation.To = scaleX;
                        scaleYAnimation.To = scaleY;
                        zindexAnimation.KeyFrames[0].Value = GetZValue(degrees);
                        contentControl.FillerStoryboard.Begin();
                        _storyboardRunningRefCount++;
                    } // else 180

                    // Cache the current placement settings
                    displayMetaData.Opacity = (1.0 - Fade) + Fade * GetCoefficient(degrees);
                    displayMetaData.Top = y + CenterY - elementHeightCenter;
                    displayMetaData.CenterX = elementWidthCenter;
                    displayMetaData.CenterY = elementHeightCenter;
                    displayMetaData.ScaleX = scaleX;
                    displayMetaData.ScaleY = scaleY;
                } // if (contentControl != null)
            } // for


            Debug.WriteLine(itemsVisible + " ITEMS VISIBLE");

        }



        void FillerStoryboard_Completed(object sender, object e)
        {            
            _storyboardRunningRefCount--;
            if(_storyboardRunningRefCount == 0)
            {
                lock (_animationSnapLock)
                {
                    if (_pendingAnimationSnap)
                    {
                        //App.MainPage.AddVisualConsoleRow("STORYBOARD COMPLETE PendingAnimationSnap");

                        if (SnapToHighestZElement())
                        {
                            //App.MainPage.AddVisualConsoleRow("PendingAnimationSnap ACTIVATED");
                            _animationSnapRunning = true;
                        }
                        _pendingAnimationSnap = false;                        
                    }
                    else if (_animationSnapRunning)
                    {
                        //App.MainPage.AddVisualConsoleRow("AnimationSnap COMPLETED");
                        _animationSnapRunning = false;
                    }
                    else
                    {
                        //App.MainPage.AddVisualConsoleRow("Regular AnimationSnap FINISHED");
                    }
                }
            }
        }



        private const double DEFAULT_LOOKDOWN_OFFSET = 0;
        private const double MINIMUM_LOOKDOWN_OFFSET = -100;
        private const double MAXIMUM_LOOKDOWN_OFFSET = 100;
        private double _lookdownOffset = DEFAULT_LOOKDOWN_OFFSET;
        public double LookDownOffset
        {
            get
            {
                return _lookdownOffset;

            }
            set
            {
                _lookdownOffset = Math.Min(Math.Max(value, MINIMUM_LOOKDOWN_OFFSET), MAXIMUM_LOOKDOWN_OFFSET);
            }
        }


        internal static double GetElementCenter(double elementDimension, double elementActualDimension)
        {
            return double.IsNaN(elementDimension) ? elementActualDimension / 2.0 : elementDimension / 2.0;
        }



        private const double DEFAULT_ROTATION_SPEED = 200;
        private const double MINIMUM_ROTATION_SPEED = 1;
        private const double MAXIMUM_ROTATION_SPEED = 1000;
        private double _rotationSpeed = DEFAULT_ROTATION_SPEED;
        public double RotationSpeed
        {
            get
            {
                //Debug.WriteLine("RotationSpeed Get " + (RotationSpeedOneOff == 0 ? _rotationSpeed : RotationSpeedOneOff).ToString());
                return RotationSpeedOneOff == 0 ? _rotationSpeed : RotationSpeedOneOff;
            }
            set
            {
                _rotationSpeed = Math.Min(Math.Max(value, MINIMUM_ROTATION_SPEED), MAXIMUM_ROTATION_SPEED);
            }
        }



        public double RotationSpeedOneOff { get; set; }



        private const double DEFAULT_SCALE = 0.5;
        private const double MINIMUM_SCALE = 0;
        private const double MAXIMUM_SCALE = 1;
        private double _scale = DEFAULT_SCALE;
        public double Scale
        {
            get
            {
                return _scale;

            }
            set
            {
                _scale = Math.Min(Math.Max(value, MINIMUM_SCALE), MAXIMUM_SCALE);
            }
        }



        /*
        private const double DEFAULT_FADE = 0.5;
        private const double MINIMUM_FADE = 0;
        private const double MAXIMUM_FADE = 1;
        private double _fade = DEFAULT_FADE;
        public double Fade
        {
            get
            {
                return _fade;

            }
            set
            {
                _fade = Math.Min(Math.Max(value, MINIMUM_FADE), MAXIMUM_FADE);
            }
        }
        */


        protected double CenterX 
        { 
            get 
            { 
                return this.ActualWidth / 2.0; 
            } 
        }



        protected double CenterY 
        { 
            get 
            { 
                return this.ActualHeight / 2.0; 
            } 
        }



        private double ConvertToRads(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }



        private double GetScaledSize(double degrees)
        {
            return (1.0 - Scale) + Scale * GetCoefficient(degrees);
        }



        private double GetCoefficient(double degrees)
        {
            return 1.0 - Math.Cos(ConvertToRads(degrees)) / 2 - 0.5;
        }



        private void SetOpacity(FrameworkElement element, double degrees)
        {
            element.Opacity = (1.0 - Fade) + Fade * GetCoefficient(degrees);
        }



        private int GetZValue(double degrees)
        {
            return (int)(36000 * GetCoefficient(degrees));
        }



        protected double ClampDegrees(double rawDegrees)
        {
            if (rawDegrees > FULLROTATIONDEGREES)
                return rawDegrees - FULLROTATIONDEGREES;

            if (rawDegrees < 0)
                return rawDegrees + FULLROTATIONDEGREES;

            return rawDegrees;
        }



        private double RotationAmount
        {
            get
            {
                double v = (_currentTime - _previousTime).TotalSeconds;

                return (_currentTime - _previousTime).TotalSeconds * RotationSpeed; // _rotationSpeed;
            }
        }


#region Properties
        public static readonly DependencyProperty OrientationAlignmentProperty = DependencyProperty.Register("OrientationAlignment", typeof(OrientationAlignments), typeof(CanvasCarouselControlItem), new PropertyMetadata(null));
        public OrientationAlignments OrientationAlignment
        {
            get { return (OrientationAlignments)GetValue(OrientationAlignmentProperty); }
            set { SetValue(OrientationAlignmentProperty, value); }
        }



        public static readonly DependencyProperty PerspectiveScaleProperty = DependencyProperty.Register("PerspectiveScale", typeof(double), typeof(CanvasCarouselControlItem), new PropertyMetadata((double) 5.0));
        public double PerspectiveScale
        {
            get { return (double)GetValue(PerspectiveScaleProperty); }
            set { SetValue(PerspectiveScaleProperty, value); }
        }



        public static readonly DependencyProperty SelectOnRotateProperty = DependencyProperty.Register("SelectOnRotate", typeof(bool), typeof(CanvasCarouselControlItem), new PropertyMetadata(false));
        public bool SelectOnRotate
        {
            get { return (bool)GetValue(SelectOnRotateProperty); }
            set { SetValue(SelectOnRotateProperty, value); }
        }



        public static readonly DependencyProperty FadeProperty = DependencyProperty.Register("Fade", typeof(double), typeof(CanvasCarouselControlItem), new PropertyMetadata((double) 0.5));
        public double Fade
        {
            get { return (double)GetValue(FadeProperty); }
            set { SetValue(FadeProperty, value); }
        }



#endregion



    }
}
