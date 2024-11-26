using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace MachineProject.CustomScripts.VehicleControls
{
    public abstract class MachineTrackBasedControls : MonoBehaviour
    {
        public GameObject leftControlLever;
        public GameObject rightControlLever;
        public CircularDrive.Axis_t controllerLocalAxis;

        public float velocity = 0.5f;
        public float leverDeadZone = 0.03f;
        private float treadDistance;
        private float leftMaxAngle = 45f;
        private float rightMaxAngle = 45f;

        protected void InitTrackMovementVars()  {
            leftMaxAngle  = leftControlLever.GetComponent<CircularDrive>().maxAngle;
            rightMaxAngle = rightControlLever.GetComponent<CircularDrive>().maxAngle;
            treadDistance = GetComponent<BoxCollider>().size.x;
        }

        protected void HandleTrackMovement() {
            float leftTrackAngle  = leftControlLever.transform.localEulerAngles.x;
            float rightTrackAngle = rightControlLever.transform.localEulerAngles.x;
            
            Move(GetSpeedFromControllerAngle(leftTrackAngle, leftMaxAngle),
                 GetSpeedFromControllerAngle(rightTrackAngle, rightMaxAngle));
        }

        private float GetSpeedFromControllerAngle(float angle, float maxAngle)
        {
            if (angle > 180)
            {
                angle -= 360;
            }

            float value = angle / maxAngle * velocity;
            return MathF.Abs(angle) >= leverDeadZone ? value : 0;
        }

        void Move(float leftSpeed, float rightSpeed){
            int dir = 0;//direction
            float s = 0;//slow tread (closer to 0)
            float f = 0;//fast tread (farther from 0)

            if (leftSpeed == rightSpeed) {//going straight
                this.transform.Translate (new Vector3 (0, 0, leftSpeed * Time.deltaTime));
            } else {
                //---set fast/slow
                if (Mathf.Abs(leftSpeed) < Mathf.Abs(rightSpeed)) {
                    dir = -1;
                    s = leftSpeed;
                    f = rightSpeed;
                } else {
                    dir = 1;
                    f = leftSpeed;
                    s = rightSpeed;
                } 

                f *= Time.deltaTime; s *= Time.deltaTime;

                //---calculacte radius of inner drive circle
                float r;//radius of inner circle
                if (s == 0) {
                    r = 0;
                } else {
                    r = treadDistance / ((f/s) - 1);
                }

                //---calc theta (degrees to rotate)
                float t = (f / (r + treadDistance));//add treadDistance (and use f) to avoind divistion by 0

                //--calculate travel distance
                float distance = ((r + treadDistance / 2) * Mathf.Sin (t / 2) * 2);//distance to move at angle
                
                //---do the moving
                this.transform.Rotate (new Vector3 (0,(Mathf.Rad2Deg *t/2)*dir,0));//rotate halfway
                this.transform.Translate (new Vector3 (0,0,distance));//move
                this.transform.Rotate (new Vector3 (0,(Mathf.Rad2Deg *t/2)*dir,0));//rotate the rest
            }
        }
    }
}