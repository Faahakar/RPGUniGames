using UnityEngine;
using UnityEngine.AI;
using RPG.Attributes;
using RPG.Combat;
using GameDevTV.Saving;
using RPG.Core;
using System.Collections;
using System.Collections.Generic;
namespace RPG.Movement
{
    public class Mover : MonoBehaviour,IAction,ISaveable
    {
        [SerializeField] Transform target;
        [SerializeField] float maxSpeed = 6f;
        NavMeshAgent navMeshAgent;
        [SerializeField] float maxNavPathLength = 40f;

        float speed = 0;
        bool canMove = true;
        Vector3 velocity, localVelocity;
        Health health;
        Animator animator;

        private void Awake()
        {

          animator = GetComponent<Animator>();
          health = GetComponent<Health>();
          navMeshAgent = GetComponent<NavMeshAgent>(); 
        }
        private void Start()
        {
        }
        void Update()
        {
            navMeshAgent.enabled = !health.IsDead();
        }
        private void LateUpdate() {
            UpdateAnimator();
            
        }
        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            if(GetComponent<Fighter>() != null)
            GetComponent<Fighter>().Cancel();
            MoveTo(destination, speedFraction);
        }
        public bool CanMoveTo(Vector3 destination)
        {
          NavMeshPath path = new NavMeshPath();

          bool hasPath = NavMesh.CalculatePath(transform.position,destination,NavMesh.AllAreas,path);
          if(!hasPath) return false;
          if (path.status != NavMeshPathStatus.PathComplete) return false;
          if(GetPathLength(path) > maxNavPathLength) return false;
          return true;
        }
        private float GetPathLength(NavMeshPath path)
        {
          float total = 0;
          if(path.corners.Length < 2) return total;
          for(int i = 0;  i <  path.corners.Length - 1 ; i++)
          {
            total += Vector3.Distance(path.corners[i], path.corners[i+1]);          
          }
          return total;
        }

        public void MoveTo(Vector3 destination, float speedFraction)
        {
            navMeshAgent.SetDestination(destination);
            navMeshAgent.speed = maxSpeed*Mathf.Clamp01(speedFraction); 
            navMeshAgent.isStopped = false;

        }

        private void UpdateAnimator()
        {
            if(GetComponent<Fighter>() == null)
            {
                if(GetComponent<CombatTarget>() == null)
                {
                    velocity = navMeshAgent.velocity;
                    speed = transform.InverseTransformDirection(velocity).z;
                    //Update animator with movement values       
                    animator.SetFloat("ForwardSpeed", speed);       
                }

            }

        }
        public void Cancel()
        {           
            navMeshAgent.isStopped = true;    
                      
        }
        public object CaptureState()
        {
            Dictionary<string,object> data = new Dictionary<string,object>();
            data["position"] =  new SerializableVector3(transform.position);
            data["rotation"] = new SerializableVector3(transform.eulerAngles);
            return  data;
        }
        public void RestoreState(object state)
        {
           Dictionary<string,object> data = (Dictionary<string,object>)state;
           navMeshAgent.enabled = false;
           transform.position =  ((SerializableVector3)data["position"]).ToVector();
           transform.eulerAngles = ((SerializableVector3)data["rotation"]).ToVector();
           navMeshAgent.enabled = true;
        }
       

    }

}

