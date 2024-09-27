using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magazine : MonoBehaviour
{


    [SerializeField] private int MaxBullets;
    [SerializeField] public int CurrentBullets;
    [SerializeField] public GameObject Bullet;

    // Start is called before the first frame update
    void Awake()
    {
        CurrentBullets = MaxBullets;
    }


}
