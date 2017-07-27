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
// File   : EExecutionStatus.cs
// Object : Instrumind.Common.EExecutionStatus (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.07.16 Néstor Sánchez A.  Creation
//

using System;

namespace Instrumind.Common
{
    /// <summary>
    /// Possible status for a runnable engine.
    /// </summary>
    public enum EExecutionStatus : byte
    {
        /// <summary>
        /// The engine has been just created, but still is not running.
        /// </summary>
        Created = ((byte)'C'),

        /// <summary>
        /// The engine is currently running.
        /// </summary>
        Running = ((byte)'R'),

        /// <summary>
        /// The engine is paused, awaiting to be run again or discarded.
        /// </summary>
        Paused = ((byte)'P'),

        /// <summary>
        /// The engine is stopped and discarded.
        /// </summary>
        Stopped = ((byte)'S')
    }
}