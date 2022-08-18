
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WolfeHooks : UdonSharpBehaviour
{
    [HideInInspector] public WolfePlayerController wolfePlayerController;

    public void SetWolfePlayerController(WolfePlayerController controller)
    {
        wolfePlayerController = controller;
    }




    /// <summary>
    /// The Synced URL has been updated
    /// </summary>
    /// <param name="syncedUrl"></param>
    public void SyncedUrlHook(VRCUrl syncedUrl)
    {
        Debug.Log("Wolfe Player Hook: syncedUrl: " + syncedUrl.ToString());
    }

    /// <summary>
    /// The Master Lock has been updated
    /// </summary>
    /// <param name="masterLocked"></param>
    public void MasterLockedHook(bool masterLocked)
    {
        Debug.Log("Wolfe Player Hook: masterLocked: " + masterLocked);
    }

    /// <summary>
    /// The Video Time has been updated
    /// </summary>
    /// <param name="videoTime"></param>
    public void VideoTimeHook(float videoTime)
    {
        Debug.Log("Wolfe Player Hook: videoTime: " + videoTime);
    }

    /// <summary>
    /// Whether the video is playing has been updated
    /// </summary>
    /// <param name="isPlaying"></param>
    public void IsPlayingHook(bool isPlaying)
    {
        Debug.Log("Wolfe Player Hook: isPlaying: " + isPlaying);
    }

    /// <summary>
    /// Video has started playing<para/>
    /// public override void OnVideoPlay()
    /// </summary>
    public void OnVideoPlayHook()
    {
        Debug.Log("Wolfe Player Hook: OnVideoPlay");
    }

    /// <summary>
    /// Video has ended<para/>
    /// public override void OnVideoEnd()
    /// </summary>
    public void OnVideoEndHook()
    {
        Debug.Log("Wolfe Player Hook: OnVideoEnd");
    }

    /// <summary>
    /// Video has looped<para/>
    /// public override void OnVideoLoop()
    /// </summary>
    public void OnVideoLoopHook()
    {
        Debug.Log("Wolfe Player Hook: OnVideoLoop");
    }

    /// <summary>
    /// Video is ready<para/>
    /// public override void OnVideoReady()
    /// </summary>
    public void OnVideoReadyHook()
    {
        Debug.Log("Wolfe Player Hook: OnVideoReady");
    }

    /// <summary>
    /// Video has Started<para/>
    /// public override void OnVideoStart()
    /// </summary>
    public void OnVideoStartHook()
    {
        Debug.Log("Wolfe Player Hook: OnVideoStart");
    }




    /*
     * //Useful Functions from the Controller what can be called by any script
     * //From my experience, it's best to only activate these as one user, as it causes ownership transfers.
     * //Please read the function descriptions in "WolfePlayerController" for use.
     * 
     * wolfePlayerController.PlayNextInQueue();
     * 
     * wolfePlayerController.LoadUrl(VRCUrl url);
     * 
     * wolfePlayerController.Pause();
     * 
     * wolfePlayerController.Play();
     * 
     * wolfePlayerController.SetTime(float timestamp);
     * 
     * 
     * 
     * //Queue Functions
     * 
     * wolfePlayerController.AppendToQueue(VRCUrl url);
     * 
     * wolfePlayerController.ClearQueue();
     * 
     * wolfePlayerController.SkipCurrentVideo();
     * 
     * 
     * 
     * 
     * 
     * 
     */







}
