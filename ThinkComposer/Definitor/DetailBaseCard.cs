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
// File   : DetailBaseCard.cs
// Object : Instrumind.ThinkComposer.Definitor.DefinitorUI.DetailBaseCard (Class)
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

using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides the user-interface components for the Domain related Definitions.
namespace Instrumind.ThinkComposer.Definitor.DefinitorUI
{
    /// <summary>
    /// Base for the representation of Idea details or detail definitions to be edited.
    /// </summary>
    public class DetailBaseCard : INotifyPropertyChanged
    {
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public DetailBaseCard(bool IsPreexistent, Assignment<DetailDesignator> Designator, bool IsForDefinition = false)
        {
            General.ContractRequiresNotNull(Designator);

            this.IsPreexistent = IsPreexistent;
            this.Designator = Designator;
            this.IsForDefinition = IsForDefinition;

            this.Name = Designator.Value.Name;
            this.Summary = Designator.Value.Summary;
            this.OwnershipTitle = Designator.Value.OwnershipTitle;
            this.OwnershipSummary = Designator.Value.OwnershipSummary;
            this.KindTitle = General.GetStaticPropertyValue(Designator.Value.GetType(), "KindTitle") as string;
            this.KindSummary = General.GetStaticPropertyValue(Designator.Value.GetType(), "KindSummary") as string;
            this.KindPictogram = General.GetStaticPropertyValue(Designator.Value.GetType(), "KindPictogram") as ImageSource;

            this.IsTableDetail = this.Designator.Value is TableDetailDesignator;

            SetLook();
        }
        private bool IsForDefinition = false;

        public virtual void SetContent()
        {
            this.Name = this.Designator.Value.Name;
            this.Summary = this.Designator.Value.Summary;
            this.OwnershipTitle = this.Designator.Value.OwnershipTitle;
            this.OwnershipSummary = this.Designator.Value.OwnershipSummary;
            this.KindTitle = General.GetStaticPropertyValue(this.Designator.Value.GetType(), "KindTitle") as string;
            this.KindSummary = General.GetStaticPropertyValue(this.Designator.Value.GetType(), "KindSummary") as string;
            this.KindPictogram = General.GetStaticPropertyValue(this.Designator.Value.GetType(), "KindPictogram") as ImageSource;

            this.IsTableDetail = this.Designator.Value is TableDetailDesignator;

            SetLook();
        }

        public virtual void SetLook(DetailAppearance Look = null)
        {
            Look = Look.NullDefault(this.Designator.Value.DetailLook);
            this.EditingLook = (this.IsForDefinition ? Look : Look.Clone());

            if (this.IsTableDetail)
            {
                var TableLook = this.EditingLook as TableAppearance;

                if (TableLook == null)
                    throw new UsageAnomaly("Formatter must be of TableAppearance type because Designator is TableDetailDesignator");
            }
        }

        /// <summary>
        /// Indicates whether this card is for an already existent detail definition, else is for a new one.
        /// </summary>
        public bool IsPreexistent { get; private set; }

        public virtual bool CanEditDesignator { get { return (this.Designator != null); } }

        public virtual bool CanSelectDesignatorDef { get { return (this.Designator != null
                                                                   && this.Designator.Value is TableDetailDesignator
                                                                   && this.Designator.Value.Alterability != EAlterability.System); } }

        public Assignment<DetailDesignator> Designator { get; protected set; }

        public IRecognizableElement DesignatorDefinitor
        {
            get { return this.Designator.Value.Definitor; }
            set
            {
                this.Designator.Value.Definitor = value;
                NotifyPropertyChange("DesignatorDefinitor");
            }
        }

        // NOTE: Do not assign as 'this.Designator.Value.Name = value',
        //       because change presists after cancellation.
        public string Name
        {
            get { return this.Name_; }
            set
            {
                this.Name_ = value;
                NotifyPropertyChange("Name");
            }
        }
        private string Name_;

        public string Summary
        {
            get { return this.Designator.Value.Summary; }
            set
            {
                this.Designator.Value.Summary = value;
                NotifyPropertyChange("Summary");
            }
        }
        
        public string OwnershipTitle { get; set; }
        
        public string OwnershipSummary { get; set; }

        public string KindTitleAndSummary { get { return KindTitle + ": " + KindSummary; } }
        public string KindTitle { get; set; }
        public string KindSummary { get; set; }
        public ImageSource KindPictogram { get; set; }

        public DetailAppearance EditingLook { get; set; }
        
        public virtual bool IsDisplayed
        {
            get { return this.EditingLook.IsDisplayed; }
            set
            {
                this.EditingLook.IsDisplayed = value;
            }
        }

        public static readonly string IsDisplayedTip = DetailAppearance.__IsDisplayed.Summary;

        public virtual bool ShowTitle
        {
            get { return this.EditingLook.ShowTitle; }
            set
            {
                this.EditingLook.ShowTitle = value;
            }
        }

        public static readonly string ShowTitleTip = DetailAppearance.__ShowTitle.Summary;
        
        public bool IsTableDetail { get; set; }

        public virtual bool TableIsMultiRecord
        {
            get
            {
                var Look = this.EditingLook as TableAppearance;
                if (Look == null)
                    return false;

                return Look.IsMultiRecord;
            }
            set
            {
                var Look = this.EditingLook as TableAppearance;
                if (Look == null)
                    return;

                Look.IsMultiRecord = value;
            }
        }

        public static readonly string TableIsMultiRecordTip = TableAppearance.__IsMultiRecord.Summary;

        public virtual ETableLayoutStyle TableLayout
        {
            get
            {
                var Look = this.EditingLook as TableAppearance;
                if (Look == null)
                    return ETableLayoutStyle.Conventional;

                return Look.Layout;
            }
            set
            {
                var Look = this.EditingLook as TableAppearance;
                if (Look == null)
                    return;

                Look.Layout = value;
            }
        }

        public static readonly string TableLayoutTip = TableAppearance.__Layout.Summary;

        public virtual bool TableShowFieldTitles
        {
            get
            {
                var Look = this.EditingLook as TableAppearance;
                if (Look == null)
                    return false;

                return Look.ShowFieldTitles;
            }
            set
            {
                var Look = this.EditingLook as TableAppearance;
                if (Look == null)
                    return;

                Look.ShowFieldTitles = value;
            }
        }

        public static readonly string TableShowFieldTitlesTip = TableAppearance.__ShowFieldTitles.Summary;

        public void NotifyPropertyChange(string PropertyName)
        {
            var Handler = this.PropertyChanged;
            if (Handler != null)
                Handler(this, new PropertyChangedEventArgs(PropertyName));
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return "DetailCard [PreExists=" + this.IsPreexistent.ToString()
                    + "]: Name='" + this.Name + "', Ownership='" + this.OwnershipTitle + "', Kind='" + this.KindTitle + "'.";
        }
    }
}