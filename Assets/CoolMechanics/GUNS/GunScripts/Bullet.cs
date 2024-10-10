using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{


    public enum BulletType { Normal, HE, AP, Healing }
    public BulletType bulletType;

    [HideInInspector] public float Damage;

    public GameObject Casing;
  
    public AudioClip ImpactSound;
    public GameObject ImpactExplosion;

    [Header("HE")]
    [SerializeField, Min(0f)] private float DamageRadius = 5;
    [SerializeField] private float ExplosionForce = 700;


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
                HE();
                break;
            case BulletType.AP:
                AP(other.transform.GetComponent<Health>());
                break;
            case BulletType.Healing:
                AP(other.transform.GetComponent<Health>());
                break;
        }

    }


    // Normal bullet
    void Normal(Health health){
        if(health){ health.TakeDamage(Damage); };
        audioManager.PlayAudio(ImpactSound, this.transform.position);
        Destroy(this.gameObject);
    }


    // HE, high explosive, explodes on impact 
    void HE(){
        
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, DamageRadius);
        foreach (Collider collider in colliders)
        {
            if(collider.GetComponent<Health>()){
                collider.GetComponent<Health>().TakeDamage(Damage);
            }
            if(collider.GetComponent<Rigidbody>()){
                collider.GetComponent<Rigidbody>().AddExplosionForce(ExplosionForce, this.transform.position, DamageRadius);
            }
        }
        Destroy(this.gameObject);
    }

    // AP, Armour piercing, pierces armour.
    void AP(Health health){
        if(health){ health.TakeDamage(Damage); };
        audioManager.PlayAudio(ImpactSound, this.transform.position);
        Destroy(this.gameObject);
    }

    void Healing(Health health){
        if(health){ health.AddHealth(Damage); };
        audioManager.PlayAudio(ImpactSound, this.transform.position);
        Destroy(this.gameObject);
    }


}
