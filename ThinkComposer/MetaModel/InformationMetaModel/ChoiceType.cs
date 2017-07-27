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
// File   : TextType.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.ChoiceType (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.06.24 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Instrumind.Common;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// Defines a simple choice (selected member of an options set, up to 256) data type.
    /// </summary>
    // NOTE: CHOICE TYPES ARE NOT USER-DEFINABLE BECAUSE OF NO EDITING (UNDO/REDO) SUPPORT.
    [Serializable]
    public class ChoiceType : BasicDataType
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ChoiceType(string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
            : base(Name, TechName, Summary, Pictogram)
        {
            this.DisplayMinLength = 1;
        }

        /// <summary>
        /// References the .NET type of the final container for the objects declared with this data type.
        /// </summary>
        // IMPORTANT: If this is changed, any dependant Field-Def should update its default-value.
        public override Type ContainerType { get { return typeof(byte); } }

        /// <summary>
        /// Set of possible options that can exist (key=option, value=name).
        /// </summary>
        protected readonly Dictionary<byte, string> OptionsSet = new Dictionary<byte,string>();

        /// <summary>
        /// Register an option and its name.
        /// </summary>
        public void RegisterOption(byte Option, string Name)
        {
            this.OptionsSet.AddNew(Option, Name);
        }

        /// <summary>
        /// Returns the set of registered options and their names.
        /// </summary>
        public IEnumerable<Tuple<byte, string>> GetRegisteredOptions()
        {
            foreach (var OptionReg in this.OptionsSet)
                yield return Tuple.Create(OptionReg.Key, OptionReg.Value); 
        }

        /// <summary>
        /// Gets the option register, with key=option and value=name, at the specified set index.
        /// </summary>
        public KeyValuePair<byte,string> GetOptionRegister(int Index)
        {
            // WARNING: The position of a Dictionary kvp (or enumeration order) is not warranted to be always the same.
            return this.OptionsSet.ElementAt(Index);
        }

        /// <summary>
        /// Returns the option associated to the supplied name.
        /// </summary>
        public byte? GetOption(string Name)
        {
            foreach (var RegOption in this.OptionsSet)
                if (RegOption.Value.IsEqual(Name))
                    return RegOption.Key;

            return null;
        }

        /// <summary>
        /// Returns the name associated to the supplied option.
        /// </summary>
        public string GetName(byte Option)
        {
            foreach (var RegOption in this.OptionsSet)
                if (RegOption.Key.IsEqual(Option))
                    return RegOption.Value;

            return null;
        }

        /// <summary>
        /// If possible, gets a value of this data-type from the supplied Source string (a option or name) and puts it in the specified Result out-reference.
        /// Returns indication of success (true) or failure (false) of the parsing.
        /// </summary>
        public override bool TryParseValueFrom(string Source, out object Result)
        {
            byte Option;
            Result = null;

            if (!byte.TryParse(Source, out Option))
            {
                foreach(var RegOption in this.OptionsSet)
                    if (RegOption.Value.IsEqual(Source))
                    {
                        Result = RegOption.Key;
                        return true;
                    }

                return false;
            }

            Result = Option;
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

            var OptionValue = (byte)Value;

            if (!this.OptionsSet.ContainsKey(OptionValue))
                return "Value does not belong to the registered options set.";

            return null;
        }
    }
}