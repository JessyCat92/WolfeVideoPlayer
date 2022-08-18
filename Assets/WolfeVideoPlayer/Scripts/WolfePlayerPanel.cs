
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public class WolfePlayerPanel : UdonSharpBehaviour
{
    [Header("Video Control UI")]

    [Tooltip("Object Owner Text Field")]
    public Text textOwner;

    [Tooltip("Error Text Field")]
    public Text textError;

    [Tooltip("Current Playing Video Text Field")]
    public Text textCurrentVideo;

    [Tooltip("Timestamp Text Field")]
    public Text textTimestamp;

    [Tooltip("About Text Field")]
    public Text textAbout;

    [Tooltip("URL Input Field")]
    public VRCUrlInputField URLField;

    [Tooltip("World Master Lock Toggle")]
    public Toggle toggleMasterLock;

    [Tooltip("Timestamp Lock Toggle")]
    public Toggle toggleTimestampLock;

    [Tooltip("Volume Lock Toggle")]
    public Toggle toggleVolumeLock;

    [Tooltip("Brightness Lock Toggle")]
    public Toggle toggleBrightnessLock;

    [Tooltip("Video Brightness Slider")]
    public Slider sliderBrightness;

    [Tooltip("Timestamp Slider")]
    public Slider sliderTimestamp;

    [Tooltip("Video Volume Slider")]
    public Slider sliderVolume;

    [Tooltip("About Canvas")]
    public GameObject canvasAbout;

    [Tooltip("Play/Pause Image Field")]
    public Image imagePlayPause;

    [Tooltip("Mute Image Field")]
    public Image imageMute;

    [Tooltip("Loading Image Field")]
    public Image imageLoading;

    [Tooltip("Sprite for the Pause Button")]
    public Sprite spritePause;

    [Tooltip("Sprite for the Play Button")]
    public Sprite spritePlay;

    [Tooltip("Sprite for the Locked Button")]
    public Sprite spriteLocked;

    [Tooltip("Sprite for the Unlocked button")]
    public Sprite spriteUnlocked;

    [Tooltip("Sprite for the Mute Button")]
    public Sprite spriteMute;

    [Tooltip("Sprite for the Unmute button")]
    public Sprite spriteUnmute;
    
    [HideInInspector] public WolfePlayerController wolfePlayerController;






    /*
     * TODO: Clean up the Player Panel codebase
     * All of this is a mess right now
     * Everything should have comments and descriptors saying what the functions do.
     */
     



    [HideInInspector] public bool isDragging = false;
    [HideInInspector] public bool isBrightnessDragging = false;
    [HideInInspector] public bool isVolumeDragging = false;
    [HideInInspector] public bool loadThis = false;
    [HideInInspector] public bool masterLockClicked = false;
    private bool isTimestampDragging = false;

    private bool volumeLocked = false;
    private bool brightnessLocked = true;
    private bool timestampLocked = true;
    private bool buffering = false;

    void Start()
    {




        if (toggleVolumeLock != null)
        {
            volumeLocked = toggleVolumeLock.isOn;
        }

        if (toggleBrightnessLock != null)
        {
            brightnessLocked = toggleBrightnessLock.isOn;
        }
        if (sliderBrightness != null)
        {
            sliderBrightness.interactable = !brightnessLocked;
        }
        if (sliderVolume != null)
        {
            sliderVolume.interactable = !volumeLocked;
        }

    }

    
    public bool HasVideoPanel()
    {
        return (wolfePlayerController == null ? false : true);
    }

    public bool GetTimestampDragging()
    {
        return isTimestampDragging;
    }

    public void SetDraggingTrue()
    {
        isDragging = true;
    }

    public void SetDraggingFalse()
    {
        isDragging = false;
        if (isTimestampDragging)
        {
            if(HasVideoPanel())
                wolfePlayerController.SetTimestampSliders();
            isTimestampDragging = false;
        }
    }

    public void SetVolume()
    {
        if (isDragging && sliderVolume != null)
        {
            if (sliderVolume.interactable)
            {
                if(HasVideoPanel())
                    wolfePlayerController.SetVolume(sliderVolume.value);
            }
        }
    }

    public void SetVolumeSlider(float volume)
    {
        if (sliderVolume != null)
        {
            sliderVolume.value = volume;
        }
    }

    public void SetBrightness()
    {
        if (isDragging && sliderBrightness != null)
        {
            if (sliderBrightness.interactable)
            {
                if(HasVideoPanel())
                    wolfePlayerController.SetBrightness(sliderBrightness.value);
            }
        }
    }

    public void SetBrightnessSlider(float brightness)
    {
        if (sliderBrightness != null)
        {
            sliderBrightness.value = brightness;
        }
    }

    public void SetCurrentVideo(string text)
    {
        if(textCurrentVideo != null)
        {
            textCurrentVideo.text = text;
        }
    }

    public void TogglePlaying()
    {
        if(HasVideoPanel())
        if (Networking.IsOwner(wolfePlayerController.gameObject))
        {
            wolfePlayerController.TogglePlaying();
        }
    }

    public void ToggleMute()
    {
        if(HasVideoPanel())
        wolfePlayerController.ToggleMute();
    }

    public void ToggleMasterLock()
    {
        if (HasVideoPanel())
            wolfePlayerController.ToggleMasterLock();
    }

    /// <summary>
    /// The player has set the Timestamp. This should be called by a Slider Pointer Down event.
    /// </summary>
    public void PlayerSetTimestamp()
    {
        if (HasVideoPanel())
        if (isDragging && sliderTimestamp != null && Networking.IsOwner(wolfePlayerController.gameObject))
        {
            if (sliderTimestamp.interactable)
            {
                isDragging = true;
                wolfePlayerController.SetTimestampWithChecks(sliderTimestamp.value);
                isTimestampDragging = true;
            }
        }
    }

    public void SetTimestamp(float timestamp)
    {
        if (sliderTimestamp != null && !isTimestampDragging &&  !float.IsNaN(timestamp))
        {
            sliderTimestamp.value = timestamp;
        }
    }

    public void SetMasterLock(bool locked)
    {
        if (toggleMasterLock != null)
        {
            toggleMasterLock.isOn = locked;
        }
    }

    public void setMuteToggle(bool state)
    {
        if (imageMute != null && spriteMute != null && spriteUnmute != null)
        {
            if (state)
            {
                imageMute.sprite = spriteMute;
            }
            else
            {
                imageMute.sprite = spriteUnmute;
            }
        }

    }

    public void SetErrorText(string text)
    {
        if(textError != null)
        {
            textError.text = text;
        }
    }

    public void SetPlayPause(bool state)
    {
        if (imagePlayPause != null && spritePause != null && spritePlay != null)
        {
            if (state)
            {
                imagePlayPause.sprite = spritePause;
            }
            else
            {
                imagePlayPause.sprite = spritePlay;
            }
        }
    }

    public void LoadUrl()
    {
        if (HasVideoPanel())
            wolfePlayerController.LoadVideoUrl(URLField.GetUrl());
    }

    public void ClearUrlField()
    {
        URLField.SetUrl(VRCUrl.Empty);
    }

    public void TakeOwnership()
    {
        if (HasVideoPanel())
            wolfePlayerController.TakeOwnership();
    }

    public void ForceSync()
    {
        if (HasVideoPanel())
            wolfePlayerController.ForceSync();
    }

    public void PlayBufferAnimation()
    {
        if(imageLoading != null)
        {
            imageLoading.enabled = true;
            if (!buffering)
            {
                SendCustomEventDelayedFrames("BufferAnimation", 1);
            }
            buffering = true;
        }
    }

    public void StopBufferAnimation()
    {
        if (imageLoading != null)
        {
            imageLoading.enabled = false;
            buffering = false;
        }
    }

    public void BufferAnimation()
    {
        imageLoading.transform.Rotate(new Vector3(0.0f, 0.0f, -2f));
        if (buffering)
        {
            SendCustomEventDelayedFrames("BufferAnimation", 1);
        }
    }


    public void ToggleVolumeLock()
    {
        if(toggleVolumeLock != null && sliderVolume != null)
        {
            volumeLocked = toggleVolumeLock.isOn;
            sliderVolume.interactable = !volumeLocked;
        }
    }

    public void ToggleBrightnessLock()
    {
        if (toggleBrightnessLock != null && sliderBrightness != null)
        {
            brightnessLocked = toggleBrightnessLock.isOn;
            sliderBrightness.interactable = !brightnessLocked;
        }
    }




    public void ToggleTimestampLock()
    {
        if (HasVideoPanel())
        if (toggleTimestampLock != null && sliderTimestamp !=null){
            if (Networking.IsOwner(wolfePlayerController.gameObject))
            {
                timestampLocked = toggleTimestampLock.isOn;
                sliderTimestamp.interactable = !timestampLocked;
            }
            else
            {
                LockTimestamp();
            }
        }
    }

    public void LockTimestamp()
    {
        timestampLocked = true;
        toggleTimestampLock.isOn = timestampLocked;
        sliderTimestamp.interactable = !timestampLocked;
    }

    public void SetTimestampText(string timestampText)
    {
        if(textTimestamp != null)
        {
            textTimestamp.text = timestampText;
        }
    }

    public void SetOwnerText(string owner)
    {
        if(textOwner != null)
        {
            textOwner.text = owner;
        }
    }

    /// <summary>
    /// Toggles the ? Menu, showing information about the Video Player's creator
    /// </summary>
    public void ToggleAbout()
    {
        if(textAbout != null && canvasAbout != null){
            canvasAbout.SetActive(!canvasAbout.activeSelf);
            if (canvasAbout.activeSelf)
            {
                textAbout.text = "X";
            }
            else
            {
                textAbout.text = "?";
            }
        }
    }
}
