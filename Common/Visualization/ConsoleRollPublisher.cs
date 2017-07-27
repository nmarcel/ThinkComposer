// ------------------------------------------------------------------------------------------
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
// File   : ConsoleRollPublisher.cs
// Object : Instrumind.Common.Visualization.ConsoleRollPublisher (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.08 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Instrumind.Common;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Publishes text, to be continously supplied either directly or as a redirect of the standard console output, into a ListBox.
    /// </summary>
    public class ConsoleRollPublisher : CentralizedTextWriter
    {
        private ObservableCollection<TextLine> TextRollList = new ObservableCollection<TextLine>();
        private TextLine CurrentLine = null;
        private int CurrentIndex = -1;

        /// <summary>
        /// Default maximum lines for console output.
        /// </summary>
        public const int MAX_CONSOLE_OUTPUT_LINES = 1024;

        /// <summary>
        /// Listbox that presentes the published text roll.
        /// </summary>
        public ListBox TargetPresenter { get; protected set; }

        /// <summary>
        /// Maximum number of console output lines to be shown.
        /// </summary>
        public int MaxLines { get; protected set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="TargetPresenter"></param>
        public ConsoleRollPublisher(ListBox TargetPresenter, int MaxLines = MAX_CONSOLE_OUTPUT_LINES)
        {
            General.ContractRequiresNotNull(TargetPresenter);

            this.TargetPresenter = TargetPresenter;
            this.MaxLines = MaxLines;

            this.TargetPresenter.Items.Clear();
            this.TargetPresenter.ItemsSource = TextRollList;
        }

        public void Clear()
        {
            this.TextRollList.Clear();
            this.CurrentLine = null;
            this.CurrentIndex = -1;
        }

        protected override void ApplyWrite(string Value, bool AddNewLine = false)
        {
            try
            {
                if (Thread.CurrentThread != TargetPresenter.Dispatcher.Thread)
                {
                    // IMPORTANT: This solve the exception thrown after editing an Attachment and calling Console.WriteX(). See FilesWatcher_Changed().
                    // Exception message: "This type of CollectionView does not support changes to its SourceCollection from a thread different from the Dispatcher thread."
                    TargetPresenter.Dispatcher.BeginInvoke(new Action(() => ApplyWrite(Value, AddNewLine)), DispatcherPriority.Background);
                    return;
                }

                if (this.CurrentLine == null)
                {
                    /* Needed only for Server type software
                    var FormattedDateTime = DateTime.Now.AsCommonDateTime(false, true);
                    Value = FormattedDateTime + ". " + Value; */
                    this.CurrentLine = new TextLine(Value);
                    this.AppendLine();
                }
                else
                {
                    this.CurrentLine.Extend(Value);

                    // Enforces the update and show of the collection item.
                    this.TextRollList[CurrentIndex] = DummyTextLine;
                    this.TextRollList[CurrentIndex] = this.CurrentLine;
                }

                if (AddNewLine)
                    this.CurrentLine = null;
            }
            catch (Exception Problem)
            {
                AppExec.LogException(Problem, "ConsoleRollPublisher");
            }
        }
        private static TextLine DummyTextLine = new TextLine();

        private void AppendLine()
        {
            // Append new line at the end, if still the limit is not reached.
            if (this.CurrentIndex < this.MaxLines - 1)
            {
                this.CurrentIndex++;
                this.TextRollList.Add(this.CurrentLine);
                this.TargetPresenter.ScrollIntoView(this.CurrentLine);
                return;
            }
            
            // Move all lines up but the first, and updates the last.
            for (int IndLine = 0; IndLine < this.MaxLines - 1; IndLine++)
                this.TextRollList[IndLine] = this.TextRollList[IndLine + 1];

            this.TextRollList[this.CurrentIndex] = this.CurrentLine;
        }

        /// <summary>
        /// Wrapping string container which allow extensions.
        /// </summary>
        public class TextLine
        {
            private string Text = null;

            public TextLine()
            {
            }

            public TextLine(string Text)
            {
                this.Text = Text;
            }

            public override string ToString()
            {
                return (this.Text == null ? String.Empty : this.Text);
            }

            public void Extend(string Text)
            {
                if (this.Text == null)
                    this.Text = Text;
                else
                    this.Text = this.Text + Text;
            }
        }
    }
}