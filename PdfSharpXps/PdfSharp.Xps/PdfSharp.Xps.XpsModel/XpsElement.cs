using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Xps.Rendering;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Base class for all XPS elements.
  /// </summary>
  public class XpsElement
  {
    /// <summary>
    /// Gets or sets the parent of this element.
    /// </summary>
    public XpsElement Parent
    {
      get { return this.parent; }
      set { this.parent = value; }
    }
    XpsElement parent;

    ///// <summary>
    ///// Gets or sets the ResourceDictionary parent of this element.
    ///// </summary>
    //public ResourceDictionary ResourceParent
    //{
    //  get { return this.resourceParent; }
    //  set { this.resourceParent = value; }
    //}
    //ResourceDictionary resourceParent;

    /// <summary>
    /// Gets the parent or ancestor of this element that is of the specified type.
    /// Returns null if no such ancestor exists.
    /// </summary>
    public T GetParent<T>() where T : XpsElement
    {
      XpsElement elem = Parent;
      while (elem != null)
      {
        if (elem is T)
          return elem as T;
        elem = elem.Parent;
      }
      return null;
    }

    //internal PdfPart Part
    //{
    //  get { return this.part; }
    //  set { this.part = value; }
    //}
    //PdfPart part;
  }
}