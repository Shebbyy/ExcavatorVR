using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace MachineProject.CustomScripts
{
    public class UpdateCircularRememberedPosToHinge : MonoBehaviour
    {
        
        protected Interactable interactable;
        protected CircularDrive circularDrive;
        protected LinearMapping linearMapping;
        protected Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand;
        
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
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
