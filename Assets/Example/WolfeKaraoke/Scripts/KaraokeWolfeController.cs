
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class KaraokeWolfeController : UdonSharpBehaviour
{
    [SerializeField] private WolfePlayerController wolfePlayerController;
    [SerializeField] private KaraokeWolfeScrollbar wolfeScrollbar;
    [SerializeField] private GameObject scrollParent;
    [SerializeField] private float itemSpacing = 10f;
    [SerializeField] private float itemHeight = 200f;

    private KaraokeWolfeSong[] karaokeItems;
    
    public void AppendToQueue(VRCUrl url)
    {
        if(wolfePlayerController != null)
        {
            wolfePlayerController.AppendToQueue(url);
        }
    }

    public void LoadUrl(VRCUrl url)
    {
        if (wolfePlayerController != null)
        {
            wolfePlayerController.LoadUrl(url);
        }
    }
    
    void Start()
    {
        karaokeItems = scrollParent.GetComponentsInChildren<KaraokeWolfeSong>();
        float scrollParentHeight = karaokeItems.Length * (itemHeight + itemSpacing);
        if(wolfeScrollbar != null)
        {
            wolfeScrollbar.SetParentCanvasHeight(scrollParentHeight);
        }
        float spacing = 0f;
        int spacingSuccesses = 0;
        for (int i = 0; i < karaokeItems.Length; i++)
        {
            if (karaokeItems[i] != null)
            {
                spacing = (scrollParentHeight / 2) - (itemHeight / 2) - ((itemHeight + itemSpacing) * spacingSuccesses);
                if (i == karaokeItems.Length - 1)
                {
                    spacing -= itemSpacing;
                }
                KaraokeWolfeSong karaokeItem = karaokeItems[i];
                karaokeItem.SetKaraokeWolfeController(this);
                karaokeItem.SetHeight(itemHeight);
                karaokeItem.transform.localPosition = new Vector3(karaokeItem.transform.localPosition.x, spacing, karaokeItem.transform.localPosition.z);
                spacingSuccesses++;
            }
            
        }
    }
}
