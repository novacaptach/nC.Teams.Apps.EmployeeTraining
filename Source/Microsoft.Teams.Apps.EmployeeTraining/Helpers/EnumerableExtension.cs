// <copyright file="EnumerableExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers;

using System;
using System.Collections.Generic;

/// <summary>
/// This class lists the extension methods for enumerable collections.
/// </summary>
public static class EnumerableExtension
{
    /// <summary>
    /// This method is to split list into given batch size.
    /// </summary>
    /// <typeparam name="T">T type.</typeparam>
    /// <param name="source">Source list to split.</param>
    /// <param name="nSize">Size value to split the list with 40 as default value.</param>
    /// <returns>A <see cref="IEnumerable{TResult}" /> representing the sub-lists by specified size.</returns>
    public static IEnumerable<List<T>> SplitList<T>(
        this List<T> source,
        int nSize = 40)
    {
        source = source ?? throw new ArgumentNullException(paramName: nameof(source));

        for (var i = 0; i < source.Count; i += nSize)
        {
            yield return source.GetRange(index: i, count: Math.Min(val1: nSize, val2: source.Count - i));
        }
    }

    /// <summary>
    /// Checks if source and destination collections are null before adding items.
    /// </summary>
    /// <typeparam name="T">Generic type T.</typeparam>
    /// <param name="source">Collection whos items will be added.</param>
    /// <param name="destination">Collection into which items will be added.</param>
    public static void AddTo<T>(
        this List<T> source,
        List<T> destination)
    {
        if ((source != null) && (destination != null))
        {
            source.AddRange(collection: destination);
        }
    }
}