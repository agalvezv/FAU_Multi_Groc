using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class PreserveProducts : MonoBehaviour
{
    private NetworkRunner _runner;
    private string _lastRoomName;

    private void Awake()
    {
        // Protects this object and its network loop from being cleared when Horizon OS forces a deep suspension. So no objects that are grabbable disappear if you long-press Meta button
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _runner = GetComponent<NetworkRunner>();

        // Hook directly into Meta's Hardware Proximity Sensor events
        OVRManager.HMDMounted += OnHeadsetPutOn;
        OVRManager.HMDUnmounted += OnHeadsetTakenOff;
    }

    private void OnDestroy()
    {
        // Clean up hardware listeners if the object is destroyed
        OVRManager.HMDMounted -= OnHeadsetPutOn;
        OVRManager.HMDUnmounted -= OnHeadsetTakenOff;
    }

    private void OnHeadsetTakenOff()
    {
        Debug.Log("Meta Proximity Sensor: Headset Taken Off! Connection is kept alive in background.");
        
        // Save the room name just in case the OS drops the socket on an extended idle pause
        if (_runner != null && _runner.IsRunning && _runner.SessionInfo.IsValid)
        {
            _lastRoomName = _runner.SessionInfo.Name;
        }

        // ReleaseAllStateAuthority() and _runner.Shutdown() are stripped out to prevent immediate network dropouts when checking your computer monitor
    }

    private void OnHeadsetPutOn()
    {
        Debug.Log("Meta Proximity Sensor: Headset Put On!");
        
        // If the headset was off for so long that the network socket NATURALLY timed out, then and only then we execute recovery to hot-reconnect back to the room
        if (_runner != null && !_runner.IsRunning && !string.IsNullOrEmpty(_lastRoomName))
        {
            ForceNetworkRecovery();
        }
    }

    private void ReleaseAllStateAuthority()
    {
        NetworkObject[] allNetObjects = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);

        foreach (NetworkObject netObject in allNetObjects)
        {
            if (netObject != null && netObject.HasStateAuthority)
            {
                netObject.ReleaseStateAuthority();
            }
        }
        Debug.Log("State authority successfully released back to the global room.");
    }

    private async void ForceNetworkRecovery()
    {
        Debug.Log($"Initiating hot-reconnect recovery sequence for room: {_lastRoomName}");

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = _lastRoomName,
            SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
            Debug.Log("Network recovery successful! Reconnected to the preserved supermarket room.");
        else
            Debug.LogError($"Network recovery failed: {result.ShutdownReason}. Spawning fallback clean session.");
    }
}