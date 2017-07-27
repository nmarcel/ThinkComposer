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
// File   : ModelDefinition.cs
// Object : Instrumind.Common.EntityBase.ModelDefinition (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.24 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows.Media;

using Instrumind.Common.Visualization;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Base class for the definition of model classifiers and their members.
    /// IMPORTANT:
    /// This class does not descend from UniqueElement (AKA "has GUID") because a new GUID
    /// would be generated in each different PC running this software, hence making all of them "different".
    /// The Idea is to make the model definitions (classes, properties, collections, etc.) referenceable via the Name/Code.
    /// So, for determine two objects pointing to the same "semantic" definition use General.IsEquivalent
    /// </summary>
    public abstract class ModelDefinition : IRecognizableElement
    {
        public ModelDefinition(string TechName, string Name, string Summary, Type DeclaringType)
        {
            General.ContractRequiresNotAbsent(Name);
            General.ContractRequiresNotNull(DeclaringType);

            this.TechName = TechName;
            this.Name = Name.AbsentDefault(TechName.IdentifierToText());
            this.Summary = Summary.AbsentDefault(Name + " property");
            this.DeclaringType = DeclaringType;
        }

        /// <summary>
        /// Name of the defined object.
        /// </summary>
        public string TechName { get; set; }

        /// <summary>
        /// Type declaring the defined object.
        /// </summary>
        public Type DeclaringType { get; protected set; }

        /// <summary>
        /// User-level name of the defined object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User-level description of the defined object.
        /// </summary>
        public string Summary { get; set; }

        public override string ToString()
        {
            return Name; // +" {" + DeclaringType.Name + "}";
        }

        /// <summary>
        /// Visual representation of this model definition.
        /// </summary>
        public ImageSource Pictogram { get; set; }

        public int CompareTo(object obj)
        {
            return this.TechName.CompareTo(obj as ModelDefinition);
        }
    }
}