using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolographicSight : MonoBehaviour
{


    public List<float> BrightnessValues = new List<float>{ 0.5f, 1f, 2f, 3f };
    public Color ReticleStartColor;
    
    [Space]
    public MeshRenderer ScopeOcular;
    public Material HoloMat;
    public float brightenSpeed = 30f;

    [Space]
    public AudioClip brightenSound;

    int selectedBrightness;
    Gun gunParent;
    CameraLook cameraLook;

    AudioManager audioManager;
    // Start is called before the first frame update
    void Start()
    {
        selectedBrightness = 1;
        gunParent = transform.GetComponentInParent<Gun>();
        cameraLook = FindAnyObjectByType<CameraLook>();

        HoloMat = ScopeOcular.material;
        //ReticleStartColor = HoloMat.GetColor("_RetBrightness");

        audioManager = FindAnyObjectByType<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Zoom();
    }

    void Zoom(){

        if(gunParent.IsAiming){

            if(Input.GetKey(KeyCode.LeftAlt)){

                if(Input.GetAxis("Mouse ScrollWheel") > 0){
                    if(selectedBrightness <= BrightnessValues.Count - 2){
                        selectedBrightness++;
                        // audioManager.PlayAudio(brightenSound, this.transform.position);
                    }
                }
                if(Input.GetAxis("Mouse ScrollWheel") < 0){
                    if(selectedBrightness >= 1){
                        selectedBrightness--;
                        // audioManager.PlayAudio(brightenSound, this.transform.position);
                    }
                }
            }
            // cameraLook.modifier = ZoomValues[selectedZoomValue] / 50;

            //HoloMat.SetFloat("_RetBrightness", Mathf.Lerp(HoloMat.GetFloat("_RetBrightness"), StartBrightness * BrightnessValues[selectedBrightness], brightenSpeed * Time.deltaTime));

        }
        else{

        }


    }

}
