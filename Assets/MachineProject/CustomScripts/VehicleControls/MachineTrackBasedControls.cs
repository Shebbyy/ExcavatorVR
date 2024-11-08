using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MachineProject.CustomScripts.VehicleControls
{
    public abstract class MachineTrackBasedControls : VehicleControls
    {
        public GameObject leftControlLever;
        public GameObject rightControlLever;

        public float velocity = 0.5f;
        public float leverDeadZone = 0.2f;
        
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        public override void DriveForward()
        {
            
        }

        public override void DriveBackward()
        {
            
        }

        public override void SteerLeft()
        {
            
        }

        public override void SteerRight()
        {
            
        }
    }
}