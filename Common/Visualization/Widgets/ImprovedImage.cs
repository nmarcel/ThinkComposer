using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Grayable image.
    /// </summary>
    public class ImprovedImage : Image
    {
        static ImprovedImage()
        {
            IsEnabledProperty.OverrideMetadata(typeof(ImprovedImage),
                                               new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnAutoGrayScaleImageIsEnabledPropertyChanged)));
        }

        private static void OnAutoGrayScaleImageIsEnabledPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var autoGrayScaleImg = source as ImprovedImage;
            var isEnable = Convert.ToBoolean(args.NewValue);

            if (autoGrayScaleImg != null)
            {
                if (!isEnable)
                {
                    var bitmapImage = new BitmapImage(new Uri(autoGrayScaleImg.Source.ToString()));
                    autoGrayScaleImg.Source = new FormatConvertedBitmap(bitmapImage, PixelFormats.Gray32Float, null, 0);
                    autoGrayScaleImg.OpacityMask = new ImageBrush(bitmapImage);
                }
                else
                {
                    autoGrayScaleImg.Source = ((FormatConvertedBitmap)autoGrayScaleImg.Source).Source;
                    autoGrayScaleImg.OpacityMask = null;
                }
            }
        }
    }
}
