using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControls : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque, maxBrake, maxSpeed, maxReverseSpeed, reverseThresholdSpeed;
    public float maxSteeringAngle;
    public float maxVelocityMagnitudeToExit;

    public bool isDriving;

    public Rigidbody vehicleRigidbody;
    public Transform steeringWheel;
    public List<Collider> interiorInteractables;
    public Collider vehicleEntrance;

    public float smallBumpVolumeBody, bigBumpVolumeBody, continuousSoundsVolumeBody, 
        smallBumpVolumeWheels, bigBumpVolumeWheels, continuousSoundsVolumeWheels; 

    private BodyCam bCam;

    private Quaternion steeringWheelInitialRotation;

    private bool canReverse, isReversing, isBraking;

    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider) {
        if (collider.transform.childCount == 0) {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    private void Start() {
        bCam = FindObjectOfType<BodyCam>();

        steeringWheelInitialRotation = steeringWheel.localRotation;

        if (smallBumpVolumeWheels != 0 && bigBumpVolumeWheels != 0  && continuousSoundsVolumeWheels != 0) {
            if (axleInfos.Count > 0) {
                foreach (AxleInfo axl in axleInfos) {
                    if (axl.leftWheel.transform.GetComponent<VehicleGroundCollisions>()) {
                        axl.leftWheel.transform.GetComponent<VehicleGroundCollisions>().SetVolumeLevels(smallBumpVolumeWheels, bigBumpVolumeWheels, continuousSoundsVolumeWheels);
                    }
                    if (axl.rightWheel.transform.GetComponent<VehicleGroundCollisions>()) {
                        axl.rightWheel.transform.GetComponent<VehicleGroundCollisions>().SetVolumeLevels(smallBumpVolumeWheels, bigBumpVolumeWheels, continuousSoundsVolumeWheels);
                    }
                }
            }
        }

        if (interiorInteractables.Count > 0) {
            foreach (Collider interactable in interiorInteractables) {
                interactable.enabled = false;
            }
        }
    }

    void FixedUpdate() {
        if (isDriving && bCam.bodyControl) {

            float motor = 0;

            //if (Input.GetAxis("Triggers") > 0.1 && vehicleRigidbody.velocity.magnitude < maxSpeed && vehicleRigidbody.velocity.magnitude > reverseThresholdSpeed) {
            //    motor = maxMotorTorque * Input.GetAxis("Triggers");
            //}
            //else if (!isReversing && Input.GetAxis("Triggers") < -0.1 && vehicleRigidbody.velocity.magnitude > reverseThresholdSpeed) {
            //    if (this.transform.InverseTransformDirection(vehicleRigidbody.velocity).z > 0)      { motor = maxBrake * Input.GetAxis("Triggers"); }
            //    else if (this.transform.InverseTransformDirection(vehicleRigidbody.velocity).z < 0) { motor = maxBrake * -Input.GetAxis("Triggers"); }
            //}

            //if (vehicleRigidbody.velocity.magnitude < reverseThresholdSpeed)    { canReverse = true; }
            //else                                                                { canReverse = false; }

            //if (canReverse && Input.GetAxis("Triggers") < -0.1) {
            //    motor = maxMotorTorque * Input.GetAxis("Triggers");
            //    isReversing = true;
            //}
            //else {
            //    isReversing = false;
            //}

            ///////////////////////
            ///

            //if (Input.GetAxis("Triggers") > 0.1 && vehicleRigidbody.velocity.magnitude < maxSpeed) { motor = maxMotorTorque * Input.GetAxis("Triggers"); }

            //if (vehicleRigidbody.velocity.magnitude > reverseThresholdSpeed) {
            //    if (Input.GetAxis("Triggers") > 0.1) {
            //        isReversing = false;
            //        if (vehicleRigidbody.velocity.magnitude < maxSpeed) { motor = maxMotorTorque * Input.GetAxis("Triggers"); }
            //    }
            //    else if (Input.GetAxis("Triggers") < -0.1) {
            //        if (isReversing) {
            //            if (vehicleRigidbody.velocity.magnitude < maxSpeed) { motor = maxMotorTorque * Input.GetAxis("Triggers"); }
            //        }
            //        else {
            //            motor = maxBrake * Input.GetAxis("Triggers");
            //            isBraking = true;
            //        }
            //    }
            //}
            //else {
            //    if (isBraking) {
            //        if (Input.GetAxis("Triggers") > -0.1) {
            //            isBraking = false;
            //        }
            //        else {

            //        }
            //    }
            //    else {
            //        if (Input.GetAxis("Triggers") < -0.1)   { isReversing = true; }
            //        else                                    { isReversing = false; }

            //        motor = maxMotorTorque * Input.GetAxis("Triggers");
            //    }                
            //}

            //Debug.Log(isBraking + ", " + vehicleRigidbody.velocity.magnitude);

            float inputSteer = Input.GetAxis("Triggers");

            if (this.transform.InverseTransformDirection(vehicleRigidbody.velocity).z > 0) { vehicleRigidbody.velocity = Vector3.ClampMagnitude(vehicleRigidbody.velocity, maxSpeed); }
            if (this.transform.InverseTransformDirection(vehicleRigidbody.velocity).z < 0) { vehicleRigidbody.velocity = Vector3.ClampMagnitude(vehicleRigidbody.velocity, maxReverseSpeed); }

            if (vehicleRigidbody.velocity.magnitude < maxSpeed) {
                if (inputSteer > 0.1) { motor = maxMotorTorque * inputSteer; }
                else if (inputSteer < -0.1) { motor = maxBrake * inputSteer; }
            }

            float steering = maxSteeringAngle * Input.GetAxis("Left Joystick Horizontal");

            foreach (AxleInfo axleInfo in axleInfos) {
                if (axleInfo.steering) {
                    axleInfo.leftWheel.steerAngle = steering;
                    axleInfo.rightWheel.steerAngle = steering;
                }
                if (axleInfo.motor) {
                    axleInfo.leftWheel.motorTorque = motor;
                    axleInfo.rightWheel.motorTorque = motor;
                }

                ApplyLocalPositionToVisuals(axleInfo.leftWheel);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel);

                axleInfo.leftWheel.transform.GetComponent<VehicleGroundCollisions>().AdjustContinuousChannelVolumeByVehicleSpeed(vehicleRigidbody.velocity.magnitude);
                axleInfo.rightWheel.transform.GetComponent<VehicleGroundCollisions>().AdjustContinuousChannelVolumeByVehicleSpeed(vehicleRigidbody.velocity.magnitude);
            }

            
        }
    }

    public void ResetSteeringWheel() {
        steeringWheel.localRotation = steeringWheelInitialRotation;
    }

    public void EnableInteriorInteractables() {
        if (interiorInteractables.Count > 0) {
            foreach (Collider interactable in interiorInteractables) {
                interactable.enabled = true;
            }
        }

        //Disable Collider of entrance transform
        vehicleEntrance.enabled = false;
    }

    public void DisableInteriorInteractables() {
        if (interiorInteractables.Count > 0) {
           foreach (Collider interactable in interiorInteractables) {
                interactable.enabled = false;
            }
        }

        //Enable Collider of entrance transform
        vehicleEntrance.enabled = true;
    }

    public void HighSpeedExitWarning() {
        bCam.StopUsing();
    }

    [System.Serializable]
    public class AxleInfo {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public bool motor;
        public bool steering;
    }
}
