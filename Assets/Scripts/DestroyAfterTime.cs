using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    // Start is called before the first frame update
    
    
    public float Lifetime = 5;
    float TimeGone;
    
    void Awake()
    {
        TimeGone = 0;
    }

    // Update is called once per frame
    void Update()
    {
        TimeGone += Time.deltaTime;
        if(TimeGone >= Lifetime){
            Destroy(this.gameObject);
        }
    }
}
