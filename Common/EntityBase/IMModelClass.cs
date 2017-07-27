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
// File   : IMModelClass.cs
// Object : Instrumind.Common.EntityBase.IMModelClass (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.25 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Base non-generic interface for defined model classes.
    /// (Intended to work around problems by lack of support for contra/co+variance and for internal usage only)
    /// </summary>
    public interface IMModelClass : ICopyable, INotifyPropertyChanged
    {
        /// <summary>
        /// Definitor of the represented model class.
        /// </summary>
        MModelClassDefinitor ClassDefinition { get; }

        /// <summary>
        /// Notifies all entity subscriptors that a property has changed.
        /// </summary>
        void NotifyPropertyChange(MModelPropertyDefinitor PropertyDef);
    }
}