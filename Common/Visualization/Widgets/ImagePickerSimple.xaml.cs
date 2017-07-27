using System;
using System.Collections.Generic;
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

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for ImagePickerSimple.xaml
    /// </summary>
    public partial class ImagePickerSimple : UserControl
    {
        public static readonly DependencyProperty SelectedImageProperty;

        public static readonly DependencyProperty ImagePickerActionFieldNameProperty;
        public static readonly DependencyProperty ImagePickerActionFieldSourceProperty;
        public static readonly DependencyProperty ImagePickerSelectActionProperty;

        static ImagePickerSimple()
        {
            ImagePickerSimple.SelectedImageProperty = DependencyProperty.Register("SelectedImage", typeof(ImageSource), typeof(ImagePickerSimple),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedImageChanged)));

            ImagePickerSimple.ImagePickerActionFieldNameProperty = DependencyProperty.Register("ImagePickerActionFieldName", typeof(string), typeof(ImagePickerSimple),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnImagePickerActionFieldNameChanged)));

            ImagePickerSimple.ImagePickerActionFieldSourceProperty = DependencyProperty.Register("ImagePickerActionFieldSource", typeof(object), typeof(ImagePickerSimple),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnImagePickerActionFieldSourceChanged)));

            // Action arguments: Field-Name and Field-Source
            ImagePickerSimple.ImagePickerSelectActionProperty = DependencyProperty.Register("ImagePickerSelectAction", typeof(Action<string, object, object>), typeof(ImagePickerSimple),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnImagePickerSelectActionChanged)));
        }

        public ImagePickerSimple()
        {
            InitializeComponent();
        }

        public ImagePickerSimple(MMemberController Controller)
             : this()
        {
            this.Controller = Controller;

            this.ImagePickerActionFieldName = null;
            this.ImagePickerActionFieldSource = null;
            this.ImagePickerSelectAction = null;
        }

        public MMemberController Controller { get; protected set; }

        public ImageSource SelectedImage
        {
            get { return (ImageSource)GetValue(ImagePickerSimple.SelectedImageProperty); }
            set { SetValue(ImagePickerSimple.SelectedImageProperty, value); }
        }

        public string ImagePickerActionFieldName
        {
            get { return (string)GetValue(ImagePickerActionFieldNameProperty); }
            set { SetValue(ImagePickerActionFieldNameProperty, value); }
        }

        public object ImagePickerActionFieldSource
        {
            get { return GetValue(ImagePickerActionFieldSourceProperty); }
            set { SetValue(ImagePickerActionFieldSourceProperty, value); }
        }

        // Action arguments: Field-Name, Field-Source and this control
        public Action<string, object, object> ImagePickerSelectAction
        {
            get { return (Action<string, object, object>)GetValue(ImagePickerSelectActionProperty); }
            set { SetValue(ImagePickerSelectActionProperty, value); }
        }

        private static void OnSelectedImageChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Picker = (ImagePickerSimple)depobj;
            var Target = Picker.Presenter;

            // Cloned to avoid Exception "The provided DependencyObject is not a context for this Freezable"
            // which happens when selecting again a previously picked image.
            var Picture = (ImageSource)evargs.NewValue;

            Target.Source = Picture;

            // NOTE: It was necessary to explictly set the property value, because the Data-Binding didn't work.
            //       What is wrong???
            if (Picker.Controller != null)
                Picker.Controller.Definition.Write(Picker.Controller.InstanceController.ControlledInstance, Picture);

            // Raise lost-focus in order to propagate data changes
            //? Picker.RaiseEvent(new RoutedEventArgs(LostFocusEvent));
        }

        private static void OnImagePickerActionFieldNameChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            ImagePickerSimple tb = depobj as ImagePickerSimple;
            tb.ImagePickerActionFieldName = evargs.NewValue as string;
        }

        private static void OnImagePickerActionFieldSourceChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            ImagePickerSimple tb = depobj as ImagePickerSimple;
            tb.ImagePickerActionFieldSource = evargs.NewValue;
        }

        private static void OnImagePickerSelectActionChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            ImagePickerSimple tb = depobj as ImagePickerSimple;
            tb.ImagePickerSelectAction = (Action<string, object, object>)evargs.NewValue;
        }


        private void Presenter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            /*- var Selection = Display.DialogGetImageFromFile();
            if (Selection == null)
                return;

            this.SelectedImage = Selection; */

            if (this.ImagePickerSelectAction != null)
                this.ImagePickerSelectAction(this.ImagePickerActionFieldName, this.ImagePickerActionFieldSource, this);
        }
    }
}
