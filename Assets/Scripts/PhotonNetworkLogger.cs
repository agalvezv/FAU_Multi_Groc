using System;
using System.Collections.Generic;
using System.Linq; // For player count
using UnityEngine;
using TMPro; // Added for TextMeshPro
using Fusion;
using Fusion.Sockets;
using Photon.Voice.Unity;

public class PhotonNetworkLogger : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Text (TMP) object")]
    public TextMeshProUGUI logText;

    private VoiceConnection voiceConnection;
    private NetworkRunner activeRunner;
    private bool registered = false;

    void Start()
    {
        LogToScreen("[LOGGER] Initialized on " + gameObject.name);

        voiceConnection = FindFirstObjectByType<VoiceConnection>();
        if (voiceConnection != null && voiceConnection.Client != null)
        {
            voiceConnection.Client.StateChanged += OnVoiceStateChanged;
            LogToScreen($"[LOGGER] VoiceConnection attached. State: {voiceConnection.Client.State}");
        }
    }

    void Update()
    {
        if (!registered)
        {
            NetworkRunner[] runners = FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None);
            foreach (var runner in runners)
            {
                if (runner != null && (runner.IsRunning || runner.IsStarting))
                {
                    runner.AddCallbacks(this);
                    activeRunner = runner;
                    registered = true;
                    LogToScreen($"[LOGGER] Registered to Active Runner ({runner.name})!");

                    if (runner.IsInSession)
                    {
                        string sName = (runner.SessionInfo != null && runner.SessionInfo.IsValid) ? runner.SessionInfo.Name : "Active Session";
                        LogToScreen($"[FUSION] Already in room: '{sName}'");
                    }
                    break;
                }
            }
        }
    }

    // Helper function to print to both Unity Console AND wall screen
    private void LogToScreen(string message)
    {
        Debug.Log(message); // Prints to Unity console on PC

        if (logText != null)
        {
            // Prepend new logs to the top of the wall text
            logText.text = message + "\n" + logText.text;

            // Keep text length manageable
            if (logText.text.Length > 1000)
            {
                logText.text = logText.text.Substring(0, 1000);
            }
        }
    }

    void OnDestroy()
    {
        if (voiceConnection != null && voiceConnection.Client != null)
        {
            voiceConnection.Client.StateChanged -= OnVoiceStateChanged;
        }

        if (activeRunner != null)
        {
            activeRunner.RemoveCallbacks(this);
        }
    }

    // --- FUSION CALLBACKS ---

    public void OnConnectedToServer(NetworkRunner runner) 
    { 
        LogToScreen($"[FUSION] Connected to Master Server! ({runner.name})"); 
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        string sessionName = (runner.SessionInfo != null && runner.SessionInfo.IsValid) ? runner.SessionInfo.Name : "Active";
        
        // Check the actual region Fusion connected to
        string region = runner.LobbyInfo.Region; 

        LogToScreen($"[FUSION] Room: '{sessionName}' | Region: {region} | Players: {runner.ActivePlayers.Count()}");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
{
    // Print immediate connection notification
    LogToScreen($"[FUSION] Player Joining... ID: {player.PlayerId}");

    // Start a delayed check so Meta's spawner has time to instantiate the clone
    StartCoroutine(CheckAvatarCountDelayed(runner, player));
}

    private System.Collections.IEnumerator CheckAvatarCountDelayed(NetworkRunner runner, PlayerRef player)
    {
        // Wait 1 second for Meta Avatar Spawner to instantiate the model
        yield return new UnityEngine.WaitForSeconds(1.0f);

        int totalPlayers = runner.ActivePlayers.Count();
        
        // Count active avatar behaviors in the scene
        var avatars = FindObjectsByType<Meta.XR.MultiplayerBlocks.Fusion.AvatarBehaviourFusion>(FindObjectsSortMode.None);
        int avatarCount = avatars.Length;

        LogToScreen($"[FUSION] Player Joined! ID: {player.PlayerId} | Room Players: {totalPlayers} | Active Avatars: {avatarCount}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        LogToScreen($"[FUSION] Player Left! ID: {player.PlayerId}");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        LogToScreen($"[FUSION] Disconnected: {reason}");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) 
    { 
        LogToScreen($"[FUSION] Connection Failed: {reason}"); 
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        LogToScreen($"[FUSION] Shutdown ({runner.name}): {shutdownReason}");
    }

    private void OnVoiceStateChanged(Photon.Realtime.ClientState previousState, Photon.Realtime.ClientState newState)
    {
        LogToScreen($"[VOICE] State Changed: {newState}");
        if (newState == Photon.Realtime.ClientState.Joined)
        {
            LogToScreen("[VOICE] Audio Connected & Ready!");
        }
    }

    // --- REQUIRED EMPTY IMPLEMENTATIONS ---
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}