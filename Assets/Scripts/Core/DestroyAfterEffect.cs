using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class DestroyAfterEffect : MonoBehaviour
    {
        [SerializeField] GameObject targettoDestroy = null;
        void Update()
        {
            if(!GetComponent<ParticleSystem>().IsAlive())
            {
                if(targettoDestroy !=null)
                {
                    Destroy(targettoDestroy);
                }
                else
                {
                    Destroy(gameObject);
                }
                
            }
        }
    }

}
