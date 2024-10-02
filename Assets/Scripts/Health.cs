using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField, Min(0f)]   private float MaxHealth = 10;
    [SerializeField] public float health;
    [Space]
    [SerializeField] private bool DieOnZeroHealth = true;
    [SerializeField] private bool PressButton = false;
    
    private void Start() {
        health = MaxHealth;
    }


    // Takes damage, self explanatory
    public void TakeDamage(float damage){
        health -= damage;

        if(PressButton){
            if(health <= 0){ ButtonPress(); }
        }
        if(DieOnZeroHealth){
            if(health <= 0){ Die(); }
        }
    }

    void Die(){
        Destroy(this.gameObject);
    }

    void ButtonPress(){
        this.GetComponent<Button>().onClick.Invoke();
    }



    public void AddHealth(float HealthToAdd){
        health  += HealthToAdd;
        if(health >= MaxHealth){ health = MaxHealth; }
    }


}
