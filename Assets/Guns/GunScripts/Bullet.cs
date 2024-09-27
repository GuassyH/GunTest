using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public GameObject Casing;

  
    public AudioClip ImpactSound;
    public GameObject ImpactExplosion;


    public enum BulletType { Normal, HE, AP }
    public BulletType bulletType;

    public int Damage = 1;
    private Rigidbody rb;

    AudioManager audioManager;



    // Start is called before the first frame update
    void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        audioManager = FindObjectOfType<AudioManager>();

    }

    private void Update() {
        transform.rotation = Quaternion.LookRotation(rb.velocity);

        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x + 90, transform.localEulerAngles.y, 0);
        // transform.RotateAround(transform.position, transform.right, 90);
        // transform.RotateAround(transform.position, transform.up, 180);
    }


    private void OnCollisionEnter(Collision other) {
        audioManager.PlayAudio(ImpactSound, this.transform.position);
        Instantiate(ImpactExplosion, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    


}
