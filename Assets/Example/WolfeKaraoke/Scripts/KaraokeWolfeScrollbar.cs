
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class KaraokeWolfeScrollbar : UdonSharpBehaviour
{
    [SerializeField] private RectTransform scrollParent;
    [SerializeField] private RectTransform parentCanvas;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Mask viewportMask;

    private float canvasHeight = 0f;
    private float canvasPosition = 0f;
    private float viewportHeight = 0f;

    public void SetParentCanvasHeight(float height)
    {
        scrollParent.sizeDelta = new Vector2(parentCanvas.sizeDelta.x, height);
        CalculateScrollSize();
    }

    public void ScrollValueChanged()
    {
        float value = scrollbar.value;

        canvasPosition = (value * (canvasHeight - viewportHeight)) - ((canvasHeight - viewportHeight) / 2) ;

        scrollParent.localPosition = new Vector3(scrollParent.localPosition.x, canvasPosition, scrollParent.localPosition.z);
    }

    public void CalculateScrollSize()
    {
        canvasHeight = scrollParent.sizeDelta.y;
        viewportHeight = parentCanvas.sizeDelta.y * viewport.localScale.y;
        ScrollValueChanged();
    }

    void Start()
    {
        CalculateScrollSize();
    }
}
