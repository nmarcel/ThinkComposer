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
// File   : GenericCommand.cs
// Object : Instrumind.Common.Visualization.GenericCommand (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.02.dd Néstor Sánchez A.  Creation
//

using System;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Command for generic purposes.
    /// </summary>
    public class GenericCommand : WorkCommand
    {
        public GenericCommand(string Name, Action<object> Apply = null, Func<object,bool> CanApply = null)
            : base(Name)
        {
            this.Apply = Apply;
            this.CanApply = CanApply.NullDefault(obj => true);
        }
    }
}