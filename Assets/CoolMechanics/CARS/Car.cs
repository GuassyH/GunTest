using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{

    [Header("Steering")]
    [SerializeField] private float SteerSpeed = 20f;
    [SerializeField] private float SteeringWheelRotations = 1;
    [Header("Wheel")]
    [SerializeField] private float maxWheelAngle = 60;
    [SerializeField] private List<WheelCollider> WheelObjects;
    [SerializeField] private List<WheelCollider> SteerableWheelObjects;
    [Header("Driving")]
    [SerializeField] private float DriveSpeed = 10;


    float steeringWheelRot;
    float wheelRot;

    float horizontal;
    float vertical;

    bool isDriving;

    Camera carCam;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        foreach (WheelCollider wheel in WheelObjects)
        {
            if(wheel.GetComponent<Wheel>().isSteered){
                SteerableWheelObjects.Add(wheel);
            }
        }    
  
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        Steering();
        Wheels();
        Drive();
    }

    public void EnterCar(){

    }


    void Steering(){


        steeringWheelRot += horizontal * SteerSpeed * Time.deltaTime * 10;
        steeringWheelRot = Mathf.Clamp(steeringWheelRot, 360 * -SteeringWheelRotations, 360 * SteeringWheelRotations);

    }

    void Wheels(){
        wheelRot = steeringWheelRot / (360 * SteeringWheelRotations) * maxWheelAngle;

        foreach (WheelCollider steeredWheel in SteerableWheelObjects){
            steeredWheel.steerAngle = wheelRot;
            steeredWheel.transform.localEulerAngles = new Vector3(0, wheelRot, 0);
            
        }
    }


    void Drive(){

        //if(vertical > 0 || vertical < 0){

            foreach (WheelCollider wheel in SteerableWheelObjects)
            {

                if(vertical > 0){
                    wheel.motorTorque = DriveSpeed * vertical;
                }
                else if(vertical < 0){
                    wheel.motorTorque = 0;
                }
            }

   
        //}

    }

} 
