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
// File   : IdentificationController.cs
// Object : Instrumind.Common.IdentificationController (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.13 Néstor Sánchez A.  Creation
//

using System;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Provides identification services within a scope.
    /// </summary>
    public class IdentificationController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public IdentificationController(Func<string, bool> IsValidKey, Func<string, bool> IsValidName,
                                        Func<string, string> NameToKey, Func<string, string> KeyToName)
        {
            General.ContractRequiresNotNull(IsValidKey, IsValidName, NameToKey, KeyToName);

            this.IsValidKey = IsValidKey;
            this.IsValidName = IsValidName;
            this.KeyToName = KeyToName;
            this.NameToKey = NameToKey;
        }

        /// <summary>
        /// Function which validates whether the supplied string can be used as Key.
        /// </summary>
        public Func<string, bool> IsValidKey { get; protected set; }

        /// <summary>
        /// Function which validates whether the supplied string can be used as Name.
        /// </summary>
        public Func<string, bool> IsValidName { get; protected set; }

        /// <summary>
        /// Function which converts the supplied Name to its Key normalized equivalent.
        /// </summary>
        public Func<string, string> NameToKey { get; protected set; }

        /// <summary>
        /// Function which converts the supplied Key to its Name normalized equivalent.
        /// </summary>
        public Func<string, string> KeyToName { get; protected set; }
     
   }
}