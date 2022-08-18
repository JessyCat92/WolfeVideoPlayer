
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RemoteController : UdonSharpBehaviour
{

    public Canvas toggleCanvas;
    private Vector3 objectScale;

    public void Start()
    {
        objectScale = toggleCanvas.transform.localScale;
        toggleCanvas.transform.localScale = new Vector3(0, 0, 0);
        toggleCanvas.transform.position = new Vector3(-1000, 10000, -1000);
        toggleCanvas.enabled = false;
    }

    
    public override void OnPickupUseDown()
    {
        UseRemote();
    }

    public void UseRemote()
    {
        toggleCanvas.enabled = !toggleCanvas.enabled;

        if (toggleCanvas.enabled)
        {
            Quaternion playerRotation = Networking.LocalPlayer.GetRotation();
            Vector3 playerPosition = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Head);
            toggleCanvas.transform.rotation = new Quaternion(playerRotation.x, playerRotation.y, playerRotation.z, playerRotation.w);
            toggleCanvas.transform.position = playerPosition + (toggleCanvas.transform.forward * 2f);
            toggleCanvas.transform.localScale = objectScale;
        }
        else
        {
            toggleCanvas.transform.localScale = new Vector3(0, 0, 0);
            toggleCanvas.transform.position = new Vector3(-1000,10000,-1000);
        }
    }


}
