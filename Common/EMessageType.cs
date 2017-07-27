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
// File   : EMessageType.cs
// Object : Instrumind.Common.EMessageType (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.07 Néstor Sánchez A.  Creation
//

using System;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Possible purposes of messages to be send/show to the user.
    /// </summary>
    public enum EMessageType : byte
    {
        /// <summary>
        /// Indicates an asked question.
        /// </summary>
        Question = ((byte)'Q'),

        /// <summary>
        /// Indicates informational message.
        /// </summary>
        Information = ((byte)'I'),

        /// <summary>
        /// Indicates a problem which can be ignored.
        /// </summary>
        Warning = ((byte)'W'),

        /// <summary>
        /// Indicates an unacceptable error condition.
        /// </summary>
        Error = ((byte)'E')
    }
}