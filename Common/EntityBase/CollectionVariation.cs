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
// File   : CollectionVariation.cs
// Object : Instrumind.Common.EntityBase.CollectionVariation (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.10.12 Néstor Sánchez A.  Creation
//

using System;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Individual calling of an editing operation action.
    /// </summary>
    public class CollectionVariation : AtomicVariation
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal CollectionVariation(EditableCollection VariatingCollection, char VariatingAlterCode, object[] PassedParameters)
        {
            this.VariatingCollection = VariatingCollection;
            this.VariatingAlterCode = VariatingAlterCode;
            this.PassedParameters = PassedParameters;
        }

        /// <summary>
        /// Target collection for the executing action.
        /// </summary>
        public EditableCollection VariatingCollection { get; protected set; }

        /// <summary>
        /// Code of the executing alteration, associated to the editable collection type.
        /// </summary>
        public char VariatingAlterCode { get; internal set; }

        /// <summary>
        /// Parameter objects passed to the executing action.
        /// </summary>
        public object[] PassedParameters { get; protected set; }

        /// <summary>
        /// Executes the registered change for the specified edit engine.
        /// </summary>
        public override void Execute(EntityEditEngine Engine)
        {
            char InverseAlterCode = '?';

            if (this.VariatingCollection.GetType().Name == typeof(EditableList<>).Name)
                InverseAlterCode = EntityEditEngine.GenerateInverseListAlterCode(this.VariatingAlterCode);
            else
                InverseAlterCode = EntityEditEngine.GenerateInverseDictionaryAlterCode(this.VariatingAlterCode);

            var InverseParameters = this.VariatingCollection.GetAlterParameters(InverseAlterCode, PassedParameters);
            EntityEditEngine.RegisterInverseCollectionChange(this.VariatingCollection.VariatingInstance, this.VariatingCollection, InverseAlterCode,  InverseParameters);

            this.VariatingCollection.AlterCollection(this.VariatingAlterCode, this.PassedParameters);
        }

        public override string ToString()
        {
            return "Variation [Collection]{" + this.GetHashCode().ToString() + "}: '" + this.VariatingCollection.ToString() + "' (AlterCode=" + (char)this.VariatingAlterCode + ", Parameters=[" + this.PassedParameters.GetConcatenation(null, ";") + "])";
        }
    }
}