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
// File   : CentralizedTextWriter.cs
// Object : Instrumind.Common.CentralizedTextWriter (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.09 Néstor Sánchez A.  Creation
//

using System;
using System.Globalization;
using System.IO;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// TextWriter which centralizes writing operations in only one method, thus simplifying descendants implementations.
    /// Plus, any NewLine within the supplied string automatically generates new line writings.
    public abstract class CentralizedTextWriter : TextWriter
    {
        /// <summary>
        /// Method for writing into the final destination.
        /// Must be implemented by non-abstract descendants.
        /// </summary>
        /// <param name="value">The text to be written.</param>
        /// <param name="AddNewLine">Indicates whether to add a new line at the end.</param>
        protected abstract void ApplyWrite(string value, bool AddNewLine = false);

        [System.Security.SecuritySafeCritical]
        public override void WriteLine(string Text)
        {
            this.Write(Text, true);
        }

        public override void Write(string value)
        {
            this.Write(value, false);
        }

        public void Write(string value, bool AddNewLine)
        {
            if (value == null)
                value = String.Empty;
            else
                if (value.IndexOf(Environment.NewLine) >= 0)
                {
                    var Lines = value.Segment(Environment.NewLine);

                    for (int IndLine = 0; IndLine < Lines.Length - 1; IndLine++)
                        WriteLine(Lines[IndLine]);

                    value = Lines[Lines.Length - 1];
                }

            ApplyWrite(value, AddNewLine);
        }

        public override void Write(bool value) { this.Write(value); }
        public override void Write(char value) { this.Write(Convert.ToString(value)); }
        public override void Write(char[] buffer) { this.Write(new String(buffer)); }
        public override void Write(decimal value) { this.Write(Convert.ToString(value)); }
        public override void Write(double value) { this.Write(Convert.ToString(value)); }
        public override void Write(float value) { this.Write(Convert.ToString(value)); }
        public override void Write(int value) { this.Write(Convert.ToString(value)); }
        public override void Write(long value) { this.Write(Convert.ToString(value)); }
        public override void Write(object value) { this.Write(Convert.ToString(value)); }
        public override void Write(uint value) { this.Write(Convert.ToString(value)); }
        public override void Write(ulong value) { this.Write(Convert.ToString(value)); }
        public override void Write(string format, object arg0) { this.Write(String.Format(format, arg0)); }
        public override void Write(string format, params object[] arg) { this.Write(String.Format(format, arg)); }
        public override void Write(char[] buffer, int index, int count)
        {
            char[] newbuffer = new char[count];
            Array.Copy(buffer, index, newbuffer, 0, count);
            this.Write(new String(newbuffer));
        }
        public override void Write(string format, object arg0, object arg1) { this.Write(String.Format(format, arg0, arg1)); }
        public override void Write(string format, object arg0, object arg1, object arg2) { this.Write(String.Format(format, arg0, arg1, arg2)); }

        public override void WriteLine() { this.WriteLine(""); }
        public override void WriteLine(bool value) { this.WriteLine(Convert.ToString(value)); }
        public override void WriteLine(char value) { this.WriteLine(Convert.ToString(value)); }
        public override void WriteLine(char[] buffer) { this.WriteLine(new String(buffer)); }
        public override void WriteLine(decimal value) { this.WriteLine(Convert.ToString(value)); }
        public override void WriteLine(double value) { this.WriteLine(Convert.ToString(value)); }
        public override void WriteLine(float value) { this.WriteLine(Convert.ToString(value)); }
        public override void WriteLine(int value) { this.WriteLine(Convert.ToString(value)); }
        public override void WriteLine(long value) { this.WriteLine(Convert.ToString(value)); }
        public override void WriteLine(object value) { this.WriteLine(Convert.ToString(value)); }
        public override void WriteLine(uint value) { this.WriteLine(Convert.ToString(value)); }
        public override void WriteLine(ulong value) { this.WriteLine(Convert.ToString(value)); }
        public override void WriteLine(string format, object arg0) { this.WriteLine(String.Format(format, arg0)); }
        public override void WriteLine(string format, params object[] arg) { this.WriteLine(String.Format(format, arg)); }
        public override void WriteLine(char[] buffer, int index, int count)
        {
            char[] newbuffer = new char[count];
            Array.Copy(buffer, index, newbuffer, 0, count);
            this.WriteLine(new String(newbuffer));
        }
        public override void WriteLine(string format, object arg0, object arg1) { this.WriteLine(String.Format(format, arg0, arg1)); }
        public override void WriteLine(string format, object arg0, object arg1, object arg2) { this.WriteLine(String.Format(format, arg0, arg1, arg2)); }

        protected System.Text.Encoding ExposedEncoding = new System.Text.UnicodeEncoding();
        public override System.Text.Encoding Encoding
        {
            get { return this.ExposedEncoding; }
        }
    }
}