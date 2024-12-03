using System;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace MachineProject.CustomScripts.VehicleControls
{
    public abstract class MachineTrackBasedControls : MonoBehaviour
    {
        [SerializeField] 
        [Tooltip("The Game Object of the left Lever, Rotation Controls the left track")]
        public GameObject leftControlLever;
        
        [SerializeField] 
        [Tooltip("The Game Object of the right Lever, Rotation Controls the right track")]
        public GameObject rightControlLever;
        
        [SerializeField] 
        [Tooltip("The Axis of the levers to be considered for calculating the speed of the track movement, counts for both")]
        public CircularDrive.Axis_t controllerLocalAxis;

        [SerializeField] 
        [Tooltip("Velocity Base Value, all the movement-Speeds are based on this value")]
        public float velocity = 0.5f;
        
        [SerializeField] 
        [Tooltip("To make sure minor changes dont make the device go haywire, a small deadzone is needed, can be configured here")]
        public float leverDeadZone = 0.03f;
        
        private float treadDistance;
        private float leftMaxAngle = 45f;
        private float rightMaxAngle = 45f;

        // Initializes the Class Properties used for the speed calculations
        protected void InitTrackMovementVars()  {
            leftMaxAngle  = leftControlLever.GetComponent<CircularDrive>().maxAngle;
            rightMaxAngle = rightControlLever.GetComponent<CircularDrive>().maxAngle;
            treadDistance = GetComponent<BoxCollider>().size.x;
        }

        // Called by Update, gets the rotation from the levers and calls the move function
        protected void HandleTrackMovement() {
            float leftTrackAngle  = leftControlLever.transform.localEulerAngles.x;
            float rightTrackAngle = rightControlLever.transform.localEulerAngles.x;
            
            Move(GetSpeedFromControllerAngle(leftTrackAngle, leftMaxAngle),
                 GetSpeedFromControllerAngle(rightTrackAngle, rightMaxAngle));
        }

        // Calculates the Speed depending on the angle given from the lever and its maxValue
        private float GetSpeedFromControllerAngle(float angle, float maxAngle)
        {
            if (angle > 180)
            {
                angle -= 360;
            }

            float value = angle / maxAngle * velocity;
            return MathF.Abs(angle) >= leverDeadZone ? value : 0;
        }

        // The Track-Movement Function, this Function needs to calculate if the machine needs to be rotated in one direction
        // or if its allowed to go straight forward
        // This algorithm is based on this StackOverflow post: https://stackoverflow.com/questions/125099/formula-for-controlling-the-movement-of-a-tank-like-vehicle
        void Move(float leftSpeed, float rightSpeed){
            int dir = 0;
            float s = 0;
            float f = 0;

            // If Speed of both sides is identical, the movement should go forward
            if (leftSpeed == rightSpeed) {
                this.transform.Translate (new Vector3 (0, 0, leftSpeed * Time.deltaTime));
            } else {
                // Configures the fast and slow track and sets the direction the machine should turn to
                if (Mathf.Abs(leftSpeed) < Mathf.Abs(rightSpeed)) {
                    dir = -1;
                    s = leftSpeed;
                    f = rightSpeed;
                } else {
                    dir = 1;
                    f = leftSpeed;
                    s = rightSpeed;
                } 

                // Framerate-Independance-Adjustments
                f *= Time.deltaTime; 
                s *= Time.deltaTime;

                // calculacte radius of inner drive circle
                float r;
                if (s == 0) {
                    r = 0;
                } else {
                    r = treadDistance / ((f/s) - 1);
                }

                // Calculate how far the vehicle needs to rotate
                // The Tread-Distance, collected from the Track-Box-Collider is used for this
                float t = (f / (r + treadDistance));

                // Dependant on how straight the angle is, the machine needs to move further or less forward
                float distance = ((r + treadDistance / 2) * Mathf.Sin (t / 2) * 2);

                this.transform.Rotate(new Vector3(0, (Mathf.Rad2Deg * t / 2) * dir, 0));
                this.transform.Translate (new Vector3 (0,0,distance));
                this.transform.Rotate (new Vector3 (0,(Mathf.Rad2Deg *t/2)*dir,0));
            }
        }
    }
}