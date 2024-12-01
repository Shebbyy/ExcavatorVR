using System;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace MachineProject.CustomScripts.VehicleControls
{
    public class Joystick : MonoBehaviour
{
    protected Interactable interactable;
    private bool isAttached = false;
    public Hand leftHand;
    public Hand rightHand;

    public float minLimit = -30f;
    public float maxLimit = 30f;

    private Vector3 beforeGrabRotation;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    // SteamVR Event, for switching from hover to grab / handle the grab behavior
    protected virtual void HandHoverUpdate(Hand hand)
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();

        if (!interactable.attachedToHand && !isAttached && startingGrabType != GrabTypes.None)
        {
            beforeGrabRotation = hand.transform.rotation.eulerAngles;
        }
        
        if (SteamVR_Input.GetState("GrabGrip", hand.handType)) {
            Vector3 currentGrabRotation = hand.transform.rotation.eulerAngles;

            transform.SetLocalPositionAndRotation(transform.localPosition,
                Quaternion.Euler(Mathf.Clamp(currentGrabRotation.x - beforeGrabRotation.x, minLimit, maxLimit),
                    Mathf.Clamp(currentGrabRotation.y - beforeGrabRotation.y, minLimit, maxLimit),
                    transform.localEulerAngles.z));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (   !SteamVR_Input.GetState("GrabGrip", leftHand.handType)
            && !SteamVR_Input.GetState("GrabGrip", rightHand.handType)
            && (MathF.Abs(transform.localEulerAngles.x) >= 0.02
                || MathF.Abs(transform.localEulerAngles.y) >= 0.02))
        {
            Debug.Log("Stopping!");
            transform.SetLocalPositionAndRotation(transform.localPosition,
                Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0f, 0f, 0f), 5f * Time.deltaTime));
        }
    }
}
}