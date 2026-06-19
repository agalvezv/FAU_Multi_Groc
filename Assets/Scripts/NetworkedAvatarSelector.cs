using UnityEngine;
using Fusion;
using System.Collections;

public class NetworkedAvatarSelector : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnAvatarSelectionChanged))]
    public int ChosenAvatarIndex { get; set; } = -1; 

    [SerializeField] private Oculus.Avatar2.OvrAvatarEntity avatarEntity;

    private string[] avatarPresets = new string[] { "0_quest_light", "1_quest_light" };
    private Coroutine activeLoadRoutine;

    public void SetAvatarSelection(int index)
    {
        if (Object.HasStateAuthority)
        {
            ChosenAvatarIndex = index;
        }
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            var uiManager = GameObject.FindAnyObjectByType<AvatarSelectionManager>();
            if (uiManager != null)
            {
                uiManager.RegisterLocalPlayer(this);
            }
        }
    }

    private void OnAvatarSelectionChanged()
    {
        if (activeLoadRoutine != null)
        {
            StopCoroutine(activeLoadRoutine);
        }

        activeLoadRoutine = StartCoroutine(WaitForAvatarAndApply());
    }

    private IEnumerator WaitForAvatarAndApply()
    {
        while (avatarEntity == null)
        {
            // SAFETY CHECK: If you click stop, this breaks the infinite loop instantly
            if (this == null || !Application.isPlaying)
            {
                yield break;
            }

            avatarEntity = GetComponentInChildren<Oculus.Avatar2.OvrAvatarEntity>(true);
            
            if (avatarEntity == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (this != null && Application.isPlaying)
        {
            ApplyAvatarPreset();
        }
        
        activeLoadRoutine = null;
    }

    private void ApplyAvatarPreset()
    {
        if (ChosenAvatarIndex < 0 || ChosenAvatarIndex >= avatarPresets.Length || avatarEntity == null) 
        {
            return;
        }
        
        string assetName = avatarPresets[ChosenAvatarIndex];
        Debug.Log($"[Multiplayer] Success! Applying preset configuration layout: {assetName}");

        GameObject targetGo = avatarEntity.gameObject;
        targetGo.SendMessage("Teardown", SendMessageOptions.DontRequireReceiver);
        targetGo.SendMessage("SetBodyAssetOverride", assetName, SendMessageOptions.DontRequireReceiver);
        targetGo.SendMessage("ReloadAvatarWithPreset", SendMessageOptions.DontRequireReceiver);
        targetGo.SendMessage("ReloadAvatar", SendMessageOptions.DontRequireReceiver);
    }
}