using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MachineProject.CustomScripts.VehicleControls
{
    public class MachineExcavatorControls : MachineTrackBasedControls
    {
        public GameObject leftJoystick;
        public GameObject rightJoystick;
        public GameObject arm1;
        public GameObject arm2;
        public GameObject bucket;
        public GameObject upperBody;
        public float arm1MaxRotation = 54f;
        public float arm1MinRotation = 0f;
        public float arm2MaxRotation = 32f;
        public float arm2MinRotation = -80f;
        public float bucketMaxRotation = 70f;
        public float bucketMinRotation = -50;
        
        private float leftJoystickMaxLimit;
        private float rightJoystickMaxLimit;

        protected void InitHandleVars()
        {
            leftJoystickMaxLimit  = leftJoystick.GetComponent<Joystick>().maxLimit;
            rightJoystickMaxLimit = rightJoystick.GetComponent<Joystick>().maxLimit;
        }
        
        // Start is called before the first frame update
        void Start()
        {
            InitTrackMovementVars();
            InitHandleVars();
        }

        // Update is called once per frame
        void Update()  {
            HandleTrackMovement();
            HandleJoystickMovement();
        }

        private void HandleJoystickMovement()
        {
            // InnerArm
            HandleArmMovement(arm1, rightJoystick.transform.localEulerAngles.x, rightJoystickMaxLimit, arm1MaxRotation, arm1MinRotation);
            
            // OuterArm
            HandleArmMovement(arm2, leftJoystick.transform.localEulerAngles.x, leftJoystickMaxLimit, arm2MaxRotation, arm2MinRotation);
            
            // Bucket
            HandleArmMovement(bucket, rightJoystick.transform.localEulerAngles.y, rightJoystickMaxLimit, bucketMaxRotation, bucketMinRotation, true);
            
            // Body
            float bodyVelocity = GetJoystickRotationChange(leftJoystick.transform.localEulerAngles.y, leftJoystickMaxLimit);

            float add = 50f;
            if (bodyVelocity < 0)
            {
                add *= -1;
            }
            
            upperBody.transform.SetLocalPositionAndRotation(upperBody.transform.localPosition,
                Quaternion.Lerp(upperBody.transform.localRotation, Quaternion.Euler(upperBody.transform.localEulerAngles.x, upperBody.transform.localEulerAngles.y + add, upperBody.transform.localEulerAngles.z), Mathf.Abs(bodyVelocity) / 5 * Time.deltaTime));
        }

        private void HandleArmMovement(GameObject arm, float joystickAngle, float joystickMaxLimit, float armMaxRotation,
            float armMinRotation, bool invertMovement = false)
        {
            float armVelocity = GetJoystickRotationChange(joystickAngle, joystickMaxLimit);

            if (invertMovement)
            {
                armVelocity *= -1;
            }
            
            arm.transform.SetLocalPositionAndRotation(arm.transform.localPosition,
                Quaternion.Lerp(arm.transform.localRotation, Quaternion.Euler(armVelocity >= 0 ? armMaxRotation : armMinRotation, 0f, 0f), Mathf.Abs(armVelocity) / 5 * Time.deltaTime));
        }

        float GetJoystickRotationChange(float angle, float maxAngle)
        {
            if (Mathf.Abs(angle) < leverDeadZone) {
                return 0;
            }
            
            // Speed in Abhängigkeit zum MaxAngle mit Absolutwert von Angle berechnen, falls negativ dann invertieren
            return (Mathf.Abs(angle) / maxAngle) * velocity * (angle <= 180 ? 1 : -1);
        }
    }
}

