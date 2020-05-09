using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using UnityEngine;

public class TemporaryStat : MonoBehaviour
{
    public void EffectTime (float effectTime)
    {
          
        effectTime -= Time.deltaTime;
        if(effectTime < 0)
        {
            effectTime = 0;
        }

    }
    public void Effect()   
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
