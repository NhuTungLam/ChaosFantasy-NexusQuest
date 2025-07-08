using System.Collections.Generic;
using UnityEngine;

// Pools of objects to pre-instantiate the objects before runtime
// This removes the need for creating and destroying objects while the game is running
// VERY IMPORTANT for games with many objects
public static class ObjectPools
{
    // A dictionary contains all object pool types. Ex: enemies, projectiles,....
    public static Dictionary<string, Queue<Component>> poolDictionary = new();

    //
    public static Dictionary<string, Component> poolBackUp = new();

    // Set up an object pool of a specific type of object
    public static void SetupPool<T>(T ItemPrefab, int poolSize, string entry) where T : Component
    {
        if (poolDictionary.ContainsKey(entry)) return;
        // Create an entry for the pool 
        poolDictionary.Add(entry, new Queue<Component>());

        poolBackUp.Add(entry, ItemPrefab);

        // Initiate the pool
        for (int i = 0; i < poolSize; i++)
        {
            T poolInstance = Object.Instantiate(ItemPrefab);
            poolInstance.gameObject.SetActive(false);
            poolDictionary[entry].Enqueue(poolInstance);
        }
    }

    // Put the objects of type T back into the queue
    public static void EnqueueObject<T>(T item, string name) where T : Component
    {
        // If the object is already inactive then return
        if (!item.gameObject.activeSelf)
        {
            return;
        }

        item.transform.position = Vector2.zero;
        item.gameObject.SetActive(false);
        poolDictionary[name].Enqueue(item);
    }

    // Push the objects of type T out of the queue to run in the game
    public static T DequeueObject<T>(string name) where T : Component
    {
        if (poolDictionary[name].TryDequeue(out var item))
        {
            return (T)item;
        }

        return (T)InstantiateNewInstance(poolBackUp[name], name);
    }

    // This will be called whenever the queue has no more object to dequeue
    public static T InstantiateNewInstance<T>(T item, string name) where T : Component
    {
        T newInstance = Object.Instantiate(item);
        newInstance.gameObject.SetActive(false);
        newInstance.transform.position = Vector2.zero;
        poolDictionary[name].Enqueue(newInstance); // Add the item to pool dictionary
        return newInstance;
    }

    public static void ClearPools()
    {
        poolDictionary.Clear();
        poolBackUp.Clear();
    }
}
