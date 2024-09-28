using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamFOV : MonoBehaviour
{

    public Transform Holder;
    Camera cam;

    bool ChangeFOV;
    float FOV;
    // Start is called before the first frame update
    void Start()
    {
        cam = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Holder.GetComponentInChildren<Gun>()){
            if(Holder.GetComponentInChildren<Gun>().IsAiming){
                ChangeFOV = true;
            }
            else{
                ChangeFOV = false;
            }
        }
        else{
            ChangeFOV = false;
        }


        if(ChangeFOV){
            FOV = Holder.GetComponentInChildren<Gun>().AimFOV;
        }
        else{
            FOV = 75;
        }

        cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, FOV, 80 * Time.deltaTime);
    }
}
