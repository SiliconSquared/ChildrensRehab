using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RehabKiosk.Controls
{
    /// <summary>
    /// Interaction logic for EditableTextBox.xaml
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {
        public class EditEnabledModeChangeEvent
        {
            public enum EventTypes { ACTIVATE, DEACTIVATE };

            public bool NameChanged { get; set; }

            public EventTypes EventType { get; set; }
        }



        public EditableTextBlock()
        {
            InitializeComponent();
            base.Focusable = true;
            base.FocusVisualStyle = null;
        }



        // We keep the old text when we go into editmode
        // in case the user aborts with the escape key
        private string oldText;


        public static readonly DependencyProperty EditModeChangeNotificationCommandProperty = DependencyProperty.Register("EditModeChangeNotificationCommand", typeof(ICommand), typeof(EditableTextBlock), new PropertyMetadata(null));
        public ICommand EditModeChangeNotificationCommand
        {
            get { return (ICommand)GetValue(EditModeChangeNotificationCommandProperty); }
            set { SetValue(EditModeChangeNotificationCommandProperty, value); }
        }

        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register(
            "MaxLength",
            typeof(int),
            typeof(EditableTextBlock),
            new PropertyMetadata(20));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(EditableTextBlock),
            new PropertyMetadata(""));

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register(
            "IsEditable",
            typeof(bool),
            typeof(EditableTextBlock),
            new PropertyMetadata(true));

        public bool IsInEditMode
        {
            get
            {
                if (IsEditable)
                    return (bool)GetValue(IsInEditModeProperty);
                else
                    return false;
            }
            set
            {
                SetValue(IsInEditModeProperty, value);
            }
        }
        public static readonly DependencyProperty IsInEditModeProperty =
            DependencyProperty.Register(
            "IsInEditMode",
            typeof(bool),
            typeof(EditableTextBlock),
            new PropertyMetadata(false, OnIsInEditModePropertyChanged));




        private static void OnIsInEditModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d as EditableTextBlock).IsEditable)
            {
                if ((bool)e.NewValue)
                {
                    (d as EditableTextBlock).oldText = (d as EditableTextBlock).Text;
                    (d as EditableTextBlock).EditModeChangeNotificationCommand?.Execute(new EditEnabledModeChangeEvent() { EventType = EditEnabledModeChangeEvent.EventTypes.ACTIVATE });
                }
                else if ((bool)e.NewValue == false && (bool)e.OldValue == true)
                {
                    (d as EditableTextBlock).EditModeChangeNotificationCommand?.Execute(new EditEnabledModeChangeEvent() { 
                                                                            EventType = EditEnabledModeChangeEvent.EventTypes.DEACTIVATE,
                                                                            NameChanged = (d as EditableTextBlock).oldText.CompareTo((d as EditableTextBlock).Text) != 0 });
                }
            }
        }



        public string TextFormat
        {
            get { return (string)GetValue(TextFormatProperty); }
            set
            {
                if (value == "") value = "{0}";
                SetValue(TextFormatProperty, value);
            }
        }
        public static readonly DependencyProperty TextFormatProperty =
            DependencyProperty.Register(
            "TextFormat",
            typeof(string),
            typeof(EditableTextBlock),
            new PropertyMetadata("{0}"));

        public string FormattedText
        {
            get { return String.Format(TextFormat, Text); }
        }



        // Invoked when we enter edit mode.
        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;

            // Give the TextBox input focus
            txt.Focus();

            txt.SelectAll();
        }

        // Invoked when we exit edit mode.
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.IsInEditMode = false;
        }

        // Invoked when the user edits the annotation.
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.IsInEditMode = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                this.IsInEditMode = false;
                Text = oldText;
                e.Handled = true;
            }
        }
        
    }
}
