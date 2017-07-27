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
// File   : CommandVariation.cs
// Object : Instrumind.Common.EntityBase.CommandVariation (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.21 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Instrumind.Common.EntityDefinition;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Group of sequenced editing changes to be executed as one.
    /// Variations will be stored in the right order, either for undo or redo, by the EntityEditEngine.
    /// </summary>
    public class CommandVariation : Variation
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CommandVariation(string AppliedCommand, bool AlterExistenceStatusWhileVariating)
        {
            this.CommandName = AppliedCommand;
            this.AlterExistenceStatusWhileVariating = AlterExistenceStatusWhileVariating;

            this.Variations = new List<Variation>();
        }

        /// <summary>
        /// Name of the command applied with this group of variations.
        /// </summary>
        public string CommandName { get; protected set; }

        /// <summary>
        /// List of nested composing variations.
        /// </summary>
        public List<Variation> Variations { get; protected set; }

        /// <summary>
        /// Indicates to change the existente-status while properties are variating
        /// </summary>
        public bool AlterExistenceStatusWhileVariating { get; set; }

        /// <summary>
        /// Adds the supplied variation to the grouping set.
        /// </summary>
        public void AddVariation(Variation ComposingVariation)
        {
            this.Variations.Add(ComposingVariation);
            //T Console.WriteLine("CV:" + this.Variations.Count.ToString() + "; ");
        }

        /// <summary>
        /// Executes the registered change for the specified edit engine.
        /// </summary>
        public override void Execute(EntityEditEngine Engine)
        {
            if (this.Variations.Count < 1 || !(Engine.IsUndoing || Engine.IsRedoing))
                throw new UsageAnomaly("Command Variation is not executable for the specified edit-engine.");

            foreach(var ExecVariation in this.Variations)
            {
                var Text = ExecVariation.ToString();
                if (Text == null)
                    Console.WriteLine("Executing Variation: NULL");
                ExecVariation.Execute(Engine);
            }
        }

        public override string ToString()
        {
            return "Variation [Command]{" + this.GetHashCode().ToString() + "} : '" + this.CommandName + "' (" + this.Variations.Count.ToString() + " vars.)";
        }
    }
}