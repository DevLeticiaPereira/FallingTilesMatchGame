using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static void AddUniqueToList<T>(ref List<T> list, T element)
    {
        if (!list.Contains(element))
        {
            list.Add(element);
        }
    }
}
