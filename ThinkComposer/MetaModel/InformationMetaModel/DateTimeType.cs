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
// File   : DateTimeType.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.DateTimeType (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.11.07 Néstor Sánchez A.  Creation
//

using System;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// Defines a date and/or time data type.
    /// </summary>
    [Serializable]
    public class DateTimeType : BasicDataType
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DateTimeType(string Name, string TechName, string Summary = "", bool HasDatePart = true, bool HasTimePart = true, ImageSource Pictogram = null)
             : base(Name, TechName, Summary, Pictogram)
        {
            this.HasDatePart = HasDatePart;
            this.HasTimePart = HasTimePart;

            this.DisplayAlignment = TextAlignment.Center;
            this.DisplayFormat = (this.HasDatePart && this.HasTimePart ? "G" : (this.HasTimePart ? "t" : "d"));
            this.DisplayMinLength = ((this.HasDatePart ? 10 : 0) + (this.HasTimePart ? 8 : 0)
                                     + (this.HasDatePart && this.HasTimePart ? 3 : 0));
        }

        /// <summary>
        /// References the .NET type of the final container for the objects declared with this data type.
        /// </summary>
        // IMPORTANT: If this is changed, any dependant Field-Def should update its default-value.
        public override Type ContainerType { get { return typeof(DateTime); } }

        /// <summary>
        /// Indicates whether the field has a Date part (default=true).
        /// </summary>
        public bool HasDatePart { get; protected set; }

        /// <summary>
        /// Indicates whether the field has a Time part (default=true).
        /// </summary>
        public bool HasTimePart { get; protected set; }

        /// <summary>
        /// If possible, gets a value of this data-type from the supplied Source string and puts it in the specified Result out-reference.
        /// Returns indication of success (true) or failure (false) of the parsing.
        /// </summary>
        public override bool TryParseValueFrom(string Source, out object Result)
        {
            DateTime Value;

            if (DateTime.TryParse(Source, out Value))
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

            var DateTimeValue = (DateTime)Value;

            if (!HasDatePart && DateTimeValue.Date != General.EMPTY_DATE)
                return "Value has Date part.";

            if (!HasTimePart && DateTimeValue.TimeOfDay != TimeSpan.Zero)
                return "Value has Time part.";

            return null;
        }
    }
}