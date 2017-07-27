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
// File   : MMemberController.cs
// Object : Instrumind.Common.EntityDefinition.MMemberController (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.01.11 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// Base class for the consumption, from external views, of an entity member.
    /// (Intended to work around problems by lack of support for contra/co+variance)
    public abstract class MMemberController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MMemberController(MEntityInstanceController InstanceController)
        {
            this.InstanceController = InstanceController;
        }

        /// <summary>
        /// References the Instance Controller containing this Member Controller.
        /// </summary>
        public MEntityInstanceController InstanceController { get; protected set; }

        /// <summary>
        /// References the definitior of the controller member type.
        /// </summary>
        public abstract MModelMemberDefinitor Definition { get; }

        /// <summary>
        /// Indicates whether the member instance is editable at this moment.
        /// </summary>
        public bool IsEditableNow { get; internal set; }

        /// <summary>
        /// Indicates whether the current member instance value is valid.
        /// </summary>
        public bool IsValid { get; internal set; }

        /// <summary>
        /// Error message stored if the last validation failed and the member value is still invalid.
        /// </summary>
        public string StatusMessage { get; internal set; }

        /// <summary>
        /// Tells the consumer view that a Detailed Definition indicator should be shown.
        /// </summary>
        public bool IndicateDetailedDefinition { get { return (this.ProvideDetailedDefinition != null); } }

        /// <summary>
        /// Gets the Detailed Definition to show related to the entity instance member.
        /// </summary>
        public Func<MMemberController, object> ProvideDetailedDefinition { get; set; }

        /// <summary>
        /// Tells the consumer view that a Validation indicator should be shown.
        /// </summary>
        public bool IndicateValidation { get; set; }

        /// <summary>
        /// Tells the consumer view that a Default Value indicator should be shown.
        /// </summary>
        public bool IndicateDefaultValue { get; set; }

        /// <summary>
        /// Tells the consumer view that a Complex Options indicator should be shown.
        /// </summary>
        public bool IndicateComplexOptions { get { return (this.ComplexOptionsProviders != null && this.ComplexOptionsProviders.Length > 0); } }

        /// <summary>
        /// Represents a collection of specialized operations for edit complex options of the property value.
        /// In each item, the first component is a Descriptor (with Name, TechName, Summary and Pictogram) and the second is the Options-provider to be called.
        /// </summary>
        public Tuple<IRecognizableElement, Action<object>>[] ComplexOptionsProviders { get; set; }

        /// <summary>
        /// Indicates whether the collection can be empty meaning a full selection (instead of individually selected items).
        /// </summary>
        public bool CanCollectionBeEmpty { get; set; }

        /// <summary>
        /// Title for the collection whem empty (usually for indicate a full selection instead of individually selected items).
        /// </summary>
        public string EmptyCollectionTitle { get; set; }

        /// <summary>
        /// Function to get the external collection to be used as source for a selectable-value/reference property, based on the supplied model-entity context.
        /// </summary>
        public Func<IModelEntity, IEnumerable> ExternalItemsSourceGetter { get; set; }

        /// <summary>
        /// Path of the selected value of the external collection to be used as source for a selectable-value/reference property.
        /// </summary>
        public string ExternalItemsSourceSelectedValuePath { get; set; }

        /// <summary>
        /// Path of the display member of the external collection to be used as source for a selectable-value/reference property.
        /// </summary>
        public string ExternalItemsSourceDisplayMemberPath { get; set; }

        public bool IndicatePreExpoLabel { get { return this.IndicateDetailedDefinition; } }

        public object PreExpoLabelData { get { return (this.ProvideDetailedDefinition == null ? null : this.ProvideDetailedDefinition(this)); } }

        public bool IndicatePostExpoLabel { get { return this.IndicateValidation; } }

        public object PostExpoLabelData { get { return this.IsValid; } }

        public bool IndicatePreExpoValue { get { return this.IndicateDefaultValue; } }

        public bool IndicatePostExpoValue { get { return IndicateComplexOptions; } }

        public string PreExpoLabelTip { get { return "Show definition"; } }

        public string PostExpoLabelTip { get { return "Validation result"; } }

        public string ExpoLabelTip { get { return this.Definition.Summary; } }

        // CANCELLED: Show Data-Type cue (for "Users");
        public string PreExpoValueTip { get { return "Reset to initial value"; } }

        /*- Changed: Use PostExpoValueDescriptor.
         
        public string PostExpoValueTip
        {
            get
            {
                if (this.ComplexOptionsProvider == null || this.ComplexOptionsProvider.Item1 == null)
                    return "Show complex options";

                return this.ComplexOptionsProvider.Item1.Summary.EmptyDefault(this.ComplexOptionsProvider.Item1.Name);
            }
        } */

        public string ExpoValueTip
        {
            get
            {
                var Value = this.Definition.Read(this.InstanceController.ControlledInstance) as IIdentifiableElement;

                if (Value == null)
                    return null;

                return Value.Summary.AbsentDefault(Value.Name);
            }
        }

    }
}