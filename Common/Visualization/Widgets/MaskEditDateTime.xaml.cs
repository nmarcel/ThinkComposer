using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for MaskEditor.xaml
    /// </summary>
    public partial class MaskEditDateTime : UserControl, IDynamicStoreDataGridEditor
    {
        public static readonly DependencyProperty StorageFieldNameProperty;
        public static readonly DependencyProperty ApplyDirectAccessProperty;

        public static readonly DependencyProperty ValueProperty;
        public static readonly DependencyProperty EditingValueProperty;
        public static readonly DependencyProperty HasDateProperty;
        public static readonly DependencyProperty HasTimeProperty;

        static MaskEditDateTime()
        {
            MaskEditDateTime.StorageFieldNameProperty = DependencyProperty.Register("StorageFieldName", typeof(string), typeof(MaskEditDateTime),
                                                        new FrameworkPropertyMetadata(null));

            MaskEditDateTime.ApplyDirectAccessProperty = DependencyProperty.Register("ApplyDirectAccess", typeof(bool), typeof(MaskEditDateTime),
                                                        new FrameworkPropertyMetadata(false));

            
            MaskEditDateTime.ValueProperty = DependencyProperty.Register("Value", typeof(DateTime), typeof(MaskEditDateTime),
                                             new FrameworkPropertyMetadata(DateTime.Now, new PropertyChangedCallback(OnValueChanged)));

            MaskEditDateTime.EditingValueProperty = DependencyProperty.Register("EditingValue", typeof(DateTime), typeof(MaskEditDateTime),
                                                    new FrameworkPropertyMetadata(General.EMPTY_DATE, new PropertyChangedCallback(OnEditingValueChanged)));

            MaskEditDateTime.HasDateProperty = DependencyProperty.Register("HasDate", typeof(bool), typeof(MaskEditDateTime),
                                               new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnHasDateChanged)));

            MaskEditDateTime.HasTimeProperty = DependencyProperty.Register("HasTime", typeof(bool), typeof(MaskEditDateTime),
                                               new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnHasTimeChanged)));
        }

        public MaskEditDateTime()
        {
            // Change disposition of Date-parts if needed
            string VisualsDateDisposition = "MDY";    // Predefined (XAML visual objects disposition) US Format
            string NewDateDisposition = CultureInfo.CurrentCulture.DateTimeFormat.DatePartsDisposition();

            //T Console.WriteLine("Current Date parts disposition: [{0}]", NewDateDisposition);

            InitializeComponent();

            if (NewDateDisposition == VisualsDateDisposition)
                this.FirstTextBox = this.ValueMonth;
            else
                if (NewDateDisposition == "DMY")
                {
                    this.DatePanel.SwapChildren(0, 2);  // From "MDY" to "DMY"
                    this.FirstTextBox = this.ValueDay;
                }
                else
                {   // Else, (NewDispositoin == "YMD")
                    this.DatePanel.SwapChildren(0, 4);  // From "MDY" to "YDM"
                    this.DatePanel.SwapChildren(2, 4);  // From "YDM" to "YMD"
                    this.FirstTextBox = this.ValueYear;
                }

            // this.PostCall(ed => this.FirstTextBox.Focus());
            this.ExposeValue();
        }
        private TextBox FirstTextBox = null;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.OwnerItemsGridControl = this.GetNearestVisualDominantOfType<ItemsGridControl>();
            this.FocusDirector = FocusManager.GetFocusScope(this);
        }
        protected DependencyObject FocusDirector { get; set; }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            // Resets to original value
            if (e.Key == Key.Escape)
            {
                if (this.PopupCalendar.IsOpen)
                    this.PopupCalendar.IsOpen = false;
                else
                    this.EditingCancelled = true;

                this.EditingValue = this.Value;
                this.ExposeValue();
            }
        }
        protected bool EditingCancelled = false;

        public void ExposeValue(bool UpdateValue = false)
        {
            this.PartsSeparator.SetVisible(this.HasDate && this.HasTime);

            this.DatePanel.SetVisible(this.HasDate);
            this.ValueMonth.Text = this.EditingValue.Month.ToString("00");
            this.ValueDay.Text = this.EditingValue.Day.ToString("00");
            this.ValueYear.Text = this.EditingValue.Year.ToString("0000");

            this.TimePanel.SetVisible(this.HasTime);
            this.ValueHour.Text = this.EditingValue.Hour.ToString("00");
            this.ValueMinute.Text = this.EditingValue.Minute.ToString("00");
            this.ValueSecond.Text = this.EditingValue.Second.ToString("00");

            if (!EditingCancelled && UpdateValue && this.FocusDirector != null)
            {
                var CurrentFocused = FocusManager.GetFocusedElement(this.FocusDirector);

                if (!this.PopupCalendar.IsOpen
                    && !CurrentFocused.IsOneOf(this.ValueMonth, this.ValueDay, this.ValueYear,
                                            this.ValueHour, this.ValueMonth, this.ValueSecond))
                    this.Value = this.EditingValue;
            }
        }

        private void ValueMonth_LostFocus(object sender, RoutedEventArgs e)
        {
            int Number = 0;

            if (Int32.TryParse(this.ValueMonth.Text, out Number))
            {
                Number = Number.EnforceRange(1, 12);

                // Adjustment for leap years
                var NumDay = this.EditingValue.Day;
                if (Number == 2 && NumDay > 28
                    && !(CultureInfo.CurrentCulture.Calendar.IsLeapYear(this.EditingValue.Year)))
                    NumDay = 28;

                NumDay = NumDay.EnforceRange(1, General.DaysOfMonths[Number]);

                this.EditingValue = new DateTime(this.EditingValue.Year, Number, NumDay,
                                          this.EditingValue.Hour, this.EditingValue.Minute, this.EditingValue.Second);
            }
            var x = sender as TextBox;

            this.ExposeValue(true);
        }

        private void ValueDay_LostFocus(object sender, RoutedEventArgs e)
        {
            int Number = 0;

            if (Int32.TryParse(this.ValueDay.Text, out Number))
            {
                Number = Number.EnforceRange(1, (this.EditingValue.Month == 2 && CultureInfo.CurrentCulture.Calendar.IsLeapYear(this.EditingValue.Year)
                                                 ? 29 : General.DaysOfMonths[this.EditingValue.Month]));

                this.EditingValue = new DateTime(this.EditingValue.Year, this.EditingValue.Month, Number,
                                                 this.EditingValue.Hour, this.EditingValue.Minute, this.EditingValue.Second);
            }

            this.ExposeValue(true);
        }

        private void ValueYear_LostFocus(object sender, RoutedEventArgs e)
        {
            int Number = 0;

            if (Int32.TryParse(this.ValueYear.Text, out Number))
            {
                Number = Number.EnforceRange(1, 9999);

                // Adjustment for leap years
                var NumDay = this.EditingValue.Day;
                if (this.EditingValue.Month == 2 && this.EditingValue.Day == 29
                    && !(CultureInfo.CurrentCulture.Calendar.IsLeapYear(Number)))
                    NumDay = 28;

                this.EditingValue = new DateTime(Number, this.EditingValue.Month, NumDay,
                                          this.EditingValue.Hour, this.EditingValue.Minute, this.EditingValue.Second);
            }

            this.ExposeValue(true);
        }

        private void ValueHour_LostFocus(object sender, RoutedEventArgs e)
        {
            int Number = 0;

            if (Int32.TryParse(this.ValueHour.Text, out Number))
            {
                Number = Number.EnforceRange(0, 23);
                this.EditingValue = new DateTime(this.EditingValue.Year, this.EditingValue.Month, this.EditingValue.Day,
                                          Number, this.EditingValue.Minute, this.EditingValue.Second);
            }

            this.ExposeValue(true);
        }

        private void ValueMinute_LostFocus(object sender, RoutedEventArgs e)
        {
            int Number = 0;

            if (Int32.TryParse(this.ValueMinute.Text, out Number))
            {
                Number = Number.EnforceRange(0, 59);
                this.EditingValue = new DateTime(this.EditingValue.Year, this.EditingValue.Month, this.EditingValue.Day,
                                          this.EditingValue.Hour, Number, this.EditingValue.Second);
            }

            this.ExposeValue(true);
        }

        private void ValueSecond_LostFocus(object sender, RoutedEventArgs e)
        {
            int Number = 0;

            if (Int32.TryParse(this.ValueSecond.Text, out Number))
            {
                Number = Number.EnforceRange(0, 59);
                this.EditingValue = new DateTime(this.EditingValue.Year, this.EditingValue.Month, this.EditingValue.Day,
                                          this.EditingValue.Hour, this.EditingValue.Minute, Number);
            }

            this.ExposeValue(true);
        }

        /// <summary>
        /// Items-Grid-Subform owning this control.
        /// Set it to avoid cell editing change while showing Calendar popup.
        /// </summary>
        public ItemsGridControl OwnerItemsGridControl { get; set; }

        private void DatePictogram_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.OwnerItemsGridControl != null)
                this.OwnerItemsGridControl.DisableCellEditEnding = true;

            this.PopupCalendar.IsOpen = true;
        }

        private void DatePartCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            this.PopupCalendar.IsOpen = false;
        }

        private void PopupCalendar_Closed(object sender, EventArgs e)
        {
            if (this.OwnerItemsGridControl != null)
                this.PostCall(ed => ed.OwnerItemsGridControl.DisableCellEditEnding = false);

            this.FirstTextBox.Focus();  // Important to be able to lost-focus and therefore update target.
        }

        public DateTime Value
        {
            get { return (DateTime)GetValue(MaskEditDateTime.ValueProperty); }
            set { SetValue(MaskEditDateTime.ValueProperty, value); }
        }

        public DateTime EditingValue
        {
            get { return (DateTime)GetValue(MaskEditDateTime.EditingValueProperty); }
            set { SetValue(MaskEditDateTime.EditingValueProperty, value); }
        }

        public bool HasDate
        {
            get { return (bool)GetValue(MaskEditDateTime.HasDateProperty); }
            set { SetValue(MaskEditDateTime.EditingValueProperty, value); }
        }

        public bool HasTime
        {
            get { return (bool)GetValue(MaskEditDateTime.HasTimeProperty); }
            set { SetValue(MaskEditDateTime.EditingValueProperty, value); }
        }

        /// <summary>
        /// Action to be called just after the value has been edited.
        /// </summary>
        public Action<DateTime> EditingAction = null;

        private static void OnValueChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            MaskEditDateTime Target = depobj as MaskEditDateTime;
            DateTime NewValue = (DateTime)evargs.NewValue;

            // This only occurs for initialization
            if (!Target.IsInitialized /* || Target.EditingValue != General.EMPTY_DATE */)
                return;

            Target.EditingValue = NewValue;

            if (Target.EditingAction != null)
                Target.EditingAction(NewValue);

            if (Target.ApplyDirectAccess && NewValue != General.Try(() => Convert.ToDateTime(Target.PerformDirectRead(Target.StorageFieldName)), DateTime.MaxValue))
                Target.PerformDirectWrite(Target.StorageFieldName, NewValue);
        }

        private static void OnEditingValueChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as MaskEditDateTime;

            if (!Target.IsInitialized)
                return;

            Target.ExposeValue();
        }

        private static void OnHasDateChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            MaskEditDateTime Target = depobj as MaskEditDateTime;
            bool NewValue = (bool)evargs.NewValue;

            if (!Target.IsInitialized)
                return;

            if (!NewValue && !Target.HasTime)
                Target.HasTime = true;  // Avoids show nothing

            Target.ExposeValue();
        }

        private static void OnHasTimeChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            MaskEditDateTime Target = depobj as MaskEditDateTime;
            bool NewValue = (bool)evargs.NewValue;

            if (!Target.IsInitialized)
                return;

            if (!NewValue && !Target.HasDate)
                Target.HasDate = true;

            Target.ExposeValue();
        }

        public string DateSeparator { get { return CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator; } }

        public string TimeSeparator { get { return CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator; } }

        public string StorageFieldName
        {
            get { return (string)GetValue(MaskEditDateTime.StorageFieldNameProperty); }
            set { SetValue(MaskEditDateTime.StorageFieldNameProperty, value); }
        }

        public bool ApplyDirectAccess
        {
            get { return (bool)GetValue(MaskEditDateTime.ApplyDirectAccessProperty); }
            set { SetValue(MaskEditDateTime.ApplyDirectAccessProperty, value); }
        }
    }
}
