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
// File   : SynthBinarySerializer.cs
// Object : Instrumind.Common.SynthBinarySerializer (Class)
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
using System.Text;
using System.Collections;

namespace Instrumind.Common
{
    /// <summary>
    /// Provides binary serialization capabilities.
    /// </summary>
    public partial class StandardBinarySerializer
    {
        protected BinaryWriter TorrentWriter { get; set; }

        /// <summary>
        /// Serializes the supplied object Graph to the underlying stream.
        /// </summary>
        public void Serialize(object GraphRoot)
        {
            General.ContractRequiresNotNull(GraphRoot);
            if (GraphRoot.GetType().IsValueType)
                throw new UsageAnomaly("Object to serialize must be a class instance.");

            // Initialize detected types
            this.DetectedTypes.Clear();
            this.DetectedTypes.Add(typeof(object), new FieldInfo[]{});  // For null references

            // Initialize writer
            this.TorrentWriter = new BinaryWriter(this.Torrent, Encoding.Unicode);

            // Write Header
            this.TorrentWriter.Write(BytesHandling.FusionateByteArrays(SERIALIZATION_HEADER_CODE,
                                                                       FORMAT_VERSION,
                                                                       FORMAT_KIND_BINARY));

            // Travel Graph and Write Content
            this.GenerateObjectBlock(GraphRoot);

            // Write Trailer
            this.TorrentWriter.Write(BytesHandling.FusionateByteArrays(SERIALIZATION_TRAILER_CODE));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        protected FieldInfo[] RegisterType(Type InsType)
        {
            FieldInfo[] Fields = null;

            if (DetectedTypes.AddNew(InsType, null))
            {
                Fields = InsType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                            .Where(fld => !fld.Attributes.HasFlag(FieldAttributes.NotSerialized)
                                          && fld.FieldType.BaseType != typeof(MulticastDelegate)
                                          && fld.FieldType.BaseType != typeof(Delegate)).ToArray();
                DetectedTypes[InsType] = Fields;

                var TypeDeclaration = InsType.Namespace + "." + InsType.Name + TYPEDEC_SEPARATOR +
                                      Fields.GetConcatenation(fld => fld.Name.CutBetween("<", ">k__BackingField", true)
                                                                     + MEMBER_TYPIFICATION_CODE + fld.FieldType.Name,
                                                              MEMBERS_SEPARATOR);

                var Block = BytesHandling.FusionateByteArrays(BLOCK_TYP_DECL.IntoArray(), TypeDeclaration.StringToBytes().PreppendLength());

                this.TorrentWriter.Write(Block);
            }
            else
                Fields = DetectedTypes[InsType];

            return Fields;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates an object-block.
        /// Returns type-id and object-id to be used as reference data.
        /// </summary>
        protected Tuple<int, int> GenerateObjectBlock(object Instance)
        {
            if (Instance == null)
                return null;

            var InsType = Instance.GetType();

            // If already traveled, just return id.
            var InstanceId = TravelTrace.IndexOf(Instance);
            if (InstanceId >= 0)
                return Tuple.Create(InsType == null ? -1 : DetectedTypes.IndexOfKey(InsType), InstanceId);

            InstanceId = TravelTrace.Count + 1;
            TravelTrace.Add(Instance);

            var ContainedFields = RegisterType(InsType);
            var TypeId = this.DetectedTypes.IndexOfKey(InsType);

            var BlocksToWrite = new List<byte[]>();

            if (typeof(IEnumerable).IsAssignableFrom(InsType))
            {
                var Items = ((IEnumerable)Instance).Cast<object>();
                var ItemsCount = Items.Count(item =>
                                             {
                                                 if (item != null)
                                                     RegisterType(item.GetType());  // Detect new types

                                                 return true;   // include all, even empties
                                             });

                this.TorrentWriter.Write(BLOCK_OBJ_COL);
                //- BlocksToWrite.Add(BLOCK_OBJ_COL.IntoArray());

                this.TorrentWriter.Write(ItemsCount);
                //- BlocksToWrite.Add(BitConverter.GetBytes(ItemsCount));

                foreach (var Item in Items)
                {
                    var ItemBlock = GenerateCollectionItemBlock(Item);
                    this.TorrentWriter.Write(ItemBlock);
                    //- BlocksToWrite.Add(ItemBlock);
                }
            }
            else
            {
                this.TorrentWriter.Write(BLOCK_OBJ_INS);
                //- BlocksToWrite.Add(BLOCK_OBJ_INS.IntoArray());

                this.TorrentWriter.Write(BitConverter.GetBytes(TypeId));
                //- BlocksToWrite.Add(BitConverter.GetBytes(TypeId));

                foreach (var Field in ContainedFields)
                {
                    var Value = Field.GetValue(Instance);
                    var DefaultTypeValue = InsType.GetDefaultValue();

                    if (Value == DefaultTypeValue)
                        continue;

                    var FieldBlock = GenerateObjectFieldBlock(InsType, Field, Value);
                    this.TorrentWriter.Write(FieldBlock);
                    //- BlocksToWrite.Add(FieldBlock);
                }
            }

            /*- // Now, after the object-graph tree has been traveled, proceed to write
            foreach (var Block in BlocksToWrite)
                this.TorrentWriter.Write(Block); */

            return Tuple.Create(DetectedTypes.IndexOfKey(Instance.GetType()), InstanceId);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates an object field as bytes-array, and returns it.
        /// </summary>
        protected byte[] GenerateObjectFieldBlock(Type ContainerObjectType, FieldInfo SourceField, object SourceObject)
        {
            byte[] Result = null;

            var DataType = SourceField.FieldType;
            var FieldId = BitConverter.GetBytes(Array.IndexOf<FieldInfo>(DetectedTypes[ContainerObjectType], SourceField));
            byte[] Data = null;

            Data = GenerateNonObjectDataBlock(DataType, SourceObject);
            if (Data == null)
            {
                var ObjectTypeIdAndInstanceId = GenerateObjectBlock(SourceObject);
                if (ObjectTypeIdAndInstanceId != null)
                    Data = BitConverter.GetBytes(ObjectTypeIdAndInstanceId.Item2);  // Only get the instance-id
            }

            Result = BytesHandling.FusionateByteArrays(BLOCK_FLD_VAL.IntoArray(), FieldId, Data);

            return Result;
        }

        /// <summary>
        /// Generates a collection item as bytes-array, and returns it.
        /// </summary>
        protected byte[] GenerateCollectionItemBlock(object SourceObject)
        {
            byte[] Result = null;

            int TypeId = -1;
            var DataType = (SourceObject == null ? typeof(object) : SourceObject.GetType());
            byte[] Data = null;

            Data = GenerateNonObjectDataBlock(DataType, SourceObject);
            if (Data == null)
            {
                var ObjectTypeIdAndInstanceId = GenerateObjectBlock(SourceObject);
                if (ObjectTypeIdAndInstanceId != null)
                {
                    TypeId = ObjectTypeIdAndInstanceId.Item1;
                    Data = BitConverter.GetBytes(ObjectTypeIdAndInstanceId.Item2);  // Only get the instance-id
                }
            }
            else
                TypeId = DetectedTypes.IndexOfKey(DataType);

            if (Data != null && TypeId < 0)
                throw new InternalAnomaly("Type should be already registered: " + DataType.Name);

            Result = BytesHandling.FusionateByteArrays(BLOCK_COL_ITM.IntoArray(),
                                                       (TypeId < 0 ? new byte[0] : BitConverter.GetBytes(TypeId)),
                                                       Data);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public byte[] GenerateNonObjectDataBlock(Type DataType, object SourceObject)
        {
            byte[] Data = null;

            if (DataType == typeof(byte[]))
                Data = BytesHandling.FusionateByteArrays(true, (byte[])SourceObject);
            else
                if (DataType == typeof(string))
                    Data = BytesHandling.FusionateByteArrays(true, BytesHandling.StringToBytesUnicode((string)SourceObject));
                else
                    if (DataType == typeof(decimal))
                    {
                        var Parts = decimal.GetBits((decimal)SourceObject).Select(intval => BitConverter.GetBytes(intval)).ToArray();
                        Data = BytesHandling.FusionateByteArrays(true, Parts);
                    }
                    else
                        if (DataType.IsValueType)
                            Data = BytesHandling.GetBytesFromFixedLengthTypeObject(SourceObject, true);

            return Data;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
