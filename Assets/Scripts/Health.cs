using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float MaxHealth = 10;
    [SerializeField] public float health;
    
    private void Start() {
        health = MaxHealth;
    }

    public void TakeDamage(float damage){
        health -= damage;

        if(health <= 0){ Die(); }
    }

    public void Die(){
        Destroy(this.gameObject);
    }

}
