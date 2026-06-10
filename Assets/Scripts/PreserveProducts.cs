using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class PreserveProducts : MonoBehaviour
{
    private NetworkRunner _runner;
    private string _lastRoomName;

    private void Awake()
    {
        // Protects this object and its network loop from being cleared 
        // when Horizon OS forces a deep suspension. So no objects that are grabbable disappear if you long-press Meta button
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
        Debug.Log("Meta Proximity Sensor: Headset Taken Off!");
        
        if (_runner != null && _runner.IsRunning)
        {
            _lastRoomName = _runner.SessionInfo.Name;
            ReleaseAllStateAuthority();
            
            // Explicitly force a cloud disconnect here so we can gracefully 
            // control the clean reconnection process when we wake up
            _runner.Shutdown();
        }
    }

    private void OnHeadsetPutOn()
    {
        Debug.Log("Meta Proximity Sensor: Headset Put On!");
        
        // If we have a cached room name and our runner is currently shut down, start recovery
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

        // Bypasses custom config structures by assigning parameter arguments directly 
        // using Fusion 2's direct matchmaking initialization dictionary
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