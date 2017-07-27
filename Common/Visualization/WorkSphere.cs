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
// File   : WorkSphere.cs
// Object : Instrumind.Common.Visualization.WorkSphere (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.30 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Represents an sphere of work about a topic.
    /// Provides commands an invocators to be exposed for automatic or interactive consumption.
    /// For example: "Diagram edition", "Metadata definition", "Database loading".
    /// </summary>
    public abstract class WorkSphere : SimplePresentationElement
    {
        /// <summary>
        /// Associated manager for document's workspace.
        /// </summary>
        public WorkspaceManager WorkspaceDirector { get; protected set; }

        /// <summary>
        /// Visualizer for document views.
        /// </summary>
        public IDocumentVisualizer Visualizer { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkSphere(string Name, string TechName, string Summary, ImageSource Pictogram,
                          WorkspaceManager WorkspaceDirector, IDocumentVisualizer Visualizer)
            : base(Name, TechName, Summary, Pictogram)
        {
            General.ContractRequiresNotNull(WorkspaceDirector);

            this.WorkspaceDirector = WorkspaceDirector;
            this.Visualizer = Visualizer;

            this.CommandAreas = new ReadOnlyCollection<SimpleElement>(this.CommandAreas_);
            this.CommandGroups = new ReadOnlyDictionary<string, List<SimpleElement>>(this.CommandGroups_);
        }

        /// <summary>
        /// Collection with the exposed Command Areas expressed as SimpleEntity.
        /// </summary>
        public ReadOnlyCollection<SimpleElement> CommandAreas { get; protected set; }
        protected List<SimpleElement> CommandAreas_ = new List<SimpleElement>();

        /// <summary>
        /// Exposed Command Groups, where Key=Key of the command Area, Value=Command Groups expressed as a SimpleEntity list.
        /// </summary>
        public ReadOnlyDictionary<string, List<SimpleElement>> CommandGroups { get; protected set; }
        protected Dictionary<string, List<SimpleElement>> CommandGroups_ = new Dictionary<string, List<SimpleElement>>();

        /// <summary>
        /// Work sphere available command expositors (for invoking, binding and executing).
        /// </summary>
        public Dictionary<string, WorkCommandExpositor> CommandExpositors { get { return this.CommandExpositors_; } } 
        private Dictionary<string, WorkCommandExpositor> CommandExpositors_ = new Dictionary<string, WorkCommandExpositor>();

        /// <summary>
        /// Generates a new sequential number for assign a new document name.
        /// </summary>
        /// <returns></returns>
        public int GetNewDocumentNumber()
        {
            return System.Threading.Interlocked.Increment(ref NewDocumentSequence);
        }
        private int NewDocumentSequence = 0;

        /// <summary>
        /// Prefix for naming related documents of this sphere.
        /// </summary>
        public string DocumentsPrefix { get; protected set; }

        /// <summary>
        /// Closes this work sphere and realeses its resources. Returns indication of closing executed or cancelled (e.g. by unsaved changes).
        /// </summary>
        public abstract bool Discard();
    }
}