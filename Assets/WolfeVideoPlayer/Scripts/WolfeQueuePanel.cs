
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public class WolfeQueuePanel : UdonSharpBehaviour
{
    [Header("Video Control UI")]

    [Tooltip("URL Input Field")]
    public VRCUrlInputField URLField;

    [Tooltip("Queue Text")]
    public Text queueText;

    [Tooltip("World Master Lock Toggle")]
    public Toggle toggleMasterLock;

    [Tooltip("Sprite for the Locked Button")]
    public Sprite spriteLocked;

    [Tooltip("Sprite for the Unlocked button")]
    public Sprite spriteUnlocked;

    private WolfeQueueController wolfeQueueController;

    public void SetWolfeQueueController(WolfeQueueController controller)
    {
        wolfeQueueController = controller;
    }

    public void SetMasterLock(bool locked)
    {
        if (toggleMasterLock != null)
        {
            toggleMasterLock.isOn = locked;
        }
    }

    public void ToggleMasterLock()
    {
        wolfeQueueController.ToggleMasterLock();
    }

    public void SetQueueLock(bool locked)
    {
        if (toggleMasterLock != null)
        {
            toggleMasterLock.isOn = locked;
        }
    }

    public void SkipCurrentVideo()
    {
        wolfeQueueController.SkipCurrentVideo();
    }

    public void ClearQueue()
    {
        wolfeQueueController.ClearQueue();
    }

    public void AddUrl()
    {
        if (!wolfeQueueController.AreControlsLocked && URLField != null)
        {
            wolfeQueueController.AppendToQueueWithChecks(URLField.GetUrl());
            URLField.SetUrl(VRCUrl.Empty);
        }
    }

    public void TakeOwnership()
    {
        wolfeQueueController.TakeOwnership();
    }

    public void LoadVideo()
    {
        wolfeQueueController.SendCustomEvent("LoadVideo");
    }

    public void SetQueueText(string t)
    {
        if(queueText != null)
        {
            queueText.text = t;
        }
    }

}
