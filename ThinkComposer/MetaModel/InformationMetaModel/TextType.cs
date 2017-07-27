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
// File   : TextType.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.TextType (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.11.07 Néstor Sánchez A.  Creation
//

using System;
using System.Windows;
using System.Windows.Media;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// Defines a simple text (string) data type.
    /// </summary>
    [Serializable]
    public class TextType : BasicDataType
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TextType(string Name, string TechName, string Summary = "", int SizeLimit = 255, ImageSource Pictogram = null)
            : base(Name, TechName, Summary, Pictogram)
        {
            this.SizeLimit = SizeLimit;

            this.DisplayMinLength = (this.SizeLimit < FieldDefinition.STD_FIELD_WITDH
                                     ? this.SizeLimit : FieldDefinition.STD_FIELD_WITDH);
        }

        /// <summary>
        /// References the .NET type of the final container for the objects declared with this data type.
        /// </summary>
        // IMPORTANT: If this is changed, any dependant Field-Def should update its default-value.
        public override Type ContainerType { get { return typeof(string); } }

        /// <summary>
        /// Limit for the length of the stored text (default=255).
        /// </summary>
        public int SizeLimit { get; protected set; }

        /// <summary>
        /// If possible, gets a value of this data-type from the supplied Source string and puts it in the specified Result out-reference.
        /// Returns indication of success (true) or failure (false) of the parsing.
        /// </summary>
        public override bool TryParseValueFrom(string Source, out object Result)
        {
            Result = Source;
            return true;
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

            var TextValue = (string)Value;

            if (TextValue != null && TextValue.Length > this.SizeLimit)
                return "Value exceeds the length limit of " + this.SizeLimit.ToString() + " characters.";

            return null;
        }
    }
}