using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Camerainteract : MonoBehaviour
{

    public RaycastHit hit;
    public RaycastHit AllHit;

    public LayerMask Interactable;
    public Animator CrosshairAnim;

    [Header("Pickup")]
    public GameObject holder;
    bool holding;

    Transform Temp_holdingObjParent;


    // Start is called before the first frame update
    void Start()
    {
        holding = false;
    }

    // Update is called once per frame
    void Update()
    {
        Physics.Raycast(this.transform.position, this.transform.forward, out AllHit, 400f);

        if(Physics.Raycast(this.transform.position, this.transform.forward, out hit, 3f, Interactable)){
            CrosshairAnim.SetBool("Hovering", true);
            if(Input.GetKeyDown(KeyCode.F)){
                if(hit.transform.CompareTag("Pickupable")){
                    Pickup(hit.transform.gameObject);
                }
            }
        }
        else{

            CrosshairAnim.SetBool("Hovering", false);
        }

        if(holder.transform.GetComponentInChildren<Gun>()){
            CrosshairAnim.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
        }else{
            CrosshairAnim.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
        }

        if(holding){
            if(Input.GetKeyDown(KeyCode.G)){
                Drop(holder.transform.GetChild(0).transform.gameObject);
            }
        }

    }
        
    void Pickup(GameObject obj){
        if(holder.transform.childCount > 0 || holding){    return;     }        

        Rigidbody objRb = obj.GetComponent<Rigidbody>(); 
        Collider objCol = obj.GetComponent<Collider>(); 

        
        objRb.isKinematic = true;
        objRb.freezeRotation = true;
        objCol.isTrigger = true;

        foreach (Collider col in obj.GetComponentsInChildren<Collider>())
        {
            col.isTrigger = true;
        }

        Temp_holdingObjParent = obj.transform.parent;

        var InteractableScript = obj.GetComponent<MonoBehaviour>();

        obj.transform.SetParent(holder.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localEulerAngles = Vector3.zero;

        holding = true;

    }
    
    void Drop(GameObject obj){
        
        Rigidbody objRb = obj.GetComponent<Rigidbody>(); 
        Collider objCol = obj.GetComponent<Collider>(); 
        
        objRb.isKinematic = false;
        objRb.freezeRotation = false;
        objCol.isTrigger = false;

        obj.transform.SetParent(Temp_holdingObjParent);

        objRb.AddForce(this.transform.forward * 4, ForceMode.Impulse);

        
        foreach (Collider col in obj.GetComponentsInChildren<Collider>())
        {
            col.isTrigger = false;
        }

        Temp_holdingObjParent = null;

        holding = false;
    }

}

