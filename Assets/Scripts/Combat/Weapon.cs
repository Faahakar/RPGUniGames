using UnityEngine;
using RPG.Resources;
namespace RPG.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/MakeNewWeapon", order = 0)]
    public class Weapon : ScriptableObject 
    {
        [SerializeField] AnimatorOverrideController animatorOverride = null;
        [SerializeField] GameObject equippedPrefab = null;
        [SerializeField] float weaponRange = 2f;
        [SerializeField] float weapondamage = 5f;
        [SerializeField] bool isRightHanded = true;
        [SerializeField] Projectile projectile = null;

        const string weaponName = "Weapon";

        public void Spawn(Transform rightHandTransform,Transform leftHandTransform, Animator animator)
        {
            DestroyOldWeapon(rightHandTransform,leftHandTransform);
            if(equippedPrefab !=null)
            {
                Transform handTransform = GetTransform(rightHandTransform,leftHandTransform);
                GameObject weapon = Instantiate(equippedPrefab, handTransform);
                weapon.name = weaponName;
            }
            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if(animatorOverride != null)
            {
                animator.runtimeAnimatorController = animatorOverride;
            }
            else if(overrideController != null)
            {                   
                animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;           
            }
           
        }
        private void DestroyOldWeapon(Transform rightHandTransform, Transform leftHandTransform)
        {
            Transform oldWeapon = rightHandTransform.Find(weaponName);
            if(oldWeapon == null)
            {
                oldWeapon = leftHandTransform.Find(weaponName);
            }
            if(oldWeapon == null) return;
            oldWeapon.name = "DESTROYING";
            Destroy(oldWeapon.gameObject);

        }
        private Transform GetTransform(Transform rightHandTransform,Transform leftHandTransform)
        {
             Transform handTransform;
                if(isRightHanded) handTransform = rightHandTransform;
                else handTransform = leftHandTransform;
                return handTransform;
        }
        public bool HasProjectile()
        {
            return projectile != null;
        }
        public void LaunchProjectile(Transform rightHandTransform,Transform leftHandTransform, Health target)
        {
             Projectile projectileInstance = Instantiate(projectile,GetTransform(rightHandTransform,leftHandTransform).position,Quaternion.identity);
             projectileInstance.SetTarget(target,weapondamage);
        }
        public float GetWeaponRange()
        {
            return weaponRange;
        }
        public float GetWeaponDamage()
        {
            return weapondamage;
        }
        
    }
}