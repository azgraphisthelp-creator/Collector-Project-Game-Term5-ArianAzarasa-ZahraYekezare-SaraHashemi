using UnityEngine;

public class DisableLogs : MonoBehaviour
{
    void Awake()
    {
#if !UNITY_EDITOR
        Debug.unityLogger.logEnabled = false;
#endif
    }
}