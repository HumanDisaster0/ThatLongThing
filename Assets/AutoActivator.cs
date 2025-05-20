using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoActivator : MonoBehaviour
{
    public static AutoActivator Instance;
    private bool hasActivated = false;

    private void Awake()
    {
        // ΩÃ±€≈Ê º≥¡§
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
       
    }
}
