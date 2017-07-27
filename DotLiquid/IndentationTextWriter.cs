using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Instrumind.Common;

namespace DotLiquid
{
    public class IndentationTextWriter : CentralizedTextWriter
    {
        public static IndentationTextWriter Create(TextWriter Writer = null, string IndentTab = null)
        {
            IndentationTextWriter Result = null;

            if (Writer == null)
                Writer = new StringWriter();

            if (IndentTab == null)
                IndentTab = " ".Replicate(AppExec.GetConfiguration<int>("FileGeneration", "IndentSize", 4));

            Result = new IndentationTextWriter(Writer, IndentTab);
            Result.IndentationDepth = DotLiquid.Tags.Inject.RecursionLevel;

            return Result;
        }

        public bool IsAtLineStart { get; internal set; }

        private int CurrentDepth;
        private string IndentText;
        private TextWriter TargetWriter;

        public IndentationTextWriter(TextWriter TargetWriter, string IndentText)
        {
            this.TargetWriter = TargetWriter;
            this.IndentText = IndentText;
            this.CurrentDepth = 0;
            this.IsAtLineStart = false;
        }

        public override void Close()
        {
            this.TargetWriter.Close();
        }

        public override void Flush()
        {
            this.TargetWriter.Flush();
        }

        protected override void ApplyWrite(string Value, bool AddNewLine = false)
        {
            if (this.IsAtLineStart && Value == "")
                return;

            this.GenerateIndent();

            if (AddNewLine)
            {
                this.TargetWriter.WriteLine(Value);
                this.IsAtLineStart = true;
            }
            else
                this.TargetWriter.Write(Value);
        }

        public int IndentationDepth
        {
            get { return this.CurrentDepth; }
            set
            {
                if (value < 0)
                    value = 0;

                this.CurrentDepth = value;
            }
        }

        private void GenerateIndent()
        {
            if (!this.IsAtLineStart)
                return;

            for (var Level = 0; Level < this.CurrentDepth; Level++)
                this.TargetWriter.Write(this.IndentText);

            this.IsAtLineStart = false;
        }

        public void WriteLineUnindented(string s)
        {
            this.TargetWriter.WriteLine(s);
        }

        public TextWriter LocalWriter { get { return this.TargetWriter; } }

        public override Encoding Encoding { get { return this.TargetWriter.Encoding; } }

        public override string NewLine
        {
            get { return this.TargetWriter.NewLine; }
            set { this.TargetWriter.NewLine = value; }
        }

        public override string ToString()
        {
            return this.LocalWriter.ToString();
        }
     }
}
