using UnityEngine;
using TMPro;

public class DisplayGameVersion : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshPro _versionTextOnWall;

    [Header("Optional Settings")]
    [SerializeField] private string _prefix = "Version: ";

    private void Start()
    {
        // If forgot to drag the component in the inspector, try to find it locally
        if (_versionTextOnWall == null)
        {
            _versionTextOnWall = GetComponent<TextMeshPro>();
        }

        DisplayVersion();
    }

    private void DisplayVersion()
    {
        if (_versionTextOnWall != null)
        {
            // Application.version grabs the exact string (e.g., "0.1.3") from Player Settings
            _versionTextOnWall.text = $"{_prefix}{Application.version}";
            Debug.Log($"Displaying Game Version: {Application.version}");
        }
        else
        {
            Debug.LogError("DisplayGameVersion: No TextMeshPro component assigned or found!");
        }
    }
}
