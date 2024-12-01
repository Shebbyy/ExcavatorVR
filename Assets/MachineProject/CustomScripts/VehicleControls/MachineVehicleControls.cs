using UnityEngine;

namespace MachineProject.CustomScripts.VehicleControls
{
    public abstract class VehicleControls : MonoBehaviour
    {
        public abstract void DriveForward();

        public abstract void DriveBackward();

        public abstract void SteerLeft();

        public abstract void SteerRight();
    }

}