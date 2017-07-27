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
// File   : Company.cs
// Object : Instrumind.Common.Company (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.19 Néstor Sánchez A.  Creation
//

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// Instrumind company information.
    public static class Company
    {
        public const string NAME_SIMPLE = "Instrumind"; // Used for directory name generation and product name prefix.
        public const string NAME_LEGAL = "Néstor Marcel Sánchez Ahumada";   // "Instrumind Software S.p.A.";
        public const string WEBSITE_URL = "http://thinkcomposer.codeplex.com";
        //- public const string CONTACT_EMAIL = "contact@instrumind.com";
        //- public const string SUPPORT_EMAIL = "support@instrumind.com";
        public const string PUBLICKEY_TOKEN = General.UNSPECIFIED;
        public const string GENERIC_CONTENTTYPE_CODE = "application/x-instrumind";
    }
}