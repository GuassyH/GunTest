using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Bullet : MonoBehaviour
{


    public enum BulletType { Normal, HE, AP }
    public BulletType bulletType;

    [HideInInspector] public float Damage;

    public GameObject Casing;
  
    public AudioClip ImpactSound;
    public GameObject ImpactExplosion;

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



        switch(bulletType){
            case BulletType.Normal:
                Normal(other.transform.GetComponent<Health>());
                break;
            case BulletType.HE:
                HE(other.transform.GetComponent<Health>());
                break;
            case BulletType.AP:
                AP(other.transform.GetComponent<Health>());
                break;
        }

    }

    void Normal(Health health){
        if(health){ health.TakeDamage(Damage); };
        audioManager.PlayAudio(ImpactSound, this.transform.position);
        Destroy(this.gameObject);
    }

    void HE(Health health){
        if(health){ health.TakeDamage(Damage); };
        Destroy(this.gameObject);
    }

    void AP(Health health){
        if(health){ health.TakeDamage(Damage); };
        audioManager.PlayAudio(ImpactSound, this.transform.position);
        Destroy(this.gameObject);
    }


}
