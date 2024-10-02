using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScopeZoom : MonoBehaviour
{


    public List<float> ZoomValues = new List<float>{ 20f };
    public float StartCamZoom;
    public Camera scopeCam;
    public float zoomSpeed = 24;
    
    [Space]
    public MeshRenderer ScopeOcular;
    public Material ScopeMat;

    [Space]
    public AudioClip zoomSound;

    int selectedZoomValue;
    Gun gunParent;
    CameraLook cameraLook;

    AudioManager audioManager;
    // Start is called before the first frame update
    void Start()
    {
        selectedZoomValue = 0;
        gunParent = transform.GetComponentInParent<Gun>();
        cameraLook = FindAnyObjectByType<CameraLook>();

        ScopeMat = ScopeOcular.material;
        StartCamZoom = ScopeMat.GetFloat("_CamScale");

        audioManager = FindAnyObjectByType<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Zoom();
    }

    void Zoom(){

        if(gunParent.IsAiming){

            scopeCam.enabled = true;


            if(Input.GetKey(KeyCode.LeftAlt)){

                if(Input.GetAxis("Mouse ScrollWheel") > 0){
                    if(selectedZoomValue <= ZoomValues.Count - 2){
                        selectedZoomValue++;
                        audioManager.PlayAudio(zoomSound, this.transform.position);
                    }
                }
                if(Input.GetAxis("Mouse ScrollWheel") < 0){
                    if(selectedZoomValue >= 1){
                        selectedZoomValue--;
                        audioManager.PlayAudio(zoomSound, this.transform.position);
                    }
                }
            }
            // cameraLook.modifier = ZoomValues[selectedZoomValue] / 50;
            scopeCam.fieldOfView = Mathf.Lerp(scopeCam.fieldOfView, ZoomValues[selectedZoomValue], zoomSpeed * Time.deltaTime);
            cameraLook.modifier = scopeCam.fieldOfView / Camera.main.fieldOfView * 1.5f;
            // ScopeMat.SetFloat("_CamScale", Mathf.Lerp(ScopeMat.GetFloat("_CamScale"), StartCamZoom * ZoomValues[selectedZoomValue], zoomSpeed * Time.deltaTime));
        }
        else{
          scopeCam.enabled = false;

        }

    }

}
