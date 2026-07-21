using UnityEngine;
using Fusion;

public class ConnectionLight : MonoBehaviour
{
    public MeshRenderer lightRenderer;
    private NetworkRunner runner;

    void Awake()
    {
        if (lightRenderer == null) lightRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (lightRenderer == null) return;

        if (runner == null)
        {
            runner = FindFirstObjectByType<NetworkRunner>();
        }

        // 1. Fully joined to the actual network session / room
        if (runner != null && runner.IsInSession)
        {
            lightRenderer.material.color = Color.green;
        }
        // 2. Starting up or negotiating server connection
        else if (runner != null && (runner.IsStarting || runner.IsRunning))
        {
            lightRenderer.material.color = Color.yellow;
        }
        // 3. Completely offline / disconnected
        else
        {
            lightRenderer.material.color = Color.red;
        }
    }
}