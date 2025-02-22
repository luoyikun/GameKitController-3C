﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class armorSurfaceSystem : MonoBehaviour
{
    [Header ("Main Settings")]
    [Space]

    public bool armorActive = true;
    public GameObject armorOwner;

    public bool useMinAngleToReturnProjectileToOwner;
    public float minAngleToReturnProjectileToOwner;

    [Space]
    [Header ("Armor Health Settings")]
    [Space]

    public bool useArmorHealthAmount;
    public float armorHealthAmount;
    public float maxArmorHealthAmount;

    public bool disableArmorOnEmptyHealth;

    [Space]
    [Header ("Break Through Armor Surface Settings")]
    [Space]

    public bool blockProjectilesFromBreakingThroughSurfaceEnabled;

    public int armorSurfacePriorityValue = -1;

    [Space]
    [Header ("Return Projectiles Settings")]
    [Space]

    public bool throwProjectilesToPreviousOwnerEnabled = true;

    public bool useNewDamageLayerOnReturnProjectiles;
    public LayerMask newDamageLayerOnReturnProjectiles;

    [Space]

    public bool setUseCustomIgnoreTagsOnReturnProjectilesState;
    public bool useCustomIgnoreTagsOnReturnProjectilesState;
    public List<string> customTagsToIgnoreOnReturnProjectilesList = new List<string> ();

    [Space]
    [Header ("Debug")]
    [Space]

    public List<projectileSystem> projectilesStored = new List<projectileSystem> ();

    [Space]
    [Header ("Event Settings")]
    [Space]

    public bool returnProjectilesOnContact;

    public UnityEvent eventToReturnProjectilesOnContact;

    [Space]

    public bool useEventOnDeflectProjectilesActivated;

    public UnityEvent eventOnDeflectProjectilesActivated;

    [Space]

    public bool useEventOnArmorHealthEmpty;
    public UnityEvent eventOnArmorHealthEmpty;

    public bool useEventOnArmorHealthDamaged;
    public UnityEvent eventOnArmorHealthDamaged;

    [Space]
    [Header ("Components")]
    [Space]

    public BoxCollider mainCollider;

    Coroutine armorSurfaceStateChangeCoroutine;

    bool changeArmorActiveStateCoroutineActive;


    void Start ()
    {
        if (armorOwner == null) {
            armorOwner = gameObject;
        }
    }

    public void addProjectile (projectileSystem newProjectile)
    {
        if (!projectilesStored.Contains (newProjectile)) {
            projectilesStored.Add (newProjectile);

            if (returnProjectilesOnContact) {
                eventToReturnProjectilesOnContact.Invoke ();
            }
        }
    }

    public void throwProjectilesStored (Vector3 throwDirection)
    {
        bool ignoreProjectileOwner = !throwProjectilesToPreviousOwnerEnabled;

        for (int i = 0; i < projectilesStored.Count; i++) {
            if (projectilesStored [i] != null) {

                projectilesStored [i].returnBullet (throwDirection, armorOwner, ignoreProjectileOwner);
            }
        }

        projectilesStored.Clear ();
    }

    public void throwProjectilesStoredCheckingDirection (Transform playerTransform, Transform mainCameraTransform)
    {
        if (projectilesStored.Count > 0) {
            if (useEventOnDeflectProjectilesActivated) {
                eventOnDeflectProjectilesActivated.Invoke ();
            }

            for (int i = 0; i < projectilesStored.Count; i++) {
                if (projectilesStored [i] != null) {

                    Vector3 projectileDirection = playerTransform.position - projectilesStored [i].transform.position;

                    projectileDirection = projectileDirection / projectileDirection.magnitude;

                    float angleForward = Vector3.SignedAngle (playerTransform.forward, projectileDirection, playerTransform.up);

                    bool returnProjectileToOwner = true;

                    if (useMinAngleToReturnProjectileToOwner) {
                        if (Mathf.Abs (angleForward) > minAngleToReturnProjectileToOwner) {
                            returnProjectileToOwner = false;
                        }
                    }

                    bool ignoreProjectileOwner = false;

                    if (!returnProjectileToOwner) {
                        ignoreProjectileOwner = true;
                    }

                    Debug.DrawRay (projectilesStored [i].transform.position, projectileDirection * 5, Color.red, 5);

                    Vector3 targetPositionToLook = playerTransform.position + playerTransform.up + playerTransform.forward;

                    if (useNewDamageLayerOnReturnProjectiles) {
                        projectilesStored [i].setTargetToDamageLayer (newDamageLayerOnReturnProjectiles);
                    }

                    if (setUseCustomIgnoreTagsOnReturnProjectilesState) {
                        projectilesStored [i].setUseCustomIgnoreTags (useCustomIgnoreTagsOnReturnProjectilesState, customTagsToIgnoreOnReturnProjectilesList);
                    }

                    projectilesStored [i].returnBullet (targetPositionToLook, armorOwner, ignoreProjectileOwner);
                }
            }

            projectilesStored.Clear ();
        }
    }

    public bool thereAreProjectilesStored ()
    {
        return projectilesStored.Count > 0;
    }

    public void destroyProjetilesOnShield ()
    {
        if (projectilesStored.Count < 0) {
            return;
        }

        for (int i = 0; i < projectilesStored.Count; i++) {
            if (projectilesStored [i] != null) {

                projectilesStored [i].destroyProjectile ();
            }
        }

        projectilesStored.Clear ();
    }

    public bool isArmorEnabled ()
    {
        return armorActive;
    }

    public bool isBlockProjectilesFromBreakingThroughSurfaceEnabled ()
    {
        return blockProjectilesFromBreakingThroughSurfaceEnabled;
    }

    public int getArmorSurfacePriorityValue ()
    {
        return armorSurfacePriorityValue;
    }

    public void setNewArmorOwner (GameObject newObject)
    {
        armorOwner = newObject;
    }

    public void setArmorActiveState (bool state)
    {
        armorActive = state;

        if (changeArmorActiveStateCoroutineActive) {
            stopSetArmorSurfaceStateAfterDelayCoroutine ();
        }
    }

    public void setTriggerScaleValues (Vector3 newCenterValues, Vector3 newSizeValues)
    {
        if (mainCollider != null) {
            mainCollider.center = newCenterValues;

            mainCollider.size = newSizeValues;
        }
    }

    public void setEnableArmorSurfaceStateWithDuration (float delayDuration, bool stateBeforeDelay, bool stateAfterDelay)
    {
        stopSetArmorSurfaceStateAfterDelayCoroutine ();

        if (gameObject.activeSelf && gameObject.activeInHierarchy) {
            armorSurfaceStateChangeCoroutine = StartCoroutine (setArmorSurfaceStateAfterDelayCoroutine (delayDuration, stateBeforeDelay, stateAfterDelay));
        }
    }

    public void stopSetArmorSurfaceStateAfterDelayCoroutine ()
    {
        if (armorSurfaceStateChangeCoroutine != null) {
            StopCoroutine (armorSurfaceStateChangeCoroutine);
        }

        changeArmorActiveStateCoroutineActive = false;
    }

    IEnumerator setArmorSurfaceStateAfterDelayCoroutine (float delayDuration, bool stateBeforeDelay, bool stateAfterDelay)
    {
        changeArmorActiveStateCoroutineActive = true;

        armorActive = stateBeforeDelay;

        yield return new WaitForSeconds (delayDuration);

        armorActive = stateAfterDelay;

        changeArmorActiveStateCoroutineActive = false;
    }

    public void setArmorDamage (float damageAmount)
    {
        if (!useArmorHealthAmount) {
            return;
        }

        armorHealthAmount -= damageAmount;

        if (armorHealthAmount <= 0) {
            armorHealthAmount = 0;

            if (disableArmorOnEmptyHealth) {
                armorActive = false;
            }

            if (useEventOnArmorHealthEmpty) {
                eventOnArmorHealthEmpty.Invoke ();
            }
        } else {
            if (useEventOnArmorHealthDamaged) {
                eventOnArmorHealthDamaged.Invoke ();
            }
        }
    }
    public void addArmorHealth (float healthAmount)
    {
        if (!useArmorHealthAmount) {
            return;
        }

        armorHealthAmount += healthAmount;

        armorHealthAmount = Mathf.Clamp (armorHealthAmount, 0, maxArmorHealthAmount);

        if (armorHealthAmount > 0) {
            armorActive = true;
        }
    }

    public bool isUseArmorHealthAmountEnabled ()
    {
        return useArmorHealthAmount;
    }
}