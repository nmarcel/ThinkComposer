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
    /// Interaction logic for ImagePicker.xaml
    /// </summary>
    public partial class ImagePicker : UserControl
    {
        public static readonly DependencyProperty SelectedImageProperty;

        static ImagePicker()
        {
            ImagePicker.SelectedImageProperty = DependencyProperty.Register("SelectedImage", typeof(ImageSource), typeof(ImagePicker),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedImageChanged)));
        }

        public ImagePicker()
        {
            InitializeComponent();

            this.InternalImageLocations = Display.GetAllResourceLocations(res => res.EndsWith(".png")).OrderBy(name => name).ToList();
            this.InternalImagesCombo.ItemsSource = this.InternalImageLocations;
        }

        public ImagePicker(MMemberController Controller)
             : this()
        {
            this.Controller = Controller;
        }

        private IEnumerable<string> InternalImageLocations = null;

        public MMemberController Controller { get; protected set; }

        private void ImageSelect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.IsClicking = true;
        }

        private bool IsClicking = false;

        private void ImageSelect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsClicking)
                return;

            if (sender == this.ImageSelectFile)
                SelectFileImage();
            else
                if (sender == this.ImageSelectInternal)
                    SelectInternalImage();
                else
                    SetEmptyImage();

            this.IsClicking = false;
        }

        public void SelectFileImage()
        {
            var Selection = Display.DialogGetImageFromFile();
            if (Selection == null)
                return;

            this.SelectedImage = Selection;
        }

        public void SelectInternalImage()
        {
            this.InternalImageSelectionInitializing = true;

            /* PENDING: for when ImageAssign is in use
            if (this.InternalImageLocations.Contains(this.SelectedImage.Reference))
                this.InternalImagesCombo.SelectedItem = this.SelectedImage.Reference;
            */

            var ImgBitmap = this.SelectedImage as BitmapImage;
            if (ImgBitmap != null && ImgBitmap.UriSource != null)
                if (this.InternalImageLocations.Contains(ImgBitmap.UriSource.AbsoluteUri))
                    this.InternalImagesCombo.SelectedItem = ImgBitmap.UriSource.AbsoluteUri;

            this.Presenter.SetVisible(false, false);
            this.InternalImagesCombo.SetVisible(true);
            this.InternalImagesCombo.IsDropDownOpen = true;

            this.InternalImageSelectionInitializing = false;
        }

        private bool InternalImageSelectionInitializing = false;

        public void SetEmptyImage()
        {
            this.ImageIsEmpty = !this.ImageIsEmpty;

            if (!this.ImageIsEmpty)
            {
                if (this.SelectedImage == null)
                    this.SelectedImage = this.LastImage;

                return;
            }

            this.SelectedImage = null;
        }

        public bool ImageIsEmpty
        {
            get { return this.ImageIsEmpty_;  }
            protected set
            {
                this.ImageIsEmpty_ = value;
                this.ImageSetEmpty.Opacity = (value ? 0.5 : 1.0);
            }
        }
        private bool ImageIsEmpty_ = true;

        public ImageSource SelectedImage
        {
            get { return (ImageSource)GetValue(ImagePicker.SelectedImageProperty); }
            set { SetValue(ImagePicker.SelectedImageProperty, value); }
        }

        private static void OnSelectedImageChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Picker = (ImagePicker)depobj;
            var Target = Picker.Presenter;

            // Cloned to avoid Exception "The provided DependencyObject is not a context for this Freezable"
            // which happens when selecting again a previously picked image.
            var Picture = (ImageSource)evargs.NewValue;

            Target.Source = Picture;
            Picker.ImageIsEmpty = (Picture == null);

            if (Picture != null)
                Picker.LastImage = Picture;

            // IMPORTANT: Avoids to change the pictogram when not needed (as when generated from a non-stored drawing sample).
            if (!Picker.IsLoaded)
                return;

            // NOTE: It was necessary to explictly set the property value, because the Data-Binding didn't work.
            //       What is wrong???
            if (Picker.Controller != null)
                Picker.Controller.Definition.Write(Picker.Controller.InstanceController.ControlledInstance, Picture);

            // Raise lost-focus in order to propagate data changes
            Picker.RaiseEvent(new RoutedEventArgs(LostFocusEvent));
        }

        private ImageSource LastImage = null;

        private void InternalImagesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.InternalImageSelectionInitializing)
                return;

            if (this.InternalImagesCombo.SelectedItem == null)
                this.SelectedImage = null;
            else
            {
                var NewImage = Display.GetAppImage(this.InternalImagesCombo.SelectedItem as string, false, true);
                this.SelectedImage = NewImage;
            }
        }

        private void InternalImagesCombo_DropDownClosed(object sender, EventArgs e)
        {
            this.Presenter.SetVisible(true);
            this.InternalImagesCombo.SetVisible(false, false);
        }

    }
}
