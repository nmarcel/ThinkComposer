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
// File   : DetailEditingCard.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.DetailEditingCard (Class)
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
using Instrumind.ThinkComposer.Definitor.DefinitorUI;

/// Provides the user-interface components for the Composition Composer.
namespace Instrumind.ThinkComposer.Composer.ComposerUI
{
    /// <summary>
    /// Represents an Idea detail being edited for either predefined or customized cases.
    /// </summary>
    public class DetailEditingCard : DetailBaseCard
    {
        /// <summary>
        /// Creates and returns a collection of global detail editing cards for the supplied Idea and Symbol.
        /// </summary>
        public static EditableList<DetailEditingCard> GenerateGlobalDetailsFor(Idea SourceIdea, VisualSymbol SourceSymbol)
        {
            General.ContractRequires(SourceSymbol == null || SourceSymbol.OwnerRepresentation.RepresentedIdea == SourceIdea);

            var GeneratedGlobalDetailEdits = new EditableList<DetailEditingCard>("GlobalDetailEdits", null);

            // First, populate considering the predefined designations of Global scope
            // IMPORTANT: This also concatenates non-local details (designations predefined in the previous Idea-Definition of a CONVERTED Source-Idea).
            var Designators = SourceIdea.IdeaDefinitor.DetailDesignators.Concat(
                                SourceIdea.Details.Where(det => !det.AssignedDesignator.IsLocal).Select(det => det.Designation)).Distinct();

            foreach (var Designator in Designators)
            {
                var NewDetEdit = new DetailEditingCard(true, new Assignment<DetailDesignator>(Designator, false),
                                                       SourceIdea.Details.FirstOrDefault(det => det.Designation.IsEqual(Designator)));

                if (SourceSymbol != null && SourceIdea.DetailsCustomLooks.ContainsKey(Designator)
                                         && SourceIdea.DetailsCustomLooks[Designator].ContainsKey(SourceSymbol))
                    NewDetEdit.SetLook(SourceIdea.DetailsCustomLooks[Designator][SourceSymbol]);

                GeneratedGlobalDetailEdits.Add(NewDetEdit);
            }

            // Then, add the custom designated details (for this particular Idea)
            foreach (var Detail in SourceIdea.Details)
            {
                var MatchPredefDsnIndex = GeneratedGlobalDetailEdits.IndexOfMatch(PredefDsn => PredefDsn.Designator.Value.IsEqual(Detail.Designation));
            }

            // Plus, set possible custom formats.
            foreach (var Detail in SourceIdea.Details)
            {
                var MatchPredefDsnIndex = GeneratedGlobalDetailEdits.IndexOfMatch(PredefDsn => PredefDsn.Designator.Value.IsEqual(Detail.Designation));

                if (SourceSymbol != null && MatchPredefDsnIndex >= 0)
                {
                    var TargetEditCard = GeneratedGlobalDetailEdits[MatchPredefDsnIndex];

                    // Set any possible custom format
                    if (TargetEditCard != null && SourceIdea.DetailsCustomLooks.ContainsKey(TargetEditCard.Designator.Value))
                    {
                        var FormattedDesignation = SourceIdea.DetailsCustomLooks[TargetEditCard.Designator.Value];

                        if (FormattedDesignation != null && FormattedDesignation.ContainsKey(SourceSymbol))
                            TargetEditCard.SetLook(FormattedDesignation[SourceSymbol]);
                    }
                }
            }

            return GeneratedGlobalDetailEdits;
        }

