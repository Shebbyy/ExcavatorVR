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
        
        // SteamVR Event, wenn vom Hover zum grabben übergegangen wird
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
        
        // SteamVR Event, wenn das Objekt losgelassen wird
        protected virtual void HandAttachedUpdate(Hand hand)
        {
            if (hand.IsGrabEnding(this.gameObject))
            {
                hand.DetachObject(gameObject);
                isAttached = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (   !isAttached
                && MathF.Abs(transform.localEulerAngles.x) >= 0.02 ) {
                transform.SetLocalPositionAndRotation(transform.localPosition, Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0f, 0f, 0f), 1.0f * Time.deltaTime));
            }
        }
    }
}
