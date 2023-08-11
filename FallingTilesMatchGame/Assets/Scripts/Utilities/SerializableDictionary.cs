using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();
    
    [SerializeField]
    private List<TValue> values = new List<TValue>();
    
    private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        
        foreach (var kvp in dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        dictionary.Clear();
        
        if (keys.Count != values.Count)
        {
            Debug.LogError("Keys and values count mismatch in SerializableDictionary.");
            return;
        }

        for (int i = 0; i < keys.Count; i++)
        {
            dictionary[keys[i]] = values[i];
        }
    }

    public Dictionary<TKey, TValue> ToDictionary()
    {
        return new Dictionary<TKey, TValue>(dictionary);
    }

    public void Add(TKey key, TValue value)
    {
        dictionary[key] = value;
    }

    public bool Remove(TKey key)
    {
        return dictionary.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return dictionary.TryGetValue(key, out value);
    }
}