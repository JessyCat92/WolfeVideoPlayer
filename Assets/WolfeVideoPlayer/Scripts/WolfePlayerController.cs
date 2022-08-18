/*
 * Code by NishaWolfe
 * Please visit vrchat.nishawolfe.com for more
 * information about this prefab script.
 * 
 * 
 */
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WolfePlayerController : UdonSharpBehaviour
{
    [Header("Video Objects")]

    [Tooltip("Full list of Speaker objects")]
    [SerializeField] private AudioSource[] speakers;

    [Tooltip("Full list of Video Materials")]
    public Material[] screenMaterials;

    [SerializeField] private WolfePlayerPanel[] wolfePlayerPanels;


    [SerializeField] private int maxUrlLength = 512;

    [Tooltip("The default Video Player volume")]
    [Range(0f,1f)]
    public float defaultVolume = 0.5f;

    [SerializeField] private bool showDebugLogErrors = true;
    [SerializeField] private WolfeHooks wolfeHooks;

    [UdonSynced, FieldChangeCallback(nameof(SyncedUrlProperty))] private VRCUrl _syncedUrl = VRCUrl.Empty;
    [UdonSynced, FieldChangeCallback(nameof(IsPlayingProperty))] private bool _isPlaying = true;
    [UdonSynced, FieldChangeCallback(nameof(MasterLockedProperty))] private bool _masterLocked = false;
    [UdonSynced, FieldChangeCallback(nameof(ServerTimestampProperty))] private double _serverTimestamp = 0f;
    [UdonSynced, FieldChangeCallback(nameof(VideoTimeProperty))] private float _videoTime = 0f;

    /// <summary>
    /// Is the current video playing a livestream?
    /// </summary>
    private bool livestream = false;

    /// <summary>
    /// This is the maximum amount of offset the video player will alow players to be from the current owner's video time.
    /// This should be at LEAST 1f greater than the videoPlayerHeartbeat. 
    /// </summary>
    private float serverSyncOffset = 2f;

    private BaseVRCVideoPlayer videoPlayer;

    /// <summary>
    /// The timestamp to start a newly loaded video at. This should only come into play with the youtube &t= url variables
    /// </summary>
    private float timeScrubbingTime = -1f;
    private bool videoBuffering = false;
    private float prevVideoTime = 0f;


    private WolfeQueueController wolfeQueueController;

    /// <summary>
    /// This is specificly for error handling, if the video is timing out due to errors this will be true.
    /// </summary>
    private bool videoTimeout = false;

    /// <summary>
    /// This is specificly for error handling, if the video url is invalid this will be true.
    /// </summary>
    private bool invalidUrl = false;

    /// <summary>
    /// The amount of time each Heartbeat for the Video Player is.
    /// This determines the sync time, when set to 1 the video player checks for syncs & buffering every 1 second.
    /// </summary>
    private float videoPlayerHeartbeat = 1f;

    /// <summary>
    /// Whether or not the audio is muted
    /// </summary>
    private bool muted = false;

    /// <summary>
    /// The volume of the attached video player speakers
    /// </summary>
    private float volume = 0.5f;

    /// <summary>
    /// Video player material brightness
    /// </summary>
    private float brightness = 1f;

    /// <summary>
    /// Whether or not the video is currently in a loading state
    /// </summary>
    private bool videoPlayerLoading = true;

    

    /* ---------------------------------------------------------------------------------------------- */
    /* -----------------------------------------Function Hooks--------------------------------------- */
    /* ---------------------------------------------------------------------------------------------- */

    //Avoid changing these directly, unless you have to. Using the WolfeHooks script should function the same as this.
    //If VRChat ever exposes a method to add these via the editor, that's where these will likely go.

    public void SyncedUrlHook(VRCUrl syncedUrl)
    {
        if(wolfeHooks != null)
        {
            wolfeHooks.SyncedUrlHook(syncedUrl);
        }
    }

    public void MasterLockedHook(bool masterLocked)
    {
        if (wolfeHooks != null)
        {
            wolfeHooks.MasterLockedHook(masterLocked);
        }
    }

    public void VideoTimeHook(float videoTime)
    {
        if (wolfeHooks != null)
        {
            wolfeHooks.VideoTimeHook(videoTime);
        }
    }

    public void IsPlayingHook(bool isPlaying)
    {
        if (wolfeHooks != null)
        {
            wolfeHooks.IsPlayingHook(isPlaying);
        }
    }

    public void OnVideoPlayHook()
    {
        if (wolfeHooks != null)
        {
            wolfeHooks.OnVideoPlayHook();
        }
    }
    public void OnVideoEndHook()
    {
        if (wolfeHooks != null)
        {
            wolfeHooks.OnVideoEndHook();
        }
    }
    public void OnVideoLoopHook()
    {
        if (wolfeHooks != null)
        {
            wolfeHooks.OnVideoLoopHook();
        }
    }
    public void OnVideoReadyHook()
    {
        if (wolfeHooks != null)
        {
            wolfeHooks.OnVideoReadyHook();
        }
    }
    public void OnVideoStartHook()
    {
        if (wolfeHooks != null)
        {
            wolfeHooks.OnVideoStartHook();
        }
    }
    
    /* ---------------------------------------------------------------------------------------------- */
    /* ---------------------------------------------------------------------------------------------- */
    /* ---------------------------------------------------------------------------------------------- */

        

    //This annoys me. Regex isn't supported, so I had to do this.
    /// <summary>
    /// Converts the input string to Int. Ignores all letters and symbols when doing so.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private int ConvertToInt(string s)
    {
        char[] numbers = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        string parsedString = "";
        for (int i = 0; i < s.Length; i++)
        {
            for (int j = 0; j < numbers.Length; j++)
            {
                if (s[i] == numbers[j])
                {
                    parsedString += s[i];
                }
            }
        }
        return int.Parse(parsedString);
    }

    //TODO: Replace this with the built-in tool
    /// <summary>
    /// Converts a float numebr of seconds to a human readable HH:MM:SS timestamp
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private string SecondsToTimestamp(float time)
    {
        string timestamp = "";

        int seconds = (int)Mathf.Floor(time % 60);
        int minutes = (int)Mathf.Floor((time / 60) % 60);
        int hours = (int)Mathf.Floor((time / 60) / 60);
        string secondsString = "" + seconds;
        string minutesString = "" + minutes;
        string hoursString = "" + hours;

        if (seconds <= 0)
        {
            secondsString = "00";
        }
        else if (seconds > 0 && seconds < 10)
        {
            secondsString = "0" + seconds;
        }

        if (minutes <= 0)
        {
            minutesString = "00";
        }
        else if (minutes > 0 && minutes < 10)
        {
            minutesString = "0" + minutes;
        }

        timestamp = hoursString + ":" + minutesString + ":" + secondsString;
        return timestamp;
    }

    /// <summary>
    /// _syncedUrl
    /// </summary>
    public VRCUrl SyncedUrlProperty
    {
        set
        {
            videoPlayerLoading = true;
            _syncedUrl = value;
            SyncedUrlHook(_syncedUrl);
            videoPlayer.PlayURL(_syncedUrl);
            if (!Networking.IsOwner(gameObject))
            {
                SyncVideoPlaying();
            }
            SetCurrentVideo();
        }
        get => _syncedUrl;
    }
    
    /// <summary>
    /// _isPlaying
    /// </summary>
    public bool IsPlayingProperty
    {
        set
        {
            _isPlaying = value;
            IsPlayingHook(_isPlaying);
            SyncVideoPlaying();
            if (!Networking.IsOwner(gameObject))
            {
                SyncVideoTimestamp();
            }
        }
        get => _isPlaying;
    }

    /// <summary>
    /// _masterLocked
    /// </summary>
    public bool MasterLockedProperty
    {
        set
        {
            _masterLocked = value;
            MasterLockedHook(_masterLocked);
            SetControlsLock();
        }
        get => _masterLocked;
    }

    /// <summary>
    /// _videoTime
    /// </summary>
    public float VideoTimeProperty
    {
        set
        {
            _videoTime = value;
            VideoTimeHook(_videoTime);
            if (!Networking.IsOwner(gameObject))
            {
                if (IsUserOffsync && !livestream)
                {
                    SyncVideoTimestamp();
                }
            }
            else
            {

            }
        }
        get => _videoTime;
    }
    
    /// <summary>
    /// _serverTimestamp
    /// </summary>
    public double ServerTimestampProperty
    {
        set { _serverTimestamp = value; }
        get => _serverTimestamp;
    }
    
    /// <summary>
    /// This function sets the local video time to the synced time
    /// </summary>
    public void SyncVideoTimestamp()
    {
        if (!livestream)
        {
            if (!Networking.IsOwner(gameObject))
            {
                SetAVProVideoTime(RealVideoTime);
                if (IsPlayingProperty)
                {
                    videoPlayer.Play();
                }
            }
                
        }
        else
        {
            SetAVProVideoTime(videoPlayer.GetDuration());
        }
    }

    /// <summary>
    /// Sets the local video player's playing state to the sync variable
    /// </summary>
    public void SyncVideoPlaying()
    {
        if (videoPlayer.IsPlaying != IsPlayingProperty)
        {
            if (IsPlayingProperty)
            {
                videoPlayer.Play();
            }
            else
            {
                videoPlayer.Pause();
            }
        }
        SetPlayButtons();
    }

    /// <summary>
    /// The video time with Server delay accounted for.
    /// This variable can be used in junction with IsUserOffsync to ensure a Player is always synced to the object owner
    /// </summary>
    public float RealVideoTime
    {
        get => (VideoTimeProperty + (float)(Networking.GetServerTimeInSeconds() - ServerTimestampProperty));
    }

    /// <summary>
    /// Loops through the Wolfe Player Panels to set the play buttons visually
    /// </summary>
    public void SetPlayButtons()
    {
        foreach (WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
        {
            if(wolfePlayerPanel != null)
            {
                wolfePlayerPanel.SetPlayPause(IsPlayingProperty);
            }
        }
    }

    /// <summary>
    /// This compares the local player's video time with what the server's video time is supposed to be, network delay accounted for.
    /// If it's greater than the set serverSyncOffset, it return true that the player IS out of sync.
    /// </summary>
    public bool IsUserOffsync
    {
        get
        {
            return (Mathf.Abs(videoPlayer.GetTime() - (VideoTimeProperty + (float)(Networking.GetServerTimeInSeconds() - ServerTimestampProperty))) > serverSyncOffset ? true : false);
        }
    }

    /// <summary>
    /// (Sync) Sets the current synced video url
    /// </summary>
    /// <param name="syncedUrl"></param>
    public void SetSyncedUrl(VRCUrl syncedUrl)
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        SyncedUrlProperty = syncedUrl;
        IsPlayingProperty = true;
        RequestSerialization();
    }

    /// <summary>
    /// (Sync) Sets the current  synced video URL, checking to make sure the controls aren't locked
    /// </summary>
    /// <param name="syncedUrl"></param>
    public void SetSyncedUrlWithChecks(VRCUrl syncedUrl)
    {
        if (!AreControlsLocked)
        {
            SetSyncedUrl(syncedUrl);
        }
    }

    /// <summary>
    /// (Sync) Sets the server timestamp
    /// </summary>
    /// <param name="serverTimestamp"></param>
    public void SetServerTimestamp(double serverTimestamp)
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        ServerTimestampProperty = serverTimestamp;
        RequestSerialization();
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
    /// When the attached Video Player starts playing (Override Function)
    /// </summary>
    public override void OnVideoPlay()
    {
        OnVideoPlayHook();
        SetCurrentVideo();
        if (!livestream && videoPlayer.GetDuration() == 0)
        {
            VideoLoaded();
        }
        if (!IsPlayingProperty)
        {
            SyncVideoPlaying();
        }
    }

    /// <summary>
    /// When the video Ends
    /// </summary>
    public override void OnVideoEnd()
    {
        OnVideoEndHook();
        VideoFinished();
    }

    /// <summary>
    /// When the video loops
    /// </summary>
    public override void OnVideoLoop()
    {
        OnVideoLoopHook();
        VideoFinished();
    }

    /// <summary>
    /// When the current video is finished playing, play the next video in the queue
    /// </summary>
    public void VideoFinished()
    {
        bool playingNext = false;
        if (Networking.IsOwner(gameObject))
        {
            if (IsNextInQueueValid)
            {
                playingNext = true;
            }
            PlayNextInQueue();
        }
        if(Networking.IsOwner(gameObject) && !livestream)
        {
            if (videoPlayer.Loop)
            {
                videoPlayer.SetTime(0);
                if (IsPlayingProperty)
                {
                    videoPlayer.Play();
                }
            }
            else if (!playingNext)
            {
                SetIsPlaying(false);
                SetTimestampWithChecks(0);
            }
        }
    }

    /// <summary>
    /// When the ownership of the object is transfered to another player
    /// </summary>
    /// <param name="player"></param>
    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        foreach (WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
        {
            if (wolfePlayerPanel != null)
            {
                wolfePlayerPanel.SetOwnerText("Owner: " + player.displayName);
                if (player != Networking.LocalPlayer)
                {
                    wolfePlayerPanel.LockTimestamp();
                }
            }
        }
    }

    /// <summary>
    /// When the attached Video Player is Ready (Override Function)
    /// </summary>
    public override void OnVideoReady()
    {
        OnVideoReadyHook();
        VideoLoaded();
    }

    /// <summary>
    /// When the attached Video Player Starts (Override Function)
    /// </summary>
    public override void OnVideoStart()
    {
        OnVideoStartHook();
        VideoLoaded();
    }

    /// <summary>
    /// This function runs when the video loads either via OnVideoStart() or OnVideoReady()
    /// </summary>
    public void VideoLoaded()
    {
        ShowError("");
        videoPlayerLoading = false;
        livestream = false;
        SetCurrentVideo();
        if (float.IsInfinity(videoPlayer.GetDuration()))
        {
            livestream = true;
        }
        if (Networking.IsOwner(gameObject))
        {
            if (videoPlayer.GetDuration() == 0)
            {
                //The video time is 0, something went wrong with the AVPro player.
                //This should never happen
            }
            if (timeScrubbingTime > 0)
            {
                SetAVProVideoTime(timeScrubbingTime);
            }
        }
        if (!IsPlayingProperty)
        {
            SyncVideoPlaying();
        }
    }

    /// <summary>
    /// Sets the visual locks for the controls based on the _masterLocked bool.
    /// </summary>
    public void SetControlsLock()
    {
        foreach (WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
        {
            if (wolfePlayerPanel != null)
            {
                wolfePlayerPanel.SetMasterLock(MasterLockedProperty);
            }
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
    /// Toggles the mute status for the video player
    /// </summary>
    public void ToggleMute()
    {
        muted = !muted;
        foreach(WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
        {
            if (wolfePlayerPanel != null)
            {
                wolfePlayerPanel.setMuteToggle(muted);
            }
        }
        SetVolume(volume);
    }

    /// <summary>
    /// Sets the volume for all the attached speakers
    /// </summary>
    /// <param name="vol"></param>
    public void SetVolume(float vol)
    {
        volume = vol;
        foreach (WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
        {
            if (wolfePlayerPanel != null)
            {
                wolfePlayerPanel.SetVolumeSlider(volume);
            }
        }
        foreach(AudioSource speaker in speakers)
        {
            speaker.volume = (muted ? 0f : volume);
        }
    }

    /// <summary>
    /// Sets the brightness for all attached video player materials
    /// </summary>
    /// <param name="bright"></param>
    public void SetBrightness(float bright)
    {
        brightness = bright;
        foreach (WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
        {
            if (wolfePlayerPanel != null)
            {
            }
                wolfePlayerPanel.SetBrightnessSlider(brightness);
        }
        foreach(Material screenMaterial in screenMaterials)
        {
            screenMaterial.SetFloat("_Emission", brightness);
        }
    }

    /// <summary>
    /// Toggles the playing status of the video player
    /// </summary>
    public void TogglePlaying()
    {
        if (!AreControlsLocked && !livestream)
        {
            SetIsPlaying(!IsPlayingProperty);
        }
    }

    /// <summary>
    /// (Sync) Sets the playing status of the video player
    /// </summary>
    /// <param name="isPlaying"></param>
    public void SetIsPlaying(bool isPlaying)
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        IsPlayingProperty = isPlaying;
        if (!isPlaying)
        {
            timeScrubbingTime = -1;
        }
        RequestSerialization();
    }

    /// <summary>
    /// Pauses the currently playing video and sets (Sync) isPlaying to false
    /// </summary>
    public void Pause()
    {
        SetIsPlaying(false);
    }

    /// <summary>
    /// Plays the currently playing video and sets (Sync) isPlaying to true
    /// </summary>
    public void Play()
    {
        SetIsPlaying(true);
    }

    /// <summary>
    /// Appends a video to the Queue Controller, if it exists.
    /// </summary>
    /// <param name="url"></param>
    public void AppendToQueue(VRCUrl url)
    {
        if (wolfeQueueController != null)
        {
            wolfeQueueController.AppendToQueue(url);
        }
    }

    /// <summary>
    /// Completely empties out the Video Queue, if the Queue Controller exists.
    /// </summary>
    public void ClearQueue()
    {
        if (wolfeQueueController != null)
        {
            wolfeQueueController.ClearQueue();
        }
    }

    /// <summary>
    /// (Sync) Sets the current video time
    /// </summary>
    public void SetVideoTime(float videoTime)
    {
        videoTime = Mathf.Abs(videoTime);
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        SetAVProVideoTime(videoTime);
        VideoTimeProperty = videoTime;
        RequestSerialization();
    }

    /// <summary>
    /// Sets the "Current Video" text field of the player panel
    /// </summary>
    public void SetCurrentVideo()
    {
        if (SyncedUrlProperty != null)
        {
            foreach (WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
            {
                if (wolfePlayerPanel != null)
                {
                    wolfePlayerPanel.SetCurrentVideo(SyncedUrlProperty.ToString());
                }
            }
        }
    }

    /// <summary>
    /// Used to set the Queue Controller for this video player.
    /// It's set up this way so players who don't want a queue can delete the Queue Controller object without causing issues.
    /// </summary>
    /// <param name="controller"></param>
    public void SetWolfeQueueController(WolfeQueueController controller)
    {
        wolfeQueueController = controller;
    }

    /// <summary>
    /// Checks if the player's video is scrubbing through to catch up to a timestamp.<para/>
    /// NOTE: This SHOULDN'T ever come into play more than once in a row, unless AVPro Video Player isn't acting as expected.<para/>
    /// This type of thing can happen due to bugs introduced by VRChat.
    /// </summary>
    public bool IsPlayerScrubbing
    {
        get{
            if(timeScrubbingTime < 0 || timeScrubbingTime > videoPlayer.GetDuration())
            {
                return false;
            }
            else
            {
                return (VideoTimeProperty < timeScrubbingTime - 1 || VideoTimeProperty > timeScrubbingTime + 1);
            }
        }
    }

    /// <summary>
    /// Sets the current video time, checking to make sure it's not a longer number than the video & the controls aren't locked
    /// </summary>
    /// <param name="videoTime"></param>
    public void SetVideoTimeWithChecks(float videoTime)
    {
        if (!AreControlsLocked && videoTime <= videoPlayer.GetDuration() && !videoPlayerLoading)
        {
            SetVideoTime(videoTime);
        }
    }


    void Start()
    {
        //Start the VideoPlayerHeartbeat() function.
        SendCustomEventDelayedSeconds(nameof(VideoPlayerHeartbeat), videoPlayerHeartbeat);

        //Set the WolfePlayerPanel Controller variables
        foreach(WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
        {
            if (wolfePlayerPanel != null)
            {
                wolfePlayerPanel.wolfePlayerController = this;
            }
        }

        //Sets the volume to the default volume.
        SetVolume(defaultVolume);

        videoPlayer = (BaseVRCVideoPlayer)gameObject.GetComponent(typeof(BaseVRCVideoPlayer));
        if (Networking.LocalPlayer != null)
        {
            OnOwnershipTransferred(Networking.GetOwner(gameObject));
        }

        if(wolfeHooks != null)
        {
            wolfeHooks.SetWolfePlayerController(this);
        }

        SetCurrentVideo();
    }

    /// <summary>
    /// Forces the local users to sync to the Synced Variables
    /// </summary>
    public void ForceSync()
    {
        if (!SyncedUrlProperty.ToString().Equals("") && !Networking.IsOwner(gameObject))
        {
            videoPlayerLoading = true;
            videoPlayer.PlayURL(_syncedUrl);
        }
        SyncVideoPlaying();
        SyncVideoTimestamp();
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
    /// Toggles the Master Lock for the player. When this is on, no one but the world master will be able to use the synced video player controls.
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
    /// Sets all of the Timestamp Sliders to match the current time of the video.
    /// </summary>
    public void SetTimestampSliders()
    {
        foreach (WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
        {
            if (wolfePlayerPanel != null)
            {
                string timestampText = "livestream";
                if (!float.IsInfinity(videoPlayer.GetDuration()))
                {
                    wolfePlayerPanel.SetTimestamp(videoPlayer.GetTime() / videoPlayer.GetDuration());
                    if (!livestream)
                    {
                        timestampText = SecondsToTimestamp(videoPlayer.GetTime()) + " / " + SecondsToTimestamp(videoPlayer.GetDuration());
                    }
                }

                wolfePlayerPanel.SetTimestampText(timestampText);
            }
        }
    }

    /// <summary>
    /// Sets the Timestamp of the video to a specific time. 
    /// This will set the synced variable, as well as the local video time.
    /// </summary>
    /// <param name="timestamp"></param>
    public void SetTime(float timestamp)
    {
        if (!float.IsInfinity(videoPlayer.GetDuration()))
        {
            timestamp = Mathf.Abs(timestamp > videoPlayer.GetDuration() ? videoPlayer.GetDuration() : timestamp);
            timeScrubbingTime = timestamp * videoPlayer.GetDuration();
            SetAVProVideoTime(timeScrubbingTime);
        }
    }

    /// <summary>
    /// Sets the Timestamp of the video to a specific time if the controls aren't locked, and it isn't a livestream
    /// </summary>
    /// <param name="timestamp"></param>
    public void SetTimestampWithChecks(float timestamp)
    {
        if (!livestream & !AreControlsLocked)
        {
            SetTime(timestamp);
        }
    }

    /// <summary>
    /// Sets the video buffering state. This is used purely for visuals.
    /// </summary>
    /// <param name="buffering"></param>
    public void SetVideoBuffering(bool buffering)
    {
        videoBuffering = buffering;

        if (videoBuffering)
        {
            //play buffering animation
            foreach(WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
            {
                if (wolfePlayerPanel != null)
                {
                    wolfePlayerPanel.PlayBufferAnimation();
                }
            }
        }
        else
        {
            //Hide buffering animation
            foreach (WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
            {
                if (wolfePlayerPanel != null)
                {
                    wolfePlayerPanel.StopBufferAnimation();
                }
            }
        }
    }

    /// <summary>
    /// Runs the PlayNextInQueue() function for the Owner of this object
    /// </summary>
    public void SkipCurrentVideo()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "PlayNextInQueue");
    }

    /// <summary>
    /// Returns whether or not the next item in the queue is valid. Returns false on an empty queue.
    /// </summary>
    public bool IsNextInQueueValid
    {
        get
        {
            if (wolfeQueueController != null)
            {
                if (!wolfeQueueController.IsQueueEmpty)
                {
                    if (wolfeQueueController.VideoQueueProperty[0] != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Plays the next video in the Queue, if there is a next video.
    /// </summary>
    public void PlayNextInQueue()
    {
        if (IsNextInQueueValid)
        {
            videoPlayer.Pause();
            LoadUrl(wolfeQueueController.VideoQueueProperty[0]);
            wolfeQueueController.AdvanceQueue();
        }
    }


    /// <summary>
    /// Loads a URL into the Video Player.
    /// This function will only work if the video player controls are not locked.
    /// </summary>
    /// <param name="url">The URL Object that the video player should load</param>
    public void LoadVideoUrl(VRCUrl url)
    {
        if (!AreControlsLocked)
        {
            if (url.Get().Length <= maxUrlLength)
            {
                LoadUrl(url);
                foreach (WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
                {
                    if (wolfePlayerPanel != null)
                    {
                        wolfePlayerPanel.ClearUrlField();
                    }
                }
            }
            else
            {
                ShowError("URL Too Long");
            }
        }
    }

    /// <summary>
    /// Loads a URL into the Video Player.
    /// There are no checks associated with this function.
    /// </summary>
    /// <param name="url">The URL Object that the video player should load</param>
    public void LoadUrl(VRCUrl url)
    {
        videoPlayer.Stop();
        SetVideoTime(0);
        SetSyncedUrlWithChecks(url);
        timeScrubbingTime = 0;
        if (url.Get().IndexOf("?t=") != -1)
        {
            string timeStamp = url.Get().Substring(url.Get().IndexOf("?t="), ((url.Get().Length) - url.Get().IndexOf("?t=")));
            string substring = timeStamp.Substring(3, (timeStamp.Length - 3));
            timeScrubbingTime = ConvertToInt(substring);
        }
        else if (url.Get().IndexOf("&t=") != -1)
        {
            string timeStamp = url.Get().Substring(url.Get().IndexOf("&t="), ((url.Get().Length) - url.Get().IndexOf("&t=")));
            string substring = timeStamp.Substring(3, (timeStamp.Length - 3));
            timeScrubbingTime = ConvertToInt(substring);
        }
    }
    

    /// <summary>
    /// This is a function to set the AVPro Video Player time, this will check to make sure the time isn't being set to a negative number, or more than the length of the video
    /// </summary>
    /// <param name="time"></param>
    public void SetAVProVideoTime(float time)
    {
        if (float.IsInfinity(time))
        {
            time = 0;
        }
        videoPlayer.SetTime(Mathf.Clamp(time, 0, videoPlayer.GetDuration()));
    }

    /// <summary>
    /// The heartbeat for the Video Player
    /// This function should run every other second, as the video player does not need to be updated continuously.
    /// </summary>
    public void VideoPlayerHeartbeat()
    {
        if (Networking.IsOwner(gameObject))
        {
            SetServerTimestamp(Networking.GetServerTimeInSeconds());
            if (!videoPlayerLoading)
            {
                if (IsPlayerScrubbing)
                {
                    ShowError("Trying to load past Buffer");
                    SetAVProVideoTime(timeScrubbingTime);
                }
                else if (timeScrubbingTime <= videoPlayer.GetDuration())
                {
                    ShowError("");
                    timeScrubbingTime = -1;
                }
            }
            
            if(prevVideoTime != videoPlayer.GetTime())
            {
                SetVideoTime(videoPlayer.GetTime());
            }
        }
        else if (!livestream && IsUserOffsync)
        {
            SetAVProVideoTime(RealVideoTime);
        }

        SetTimestampSliders();

        if(prevVideoTime == videoPlayer.GetTime() && IsPlayingProperty)
        {
            SetVideoBuffering(true);
        }
        else
        {
            SetVideoBuffering(false);
        }
        prevVideoTime = videoPlayer.GetTime();

        if (videoTimeout)
        {
            videoTimeout = false;
            videoPlayer.PlayURL(_syncedUrl);
            if (!Networking.IsOwner(gameObject))
            {
                SyncVideoPlaying();
            }
        }
        if (invalidUrl)
        {
            invalidUrl = false;
            if (Networking.IsOwner(gameObject))
            {
                if (IsNextInQueueValid)
                {
                    PlayNextInQueue();
                }
                else
                {
                    videoPlayer.Stop();
                    SetIsPlaying(false);
                }
            }
        }

        SendCustomEventDelayedSeconds(nameof(VideoPlayerHeartbeat), videoPlayerHeartbeat);
    }

    /// <summary>
    /// Prints the current error/status message on the video player
    /// </summary>
    /// <param name="error"></param>
    public void ShowError(string error)
    {
        if (!error.Equals("") && showDebugLogErrors)
        {
            Debug.Log("Wolve Video Player Error: " + error);
        }
        foreach(WolfePlayerPanel wolfePlayerPanel in wolfePlayerPanels)
        {
            if (wolfePlayerPanel != null)
            {
                wolfePlayerPanel.SetErrorText("Video Status: " + error);
            }
        }
    }

    /* 
    * A word of warning, putting functions directly in here causes VRChat to crash a lot.
    * I believe it has something to do with this function being called too many times too quickly, 
    * So it just ends up completely crashing the client.
    * I try to only set a bool in here, and check that bool on the Heartbeat for this very reason.
    */
    /// <summary>
    /// When the Video Player throws an error, this function is called.
    /// </summary>
    /// <param name="videoError"></param>
    public override void OnVideoError(VideoError videoError)
    {
        ShowError(videoError.ToString());

        if (videoError == VideoError.RateLimited)
        {
            videoTimeout = true;
        }

        if (videoError == VideoError.PlayerError)
        {
            videoTimeout = true;
        }

        if (videoError == VideoError.InvalidURL)
        {
            invalidUrl = true;
            videoPlayer.Stop();
        }
    }
    
}
