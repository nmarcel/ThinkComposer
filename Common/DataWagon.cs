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
// File   : DataWagon.cs
// Object : Instrumind.Common.DataWagon (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.24 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Text;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// General purpose data container for linked objects with attached information into them.
    /// </summary>
    public class DataWagon
    {
        /// Constructor
        public DataWagon(string Name, object Value=null, DataWagon Link=null)
        {
            this.Name = Name;
            this.Value = Value;
            this.Link = Link;
        }

        /// <summary>
        /// Name of the wagon.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Data contained.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Reference to a linked wagon.
        /// </summary>
        public DataWagon Link { get; protected set; }

        /// <summary>
        /// Store for possible attachments.
        /// </summary>
        public Dictionary<string, object> Attachments = null;

        /// <summary>
        /// Creates and returns a new wagon with the supplied value.
        /// </summary>
        /// <param name="Value">Content for the new DataWagon.</param>
        /// <returns>Created DataWagon.</returns>
        public DataWagon Add(string Name, object Value=null, DataWagon Link=null)
        {
            return (new DataWagon(Name, Value, this));
        }

        /// <summary>
        /// Attachs arbitrary tagged/named information to the wagon.
        /// </summary>
        /// <param name="Tag">Tag of the attachment.</param>
        /// <param name="Attachment">Attachment content.</param>
        /// <returns>Same/this wagon.</returns>
        public DataWagon Attach(string Tag, object Attachment)
        {
            if (this.Attachments == null)
                this.Attachments = new Dictionary<string, object>();

            this.Attachments.Add(Tag, Attachment);

            return this;
        }

        /// <summary>
        /// Detachs the indicated attachment.
        /// </summary>
        /// <param name="Tag">Tag of the attachment to discard.</param>
        /// <returns>Same/this wagon.</returns>
        public DataWagon Detach(string Tag)
        {
            if (this.Attachments != null)
                if (this.Attachments.ContainsKey(Tag))
                    this.Attachments.Remove(Tag);

            return this;
        }

        /// <summary>
        /// Returns the content plus attachments of the data wagon chain, expressed as string.
        /// </summary>
        /// <returns>Internal content</returns>
        public override string ToString()
        {
            StringBuilder Content = new StringBuilder();
            this.ToString(Content);

            return Content.ToString();
        }

        /// <summary>
        /// Gives the content plus attachments of the data wagon chain, expressed as string, into a preceding supplied content.
        /// </summary>
        /// <param name="PrecedingContent">Preexisting StringBuilder for attach content.</param>
        public void ToString(StringBuilder Content)
        {
            Content.Append("{'");
            Content.Append(this.Name);
            Content.Append("'=[");
            Content.Append(General.ConvertToString(this.Value));
            Content.Append("]");

            if (this.Attachments!=null && this.Attachments.Count>0)
                foreach(KeyValuePair<string,object> Attachment in this.Attachments)
                {
                    Content.Append(";");
                    Content.Append("('");
                    Content.Append(this.Name);
                    Content.Append("'=[");
                    Content.Append(General.ConvertToString(Attachment.Value));
                    Content.Append("])");
                }

            Content.Append("}");

            if (this.Link != null)
            {
                Content.Append("+");
                this.Link.ToString(Content);
            }
        }

        // PENDING:
        // - REMOVE WAGONS FROM LINKED CHAIN
        // - GET ATTACHMENT BY NAME AND INDEX?

    }
}