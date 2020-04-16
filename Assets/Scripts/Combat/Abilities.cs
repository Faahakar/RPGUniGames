using System.Collections;
using System.Collections.Generic;
using RPG.Movement;
using RPG.Attributes;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Combat
{
    public class Abilities : MonoBehaviour
    {
        int attack = 0;
        bool isAttacking;
        bool canChain, isStunned;
        bool isBlocking = false;
        bool inBlock;
        Animator animator;
        private void Awake()
        {
        }
        void Start()
        {
            animator = GetComponent<Animator>();
        }

        void Update()
        {
        }


    }


}
