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
// File   : InternalAnomaly.cs
// Object : Instrumind.Common.InternalAnomaly (Class)
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
    /// Indicates an internal detected error, such as programming bugs.
    /// For example: In an indexed collection, the pointer exceeded the range of existing elements.
    /// </summary>
    public class InternalAnomaly : ControlledAnomaly
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public InternalAnomaly(string Message, object Evidence = null) :
            base(Message, Evidence)
        {        }

        public InternalAnomaly(string Message, Exception Cause, object Evidence = null) :
            base(Message, Cause, Evidence)
        {        }

        public InternalAnomaly(SerializationInfo SerializationData, StreamingContext StreamContext, object Evidence = null) :
            base(SerializationData, StreamContext, Evidence)
        {        }
    }
}