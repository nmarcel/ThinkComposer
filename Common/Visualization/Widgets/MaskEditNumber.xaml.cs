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
using System.Windows.Controls.Primitives;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for MaskEditNumber.xaml
    /// </summary>
    public partial class MaskEditNumber : UserControl, IDynamicStoreDataGridEditor
    {
        public static readonly DependencyProperty StorageFieldNameProperty;
        public static readonly DependencyProperty ApplyDirectAccessProperty;

        public static readonly DependencyProperty ValueProperty;
        public static readonly DependencyProperty EditingValueProperty;
        public static readonly DependencyProperty FormatProperty;
        public static readonly DependencyProperty IntegerDigitsProperty;
        public static readonly DependencyProperty DecimalDigitsProperty;
        public static readonly DependencyProperty MinLimitProperty;
        public static readonly DependencyProperty MaxLimitProperty;

        public static readonly DependencyProperty ValuesSourceProperty;
        public static readonly DependencyProperty ValuesSourceMemberPathProperty;
        public static readonly DependencyProperty ValuesSourceNumericConverterProperty;

        static MaskEditNumber()
        {
            MaskEditNumber.StorageFieldNameProperty = DependencyProperty.Register("StorageFieldName", typeof(string), typeof(MaskEditNumber),
                                                      new FrameworkPropertyMetadata(null));

            MaskEditNumber.ApplyDirectAccessProperty = DependencyProperty.Register("ApplyDirectAccess", typeof(bool), typeof(MaskEditNumber),
                                                      new FrameworkPropertyMetadata(false));


            MaskEditNumber.ValueProperty = DependencyProperty.Register("Value", typeof(decimal), typeof(MaskEditNumber),
                                           new FrameworkPropertyMetadata(0m, new PropertyChangedCallback(OnValueChanged)));

            MaskEditNumber.EditingValueProperty = DependencyProperty.Register("EditingValue", typeof(decimal), typeof(MaskEditNumber),
                                                  new FrameworkPropertyMetadata(0m, new PropertyChangedCallback(OnEditingValueChanged)));

            MaskEditNumber.FormatProperty = DependencyProperty.Register("Format", typeof(string), typeof(MaskEditNumber),
                                            new FrameworkPropertyMetadata("N", new PropertyChangedCallback(OnFormatChanged)));

            MaskEditNumber.IntegerDigitsProperty = DependencyProperty.Register("IntegerDigits", typeof(byte), typeof(MaskEditNumber),
                                                   new FrameworkPropertyMetadata((byte)10, new PropertyChangedCallback(OnIntegerDigitsChanged)));

            MaskEditNumber.DecimalDigitsProperty = DependencyProperty.Register("DecimalDigits", typeof(byte), typeof(MaskEditNumber),
                                                   new FrameworkPropertyMetadata((byte)4, new PropertyChangedCallback(OnDecimalDigitsChanged)));

            MaskEditNumber.MinLimitProperty = DependencyProperty.Register("MinLimit", typeof(decimal), typeof(MaskEditNumber),
                                                   new FrameworkPropertyMetadata(decimal.MinValue, new PropertyChangedCallback(OnMinLimitChanged)));

            MaskEditNumber.MaxLimitProperty = DependencyProperty.Register("MaxLimit", typeof(decimal), typeof(MaskEditNumber),
                                                   new FrameworkPropertyMetadata(decimal.MaxValue, new PropertyChangedCallback(OnMaxLimitChanged)));

            MaskEditNumber.ValuesSourceProperty = DependencyProperty.Register("ValuesSource", typeof(IEnumerable<object>), typeof(MaskEditNumber),
                                                   new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValuesSourceChanged)));

            MaskEditNumber.ValuesSourceMemberPathProperty = DependencyProperty.Register("ValuesSourceMemberPath", typeof(string), typeof(MaskEditNumber),
                                                   new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnValuesSourceMemberPathChanged)));

            MaskEditNumber.ValuesSourceNumericConverterProperty = DependencyProperty.Register("ValuesSourceNumericConverter", typeof(IValueConverter), typeof(MaskEditNumber),
                                                                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValuesSourceNumericConverterChanged)));
        }

        public Window ParentWindow { get; protected set; }

        public MaskEditNumber()
        {
            InitializeComponent();

            ListActioner.SetVisible(false);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // When exposed inside a pop-up, maybe there is no parent Window, therefore use the main.
            this.ParentWindow = this.GetNearestVisualDominantOfType<Window>().NullDefault(Application.Current.MainWindow);

            this.ParentWindow.KeyDown += WhenKeyPressed;

            // this.PostCall(ed => ed.Focus());
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ParentWindow.KeyDown -= WhenKeyPressed;
            }
            catch (Exception Problem)
            {
                // this happens when the Loaded event was never fired, hence no event handler was attached nor parent-window was populated.
            }
        }

        private void WhenKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.ResolveListActionerHide();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateAndExpose();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.EditingValue = this.Value;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.ResolveListActionerShow();

            this.Editor.SelectAll();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.Value = this.EditingValue;

            this.ResolveListActionerHide();
        }

        public void ValidateAndExpose()
        {
            if (this.IsValidating)
                return;

            this.IsValidating = true;

            var NumberText = this.Editor.Text.Trim();
            if (NumberText == string.Empty)
                NumberText = "0";

            //T Console.WriteLine("NumberText=[{0}]", NumberText);
            var NumGroupSep = CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator;
            var NumDecsSep = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;

            // Remove group separator to avoid interpret numbers such "8.7" as "87".
            NumberText = NumberText.Remove(NumGroupSep);

            decimal Number;
            byte DecDigsInput = 0;
            bool Parsed = decimal.TryParse(NumberText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out Number);

            if (!Parsed)
                this.EditingValue = this.Value;
            else
                if (Number.GetIntegerDigits() > this.IntegerDigits)
                    this.EditingValue = decimal.Parse(Math.Truncate(Number).ToString(CultureInfo.InvariantCulture).GetLeft(this.IntegerDigits),
                                                      CultureInfo.InvariantCulture);
                else
                    if ((DecDigsInput = Number.GetDecimalDigits()) > this.DecimalDigits)
                        this.EditingValue = decimal.Parse(Math.Truncate(Number).ToString(CultureInfo.InvariantCulture) + "." +
                                                          (Number - Math.Truncate(Number)).ToString(CultureInfo.InvariantCulture).Replace("0.", "").GetLeft(this.DecimalDigits),
                                                          CultureInfo.InvariantCulture);
                    else
                        if (Number < this.MinLimit)
                            this.EditingValue = this.MinLimit;
                        else
                            if (Number > this.MaxLimit)
                                this.EditingValue = this.MaxLimit;
                            else
                                this.EditingValue = Number;

            bool DigitinsDecimalSeparator = (DecDigsInput == 0 && this.DecimalDigits > 0
                                             && NumberText.EndsWith(NumDecsSep));

            // Do not use format on editing (would put group separators).
            this.Editor.Text = this.EditingValue.ToString(CultureInfo.InvariantCulture.NumberFormat) + (DigitinsDecimalSeparator ? NumDecsSep : "");

            this.Editor.Select(this.Editor.Text.Length, 1);

            this.IsValidating = false;
        }
        bool IsValidating = false;

        public decimal Value
        {
            get { return (decimal)GetValue(MaskEditNumber.ValueProperty); }
            set { SetValue(MaskEditNumber.ValueProperty, value); }
        }

        public decimal EditingValue
        {
            get { return (decimal)GetValue(MaskEditNumber.EditingValueProperty); }
            set { SetValue(MaskEditNumber.EditingValueProperty, value); }
        }

        public string Format
        {
            get { return (string)GetValue(MaskEditNumber.FormatProperty); }
            set { SetValue(MaskEditNumber.FormatProperty, value); }
        }

        public byte IntegerDigits
        {
            get { return (byte)GetValue(MaskEditNumber.IntegerDigitsProperty); }
            set { SetValue(MaskEditNumber.IntegerDigitsProperty, value); }
        }

        public byte DecimalDigits
        {
            get { return (byte)GetValue(MaskEditNumber.DecimalDigitsProperty); }
            set { SetValue(MaskEditNumber.DecimalDigitsProperty, value); }
        }

        public decimal MinLimit
        {
            get { return (decimal)GetValue(MaskEditNumber.MinLimitProperty); }
            set { SetValue(MaskEditNumber.MinLimitProperty, value); }
        }

        public decimal MaxLimit
        {
            get { return (decimal)GetValue(MaskEditNumber.MaxLimitProperty); }
            set { SetValue(MaskEditNumber.MaxLimitProperty, value); }
        }

        /// <summary>
        /// Action to be called just after the value has been edited.
        /// </summary>
        public Action<decimal> EditingAction = null;

        private static void OnValueChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            // IMPORTANT: Do not accept null values, because it can fall in
            // an event infinite-loop chain when selected thru popup listbox.
            if (evargs.NewValue == null)   // Drop-down cancelled?
                return;

            MaskEditNumber Target = depobj as MaskEditNumber;
            decimal NewValue = (decimal)evargs.NewValue;

            Target.EditingValue = NewValue;

            if (Target.EditingAction != null)
                Target.EditingAction(NewValue);

            if (Target.ApplyDirectAccess && NewValue != General.Try(() => Convert.ToDecimal(Target.PerformDirectRead(Target.StorageFieldName)), decimal.MaxValue))
                Target.PerformDirectWrite(Target.StorageFieldName, NewValue);
        }

        private static void OnEditingValueChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            // IMPORTANT: Do not accept null values, because it can fall in
            // an event infinite-loop chain when selected thru popup listbox.
            if (evargs.NewValue == null)   // Drop-down cancelled?
                return;

            MaskEditNumber Target = depobj as MaskEditNumber;
            decimal NewValue = (decimal)evargs.NewValue;

            var NewText = NewValue.ToString(CultureInfo.InvariantCulture.NumberFormat);

            if (NewText != Target.Editor.Text)
            {
                Target.Editor.Text = NewText;
                Target.Editor.SelectAll();
            }
        }

        private static void OnFormatChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            MaskEditNumber Target = depobj as MaskEditNumber;
            string NewValue = (string)evargs.NewValue;  // Do not use format on editing (would put group separators).

            Target.ValidateAndExpose();
        }

        private static void OnIntegerDigitsChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            MaskEditNumber Target = depobj as MaskEditNumber;
            byte NewValue = (byte)evargs.NewValue;

            Target.ValidateAndExpose();
        }

        private static void OnDecimalDigitsChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            MaskEditNumber Target = depobj as MaskEditNumber;
            byte NewValue = (byte)evargs.NewValue;

            Target.ValidateAndExpose();
        }

        private static void OnMinLimitChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            MaskEditNumber Target = depobj as MaskEditNumber;
            decimal NewValue = (decimal)evargs.NewValue;

            Target.ValidateAndExpose();
        }

        private static void OnMaxLimitChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            MaskEditNumber Target = depobj as MaskEditNumber;
            decimal NewValue = (decimal)evargs.NewValue;

            Target.ValidateAndExpose();
        }

        public string StorageFieldName
        {
            get { return (string)GetValue(MaskEditNumber.StorageFieldNameProperty); }
            set { SetValue(MaskEditNumber.StorageFieldNameProperty, value); }
        }

        public bool ApplyDirectAccess
        {
            get { return (bool)GetValue(MaskEditNumber.ApplyDirectAccessProperty); }
            set { SetValue(MaskEditNumber.ApplyDirectAccessProperty, value); }
        }

        public static void OnValuesSourceChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as MaskEditNumber;

            if (evargs.NewValue is IEnumerable<object>)
                Target.ValuesSource = (IEnumerable<object>)evargs.NewValue;
        }

        public IEnumerable<object> ValuesSource
        {
            get { return (IEnumerable<object>)GetValue(MaskEditNumber.ValuesSourceProperty); }
            set { SetValue(MaskEditNumber.ValuesSourceProperty, value); }
        }

        private static void OnValuesSourceMemberPathChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as MaskEditNumber;
            var NewValue = (string)evargs.NewValue;

            Target.ValuesSourceMemberPath = NewValue;
        }
        public string ValuesSourceMemberPath
        {
            get { return (string)GetValue(MaskEditNumber.ValuesSourceMemberPathProperty); }
            set { SetValue(MaskEditNumber.ValuesSourceMemberPathProperty, value); }
        }

        public static void OnValuesSourceNumericConverterChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as MaskEditNumber;
            var NewConverter = (IValueConverter)evargs.NewValue;

            Target.ValuesSourceNumericConverter = NewConverter;
        }

        public IValueConverter ValuesSourceNumericConverter
        {
            get { return (IValueConverter)GetValue(MaskEditNumber.ValuesSourceNumericConverterProperty); }
            set { SetValue(MaskEditNumber.ValuesSourceNumericConverterProperty, value); }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            this.ResolveListActionerShow();
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //No! (clumsy) ResolveListActionerHide();
        }

        private void ResolveListActionerShow()
        {
            if (this.ValuesSource == null || !this.ValuesSource.Any())
                return;

            this.ListActioner.SetVisible(true);
        }

        private void ResolveListActionerHide()
        {
            if (PopupSelectorList.IsMouseOver || this.IsMouseOver)
                return;

            this.ListActioner.SetVisible(false);
            if (this.PopupSelectorList.IsOpen)
                this.PopupSelectorList.IsOpen = false;
        }

        private void ListActioner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ParentWindow.Cursor = Cursors.Wait;
            this.PopupSelectorList.IsOpen = !this.PopupSelectorList.IsOpen;
            this.ParentWindow.Cursor = Cursors.Arrow;
        }

        private void LbxSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsInitialized || e.AddedItems == null || e.AddedItems.Count < 1 || e.AddedItems[0] == null
                || this.ValuesSource == null || this.ValuesSourceMemberPath.IsAbsent())
                return;

            //T Console.WriteLine("Selected: " + e.AddedItems[0].ToStringAlways());

            var Number = (decimal)this.ValuesSourceNumericConverter.ConvertBack(e.AddedItems[0], typeof(decimal), null, CultureInfo.CurrentCulture);
            this.PostCall(edt => edt.Value = Number);

            if (this.PopupSelectorList.IsOpen)
                this.PopupSelectorList.IsOpen = false;
        }
    }
}
