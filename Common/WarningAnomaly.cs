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
// File   : WarningAnomaly.cs
// Object : Instrumind.Common.WarningAnomaly (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.01 Néstor Sánchez A.  Creation
//

using System;
using System.Runtime.Serialization;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// Represents a non fatal anomaly that would be treated as a warning.
    public class WarningAnomaly : ControlledAnomaly
    {
        public ControlledAnomaly UnderlyingAnomaly { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public WarningAnomaly(ControlledAnomaly UnderlyingAnomaly) :
            base(UnderlyingAnomaly.Message, UnderlyingAnomaly.InnerException, UnderlyingAnomaly.AssociatedEvidence)
        {
            this.UnderlyingAnomaly = UnderlyingAnomaly;
        }
    }
}