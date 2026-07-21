using System.Collections.Concurrent;
using TMPro;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    [SerializeField] private int maxCharacters = 1000;

    // Thread-safe queue for incoming logs from Fusion/Voice threads
    private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();

    void OnEnable() { Application.logMessageReceived += HandleLog; }
    void OnDisable() { Application.logMessageReceived -= HandleLog; }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Safely catch logs from any thread
        _logQueue.Enqueue(logString);
    }

    void Update()
    {
        if (_logQueue.IsEmpty || debugText == null) return;

        // Process logs on Unity's main thread
        while (_logQueue.TryDequeue(out string logMessage))
        {
            // Prepend new log to the TOP
            debugText.text = logMessage + "\n" + debugText.text;
        }

        // Keep newest logs (at the start) and trim oldest logs off the BOTTOM
        if (debugText.text.Length > maxCharacters)
        {
            debugText.text = debugText.text.Substring(0, maxCharacters);
        }
    }
}