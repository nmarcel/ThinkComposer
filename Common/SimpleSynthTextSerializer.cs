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
// File   : SimpleSynthSerializer.cs
// Object : Instrumind.Common.SimpleSynthSerializer (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.06.03 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /* TO DO:
     * - Detect, by each entity model class, which members are serializable but no entity aware.
     * - Define Simple-Synth format
     * - Determine fast copy-pasteable code block for enable serialization of each base ancestor entity model.
     * - Implement and test.
     */

    /* SIMPLE-SYNTH FORMAT
     
SAMPLE-BEGIN (STRING ENCODED)							
IMSS0100TXT							
01234567-0123-0123-0123-0123456789AB							
{							
Name=Composition 1							
TechName=Composition_1							
Summary=This is the summary							
second line of the summary							
third and final line~							
Version:11223344-1122-1122-1122-112233445566							
Rect={X=1740.23~Y=2128.04~Size{Width=102.42~Height=98.5}}							
Anumber=0.12							
}							
11223344-1122-1122-1122-112233445566							
{							
Creator=Nsanchez							
Creation=20101214 134533021							
LastModifier=User01							
LastModification=20101214 134533021							
Version=0102							
}							
\[END]0123456789ABCDEF0123456789ABCDEF							
SAMPLE-END							

     * */

    /// <summary>
    /// Provides services for serializing objects in the Instrumind's Simple-Synth format.
    /// </summary>
    public class SimpleSynthSerializer : IFormatter
    {
        public const string VERSION = "0100";
        public const string SERIALIZATION_HEADER_CODE = "IMSS";
        public const string SERIALIZATION_TRAILER_CODE = @"\[END]";
        public const string FORMAT_CODE_TEXT = "TXT";
        public const string FORMAT_CODE_BINARY = "BIN";

        protected Stream SerializationStream { get; set; }

        protected StreamWriter Writer { get; set; }

        public object Graph { get; protected set; }

        public bool FormatIsBinary { get; protected set; }

        public StreamingContext Context { get; set; }

        public void Serialize(Stream SerializationStream, object Graph)
        {
            this.Serialize(SerializationStream, Graph, false);
        }

        public void Serialize(Stream SerializationStream, object Graph, bool FormatIsBinary)
        {
            General.ContractRequiresNotNull(SerializationStream, Graph);

            this.SerializationStream = SerializationStream;
            this.Graph = Graph;
            this.FormatIsBinary = FormatIsBinary;
            this.Context = new StreamingContext(StreamingContextStates.All);

            /* var ModelClass = graph as IMModelClass;
            if (ModelClass == null)
                throw new UsageAnomaly("Cannot serialize a non Instrumind object graph to Simple-Synth format.", graph); */

            this.Writer = new StreamWriter(this.SerializationStream);

            this.WriteHeader();
            this.WriteGraph(Graph);
            this.WriteTrailer();

            this.Writer.Flush();
            this.Writer.Close();
        }

        public void WriteHeader()
        {
            this.Writer.WriteLine(SERIALIZATION_HEADER_CODE + VERSION + (this.FormatIsBinary ? FORMAT_CODE_BINARY : FORMAT_CODE_TEXT));
        }

        public bool WriteGraph(object Root)
        {
            if (Root == null)
                return false;

            var SourceType = Root.GetType();
            // string GeneratedContent = null;

            return true;
        }

        public void WriteObjectRepresentation(Type SourceType, object Source)
        {
        }

        public string GeneratePropertyRepresentation(Type SourceType, object Source)
        {
            return "PropertyValue";
        }

        public string GenerateValueRepresentation(Type SourceType, object Source)
        {
            return "ValueRepresentation";
        }

        public void GenerateCollectionRepresentation(Type SourceType, object Source)
        {
        }

        public void WriteTrailer()
        {
            this.Writer.WriteLine(SERIALIZATION_TRAILER_CODE);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        protected Dictionary<Guid, object> ObjectsCatalog = new Dictionary<Guid, object>(256);

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public object Deserialize(Stream serializationStream)
        {
            throw new NotImplementedException();
        }

        public SerializationBinder Binder { get; set; }

        public ISurrogateSelector SurrogateSelector { get; set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}