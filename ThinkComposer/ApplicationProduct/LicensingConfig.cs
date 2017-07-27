using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Instrumind.Common;

/// Main manager for the Instrumind ThinkComposer product application.

namespace Instrumind.ThinkComposer.ApplicationProduct
{
    /// <summary>
    /// Stores configuration information about lincesing.
    /// </summary>
    public static class LicensingConfig
    {
        public static Dictionary<string, int> IdeasCreationQuotas = new Dictionary<string, int>
                                { { AppExec.LIC_EDITION_FREE, 100 },
                                  { AppExec.LIC_EDITION_LITE, 200 },
                                  { AppExec.LIC_EDITION_STANDARD, 350 },
                                  { AppExec.LIC_EDITION_PROFESSIONAL, 500 },
                                  { AppExec.LIC_EDITION_ULTIMATE, int.MaxValue } };

        public static Dictionary<string, int> IdeaDefinitionsCreationQuotas = new Dictionary<string, int>
                                { { AppExec.LIC_EDITION_FREE, 6 },
                                  { AppExec.LIC_EDITION_LITE, 8 },
                                  { AppExec.LIC_EDITION_STANDARD, 10 },
                                  { AppExec.LIC_EDITION_PROFESSIONAL, 14 },
                                  { AppExec.LIC_EDITION_ULTIMATE, int.MaxValue } };

        public static Dictionary<string, int> ComposabilityLevelsQuotas = new Dictionary<string, int>
                                { { AppExec.LIC_EDITION_FREE, 2 },
                                  { AppExec.LIC_EDITION_LITE, 2 },
                                  { AppExec.LIC_EDITION_STANDARD, 3 },
                                  { AppExec.LIC_EDITION_PROFESSIONAL, 4 },
                                  { AppExec.LIC_EDITION_ULTIMATE, int.MaxValue } };
    }
}
