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
// File   : NumberType.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.NumberType (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 20yy.mm.dd Néstor Sánchez A.  Creation
//

using System;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// Defines a numeric data type.
    /// </summary>
    [Serializable]
    public class NumberType : BasicDataType
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <summary>
        /// Constructor.
        /// </summary>
        public NumberType(string Name, string TechName, string Summary = "",
                          byte IntegerDigits = 10, byte DecimalDigits = 5, decimal MinLimit = decimal.MinValue, decimal MaxLimit = decimal.MaxValue, ImageSource Pictogram = null)
             : base(Name, TechName, Summary, Pictogram)
        {
            this.IntegerDigits = IntegerDigits;
            this.DecimalDigits = DecimalDigits;
            this.MinLimit = MinLimit;
            this.MaxLimit = MaxLimit;

            this.DisplayAlignment = TextAlignment.Right;
            //Pending: Try "##,###,###,###,###,##0.###############" 
            this.DisplayFormat = "#0" + (this.DecimalDigits < 1 ? "" : ".00");
            this.DisplayMinLength = ((MinLimit < 0 ? 1 : 0) +
                                     + (MaxLimit < decimal.MaxValue ? MaxLimit.ToString().Length : IntegerDigits)
                                     + (DecimalDigits > 0 ? 1 : 0) + DecimalDigits);
        }

        /// <summary>
        /// References the .NET type of the final container for the objects declared with this data type.
        /// </summary>
        // IMPORTANT: If this is changed, any dependant Field-Def should update its default-value.
        public override Type ContainerType { get { return typeof(decimal); } }

        /// <summary>
        /// Number of integer digits (up to 15, shared with decimal digits. Default=10).
        /// </summary>
        public byte IntegerDigits { get; protected set; }

        /// <summary>
        /// Number of decimal digits (up to 15, shared with integer digits. Default=5).
        /// </summary>
        public byte DecimalDigits { get; protected set; }

        /// <summary>
        /// Minimum allowed number (default=not set).
        /// </summary>
        public decimal MinLimit { get; protected set; }

        /// <summary>
        /// Maximum allowed number (default=not set).
        /// </summary>
        public decimal MaxLimit { get; protected set; }

        /// <summary>
        /// If possible, gets a value of this data-type from the supplied Source string and puts it in the specified Result out-reference.
        /// Returns indication of success (true) or failure (false) of the parsing.
        /// </summary>
        public override bool TryParseValueFrom(string Source, out object Result)
        {
            decimal Value;

            if (decimal.TryParse(Source, out Value))
            {
                Result = Value;
                return true;
            }

            Result = null;
            return false;
        }

        /// <summary>
        /// Validates the supplied Value against this data-type.
        /// Returns null if succeeded, or an error message if validation is not passed.
        /// </summary>
        public override string Validate(object Value)
        {
            var Result = base.Validate(Value);
            if (Result != null)
                return Result;

            var NumberValue = (decimal)Value;

            if (NumberValue.GetIntegerDigits() > this.IntegerDigits)
                return "Value has more than " + this.IntegerDigits.ToString() + " integer digits.";

            if (NumberValue.GetDecimalDigits() > this.DecimalDigits)
                return "Value has more than " + this.DecimalDigits.ToString() + " decimal digits.";

            if (NumberValue < this.MinLimit || NumberValue > this.MaxLimit)
                return "Value is out of the range: " + this.MinLimit.ToString() + " to " + this.MaxLimit + ".";

            return null;
        }
    }
}