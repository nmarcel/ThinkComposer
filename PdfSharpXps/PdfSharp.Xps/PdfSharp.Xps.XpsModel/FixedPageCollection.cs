using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  public class FixedPageCollection : ICollection<FixedPage>
  {
    FixedDocument fdoc;

    internal FixedPageCollection(FixedDocument fdoc)
    {
      this.fdoc = fdoc;
    }

    public void Add(FixedPage item)
    {
      throw new InvalidOperationException();
    }

    public void Clear()
    {
      throw new InvalidOperationException();
    }

    public bool Contains(FixedPage item)
    {
      throw new InvalidOperationException();
    }

    public void CopyTo(FixedPage[] array, int arrayIndex)
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

    public bool Remove(FixedPage item)
    {
      throw new InvalidOperationException();
    }

    public IEnumerator<FixedPage> GetEnumerator()
    {
      int count = this.fdoc.PageCount;
      for (int idx = 0; idx < count; idx++)
        yield return this.fdoc.GetFixedPage(idx);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}