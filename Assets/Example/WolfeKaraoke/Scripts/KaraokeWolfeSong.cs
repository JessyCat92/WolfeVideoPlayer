using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

public class KaraokeWolfeSong : UdonSharpBehaviour
{
    [SerializeField] public VRCUrl SongURL = VRCUrl.Empty;
    [SerializeField] public string songName = "";
    [SerializeField] public string songArtist = "";
    [SerializeField] public Sprite albumArt;
    [SerializeField] private Image albumArtImage;
    [SerializeField] private Text songNameText;
    [SerializeField] private Text songArtistText;
    [SerializeField] private VRCPlayerApi player;

    private KaraokeWolfeController karaokeWolfeController;
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (songNameText != null)
        {
            songNameText.text = songName;
        }
        if (songArtistText != null)
        {
            songArtistText.text = songArtist;
        }
        if (albumArtImage != null && albumArt != null)
        {
            albumArtImage.sprite = albumArt;
        }
        player = Networking.LocalPlayer;
    }

    public void SetHeight(float height)
    {
        if(rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
        }
    }

    public void SetKaraokeWolfeController(KaraokeWolfeController controller)
    {
        karaokeWolfeController = controller;
    }

    public void AddToKaraokeQueue()
    {
        if (karaokeWolfeController != null)
        {
            karaokeWolfeController.AppendToQueue(SongURL);
        }
    }

    public void PlayKaraokeSong()
    {
        if (karaokeWolfeController != null)
        {
            karaokeWolfeController.LoadUrl(SongURL);
        }
    }
}
