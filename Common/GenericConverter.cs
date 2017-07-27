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
// File   : GenericConverter.cs
// Object : Instrumind.Common.GenericConverter (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.06.24 Néstor Sánchez A.  Creation
//

using System;
using System.Windows.Data;
using System.Globalization;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Implements a generic conversion, from a source to a target value of different types.
    /// </summary>
    /// <typeparam name="TStorageSource">Source type of the stored value.</typeparam>
    /// <typeparam name="TExpositionTarget">Target type of the exposed value.</typeparam>
    public class GenericConverter<TStorageSource, TExpositionTarget> : IValueConverter
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public GenericConverter(Func<TStorageSource, TExpositionTarget> ConvertOperation, Func<TExpositionTarget, TStorageSource> ConvertBackOperation)
        {
            this.ConvertOperation = ConvertOperation;
            this.ConvertBackOperation = ConvertBackOperation;
        }

        public Func<TStorageSource, TExpositionTarget> ConvertOperation { get; protected set; }

        public Func<TExpositionTarget, TStorageSource> ConvertBackOperation { get; protected set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TStorageSource) && value != null)
                throw new InvalidOperationException("Source to convert is not a " + typeof(TStorageSource).Name + ".");

            var Result = this.ConvertOperation((TStorageSource)value);
            return Result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TExpositionTarget) && value != null)
                throw new InvalidOperationException("Source to convert-back is not a " + typeof(TExpositionTarget).Name + ".");

            var Result = this.ConvertBackOperation((TExpositionTarget)value);
            return Result;
        }
    }
}