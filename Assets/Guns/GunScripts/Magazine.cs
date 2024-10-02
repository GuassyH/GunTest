using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magazine : MonoBehaviour
{


    [SerializeField, Min(0)] private int MaxBullets;
    [SerializeField] public int CurrentBullets;
    [SerializeField] public GameObject Bullet;

    BoxCollider boxCollider;


    // Start is called before the first frame update
    void Awake()
    {
        CurrentBullets = MaxBullets;
        boxCollider = this.GetComponentInChildren<BoxCollider>();
    }

    private void Update() {
        if(transform.parent){
            boxCollider.isTrigger = true;
        }
    }

}
