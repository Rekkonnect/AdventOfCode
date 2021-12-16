﻿using AdventOfCSharp.Utilities;
using System.Collections;

namespace AdventOfCode.Utilities;

public abstract class BaseGrid<TElement, TLocation> : IEnumerable<TElement>
{
    public NextValueCounterDictionary<TElement> ValueCounters { get; protected init; }

    public abstract TLocation Dimensions { get; }
    public abstract TLocation Center { get; }

    public abstract FlexibleListDictionary<TElement, TLocation> ElementDictionary { get; }

    #region Enumerator
    public abstract IEnumerator<TElement> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    public abstract TElement this[TLocation location] { get; set; }
}
