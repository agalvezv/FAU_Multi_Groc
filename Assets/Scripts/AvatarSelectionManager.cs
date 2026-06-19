using UnityEngine;

public class AvatarSelectionManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject selectionCanvas;

    // This will be automatically filled when the local player spawns
    private NetworkedAvatarSelector localPlayerSelector;

    // 1. The Local Player calls this immediately upon spawning
    public void RegisterLocalPlayer(NetworkedAvatarSelector selector)
    {
        localPlayerSelector = selector;
        Debug.Log("[AvatarUI] Local player registered successfully!");
    }

    // 2. These go on UI Button OnClick() events in inspector
    public void SelectAvatarA()
    {
        SendSelectionToNetwork(0); // Index 0 for Avatar A
    }

    public void SelectAvatarB()
    {
        SendSelectionToNetwork(1); // Index 1 for Avatar B
    }

    private void SendSelectionToNetwork(int avatarIndex)
    {
        if (localPlayerSelector == null)
        {
            Debug.LogError("[AvatarUI] No local player registered yet! Cannot change avatar.");
            return;
        }

        // Tell our networked object to change its synchronized state
        localPlayerSelector.SetAvatarSelection(avatarIndex);

        // Hide the selection menu
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(false);
            Debug.Log($"[AvatarUI] Selection {avatarIndex} sent. Closing UI frame.");
        }
    }
}