using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    [SerializeField] GameObject targetoDestroy = null;
     public void DestroyTarget()
    {
        Destroy(targetoDestroy);
    }
}
