using System;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace MachineProject.CustomScripts.VehicleControls
{
    public class Joystick : MonoBehaviour
    {
        [SerializeField] 
        [Tooltip("The Left Hand Game-Object, used to find out, if the hand is currently grabbing the object")]
        public Hand leftHand;

        [SerializeField] 
        [Tooltip("The right Hand Game-Object, used to find out, if the hand is currently grabbing the object")]
        public Hand rightHand;

        [SerializeField] 
        [Tooltip("the left/back limit on how far the joystick can be rotated at max")]
        public float minLimit = -30f;

        [SerializeField] 
        [Tooltip("the right/forward limit on how far the joystick can be rotated at max")]
        public float maxLimit = 30f;

        protected Interactable interactable;
        private bool isAttached = false;
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
    
            // Always remember the prior grab-Rotation so the rotation can be calculated based on it
            if (!interactable.attachedToHand && !isAttached && startingGrabType != GrabTypes.None)
            {
                beforeGrabRotation = hand.transform.rotation.eulerAngles;
            }
            
            // If a hand currently grips the object, then adjust the stick-rotation according to the hand rotation,
            // this simulates the joystick being grabbed by the hand
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
            // If no Hand is grabbing the object and the Joystick is rotated, rotate it back to 0/0/0 over time (Spring Behavior)
            if (   !SteamVR_Input.GetState("GrabGrip", leftHand.handType)
                && !SteamVR_Input.GetState("GrabGrip", rightHand.handType)
                && (MathF.Abs(transform.localEulerAngles.x) >= 0.02
                    || MathF.Abs(transform.localEulerAngles.y) >= 0.02))
            {
                transform.SetLocalPositionAndRotation(transform.localPosition,
                    Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0f, 0f, 0f), 5f * Time.deltaTime));
            }
        }
    }
}