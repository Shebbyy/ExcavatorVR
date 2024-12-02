using System;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace MachineProject.CustomScripts
{
    public class UpdateCircularRememberedPosToHinge : MonoBehaviour
    {

        protected Interactable interactable;
        protected CircularDrive circularDrive;
        protected Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand;
        private bool isAttached = false;
        
        // Start is called before the first frame update
        void Start()
        {
            interactable = GetComponent<Interactable>();
            circularDrive = GetComponent<CircularDrive>();
        }
        
        // SteamVR Event, to bind the hand to the lever after a grab interaction is started, it should also set the circular-drive out-Angle to 0,
        // so after grabbing the lever again it does not jump to the rotation it was at when it was let go
        protected virtual void HandHoverUpdate( Hand hand )
        {
            GrabTypes startingGrabType = hand.GetGrabStarting();

            if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
            {
                isAttached = true;
                // todo get current rotation to keep lever at position
                //circularDrive.outAngle = transform.localRotation.x;
                circularDrive.outAngle = 0;
                // todo adjust hand position so its on the handle
                hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
            }
        }
        
        // SteamVR Event, after Event is let go
        protected virtual void HandAttachedUpdate(Hand hand)
        {
            // If Grab is let go, then detach hand so it adjusts back to the controller position
            if (hand.IsGrabEnding(this.gameObject))
            {
                hand.DetachObject(gameObject);
                isAttached = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // If its not attached, do a spring-like behavior and slowly put the rotation back to 0/0/0 over time;
            // Theoretically available in the Hinge, but due to IsKinetic = true of the Rigidbody, the Spring does not behave as it should
            if (   !isAttached
                && MathF.Abs(transform.localEulerAngles.x) >= 0.02 ) {
                transform.SetLocalPositionAndRotation(transform.localPosition, Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0f, 0f, 0f), 1.0f * Time.deltaTime));
            }
        }
    }
}
