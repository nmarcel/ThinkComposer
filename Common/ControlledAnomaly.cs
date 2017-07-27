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
// File   : ControlledAnomaly.cs
// Object : Instrumind.Common.ControlledAnomaly (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.23 Néstor Sánchez A.  Creation
//

using System;
using System.Runtime.Serialization;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Centralizes the exception handling of Instrumind software.
    /// </summary>
    public abstract class ControlledAnomaly : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ControlledAnomaly(string Message, object Evidence = null) :
                            base(Message)
        {
            this.AssociatedEvidence = Evidence;
        }

        public ControlledAnomaly(string Message, Exception Cause, object Evidence = null) :
                            base(Message, Cause)                                
        {
            this.AssociatedEvidence = Evidence;
        }

        public ControlledAnomaly(SerializationInfo SerializationData, StreamingContext StreamContext, object Evidence = null) :
                            base(SerializationData, StreamContext)
        {
            this.AssociatedEvidence = Evidence;
        }

        /// <summary>
        /// Returns this anomaly expressed as a Warning one.
        /// </summary>
        /// <returns>Warning anomaly wrapper.</returns>
        public WarningAnomaly AsWarning()
        {
            return (new WarningAnomaly(this));
        }

        /// <summary>
        /// Data which was rejected or has the problem reported by the anomaly.
        /// </summary>
        public object AssociatedEvidence { get; protected set; }
    }
}