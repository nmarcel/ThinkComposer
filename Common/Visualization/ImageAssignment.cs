// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) Néstor Marcel Sánchez Ahumada. Santiago, Chile.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : ImageAssignment.cs
// Object : Instrumind.Common.Visualization.ImageAssignment (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.01.06 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Provides reference or storage of predefined images or custom ones, respectively.
    /// </summary>
    [Serializable]
    public class ImageAssignment : INotifyPropertyChanged
    {
        /// <summary>
        /// Image either actually stored or generated from reference.
        /// </summary>
        public ImageSource Image
        {
            get
            {
                if (this.Image_ != null && this.Image_.Value != null)
                    return this.Image_.Value;

                if (this.Reference.IsAbsent())
                    return null;

                var GeneratedImage = Display.CachedImages.GetOrAdd(this.Reference, new BitmapImage(new Uri(this.Reference)));
                return GeneratedImage;
            }
            set
            {
                var ImageBitmap = value as BitmapImage;
                if (ImageBitmap != null && ImageBitmap.UriSource != null)
                {
                    if (!Display.CachedImages.ContainsKey(ImageBitmap.UriSource.AbsoluteUri))
                        Display.CachedImages.TryAdd(ImageBitmap.UriSource.AbsoluteUri, value);

                    // Empties Store_ and Notifies changes
                    this.Reference = ImageBitmap.UriSource.AbsoluteUri;

                    return;
                }
                // Console.WriteLine("Cannot assign image of type '{0}'.", value.GetType());

                this.Reference_ = null;
                this.Image_ = value.Store();

                var Handler = this.PropertyChanged;
                if (Handler != null)
                    Handler(this, new PropertyChangedEventArgs("Image"));
            }
        }

        /// <summary>
        /// Contains a custom image.
        /// </summary>
        private StoreBox<ImageSource> Image_ = null;

        /// <summary>
        /// Points to a predefined image.
        /// </summary>
        public string Reference
        {
            protected get { return this.Reference_;  }
            set
            {
                this.Image_ = null;
                this.Reference_ = value;

                var Handler = this.PropertyChanged;
                if (Handler != null)
                    Handler(this, new PropertyChangedEventArgs("Image"));
            }
        }
        private string Reference_ = null;

        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}