//Script to select either Avatar A or Avatar B by sending messages to the avatar's game object
using UnityEngine;

public class AvatarSelectionManager : MonoBehaviour
{
    [Header("Meta Avatar Root GameObject")]
    [Tooltip("Drag the game object from your Hierarchy that has your Avatar scripts on it")]
    [SerializeField] private GameObject localAvatarObject;

    [Header("UI Reference")]
    [SerializeField] private GameObject selectionCanvas;

    public void SelectAvatarA()
    {
        SendMetaPresetSignal("0_quest_light");
    }

    public void SelectAvatarB()
    {
        SendMetaPresetSignal("1_quest_light");
    }

    private void SendMetaPresetSignal(string assetName)
    {
        if (localAvatarObject == null)
        {
            Debug.LogError("[AvatarSelection] Local Avatar Object reference is missing!");
            return;
        }

        Debug.Log($"[AvatarSelection] Triggering asset load for: {assetName}");

        // Using Unity's native messaging pipeline completely to bypass version-locked API methods.
        // Dynamically searches the avatar object for Meta's built-in loading functions
        localAvatarObject.SendMessage("Teardown", SendMessageOptions.DontRequireReceiver);
        localAvatarObject.SendMessage("SetBodyAssetOverride", assetName, SendMessageOptions.DontRequireReceiver);
        localAvatarObject.SendMessage("ReloadAvatarWithPreset", SendMessageOptions.DontRequireReceiver);
        localAvatarObject.SendMessage("ReloadAvatar", SendMessageOptions.DontRequireReceiver);

        // Hide the selection menu after making a selection
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(false);
            Debug.Log("[AvatarSelection] Setup complete! Closing UI selection frame.");
        }
    }
}