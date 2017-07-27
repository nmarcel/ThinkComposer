using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  public class FixedDocumentCollection : ICollection<FixedDocument>
  {
    FixedPayload fpayload;

    internal FixedDocumentCollection(FixedPayload fpayload)
    {
      this.fpayload = fpayload;
    }

    public void Add(FixedDocument item)
    {
      throw new InvalidOperationException();
    }

    public void Clear()
    {
      throw new InvalidOperationException();
    }

    public bool Contains(FixedDocument item)
    {
      throw new InvalidOperationException();
    }

    public void CopyTo(FixedDocument[] array, int arrayIndex)
    {
      throw new InvalidOperationException();
    }

    public int Count
    {
      get { throw new InvalidOperationException(); }
    }

    public bool IsReadOnly
    {
      get { throw new InvalidOperationException(); }
    }

    public bool Remove(FixedDocument item)
    {
      throw new InvalidOperationException();
    }

    public IEnumerator<FixedDocument> GetEnumerator()
    {
      int count = this.fpayload.DocumentCount;
      for (int idx = 0; idx < count; idx++)
        yield return this.fpayload.GetDocument(idx);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}