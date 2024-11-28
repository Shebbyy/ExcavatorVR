using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MachineProject.CustomScripts.VehicleControls
{
    public class MachineExcavatorControls : MachineTrackBasedControls
    {
        public GameObject leftJoystick;
        public GameObject rightJoystick;
        // Start is called before the first frame update
        void Start()
        {
            InitTrackMovementVars();
        }

        // Update is called once per frame
        void Update()
        {
            HandleTrackMovement();
        }
    }
}

