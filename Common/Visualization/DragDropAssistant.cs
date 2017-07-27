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
// File   : DragDropAssistant.cs
// Object : Instrumind.Common.Visualization.DragDropAssistant (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.13 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Instrumind.Common;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
   /// <summary>
    /// Helper for the drag & drop operations.
   /// </summary>
   public static class DragDropAssistant
   {
        private static FrameworkElement RegContext = null;
        private static Point PreviousPosition = new Point(-1, -1);
        private static Popup Announce = new Popup();

        public static string MessageText { get; private set; }
        public static FrameworkElement SourceControl { get; private set; }
        public static object TransferredData { get; private set; }
        public static Func<FrameworkElement, FrameworkElement, bool> AcceptableTargetCondition { get; private set; }
        public static bool RequiresOneMatchingTarget;
        public static Action<FrameworkElement, FrameworkElement, object, Point> DropInTargetOperation { get; private set; }

        /// <summary>
        /// Action to perform always at finishing, to whom is indicated whether the drop over valid-destination operation was executed.
        /// </summary>
        public static Action<bool> FinishOperation { get; set; }

        public static bool IsDragging { get; private set; }

        /// <summary>
        /// Starts an operation of drag-drop a data, from an origin control to a destination one,
        /// in the scope of a context and under certain conditions.
        /// </summary>
        /// <param name="Context">Scope Control over whom the operation will be performed (normally the main UserControl).</param>
        /// <param name="Position">Point over the dragging is initiated.</param>
        /// <param name="Source">Control from where the dragging is initiated.</param>
        /// <param name="Data">Transferred data object.</param>
        /// <param name="AcceptableTargetCond">Expression that will indicate an acceptable destination for drop.</param>
        /// <param name="ReqsOneMatchingTarget">
        /// Indicates (true) what valid-destination condition must be valid for at least one pointed Control,
        /// otherwise (false) what valid-destination condition must be valid for all the pointed Controls.
        /// </param>
        /// <param name="DropInTargetOp">Action to perform while at dropping over an acceptable destination.</param>
        /// <param name="Message">Little text to show at the dragging (text=="" shows nothing, texto==null shows "Drag...")</param>
        public static void Start(FrameworkElement Context,
                                 Point Position,
                                 FrameworkElement Source,
                                 object Data,
                                 Func<FrameworkElement, FrameworkElement, bool> AcceptableTargetCond,
                                 bool ReqsOneMatchingTarget,
                                 Action<FrameworkElement, FrameworkElement, object, Point> DropInTargetOp,
                                 params string[] Message)
        {
            if (Context == null || SourceControl == null || TransferredData == null
                || AcceptableTargetCondition == null || DropInTargetOperation == null)
                throw new Exception("Insufficient arguments for the drag-drop assistant.");

            if (IsDragging)
                Finish(false);

            IsDragging = true;

            MessageText = (Message == null || Message.Length < 1 ? null : Message[0]);
            RegContext = Context;
            SourceControl = Source;
            TransferredData = Data;
            AcceptableTargetCondition = AcceptableTargetCond;
            RequiresOneMatchingTarget = ReqsOneMatchingTarget;
            DropInTargetOperation = DropInTargetOp;

            Border AnnounceBorder = new Border();
            AnnounceBorder.MouseMove += new System.Windows.Input.MouseEventHandler(AnnounceBorder_MouseMove);
            AnnounceBorder.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(AnnounceBorder_MouseLeftButtonUp);
            AnnounceBorder.MouseLeave += new System.Windows.Input.MouseEventHandler(AnnounceBorder_MouseLeave);

            AnnounceBorder.Background = new SolidColorBrush(Colors.Yellow);
            AnnounceBorder.BorderBrush = new SolidColorBrush(Colors.DarkGray);
            AnnounceBorder.BorderThickness = new Thickness(1);
            AnnounceBorder.Opacity = 0.5;
            AnnounceBorder.CornerRadius = new CornerRadius(5);
            AnnounceBorder.Cursor = Cursors.Hand;
            AnnounceBorder.Width = SourceControl.ActualWidth;
            AnnounceBorder.Height = SourceControl.ActualHeight;

            TextBlock AnnounceText = new TextBlock();
            AnnounceText.FontSize = 10;
            AnnounceText.Margin = new Thickness(2, 0, 2, 0);

            if (MessageText == null)
                AnnounceText.Text = "Drag...";
            else
                AnnounceText.Text = MessageText;

            AnnounceBorder.Child = AnnounceText;
            Announce.Child = AnnounceBorder;
            PreviousPosition = Position;
        }

        static void AnnounceBorder_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DoDrag(e.GetPosition(null));
        }

        static void AnnounceBorder_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Finish(true);
        }

        static void AnnounceBorder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsDragging)
                DoDrag(e.GetPosition(null));
        }

        /// <summary>
        /// Updates the position of the dragged object while the mouse pointer is moving.
        /// Receives argument of absolute positioning.
        /// </summary>
        public static void DoDrag(Point Posicion)
        {
            // (alternative: MouseEventArgs.GetPosition(null))

            if (!IsDragging || Posicion == PreviousPosition)
                return;

            if (!Announce.IsOpen)
                Announce.IsOpen = true;

            double PosX = Posicion.X - Math.Round(SourceControl.ActualWidth / 2);
            double PosY = Posicion.Y - Math.Round(SourceControl.ActualHeight / 2);

            if (PosX >= 0 || PosY >= 0)
            {
                Announce.HorizontalOffset = PosX;
                Announce.VerticalOffset = PosY;
            }

            PreviousPosition = Posicion;
        }

        /// <summary>
        /// Ends the dragging operation, indicating whether to call the drop in valid-destination action.
        /// </summary>
        public static void Finish(bool SoltarEnDestino)
        {
            bool DropInTarget = false;

            try
            {
                if (SoltarEnDestino)
                {
                    FrameworkElement Receptor = ValidReceiver(PreviousPosition);

                    if (Receptor != null)
                    {
                        DropInTargetOperation(SourceControl, Receptor, TransferredData, PreviousPosition);
                        DropInTarget = true;
                    }
                }
            }
            finally
            {
                if (FinishOperation != null)
                    FinishOperation(DropInTarget);

                Announce.IsOpen = false;
                IsDragging = false;
            }
        }

        /// <summary>
        /// Returns the first destination object, under the specified position, that accomplishes
        /// the acceptable-destination condition (for only one or all the pointed objects) of this assitant, else null.
        /// </summary>
        public static FrameworkElement ValidReceiver(Point Position)
        {
            var Pointed = VisualTreeHelper.HitTest(RegContext, Position);
            return Pointed.VisualHit as FrameworkElement;

            
            /* // BEFORE: var Pointed = Contex.HitTest(Position);
            var Pointed = System.Windows.Media.VisualTreeHelper.FindElementsInHostCoordinates(Position, RegContext) as List<UIElement>;

            if (RequiresOneMatchingTarget)
            {
                foreach (FrameworkElement Receptor in Pointed)
                    if (AcceptableTargetCondition(SourceControl, Receptor))
                        return Receptor;
            }
            else
            {
                FrameworkElement First = null;

                foreach (FrameworkElement Receptor in Pointed)
                {
                    if (First == null)
                        First = Receptor;

                    if (!AcceptableTargetCondition(SourceControl, Receptor))
                        return null;
                }

                return First;
            }

            return null; */
        }
    }
}
