
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public class ToggleObject : UdonSharpBehaviour
{
    public GameObject[] toggleObjects;
    public MeshRenderer[] toggleRenderers;
    public Canvas[] toggleCanvases;
    public bool resetPosition;
    public string showText = "o";
    public string hideText = "x";
    public Text[] textToggles;
    private bool toggleState = true;
    private Vector3[] objectLocations;
    private Quaternion[] objectRotations;

    public void Start()
    {
        objectLocations = new Vector3[toggleObjects.Length];
        objectRotations = new Quaternion[toggleObjects.Length];
        //Debug.Log("Start..");
        for(int i = 0; i < toggleObjects.Length; i++)
        {
            objectLocations[i] = toggleObjects[i].transform.position;
            objectRotations[i] = toggleObjects[i].transform.rotation;
        }
        //Debug.Log("Finish..");
    }

    public void Toggle()
    {
        for(int i = 0; i < toggleObjects.Length; i++)
        {
            toggleObjects[i].SetActive(!toggleObjects[i].activeSelf);
        }
        for(int i = 0; i < toggleRenderers.Length; i++)
        {
            toggleRenderers[i].enabled = !toggleRenderers[i].enabled;
        }
        for (int i = 0; i < toggleCanvases.Length; i++)
        {
            toggleCanvases[i].enabled = !toggleCanvases[i].enabled;
        }

    }
    
    public void ToggleReset()
    {
        for (int i = 0; i < toggleObjects.Length; i++)
        {
            toggleObjects[i].SetActive(!toggleObjects[i].activeSelf);
            if (toggleObjects[i].activeSelf)
            {
                toggleObjects[i].transform.position = objectLocations[i];
                toggleObjects[i].transform.rotation = objectRotations[i];
            }
        }
    }

    public override void Interact()
    {
        for (int i = 0; i < toggleObjects.Length; i++)
        {
            toggleObjects[i].SetActive(!toggleObjects[i].activeSelf);
            if (resetPosition)
            {
                if (toggleObjects[i].activeSelf)
                {
                    toggleObjects[i].transform.position = objectLocations[i];
                    toggleObjects[i].transform.rotation = objectRotations[i];
                }
            }
        }
        for (int i = 0; i < toggleRenderers.Length; i++)
        {
            toggleRenderers[i].enabled = !toggleRenderers[i].enabled;
        }
        for (int i = 0; i < toggleCanvases.Length; i++)
        {
            toggleCanvases[i].enabled = !toggleCanvases[i].enabled;
        }
    }


    public void ToggleState()
    {
        toggleState = !toggleState;
        for (int i = 0; i < toggleObjects.Length; i++)
        {
            toggleObjects[i].SetActive(toggleState);
        }
        for (int i = 0; i < toggleRenderers.Length; i++)
        {
            toggleRenderers[i].enabled = toggleState;
        }
        for (int i = 0; i < toggleCanvases.Length; i++)
        {
            toggleCanvases[i].enabled = toggleState;
        }
        for(int i = 0; i < textToggles.Length; i++)
        {
            if (toggleState)
            {
                textToggles[i].text = hideText;
            }
            else
            {
                textToggles[i].text = showText;
            }
            
        }
    }


}
