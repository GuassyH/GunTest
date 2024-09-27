using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScopeZoom : MonoBehaviour
{


    public List<float> ZoomValues = new List<float>{ 40f };
    public Camera scopeCam;
    public float zoopSpeed = 24;
    [Space]

    public MeshRenderer ScopeOcular;

    int selectedZoomValue;
    Gun gunParent;
    CameraLook cameraLook;

    // Start is called before the first frame update
    void Start()
    {
        selectedZoomValue = 0;
        gunParent = transform.GetComponentInParent<Gun>();
        cameraLook = FindAnyObjectByType<CameraLook>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gunParent.IsAiming){

            scopeCam.enabled = true;


            if(Input.GetKey(KeyCode.LeftAlt)){

                if(Input.GetAxis("Mouse ScrollWheel") > 0){
                    Debug.Log("Scroll Up");
                    if(selectedZoomValue <= ZoomValues.Count - 2){
                        selectedZoomValue++;
                    }
                }
                if(Input.GetAxis("Mouse ScrollWheel") < 0){
                    Debug.Log("Scroll Down");
                    if(selectedZoomValue >= 1){
                        selectedZoomValue--;
                    }
                }
            }
            //cameraLook.modifier = ZoomValues[selectedZoomValue] / 50;
            cameraLook.modifier = scopeCam.fieldOfView / Camera.main.fieldOfView * 1.5f;
        }
        else{
          scopeCam.enabled = false;

        }

        scopeCam.fieldOfView = Mathf.Lerp(scopeCam.fieldOfView, ZoomValues[selectedZoomValue], zoopSpeed * Time.deltaTime);
        
        
    }
}
