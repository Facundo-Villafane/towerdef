using UnityEngine;

/// <summary>
/// Generic Singleton pattern implementation for MonoBehaviour classes
/// </summary>
/// <typeparam name="T">Type of the singleton instance</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // The static reference to the instance
    private static T instance;
    
    // Property to access the instance from other scripts
    public static T Instance
    {
        get
        {
            // If the instance doesn't exist
            if (instance == null)
            {
                // Find if there's an existing instance in the scene
                instance = FindObjectOfType<T>();
                
                // If there's no instance in the scene
                if (instance == null)
                {
                    // Create a new GameObject and add component
                    GameObject singletonObject = new GameObject($"{typeof(T).Name} (Singleton)");
                    instance = singletonObject.AddComponent<T>();
                }
            }
            
            return instance;
        }
    }
    
    /// <summary>
    /// Called when the script instance is being loaded
    /// </summary>
    protected virtual void Awake()
    {
        // If an instance already exists, destroy this object
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"Multiple instances of {typeof(T).Name} detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Set the instance
        instance = this as T;
        
        // Optional: Keep the singleton alive when loading a new scene
        // DontDestroyOnLoad(gameObject);
        
        OnAwake();
    }
    
    /// <summary>
    /// Override this method for additional Awake functionality
    /// </summary>
    protected virtual void OnAwake() { }
}