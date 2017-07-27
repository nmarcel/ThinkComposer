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
// File   : Assignment.cs
// Object : Instrumind.Common.Assignment (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.12.09 Néstor Sánchez A.  Creation
//

using System;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// References an object value, or extraction-parameters tuple to it, which is either stored locally or shared with other instances, hence it depends (or is owned) externally.
    /// The idea is later to authorize only the owner to make changes but not the referencers.
    /// Of course, the stored object cannot be an extraction-parameters tuple.
    /// </summary>
    [Serializable]
    public class Assignment<TValue> : MAssignment
    {
        /// <summary>
        /// Constructor (empty) for variable declaration.
        /// </summary>
        public Assignment()
        {
        }

        /// <summary>
        /// Constructor requiring value and locality, for variable assignment.
        /// </summary>
        public Assignment(TValue Value, bool IsLocal)
        {
            this.Value_ = Value;
            this.IsLocal = IsLocal;
        }

        /// <summary>
        /// Constructor requiring instance and member name (extraction parameters), for global variable assignment.
        /// </summary>
        public Assignment(IMModelClass Instance, string MemberName)
        {
            General.ContractRequiresNotNull(Instance);
            General.ContractRequiresNotAbsent(MemberName);

            if (Instance.ClassDefinition.GetMemberDef(MemberName, false) == null)
                throw new UsageAnomaly("The specified Member does not exist for the supplied instance Class.");

            this.Value_ = new Tuple<IMModelClass, string>(Instance, MemberName);
            this.IsLocal = false;
        }

        /// <summary>
        /// Creates and returns a clone of this Assignment.
        /// </summary>
        public override MAssignment CreateClone(object NewAssignedValue = null, bool? NewIsLocal = null)
        {
            var Result = new Assignment<TValue>();
            Result.Value_ = (NewAssignedValue == null
                             ? this.Value_
                             : NewAssignedValue);    // Notice direct internal assignation (which maybe a reference, not a final value).
            Result.IsLocal = (NewIsLocal == null || !NewIsLocal.HasValue
                              ? this.IsLocal
                              : NewIsLocal.Value);

            return Result;
        }

        /// <summary>
        /// Reference to the assigned object which is local or external (shared).
        /// </summary>
        public TValue Value
        {
            get
            {
                if (Value_ is Tuple<IMModelClass, string>)
                {
                    var ExtractionParameters = (Tuple<IMModelClass, string>)Value_;
                    var Accessor = ExtractionParameters.Item1.ClassDefinition.GetMemberDef(ExtractionParameters.Item2);
                    return (TValue)Accessor.Read(ExtractionParameters.Item1);
                }

                return (TValue)Value_;
            }
        }
        private object Value_;

        public override object AssignedValue { get { return this.Value; } }

        public override string ToString()
        {
            return "Assignment (" + (this.IsLocal ? "Local" : "Global") + ", HC=" + this.GetHashCode().ToString()  +"). Value: " + this.Value.ToStringAlways();
        }
    }
}
