using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;
    public Transform orientation;

    float rotationX = 0F;
    float rotationY = 0F;

    Quaternion originalRotation;

    Rigidbody rb;

    [HideInInspector] public float modifier;
    [HideInInspector] public bool IsZoomed;

    void Update ()
    {

        if(!IsZoomed){
            modifier = 1;
        }

        if (axes == RotationAxes.MouseXAndY)
        {
            // Read the mouse input axis
            rotationX += Input.GetAxis("Mouse X") * sensitivityX * modifier;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY * modifier;

            rotationX = ClampAngle (rotationX, minimumX, maximumX);
            rotationY = ClampAngle (rotationY, minimumY, maximumY);

            Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, -Vector3.right);

            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        }
        else if (axes == RotationAxes.MouseX)
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationX = ClampAngle (rotationX, minimumX, maximumX);

            Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
            transform.localRotation = originalRotation * xQuaternion;
        }
        else
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = ClampAngle (rotationY, minimumY, maximumY);

            Quaternion yQuaternion = Quaternion.AngleAxis (-rotationY, Vector3.right);
            transform.localRotation = originalRotation * yQuaternion;
        }

        orientation.eulerAngles = new Vector3(0,rotationX);
    }

    void Start ()
    {
        // Make the rigid body not change rotation
        originalRotation = transform.localRotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static float ClampAngle (float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp (angle, min, max);
    }
}

