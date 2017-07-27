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
// File   : ISphereModel.cs
// Object : Instrumind.Common.Visualization.ISphereModel (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.13 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;
using System.Collections.Generic;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using System.Windows.Media;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Represents the universe modeled by a work sphere.
    /// </summary>
    public interface ISphereModel : IFormalizedRecognizableElement, ISelectable, IIdentificationScope, INotifyPropertyChanged
    {
        /// <summary>
        /// Title for the modeled sphere.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Overview of the modeled sphere.
        /// </summary>
        string Overview { get; }

        /// <summary>
        /// Representative image of the modeled sphere.
        /// </summary>
        ImageSource Icon { get;  }

        /// <summary>
        /// View currently active of this work Document Engine.
        /// </summary>
        IDocumentView ActiveDocumentView { get; }

        /// <summary>
        /// Document engine in charge of the sphere document.
        /// </summary>
        DocumentEngine DocumentEditEngine { get; }

        /// <summary>
        /// Relative location of the sphere document.
        /// </summary>
        Uri Location { get; }

        /// <summary>
        /// User-level location of the sphere document.
        /// </summary>
        string SimplifiedLocation { get; }

        /// <summary>
        /// Gets the navigable items of the sphere document.
        /// </summary>
        System.Collections.IEnumerable NavigableItems { get; }
    }
}