using UnityEngine;

public class AmbientLoop : MonoBehaviour
{
    private static AmbientLoop instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
