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
// File   : SynthBinarySerializer.cs
// Object : Instrumind.Common.SynthBinarySerializer (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.11.12 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Instrumind.Common
{
    /// <summary>
    /// Provides binary serialization capabilities.
    /// </summary>
    public partial class StandardBinarySerializer
    {
        /// <summary>
        /// Deserializes and returns the object graph contained in the underlying stream.
        /// </summary>
        public TGraphRoot Deserialize<TGraphRoot>()
        {
            TGraphRoot Result = default(TGraphRoot);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
