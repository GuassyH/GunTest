using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float Smoothness;

    // Start is called before the first frame update
    Rigidbody rb;
    
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.position, Smoothness * Time.deltaTime);
        //rb.position = player.position;
    }

}
