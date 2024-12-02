using UnityEngine;

namespace MachineProject.CustomScripts.VehicleControls
{
    public class MachineExcavatorControls : MachineTrackBasedControls
    {
        [SerializeField] 
        [Tooltip("The Game Object of the left Joystick, x Rotation controls the outlier arm and y rotation the upper body rotation")]
        public GameObject leftJoystick;
        
        [SerializeField] 
        [Tooltip("The Game Object of the right Joystick, x Rotation controls the main arm and y rotation the bucket opening state4")]
        public GameObject rightJoystick;
        
        [SerializeField] 
        [Tooltip("The Game Object of the mainArm, controlled by the right joystick")]
        public GameObject arm1;
        
        [SerializeField] 
        [Tooltip("The Game Object of outlier arm, controlled by the left joystick")]
        public GameObject arm2;
        
        [SerializeField] 
        [Tooltip("The Game Object of the Bucket, controlled by the right joystick")]
        public GameObject bucket;
        
        [SerializeField] 
        [Tooltip("The Game Object of the upper body, controlled by the left joystick")]
        public GameObject upperBody;
        
        [SerializeField] 
        [Tooltip("The max-Rotation of the main arm, downwards")]
        public float arm1MaxRotation = 54f;
        
        [SerializeField] 
        [Tooltip("The min Rotation of the main arm, or max rotation upwards")]
        public float arm1MinRotation = 0f;
        
        [SerializeField] 
        [Tooltip("The max Rotation of the outlier Arm towards the main arm / main body of the excavator")]
        public float arm2MaxRotation = 32f;
        
        [SerializeField] 
        [Tooltip("The min Rotation of the outlier Arm, or max rotation away from the main arm / excavator body")]
        public float arm2MinRotation = -80f;
        
        [SerializeField] 
        [Tooltip("The max rotation of the bucket, or how far it can be closed")]
        public float bucketMaxRotation = 70f;
        
        [SerializeField] 
        [Tooltip("The min rotation of the bucket, or how far it can be opened")]
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

        // Handles the Joystick-Movements for all the different movable components
        private void HandleJoystickMovement()
        {
            //// InnerArm
            HandleArmMovement(arm1, rightJoystick.transform.localEulerAngles.x, rightJoystickMaxLimit, arm1MaxRotation, arm1MinRotation);
            
            // OuterArm
            HandleArmMovement(arm2, leftJoystick.transform.localEulerAngles.x, leftJoystickMaxLimit, arm2MaxRotation, arm2MinRotation, true);
            
            // Bucket
            HandleArmMovement(bucket, rightJoystick.transform.localEulerAngles.y, rightJoystickMaxLimit, bucketMaxRotation, bucketMinRotation, true);
            
            // Body
            float bodyVelocity = GetJoystickRotationChange(leftJoystick.transform.localEulerAngles.y, leftJoystickMaxLimit);

            float add = 40f;
            if (bodyVelocity < 0)
            {
                add *= -1f;
            }

            upperBody.transform.SetLocalPositionAndRotation(upperBody.transform.localPosition,
                Quaternion.Lerp(upperBody.transform.localRotation, Quaternion.Euler(upperBody.transform.localEulerAngles.x, upperBody.transform.localEulerAngles.y + add, upperBody.transform.localEulerAngles.z), Mathf.Abs(bodyVelocity) / 2 * Time.deltaTime));


            // In case the Joystick goes to Rotation 0/0/0, dont wait for the linear interpolation to finish,
            // but instantly stop the movement, so it does not feel unresponsive 
            if (leftJoystick.transform.localEulerAngles.y < leverDeadZone)  {
                upperBody.transform.localEulerAngles = upperBody.transform.localEulerAngles;
            }
        }

        // Rotates the according arm based on the joystick-information
        private void HandleArmMovement(GameObject arm, float joystickAngle, float joystickMaxLimit, float armMaxRotation,
            float armMinRotation, bool invertMovement = false)
        {
            float armVelocity = GetJoystickRotationChange(joystickAngle, joystickMaxLimit);
            

            // For the addition a fixed value is used, the speed is controlled via the Linear interpolation time
            float add = 40f;
            if (armVelocity < 0)
            {
                add *= -1f;
            }

            if (invertMovement)
            {
                add *= -1;
            }

            // if the arm-Angle is > 180, the angle should be negative
            float armAngle = arm.transform.localEulerAngles.x;
            if (armAngle > 180)  {
                armAngle -= - 360;
            }
            
            arm.transform.SetLocalPositionAndRotation(arm.transform.localPosition,
                Quaternion.Lerp(arm.transform.localRotation, Quaternion.Euler(Mathf.Clamp(armAngle + add, armMinRotation, armMaxRotation), 0f, 0f), Mathf.Abs(armVelocity) * Time.deltaTime));
        }

        // Calculates the realtive Rotation Change considering the max-Angle
        float GetJoystickRotationChange(float angle, float maxAngle)
        {
            

            float calcAngle = angle;
            if (angle > 180)
            {
                calcAngle -= 360;
            }
            
            if (Mathf.Abs(angle) < leverDeadZone) {
                return 0;
            }
            
            // Speed calculated with maxAngle as maxspeed and converted into negative in case the joystick points back
            return (Mathf.Abs(calcAngle) / maxAngle) * velocity * (angle <= 180 ? 1 : -1);
        }
    }
}

