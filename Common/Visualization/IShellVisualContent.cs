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
// File   : IShellVisualContent.cs
// Object : Instrumind.Common.Visualization.IShellVisualContent (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.21 Néstor Sánchez A.  Creation
//

using System;
using System.Windows;

using Instrumind.Common;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Represents a visual content to be hosted on the application.
    /// </summary>
    public interface IShellVisualContent
    {
        /// <summary>
        /// Key/Code that uniquely identifies this content.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Title for the hosting container.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Returns the purpose type of the underlying content.
        /// </summary>
        EShellVisualContentType ContentType { get; }

        /// <summary>
        /// Returns a reference to the visual object contained.
        /// </summary>
        FrameworkElement ContentObject { get; }

        /// <summary>
        /// Indicates that the content is no longer required to be hosted.
        /// </summary>
        void Discard();

    }
}