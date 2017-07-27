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
// File   : WorkspaceManager.cs
// Object : Instrumind.Common.Visualization.WorkspaceManager (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.01 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Instrumind.Common;
using Instrumind.Common.EntityBase;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Manages and organizes the multiple documents a user can be working on.
    /// </summary>
    public class WorkspaceManager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ShellProvider">The provider of application shell services.</param>
        public WorkspaceManager(IShellProvider ShellProvider)
        {
            General.ContractRequiresNotNull(ShellProvider);
            this.ShellProvider = ShellProvider;
        }

        /// <summary>
        /// Adds and activates a new document to the managed ones.
        /// </summary>
        /// <param name="NewDocument">Document being added.</param>
        public void LoadDocument(ISphereModel NewDocument)
        {
            General.ContractRequiresNotNull(NewDocument);

            if (this.Documents.Contains(NewDocument))
                throw new UsageAnomaly("Cannot add an already registered document to the Workspace", NewDocument);

            this.Documents.Add(NewDocument);
            this.ActiveDocument = NewDocument;
        }

        /// <summary>
        /// Activates the supplied document.
        /// </summary>
        public void ActivateDocument(ISphereModel Document)
        {
            this.ActiveDocument = Document;
        }

        /// <summary>
        /// Removes the specified document from the managed ones.
        /// </summary>
        /// <param name="TargetDocument">Document to be removed.</param>
        public void RemoveDocument(ISphereModel TargetDocument)
        {
            General.ContractRequiresNotNull(TargetDocument);

            if (!this.Documents.Contains(TargetDocument))
                return;
                //- throw new UsageAnomaly("Document intended to be removed is not registered in the Workspace.", TargetDocument);

            int ActiveDocIndex = (this.ActiveDocument == null ? -1 : this.Documents.IndexOf(this.ActiveDocument));

            this.Documents.Remove(TargetDocument);

            var Handler = PostDocumentRemoval;
            if (Handler != null)
                Handler(this, TargetDocument);

            if (ActiveDocIndex >= 0)
                this.ActiveDocument = (ActiveDocIndex < this.Documents.Count ? this.Documents[ActiveDocIndex] : (this.Documents.Count > 0 ? this.Documents[ActiveDocIndex - 1] : null));
            else
                this.ActiveDocument = null;
        }

        /// <summary>
        /// Assigned shell provider.
        /// </summary>
        public IShellProvider ShellProvider { get; protected set; }

        /// <summary>
        /// List of managed documents.
        /// </summary>
        // NOTE: Refresh problems were not solved using an EditableList.
        public ObservableCollection<ISphereModel> Documents = new ObservableCollection<ISphereModel>();

        /// <summary>
        /// Gets or sets the current active document.
        /// </summary>
        public ISphereModel ActiveDocument
        {
            get
            {
                return this.ActiveDocument_;
            }
            set
            {
                if (this.ActiveDocument_ == value)
                    return;

                var PreHandler = this.PreDocumentDeactivation;
                if (PreHandler != null && this.ActiveDocument_ != null)
                    PreHandler(this, this.ActiveDocument_);

                this.ActiveDocument_ = value;
                EntityEditEngine.ActiveEntityEditor = (value == null ? null : value.DocumentEditEngine as EntityEditEngine);

                if (value == null)
                    return;

                if (!this.Documents.Contains(value))
                    throw new UsageAnomaly("Document intended to be activated is not registered in the Workspace.", value);

                var PostHandler = this.PostDocumentActivation;
                if (PostHandler != null)
                    PostHandler(this, value);
            }
        }
        private ISphereModel ActiveDocument_ = null;

        /// <summary>
        /// Gets the current active document engine
        /// </summary>
        public DocumentEngine ActiveDocumentEngine { get { return (this.ActiveDocument == null ? null : this.ActiveDocument.DocumentEditEngine); } }

        /// <summary>
        /// Action to take when a document is removed.
        /// </summary>
        [NonSerialized]
        public Action<WorkspaceManager, ISphereModel> PostDocumentRemoval;

        /// <summary>
        /// Action to take when a document is going to be deactivated.
        /// </summary>
        [NonSerialized]
        public Action<WorkspaceManager, ISphereModel> PreDocumentDeactivation;

        /// <summary>
        /// Action to take when a document is activated.
        /// </summary>
        [NonSerialized]
        public Action<WorkspaceManager, ISphereModel> PostDocumentActivation;

        /// <summary>
        /// Action to take when a document has changed its status.
        /// </summary>
        [NonSerialized]
        public Action<WorkspaceManager, ISphereModel> PostDocumentStatusChange;

    }
}