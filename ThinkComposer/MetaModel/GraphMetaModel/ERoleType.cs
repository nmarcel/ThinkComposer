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
// File   : ERoleType.cs
// Object : Instrumind.ThinkComposer.MetaModel.GraphMetaModel.ERoleType (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.16 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

using Instrumind.Common;

/// Base abstractions for the user metadefinition of Graph schemas
namespace Instrumind.ThinkComposer.MetaModel.GraphMetaModel
{
    /// <summary>
    /// Role categories.
    /// </summary>
    public enum ERoleType : byte
    {
        /// <summary>
        /// Role indicates source in a directional relationship (i.e. X in "from X to Y").
        /// </summary>
        [FieldName("Origin"), Description("Role indicates source in a directional or non-directional relationship (i.e. X in \"from X to Y\" or in \"X with Y\").")]
        Origin = ((byte)'O'),

        /// <summary>
        /// Role indicates destination in a directional relationship (i.e. Y in "from X to Y").
        /// </summary>
        [FieldName("Target"), Description("Role indicates destination in a directional relationship (i.e. Y in \"from X to Y\").")]
        Target = ((byte)'T'),

        /*- /// <summary>
        /// Role indicates involvement in a non-directional relationship (i.e. "X is brother of Y").
        /// </summary>
        [Description("Role indicates involvement in a non-directional relationship (i.e. \"X is brother of Y\").")]
        Participant = ((byte)'P') */
    }
}