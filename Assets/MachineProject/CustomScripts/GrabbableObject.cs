using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace MachineProject.CustomScripts
{
    public class GrabbableObject : MonoBehaviour
    {
        
        protected Interactable interactable;
        protected Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand;
        
        // Start is called before the first frame update
        void Start()
        {
            interactable = GetComponent<Interactable>();
        }
        
        // SteamVR Event, wenn vom Hover zum grabben übergegangen wird
        protected virtual void HandHoverUpdate( Hand hand )
        {
            GrabTypes startingGrabType = hand.GetGrabStarting();

            if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
            {
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
            // Hier sollte dann immer die Position von der Hand gepollt werden und das Objekt zum nähesten Punkt nachziehen; TODO
        }
    }
}
