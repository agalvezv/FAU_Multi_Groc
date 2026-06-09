using UnityEngine;
using Fusion;

public class PreserveProducts : MonoBehaviour
{
    private NetworkRunner _runner;
    private string _lastRoomName;

    private void Awake()
    {
        // Protects this object and its network loop from being cleared 
        // when Horizon OS forces a deep suspension. So no obejects that are grabbable dissapear if you long-press Meta button
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _runner = GetComponent<NetworkRunner>();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // pauseStatus == true means headset was just taken off (entering standby)
        // pauseStatus == false means headset was just put back on (waking up)
        HandleHeadsetStandby(pauseStatus);
    }

    private void HandleHeadsetStandby(bool goingToSleep)
    {
        if (_runner == null) return;

        if (goingToSleep)
        {
            // Cache the active room name right before the Wi-Fi card sleeps
            if (_runner.IsRunning)
            {
                _lastRoomName = _runner.SessionInfo.Name;
            }
            Debug.Log("Quest 3 entering proximity sleep mode.");
        }
        else
        {
            // Headset just woke back up! Check if the cloud dropped us
            Debug.Log("Quest 3 woke up from proximity standby.");
            
            if (!_runner.IsRunning && !string.IsNullOrEmpty(_lastRoomName))
            {
                Debug.Log("Network connection lost during standby. Initiating automatic hot-reconnect...");
                ForceNetworkRecovery();
            }
        }
    }

    private async void ForceNetworkRecovery()
    {
        // Bootstraps a fresh connection to the exact same room room state 
        // to download the cached scene products back from the cloud server
        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = _lastRoomName,
            SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log("Successfully recovered network session! Products restored.");
        }
        else
        {
            Debug.LogError($"Network recovery failed: {result.ShutdownReason}");
        }
    }
}