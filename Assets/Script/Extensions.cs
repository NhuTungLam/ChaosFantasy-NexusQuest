using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public static class Extensions
{
    public static bool HasComponent<T>(this GameObject flag) where T : Component
    {
        return flag.GetComponent(typeof(T)) != null;
    }
    public static string DecimalFormat(this float val, int decimalPlaces)
    {
        return val.ToString("F" + decimalPlaces);
    }
    public static Transform Clear(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        return transform;
    }

    public static T FindName<T>(this List<T> list, string name)
        where T : ScriptableObject
    {
        foreach (T obj in list)
        {
            if (obj.name == name)
                return obj;
        }
        return default(T);
    }
    public static int WeighedChance(this List<float> list)
    {
        float sum = 0;
        float ran = Random.Range(0f, 100f);
        int result = list.Count - 1;
        for (int i = 0; i < list.Count; i++)
        {
            sum += list[i];
            if (ran < sum)
            {
                result = i;
                break;
            }
        }
        return result;
    }

    public static T GetRandom<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            //Debug.LogWarning("GetRandom was called on a null or empty list.");
            return default; // Returns null for reference types and default values for value types
        }

        return list[Random.Range(0, list.Count)];
    }
    public static List<T> GetRandom<T>(this List<T> list, int count)
    {
        if (list == null || list.Count == 0 || count <= 0)
        {
            return new List<T>(); // Return an empty list if invalid input
        }

        count = Mathf.Min(count, list.Count); // Ensure count does not exceed available items

        List<T> availableItems = new List<T>(list); // Create a copy to modify
        List<T> randomItems = new List<T>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableItems.Count);
            randomItems.Add(availableItems[randomIndex]);
            availableItems.RemoveAt(randomIndex); // Ensure uniqueness
        }

        return randomItems;
    }
    public static Color HexColor(this string value)
    {
        ColorUtility.TryParseHtmlString(value, out Color myColor);
        return myColor;
    }
    public static Color SetAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
    public static bool HasTransform(this Transform t, string s)
    {
        Transform check = t.Find(s);
        return check != null;
    }

    //try this so that i can feel not being an idiot
    public static void Invoke(this MonoBehaviour mb, Action f, float delay, bool useUnscaledTime = false)
    {
        mb.StartCoroutine(InvokeRoutine(f, delay, useUnscaledTime));
    }

    private static IEnumerator InvokeRoutine(Action f, float delay, bool useUnscaledTime)
    {
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(delay); // Ignores time scale
        else
            yield return new WaitForSeconds(delay); // Uses scaled time

        f?.Invoke();
    }
    public static float ShortestDistanceTo(this CircleCollider2D circle1, CircleCollider2D circle2)
    {
        // Get world positions of the centers
        Vector2 center1 = circle1.transform.position + (Vector3)circle1.offset;
        Vector2 center2 = circle2.transform.position + (Vector3)circle2.offset;

        // Calculate the distance between the two centers
        float centerDistance = Vector2.Distance(center1, center2);

        // Subtract the radii of both circles
        float perimeterDistance = centerDistance - (circle1.radius * circle1.transform.lossyScale.x + circle2.radius * circle2.transform.lossyScale.x);

        // If circles overlap, return 0 (no distance between perimeters)
        return Mathf.Max(0, perimeterDistance);
    }
    public static GameObject GetHighestParent(this GameObject gameObject)
    {
        if (gameObject == null)
            throw new System.ArgumentNullException(nameof(gameObject), "GameObject cannot be null.");

        Transform current = gameObject.transform;

        while (current.parent != null)
        {
            current = current.parent;
        }

        return current.gameObject;
    }
    public static Component AddComponentByString(this GameObject target, string componentName)
    {
        var type = System.Type.GetType(componentName);
        if (type != null && typeof(Component).IsAssignableFrom(type))
        {
            var existingComponent = target.GetComponent(type);
            if (existingComponent == null)
            {
                var addedComponent = target.AddComponent(type);
                Debug.Log($"Added {type.Name} to {target.name}");
                return addedComponent;
            }
            else
            {
                Debug.LogWarning($"{type.Name} already exists on {target.name}");
                return existingComponent;
            }
        }
        else
        {
            Debug.LogError("Invalid Component type or not a valid MonoBehaviour.");
            return null;
        }
    }

    public static Vector3 ToWorldPosition(this RectTransform uiElement, Camera uiCamera)
    {
        if (uiElement == null || uiCamera == null)
        {
            Debug.LogError("RectTransform or Camera is null.");
            return Vector3.zero;
        }

        // Get the screen space position of the RectTransform
        Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, uiElement.position);

        // Convert screen position to world position
        return uiCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, uiCamera.nearClipPlane));
    }

    public static KeyValuePair<TKey, TValue> GetRandom<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            throw new System.InvalidOperationException("Cannot get a random element from an empty or null dictionary.");
        }

        int index = Random.Range(0, dictionary.Count);
        return dictionary.ElementAt(index);
    }

    public static TKey GetRandomKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            throw new System.InvalidOperationException("Cannot get a random key from an empty or null dictionary.");
        }

        int index = Random.Range(0, dictionary.Count);
        return dictionary.Keys.ElementAt(index);
    }

    public static TValue GetRandomValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            throw new System.InvalidOperationException("Cannot get a random value from an empty or null dictionary.");
        }

        int index = Random.Range(0, dictionary.Count);
        return dictionary.Values.ElementAt(index);
    }

    public static Vector2 RandomPointInCircle(this Vector2 center, float radius, float minDistance = 0)
    {
        float angle = Random.Range(0f, Mathf.PI * 2); // Random angle in radians
        float distance = Mathf.Sqrt(Random.Range(minDistance * minDistance, radius * radius)); // Random distance using square root for uniform distribution
        return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
    }
    public static Vector2 ToVector2(this Vector3 v)
    {
        return (Vector2)v;
    }
    public static Vector3 ToVector3(this Vector2 v)
    {
        return (Vector3)v;
    }
    public static Vector3 Vec3(this float value) => new Vector3(value, value, value);
    public static Vector2 Vec2(this float value) => new Vector2(value, value);
    //yeah why??
    public static List<Vector3> DivideDistance(this Vector3 start, Vector3 end, int segments)
    {
        List<Vector3> points = new List<Vector3>();

        // Ensure we don't return an empty list if segments is 0 or 1
        if (segments <= 0)
        {
            return points;
        }

        // Calculate the vector that represents the direction and distance between start and end
        Vector3 direction = end - start;

        for (int i = 1; i < segments; i++)  // Exclude the last point by going up to segments - 1
        {
            // Interpolate between start and end, dividing the distance into equal parts
            Vector3 point = start + direction * (i / (float)segments);
            points.Add(point);
        }

        return points;
    }
    public static bool SafePlay(this Animator animator, string stateName, int layer = -1, float normalizedTime = 0f)
    {
        if (animator == null || !animator.gameObject.activeInHierarchy)
            return false; // Prevent playing if the animator is null or inactive

        animator.Play(stateName, layer, normalizedTime);
        return true; // Animation played successfully
    }
    /// <summary>
    /// Rotate a Vector2 by an angle clockwise
    /// </summary>
    /// <param name="v"></param>
    /// <param name="degrees"></param>
    /// <returns></returns>
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        return Quaternion.AngleAxis(degrees, Vector3.forward) * v;
    }

}
