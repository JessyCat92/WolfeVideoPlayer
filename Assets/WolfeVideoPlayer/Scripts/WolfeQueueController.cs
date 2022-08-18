
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WolfeQueueController : UdonSharpBehaviour
{

    [Tooltip("The attached Wolfe Player Controller this queue is for")]
    [SerializeField] private WolfePlayerController wolfePlayerController;

    [SerializeField] private WolfeQueuePanel[] wolfeQueuePanels;

    [Tooltip("Maximum videos allowed in a queue at once")]
    [SerializeField] private int maxQueueLength = 15;

    [UdonSynced, FieldChangeCallback(nameof(VideoQueueProperty))] private VRCUrl[] _videoQueue = new VRCUrl[0];
    [UdonSynced, FieldChangeCallback(nameof(MasterLockedProperty))] private bool _masterLocked = false;

    /// <summary>
    /// _masterLocked
    /// </summary>
    public bool MasterLockedProperty
    {
        set
        {
            _masterLocked = value;
            SetControlsLock();
        }
        get => _masterLocked;
    }

    /// <summary>
    /// _videoQueue
    /// </summary>
    public VRCUrl[] VideoQueueProperty
    {
        set
        {
            bool diff = false;
            if (_videoQueue.Length != value.Length)
            {
                diff = true;
            }
            else
            {
                for (int i = 0; i < _videoQueue.Length; i++)
                {
                    if (_videoQueue[i] != value[i])
                    {
                        i = _videoQueue.Length;
                        diff = true;
                    }
                }
            }
            if (diff)
            {
                _videoQueue = value;
                SetQueueText();
            }
        }
        get => _videoQueue;
    }

    /// <summary>
    /// Converts the _videoQueue array into a string
    /// </summary>
    public string GetQueueText
    {
        get
        {
            string text = "";
            for (int i = VideoQueueProperty.Length - 1; i >= 0; i--)
            {
                if (VideoQueueProperty[i].Get() != "")
                    text += "\n" + (i + 1) + ". " + VideoQueueProperty[i].Get();
            }
            return text;
        }
    }

    public bool IsQueueEmpty
    {
        get => (VideoQueueProperty.Length > 0 ? false : true);
    }

    /// <summary>
    /// Sets the current Queue text for the video players.
    /// Note, currently there is no scrolling functionality, so long queues will visually break.
    /// </summary>
    public void SetQueueText()
    {
        foreach(WolfeQueuePanel wolfeQueuePanel in wolfeQueuePanels)
        {
            wolfeQueuePanel.SetQueueText(GetQueueText);
        }
    }

    /// <summary>
    /// Takes ownership of the Video Player Controller, allowing the user to pause and scrub through the video.
    /// </summary>
    public void TakeOwnership()
    {
        if (!Networking.IsOwner(gameObject) && !AreControlsLocked)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    /// <summary>
    /// Sets the visual locks for the controls based on the _masterLocked bool.
    /// </summary>
    public void SetControlsLock()
    {
        foreach (WolfeQueuePanel wolfeQueuePanel in wolfeQueuePanels)
        {
            wolfeQueuePanel.SetMasterLock(MasterLockedProperty);
        }
    }

    /// <summary>
    /// (Sync) Sets the master lock
    /// </summary>
    /// <param name="masterLock"></param>
    public void SetMasterLock(bool masterLock)
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        MasterLockedProperty = masterLock;
        RequestSerialization();
    }

    /// <summary>
    /// Toggles the Master Lock for the player. When this is on, no one but the world master will be able to use the queue controls.
    /// </summary>
    public void ToggleMasterLock()
    {
        if (Networking.IsMaster)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            SetMasterLock(!MasterLockedProperty);
        }
        SetControlsLock();
    }

    /// <summary>
    /// Checks to see if the controls are locked, if they are only the world master can use them
    /// </summary>
    /// <returns>True if the controls are locked, false if they are not</returns>
    public bool AreControlsLocked
    {
        get
        {
            bool controlsLocked = true;
            if (!MasterLockedProperty)
            {
                controlsLocked = false;
            }
            else if (Networking.IsMaster)
            {
                controlsLocked = false;
            }
            return controlsLocked;
        }
    }

    /// <summary>
    /// Sets the attached Wolfe Player Controler
    /// </summary>
    /// <param name="controller"></param>
    public void SetWolfePlayerController(WolfePlayerController controller)
    {
        wolfePlayerController = controller;
    }

    /// <summary>
    /// (Sync) Sets the current Queue url array
    /// </summary>
    /// <param name="urls"></param>
    public void SetVideoQueue(VRCUrl[] urls)
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        VideoQueueProperty = urls;
        RequestSerialization();
    }

    /// <summary>
    /// (Sync) Appends a video to the end of the video queue array
    /// </summary>
    /// <param name="url"></param>
    public void AppendToQueue(VRCUrl url)
    {
        if (url != null)
        {
            VRCUrl[] tempQueue = new VRCUrl[VideoQueueProperty.Length + 1];
            for (int i = 0; i < VideoQueueProperty.Length; i++)
            {
                tempQueue[i] = VideoQueueProperty[i];
            }
            tempQueue[tempQueue.Length-1] = url;
            SetVideoQueue(tempQueue);
        }
    }

    /// <summary>
    /// (Sync) Appends a video to the end of the video queue array if the controls arent locked and the queue length hasn't been exceeded
    /// </summary>
    /// <param name="url"></param>
    public void AppendToQueueWithChecks(VRCUrl url)
    {
        if (!AreControlsLocked && VideoQueueProperty.Length < maxQueueLength)
        {
            AppendToQueue(url);
        }
    }

    /// <summary>
    /// Completely empties out the Video Queue.
    /// </summary>
    public void ClearQueue()
    {
        if (!AreControlsLocked)
        {
            SetVideoQueue(new VRCUrl[0]);
        }
    }

    /// <summary>
    /// Shorthand for advancing the queue one URL
    /// </summary>
    public void AdvanceQueue()
    {
        RemoveFromQueue(0);
    }

    /// <summary>
    /// Skips the currently playing video to play the next video in the queue
    /// </summary>
    public void SkipCurrentVideo()
    {
        if (!AreControlsLocked && VideoQueueProperty.Length > 0)
        {
            wolfePlayerController.SkipCurrentVideo();
        }
    }

    /// <summary>
    /// Removes a URL from the queue at the specified index.
    /// </summary>
    /// <param name="index"></param>
    public void RemoveFromQueue(int index)
    {
        if (index >= 0 && index < VideoQueueProperty.Length)
        {
            VRCUrl[] tempQueue = new VRCUrl[VideoQueueProperty.Length - 1];
            for (int i = 0; i < VideoQueueProperty.Length - 1; i++)
            {
                if (i == index)
                {
                    index++;
                }
                tempQueue[i] = VideoQueueProperty[index];
            }
            SetVideoQueue(tempQueue);
        }
    }

    void Start()
    {
        if(wolfePlayerController != null)
        {
            wolfePlayerController.SetWolfeQueueController(this);
        }
        foreach(WolfeQueuePanel wolfeQueuePanel in wolfeQueuePanels)
        {
            wolfeQueuePanel.SetWolfeQueueController(this);
        }
    }
}