        /// <summary>
        /// Creates and returns a collection of local detail editing cards for the supplied Idea and Symbol.
        /// </summary>
        public static EditableList<DetailEditingCard> GenerateLocalDetailsFor(Idea SourceIdea, VisualSymbol SourceSymbol,
                                                                              IEnumerable<DetailEditingCard> GeneratedGlobalDetails)
        {
            General.ContractRequires(SourceSymbol == null || SourceSymbol.OwnerRepresentation.RepresentedIdea == SourceIdea);

            var GeneratedLocalDetailEdits = new EditableList<DetailEditingCard>("LocalDetailEdits", null);

            // Add the custom designated details (for this particular Idea), plus its possible custom formats.
            foreach (var Detail in SourceIdea.Details.Where(det => det.ContentDesignator.IsLocal))
            {
                var TargetEditCard = new DetailEditingCard(true, new Assignment<DetailDesignator>(Detail.Designation, true), Detail);
                GeneratedLocalDetailEdits.Add(TargetEditCard);

                // Set any possible custom format
                if (SourceSymbol != null && TargetEditCard != null
                    && SourceIdea.DetailsCustomLooks.ContainsKey(TargetEditCard.Designator.Value))
                {
                    var FormattedDesignation = SourceIdea.DetailsCustomLooks[TargetEditCard.Designator.Value];
                    if (FormattedDesignation != null && FormattedDesignation.ContainsKey(SourceSymbol))
                        TargetEditCard.SetLook(FormattedDesignation[SourceSymbol]);
                }
            }

            // Finally, add custom formats of Local Internal/Property details (not related to an existent detail).
            // Notice that global-designators (those previously generated) are excluded.
            // IMPORTANT: Don't ask solely for Idea-Definitor's detail designators, because for a Converted Idea
            //            can exist globally-defined detail designators which remain declared by the previous Idea-Definition.
            var AssignedCustomLooks = SourceIdea.DetailsCustomLooks
                     .Where(cuslook => cuslook.Key is LinkDetailDesignator
                                       && !GeneratedLocalDetailEdits.Any(det => det.Designator.Value.IsEquivalent(cuslook.Key))
                                       && (GeneratedGlobalDetails != null &&
                                           !GeneratedGlobalDetails.Any(
                                               det =>
                                               {
                                                   if (!(det.Designator.Value is LinkDetailDesignator))
                                                       return det.Designator.Value.IsEqual(cuslook.Key);

                                                   var dsnlink = det.Designator.Value.GetFinalContent(SourceIdea) as Link;
                                                   var clklink = cuslook.Key.GetFinalContent(SourceIdea) as Link;

                                                   var result = dsnlink.Target.IsEqual(clklink.Target);
                                                   return result;
                                               }))
                /*- && !SourceIdea.IdeaDefinitor.DetailDesignators.Any(dsn => dsn.IsEquivalent(cuslook.Key))*/ );
 
            foreach (var CustLook in AssignedCustomLooks)
                // Notice that no detail is supplied because Internal-Detail is just a designator-to-property reference.
                GeneratedLocalDetailEdits.Add(new DetailEditingCard(true, new Assignment<DetailDesignator>(CustLook.Key, true), null));

            return GeneratedLocalDetailEdits;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public DetailEditingCard(bool IsPreexistent, Assignment<DetailDesignator> Designator, ContainedDetail Detail)
            : base(IsPreexistent, Designator)
        {
            this.DetailContent = Detail;
        }

        public override void SetContent()
        {
            base.SetContent();

            this.IsCustomDesignation = (this.DetailContent != null && !this.DetailContent.Designation.IsEqual(this.Designator.Value));
        }

        public override void SetLook(DetailAppearance Look = null)
        {
            base.SetLook(Look);
            this.IsCustomLook = !this.EditingLook.IsEqual(this.Designator.Value.DetailLook);
        }

        public override bool CanEditDesignator { get { return (this.Designator != null && this.Designator.IsLocal); } }

        // Notice that a Link content can be null for internal-links.
        public bool CanAccessContent { get { return (this.Designator.Value is LinkDetailDesignator || (this.Designator.Value is AttachmentDetailDesignator
                                                                                                       && this.DetailContent != null)); } }

        public bool CanExtractContent { get { return ((this.Designator.Value is AttachmentDetailDesignator || this.Designator.Value is TableDetailDesignator)
                                                      && this.DetailContent != null); } }

        /// <summary>
        /// Detail, which maybe empty.
        /// </summary>
        public ContainedDetail DetailContent
        {
            get { return this.DetailContent_; }
            set
            {
                this.DetailContent_ = value;

                // Just to update the ComboBox using the Designator.AvailableDefinitors property
                NotifyPropertyChange("Designator");
                NotifyPropertyChange("DetailContent");
            }
        }
        private ContainedDetail DetailContent_;
        
        public bool IsCustomDesignation { get; set; }

        public bool IsCustomLook { get; set; }

        public override bool IsDisplayed
        {
            get { return base.IsDisplayed; }
            set
            {
                base.IsDisplayed = value;
                this.IsCustomLook = this.Designator.Value.DetailLook.SettingsDiffersFrom(this.EditingLook);
            }
        }

        public override bool ShowTitle
        {
            get { return base.ShowTitle; }
            set
            {
                base.ShowTitle = value;
                this.IsCustomLook = this.Designator.Value.DetailLook.SettingsDiffersFrom(this.EditingLook);
            }
        }

        public override bool TableIsMultiRecord
        {
            get { return base.TableIsMultiRecord; }
            set
            {
                base.TableIsMultiRecord = value;
                this.IsCustomLook = this.Designator.Value.DetailLook.SettingsDiffersFrom(this.EditingLook);
            }
        }

        public override ETableLayoutStyle TableLayout
        {
            get { return base.TableLayout; }
            set
            {
                base.TableLayout = value;
                this.IsCustomLook = this.Designator.Value.DetailLook.SettingsDiffersFrom(this.EditingLook);
            }
        }

        public override bool TableShowFieldTitles
        {
            get { return base.TableShowFieldTitles; }
            set
            {
                base.TableShowFieldTitles = value;
                this.IsCustomLook = this.Designator.Value.DetailLook.SettingsDiffersFrom(this.EditingLook);
            }
        }
    }
}