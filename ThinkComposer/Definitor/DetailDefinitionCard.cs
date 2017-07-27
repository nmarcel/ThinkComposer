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
// File   : DetailDefintionCard.cs
// Object : Instrumind.ThinkComposer.Definitor.DefinitorUI.DetailDefintionCard (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.01.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides the user-interface components for the Domain related Definitions.
namespace Instrumind.ThinkComposer.Definitor.DefinitorUI
{
    /// <summary>
    /// Represents an Idea detail definition being edited for predefined cases.
    /// </summary>
    public class DetailDefinitionCard : DetailBaseCard
    {
        /// <summary>
        /// Creates and returns a collection of global detail editing cards for the supplied definitor.
        /// </summary>
        public static EditableList<DetailDefinitionCard> GenerateGlobalDetailsForDefinitor(IdeaDefinition Definitor)
        {
            var GeneratedGlobalDetailDefs = new EditableList<DetailDefinitionCard>("GlobalDetailDefs", null);

            foreach (var PredefDesignator in Definitor.DetailDesignators)
            {
                var NewDetEdit = new DetailDefinitionCard(true, new Assignment<DetailDesignator>(PredefDesignator, false));
                GeneratedGlobalDetailDefs.Add(NewDetEdit);
            }

            return GeneratedGlobalDetailDefs;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public DetailDefinitionCard(bool IsPreexistent, Assignment<DetailDesignator> Designator)
             : base(IsPreexistent, Designator, true)
        {
        }
    }
}