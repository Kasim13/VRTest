﻿using UnityEngine;
using VRShooterKit.DamageSystem;
using System.Collections.Generic;
using System.Linq;

namespace VRShooterKit.WeaponSystem
{
    //this script controls the melee weapons like the sword, 
    //in the demo scene and the weapons prefabs, all the weapons can be use as melee weapons to, 
    //so they use this script
    public class VR_MeleeWeapon : MonoBehaviour
    {
        #region INSPECTOR              
       
        [SerializeField] private FastCollisionListener fastCollisionListener = null;
        [SerializeField] private Transform rayStart = null;
        [SerializeField] private Transform rayEnd = null;
        [SerializeField] private float minSpeed = 0.0f;
        [SerializeField] private float dmg = 0.0f;
        [SerializeField] private float hitForce = 0.0f;
        [SerializeField] private float maxHitForce = 800.0f;       
        [SerializeField] private bool canDismember = false;
        #endregion

        #region PRIVATE      
        private VR_Grabbable grabbable = null;       
        private List<Damageable> thisDamageableList = null;    
        private DamageInfo damageInfoCache = new DamageInfo();
        #endregion

        private void Awake()
        {            
            grabbable = GetComponent<VR_Grabbable>();            
            thisDamageableList = transform.GetComponentsInChildren<Damageable>().ToList();           
        }


        private void Update()
        {           
            //check if we are hitting something 
            //we do it in the fixed update because the player can move his hands very quickly
            if ( grabbable.CurrentGrabState == GrabState.Grab && grabbable.GrabController.Velocity.magnitude > minSpeed )
            {
               
                List<Collider> hitColliders = fastCollisionListener.CheckForCollisionsThisFrame();

                for (int n = 0; n < hitColliders.Count; n++)
                {
                    TryDoDamage(hitColliders[n].transform, hitColliders[n].transform.position);
                }

                
            }
                        
        }
        

        protected bool TryDoDamage(Transform target, Vector3 hitPoint)
        {
            Damageable[] damageableArray = target.GetComponents<Damageable>();
            
            if (damageableArray != null && damageableArray.Length > 0)
            {
                for (int n = 0; n < damageableArray.Length; n++)
                {
                    if (damageableArray[n] != null && !thisDamageableList.Contains( damageableArray[n]) )
                    {
                        RaycastHit hitInfo;
                        if (Physics.Linecast(rayStart.position, rayEnd.position, out hitInfo, 1 << target.gameObject.layer))
                        {
                            DamageInfo damageInfo = CreateDamageInfo(hitInfo.point);
                            damageableArray[n].DoDamage(damageInfo);
                        }                        
                    }
                }

                return true;
            }

            return false;
        }

        private DamageInfo CreateDamageInfo(Vector3 hitPoint)
        {
            Vector3 controllerVelocity = grabbable.GrabController.Velocity;

            damageInfoCache.dmg = dmg * controllerVelocity.magnitude;
            damageInfoCache.hitDir = controllerVelocity.normalized;
            damageInfoCache.hitPoint = hitPoint;
            damageInfoCache.hitForce = Mathf.Min( ( controllerVelocity * hitForce ).magnitude, maxHitForce );
            damageInfoCache.sender = grabbable.GrabController != null ? grabbable.GrabController.transform.root.gameObject : null;
            damageInfoCache.canDismember = canDismember;

            return damageInfoCache;
        }
      
    }
    

}
