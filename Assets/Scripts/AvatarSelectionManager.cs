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

        // 1. Universal String Lookup: Finds the entity child without needing any Meta namespaces
        Transform targetTransform = localAvatarObject.transform;
        GameObject actualTarget = localAvatarObject;

        // Loop through all child objects to look for the script containing "AvatarEntity"
        Component[] allComponents = localAvatarObject.GetComponentsInChildren<Component>(true);
        foreach (Component comp in allComponents)
        {
            if (comp != null && comp.GetType().Name.Contains("AvatarEntity"))
            {
                actualTarget = comp.gameObject;
                Debug.Log($"[AvatarSelection] Successfully targeted local asset entity child: {actualTarget.name}");
                break;
            }
        }

        // 2. Fire the native message pipeline down to the target entity
        actualTarget.SendMessage("Teardown", SendMessageOptions.DontRequireReceiver);
        actualTarget.SendMessage("SetBodyAssetOverride", assetName, SendMessageOptions.DontRequireReceiver);
        actualTarget.SendMessage("ReloadAvatarWithPreset", SendMessageOptions.DontRequireReceiver);
        actualTarget.SendMessage("ReloadAvatar", SendMessageOptions.DontRequireReceiver);

        // 3. Hide the selection menu after making a selection
        if (selectionCanvas != null)
        {
            selectionCanvas.SetActive(false);
            Debug.Log("[AvatarSelection] Setup complete! Closing UI selection frame.");
        }
    }
}