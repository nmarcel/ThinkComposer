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
// File   : AssignmentVariation.cs
// Object : Instrumind.Common.EntityBase.AssignmentVariation (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.21 Néstor Sánchez A.  Creation
//

using System;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Individual assignment of a value to a controlled property.
    /// </summary>
    public class AssignmentVariation : AtomicVariation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        internal AssignmentVariation(MModelPropertyDefinitor VariatingProperty, IMModelClass VariatingInstance, object VariatingValue)
        {
            this.VariatingProperty = VariatingProperty;
            this.VariatingInstance = VariatingInstance;
            this.VariatingValue = VariatingValue;

            /*T if (VariatingProperty.TechName == UniqueElement.__GlobalId.TechName)
                Console.WriteLine("GlobalId"); */
        }

        /// <summary>
        /// Definitor of the changing property.
        /// </summary>
        public MModelPropertyDefinitor VariatingProperty { get; protected set; }

        /// <summary>
        /// Instance owning the changing property.
        /// </summary>
        public IMModelClass VariatingInstance { get; protected set; }

        /// <summary>
        /// The registered value to assign.
        /// </summary>
        public object VariatingValue { get; protected set; }

        /// <summary>
        /// Executes the registered change for the specified edit engine.
        /// </summary>
        public override void Execute(EntityEditEngine Engine)
        {
            EntityEditEngine.RegisterInverseAssignment(this.VariatingProperty, (IModelEntity)this.VariatingInstance, this.VariatingProperty.Read(this.VariatingInstance));

            this.VariatingProperty.Write(this.VariatingInstance, this.VariatingValue);
        }

        public override string ToString()
        {
            return "Variation{" + this.GetHashCode().ToString("0000000000") + "} Assignment[" + this.VariatingInstance.GetHashCode().ToString("0000000000") + "]: '" + this.VariatingProperty.Name +
                "' (Instance='" + this.VariatingInstance.ToStringAlways() +
                "', Value='" + this.VariatingValue.ToStringAlways("{NULL}") + "')";
        }
    }
}