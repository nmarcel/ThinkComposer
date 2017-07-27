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
// File   : SimpleAdorner.cs
// Object : Instrumind.Common.Visualization.SimpleAdorner (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.02.05 Néstor Sánchez A.  Creation
//

using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

using Instrumind.Common;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Basic visual adorner.
    /// </summary>
    public class SimpleAdorner : Adorner
    {
        public SimpleAdorner(UIElement Target, Drawing ExposedDrawing) : base(Target)
        {
            this.Target = Target;
            this.ExposedDrawing = ExposedDrawing;
        }

        protected override void OnRender(DrawingContext Context)
        {
            base.OnRender(Context);

            if (this.ExposedDrawing != null)
                Context.DrawDrawing(this.ExposedDrawing);
        }

        public UIElement Target { get; protected set; }
        public Drawing ExposedDrawing { get; protected set; }
    }

    //public class ImprovedAdornerLayer : AdornerLayer
    //{
    //    public ImprovedAdornerLayer() : base()
    //    {

    //    }
    //}
}