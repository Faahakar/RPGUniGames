﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Combat;
using RPG.Movement;
using RPG.Resources;
using RPG.Core;
namespace RPG.Control
{
     public class AIController : MonoBehaviour
    {
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float suspicionTime = 5f;
        [SerializeField] float DwellingTime = 3f;
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float waypointTolerance = 1f;
        [Range(0,1)]
        [SerializeField] float patrolSpeedFraction = 0.2f;
       
        Fighter fighter;
        GameObject player;
        Health health;
        Mover mover;
        Vector3 guardPosition;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        float timeSinceLastArrivedWayPoint = Mathf.Infinity;
        int currentPointIndex = 0;
        private void Start()
        {
            health = GetComponent<Health>();
            fighter =  GetComponent<Fighter>();
            mover =  GetComponent<Mover>();
            player = GameObject.FindWithTag("Player");
            guardPosition =  transform.position;
        }
        private void Update()
        {   
            if(health.IsDead()) return;     
            if(InAttackRangeOfPlayer(player) && fighter.CanAttack(player))
            {
                timeSinceLastSawPlayer = 0;
                AttackBehaviour();
            }
            else if(timeSinceLastSawPlayer < suspicionTime)
            {               
                // Suspicion State
                SuspicionBehaviour();
            }
            else
            {
               
               PatrolBehaviour();
            }
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceLastArrivedWayPoint += Time.deltaTime;   
     
        }

        private void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition;
            if(patrolPath !=null)
            { 
                if(AtWayPoint())
                {
                    timeSinceLastArrivedWayPoint = 0;  
                    CycleWayPoint();
                }
                nextPosition = GetCurrentWayPoint(); 
            }            
            if(timeSinceLastArrivedWayPoint > DwellingTime)
            {
                // Dwelling State
              mover.StartMoveAction(nextPosition, patrolSpeedFraction);            
            }
           
        }
        private bool AtWayPoint()
        {           
            float distanceToWayPoint = Vector3.Distance(transform.position,GetCurrentWayPoint());
            return  distanceToWayPoint < waypointTolerance;
        }
        private Vector3 GetCurrentWayPoint()
        { 
           return patrolPath.GetWayPoint(currentPointIndex); 
        }
        private void CycleWayPoint()
        {   
           currentPointIndex = patrolPath.GetNextIndex(currentPointIndex);
        }
        private void AttackBehaviour()
        {
            fighter.Attack(player);
        }

        private void SuspicionBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }
        private bool InAttackRangeOfPlayer(GameObject player)
        {
       
            return Vector3.Distance(transform.position,player.transform.position) < chaseDistance;
        }
        // Called By Unity
        private void OnDrawGizmosSelected() 
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
                

        
    }

}
