// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : PointingAssistant.cs
// Object : Instrumind.Common.Visualization.PointingAssistant (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.13 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Helper for mouse-pointing operations.
    /// </summary>
    public static class PointingAssistant
    {
        public static bool IsPointerDisplayed { get; private set; }

        private static IDocumentView TargetContext = null;
        private static FrameworkElement TargetFrameworkPresenter = null;
        private static AdornerLayer TargetAdornerLayer = null;
        private static PointerAdorner VisualPointer = null;
        private static Cursor PreviousCursor = null;

        /// <summary>
        /// Starts a new pointing operation over the specified context control with the supplied decorator,
        /// plus indicating whether the decorator should be put closer of the specified cursor.
        /// </summary>
        public static void Start(IDocumentView Context, Drawing PointingDecorator, bool DecoratorCloser = false, Cursor PointingCursor = null)
        {
            if (TargetContext != null)
                Finish();

            TargetContext = Context;    // This works (but it should be determined the context container Window): Application.Current.MainWindow.Content as FrameworkElement;
            // this didn't work: TargetFrameworkPresenter = TargetContext.PresenterControl.GetNearestVisualDominantOfType<Window>();
            TargetFrameworkPresenter = TargetContext.PresenterControl;

            PreviousCursor = TargetFrameworkPresenter.Cursor;
            TargetFrameworkPresenter.Cursor = PointingCursor ?? Cursors.Arrow;
            TargetAdornerLayer = TargetContext.PresenterLayer;

            VisualPointer = new PointerAdorner(TargetContext, TargetFrameworkPresenter, PointingDecorator, DecoratorCloser);

            TargetFrameworkPresenter.MouseEnter += Context_MouseEnter;
            TargetFrameworkPresenter.MouseLeave += Context_MouseLeave;
            TargetFrameworkPresenter.MouseMove += Context_MouseMove;

            ShowPointer();
        }

        public static void PutConnector(Drawing NewConnector)
        {
            if (VisualPointer == null)
                return;

            VisualPointer.CurrentConnectingPointer = NewConnector;
        }

        public static void ClearConnector()
        {
            if (VisualPointer == null)
                return;

            VisualPointer.CurrentConnectingPointer = null;
        }

        public static void Finish()
        {
            if (TargetContext == null)
                return; //Already finished.

            HidePointer();

            TargetFrameworkPresenter.MouseMove -= Context_MouseMove;
            TargetFrameworkPresenter.MouseLeave -= Context_MouseLeave;
            TargetFrameworkPresenter.MouseEnter -= Context_MouseEnter;

            TargetFrameworkPresenter.Cursor = PreviousCursor;
            VisualPointer = null;
            TargetAdornerLayer = null;
            PreviousCursor = null;
            TargetContext = null;
        }

        static void Context_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowPointer();
        }

        static void Context_MouseLeave(object sender, MouseEventArgs e)
        {
            HidePointer();
        }

        static void ShowPointer()
        {
            if (IsPointerDisplayed || TargetAdornerLayer == null)
                return;

            TargetAdornerLayer.Add(VisualPointer);
            var Position = Mouse.GetPosition(TargetFrameworkPresenter);
            VisualPointer.RenderAt(Position.X, Position.Y);

            IsPointerDisplayed = true;
        }

        static void HidePointer()
        {
            if (!IsPointerDisplayed)
                return;

            TargetAdornerLayer.Remove(VisualPointer);
            IsPointerDisplayed = false;
        }

        static void Context_MouseMove(object sender, MouseEventArgs e)
        {
            if (e == null || TargetContext == null || TargetFrameworkPresenter == null)
                return;

            var Position = e.GetPosition(TargetFrameworkPresenter);
            VisualPointer.RenderAt(Position.X, Position.Y);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public class PointerAdorner : Adorner
        {
            public Drawing ExposedPointer { get; protected set; }

            IDocumentView Target = null;

            internal Drawing CurrentConnectingPointer = null;

            bool DecorateCloser = false;

            public PointerAdorner(IDocumentView Target, UIElement AdornedTarget, Drawing ExposedPointer, bool DecorateCloser)
                 : base(AdornedTarget)
            {
                this.IsHitTestVisible = false;
                this.Target = Target;
                this.ExposedPointer = ExposedPointer;
                this.DecorateCloser = DecorateCloser;
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                var Position = e.GetPosition(this.Target.PresenterControl);
                this.RenderAt(Position.X, Position.Y);
            }

            public void RenderAt(double X, double Y)
            {
                if (LastX == Y && LastY == Y)
                    return;

                LastX = X;
                LastY = Y;

                this.InvalidateVisual();
            }

            private double LastX = 0;
            private double LastY = 0;
            protected override void OnRender(DrawingContext Destination)
            {
                DrawingGroup Drawer = new DrawingGroup();
                Drawer.Children.Add(this.ExposedPointer);

                var PosX = LastX + (this.DecorateCloser ? 2 : 12); ;
                var PosY = LastY + (this.DecorateCloser ? -4 : 12);
                Drawer.Transform = new TranslateTransform(PosX, PosY);

                if (CurrentConnectingPointer != null)
                    Destination.DrawDrawing(CurrentConnectingPointer);

                Destination.DrawDrawing(Drawer);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}