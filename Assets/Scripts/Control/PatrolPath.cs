using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RPG.Control
{
    
    public class PatrolPath : MonoBehaviour
    {
        const float waypointGizmoRadius = 0.3f;
         void OnDrawGizmosSelected()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                int j = GetNextIndex(i);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(GetWayPoint(i),waypointGizmoRadius);
                Gizmos.DrawLine(GetWayPoint(i),GetWayPoint(j));
                
            }
        }
        public Vector3 GetWayPoint(int i)
        {
            return transform.GetChild(i).position;
        }
        public int GetNextIndex(int i)
        {
            if(i + 1 == transform.childCount)
            {
                 return 0;
            }
            return i +1;
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

}