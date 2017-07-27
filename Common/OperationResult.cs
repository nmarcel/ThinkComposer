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
// File   : OpResult.cs
// Object : Instrumind.Common.OpResult (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.15 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Encapsulates information about an executed operation.
    /// </summary>
    /// <typeparam name="TResult">Type of the returned value.</typeparam>
    public class OperationResult
    {
        /// <summary>
        /// Creates and returns a success operation result with the supplied result and extra info.
        /// </summary>
        public static OperationResult<TResult> Success<TResult>(TResult Result, string Message = null, object Target = null, object Context = null, params object[] Parameters)
        {
            var Return = new OperationResult<TResult>();

            Return.WasSuccessful = true;
            Return.Result = Result;
            Return.Message = Message;
            Return.Target = Target;
            Return.Context = Context;
            Return.Parameters = Parameters;

            return Return;
        }

        /// <summary>
        /// Creates and returns a failed operation result with the supplied result and extra info.
        /// </summary>
        public static OperationResult<TResult> Failure<TResult>(string Message, object Target = null, object Context = null, TResult Result = default(TResult), params object[] Parameters)
        {
            var Return = Success(Result, Message, Target, Context, Parameters);

            Return.WasSuccessful = false;

            return Return;
        }
    }

    public class OperationResult<TResult>
    {
        /// <summary>
        /// Result value returned by the operation.
        /// </summary>
        public TResult Result { get; internal set; }

        /// <summary>
        /// Indicates whether the operation invocation was successful.
        /// </summary>
        public bool WasSuccessful { get; internal set; }

        /// <summary>
        /// Attached note that can be used for inform the cause of an unsuccessful execution or the status of a successful one.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Target of the operation (optional).
        /// </summary>
        public object Target { get; internal set; }

        /// <summary>
        /// Context of the operation (optional).
        /// </summary>
        public object Context { get; internal set; }

        /// <summary>
        /// Parameters passed (optional).
        /// </summary>
        public IEnumerable<object> Parameters { get; internal set; }
    }
}