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
// File   : StandardBinarySerializer.cs
// Object : Instrumind.Common.StandardBinarySerializer (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.11.12 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Instrumind.Common
{
    /// <summary>
    /// Provides binary serialization capabilities.
    /// </summary>
    public partial class StandardBinarySerializer
    {
        public static readonly byte[] SERIALIZATION_HEADER_CODE;
        public static readonly byte[] SERIALIZATION_TRAILER_CODE;
        public static readonly byte[] FORMAT_VERSION;
        public static readonly byte[] FORMAT_KIND_BINARY;

        static StandardBinarySerializer()
        {
        // IMPORTANT: Note that these format codes are stored in UTF-8, but data strings are in Unicode.
            SERIALIZATION_HEADER_CODE = "IMSS".StringToBytes();
            SERIALIZATION_TRAILER_CODE = @"\[END]".StringToBytes();
            FORMAT_VERSION = "0100".StringToBytes();
            FORMAT_KIND_BINARY = "BIN".StringToBytes();
        }

        public Stream Torrent { get; protected set; }

        public const string TYPEDEC_SEPARATOR = "!";

        public const string MEMBERS_SEPARATOR = "~";

        public const string MEMBER_TYPIFICATION_CODE = ":";

        /// <summary>
        /// Code of bytes-block for Type-Declaration
        /// </summary>
        public const byte BLOCK_TYP_DECL = ((byte)'@');  // To be followed by block-size and type name plus fields def

        /// <summary>
        /// Code of bytes-block for Object-Instance (Concept, Customer, etc.)
        /// </summary>
        public const byte BLOCK_OBJ_INS = ((byte)'$');  // To be followed by type-id, block-size and fields-data

        /// <summary>
        /// Code of bytes-block for Object-Collection (List, Array, Dictionary, etc.)
        /// </summary>
        public const byte BLOCK_OBJ_COL = ((byte)'#');  // To be followed by block-size, items-count and items-data

        /// <summary>
        /// Code of bytes-block for Object-Collection Item
        /// </summary>
        public const byte BLOCK_COL_ITM = ((byte)'%');  // To be followed by collection item type-id, data-size (when variable) and data-value

        /// <summary>
        /// Code of bytes-block for Field-Value data (int, double, string, etc.)
        /// </summary>
        public const byte BLOCK_FLD_VAL = ((byte)'&');  // To be followed by field-id, data-size (when variable) and data-value

        private Dictionary<Type, FieldInfo[]> DetectedTypes = new Dictionary<Type, FieldInfo[]>();

        private List<object> TravelTrace = new List<object>();

        public StandardBinarySerializer(Stream Torrent)
        {
            General.ContractRequiresNotNull(Torrent);

            this.Torrent = Torrent;
        }

        public bool TypeIsDirectlyStorable(Type DataType)
        {
            var Result = false;

            Result = (DataType.IsValueType || DataType == typeof(string));

            return Result;
        }

        public int GetTypeFixedSize(Type DataType)
        {
            var Result = 0;

            if (BytesHandling.FixedSizeSimpleTypes.ContainsKey(DataType))
                Result = BytesHandling.FixedSizeSimpleTypes[DataType];

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
