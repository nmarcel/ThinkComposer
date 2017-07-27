using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Instrumind.Common
{
    /// <summary>
    /// Represents an object capable of update its version and its parents/owners hierarchy.
    /// </summary>
    public interface IVersionUpdater
    {
        /// <summary>
        /// Updates the version and propagates to parents/owners hierarchy.
        /// </summary>
        void UpdateVersion();
    }
}
