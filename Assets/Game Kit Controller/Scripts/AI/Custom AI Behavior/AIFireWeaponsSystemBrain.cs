﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AIFireWeaponsSystemBrain : MonoBehaviour
{
    [Header ("Main Settings")]
    [Space]

    public bool weaponsSystemEnabled = true;

    public bool drawWeaponsWhenResumingAI;

    public bool drawWeaponWhenAttackModeSelected;

    public bool keepWeaponsIfNotTargetsToAttack;

    public bool canDropWeaponExternallyEnabled = true;

    [Space]
    [Header ("Attack Settings")]
    [Space]

    public bool attackEnabled;

    public float fireWeaponAttackRate = 0.17f;

    public bool setWaitTimeOnFireWeaponBlast;
    public Vector2 waitTimeRangeOnFireWeaponBlast;

    public Vector2 fireTimeRangeBeforeWaitForNextWeaponBlast;

    [Space]

    public bool checkToForceAttackIfNotUsedLongTimeAgo;

    public float maxWaitTimeToForceAttackIfNotUsedLongTimeAgo = 10;

    [Space]
    [Header ("Weapons Settings")]
    [Space]

    public bool changeWeaponAfterTimeEnabled;

    public bool changeWeaponRandomly;

    public Vector2 randomChangeWeaponRate;

    [Space]
    [Header ("Weapon List")]
    [Space]

    public int currentWeaponIndex;

    [Space]

    public List<AIWeaponInfo> AIWeaponInfoList = new List<AIWeaponInfo> ();

    [Space]
    [Space]
    [Space]
    [Header ("Roll/Dodge Settings")]
    [Space]

    public bool rollEnabled;

    public Vector2 randomRollWaitTime;

    public float minWaitTimeAfterRollActive = 1.3f;

    public List<Vector2> rollMovementDirectionList = new List<Vector2> ();

    [Space]
    [Header ("Random Walk Settings")]
    [Space]

    public bool randomWalkEnabled;

    public Vector2 randomWalkWaitTime;
    public Vector2 randomWalkDuration;
    public Vector2 randomWalkRadius;

    [Space]

    public bool pauseFireWeaponDuringWalkEnabled = true;

    public bool ignoreRandomWalkInFrontDirectionEnabled = true;

    [Space]
    [Header ("Jump To Target Settings")]
    [Space]

    public bool jumpToTargetOnRangeDistance;
    public Vector2 rangeDistanceToJumpToTarget;
    public bool avoidAttackOnAir = true;
    public float minTimeToJumpAgain = 4;
    public bool useJumpProbability;
    [Range (0, 100)] public float jumpProbability;

    [Space]

    public bool setNewJumpForce;
    public float newJumpForce;
    public bool setNewJumpForceMinDistance;
    public float newJumpForceMinDistance;

    [Space]
    [Header ("Other Settings")]
    [Space]

    public bool changeWaitToActivateAttackEnabled = true;

    public bool activateRandomWalkIfWaitToActivateAttackActive;

    public bool checkIfAttackInProcessBeforeCallingNextAttack = true;

    public bool checkInitialWeaponsOnCharacterSpawnEnabled = true;

    [Space]
    [Header ("Search Weapons On Scene Settings")]
    [Space]

    public bool searchWeaponsPickupsOnLevelIfNoWeaponsAvailable;

    public bool useMaxDistanceToGetPickupWeapon;
    public float maxDistanceToGetPickupWeapon;

    public bool ignoreCheckIfUsableWeaponsPrefabListActive;

    [Space]

    public bool useEventOnNoWeaponToPickFromScene;
    public UnityEvent eventOnNoWeaponToPickFromScene;

    [Space]

    public bool useEventsOnNoWeaponsAvailable;
    public bool useEventsOnNoWeaponsAvailableAfterCheckWeaponsOnScene;
    public UnityEvent eventOnNoWeaponsAvailable;

    [Space]
    [Header ("Steal Weapons From Targets Settings")]
    [Space]

    public bool stealWeaponFromCurrentTarget;

    [Range (0, 100)] public float probabilityToStealWeapon = 0;

    public bool stealWeaponOnlyIfNotPickupsLocated;

    public bool useMaxDistanceToStealWeapon;
    public float maxDistanceToGetStealWeapon;

    [Space]

    public bool useEventOnSteal;
    public UnityEvent eventOnStealWeapon;

    [Space]
    [Header ("Debug")]
    [Space]

    public bool showDebugPrint;

    public bool weaponsSystemActive;

    public bool weaponEquiped;

    public bool aimingWeapon;

    public bool waitingForAttackActive;
    float currentRandomTimeToAttack;

    public bool shootingWeapon;

    public bool canUseAttackActive;

    [Space]

    public bool walkActive;

    public bool waitingWalkActive;

    public bool waitingRollActive;

    [Space]

    public bool attackStatePaused;

    public bool insideMinDistanceToAttack;

    public bool searchingWeapon;

    public bool characterHasWeapons;

    public bool behaviorStatesPaused;

    public bool checkingToChangeWeaponActive;

    public bool onSpotted;

    public bool changeWeaponCheckActive;

    public bool waitToActivateAttackActive;

    public GameObject currentWeaponToGet;

    public bool turnBasedCombatActionActive;

    public bool ignoreOtherActionsToForceAttackActive;

    [Space]
    [Header ("Events Settings")]
    [Space]

    public bool useEventsOnCombatActive;
    public UnityEvent eventOnCombatActive;
    public UnityEvent eventOnCombatDeactivate;

    [Space]

    public eventParameters.eventToCallWithString eventToChangeAttackMode;

    [Space]
    [Header ("Components")]
    [Space]

    public playerWeaponsManager mainPlayerWeaponsManager;
    public dashSystem mainDashSystem;
    public findObjectivesSystem mainFindObjectivesSystem;
    public AINavMesh mainAINavmeshManager;


    float lastTimeAttack;

    int currentAttackTypeIndex;

    bool weaponInfoStored;

    int currentAttackIndex;

    int currentAttackTypeToAlternateIndex;


    float lastTimeRollActive;

    float lastTimeWaitRollActive;

    float currentRollWaitTime;


    float lastTimeWaitWalkActive;
    float currentWalkTime;
    float lastTimeWalkActive;

    float currentWalkDuration;
    float currentWalkRadius;

    bool rollCoolDownActive;

    float currentPauseAttackStateDuration;
    float lastTimeAttackPauseWithDuration;

    bool checkIfDrawWeaponActive;

    float randomWaitTime;

    float lastTimeFireWeaponAttackAtDistance;

    bool checkIfAICarryingWeapon;

    float currentPathDistanceToTarget;
    float minDistanceToAim;
    float minDistanceToDraw;
    float minDistanceToShoot;

    bool checkIfSearchWeapon;

    bool AIPaused;

    bool cancelCheckAttackState;

    float lastTimeChangeWeapon;

    float timeToChangeWeapon;

    AIWeaponInfo currentAIWeaponInfo;

    bool originalRandomWalkEnabled;

    float lastTimeWeaponPicked = 0;

    string currentWeaponNameToPick;

    float currentMinTimeToUseNewWeaponAfterChange;

    float lastTimeWeaponEquippedChecked;

    float lastTimeJump;


    bool waitingForNextFireWeaponBlastActive;
    float lastTimeWaitingForNextFireWeaponBlastActive;

    float currentWaitTimeRangeOnFireWeaponBlast;

    float currentFireTimeRangeBeforeWaitForNextWeaponBlast;

    float lastTimeSettingFireWeaponBlast;


    void Start ()
    {
        originalRandomWalkEnabled = randomWalkEnabled;

        lastTimeWeaponEquippedChecked = Time.time;
    }

    public void updateAI ()
    {
        if (weaponsSystemActive) {
            AIPaused = mainFindObjectivesSystem.isAIPaused ();

            if (!AIPaused) {
                if (!checkIfAICarryingWeapon) {
                    if (Time.time > lastTimeWeaponEquippedChecked + 0.4f) {
                        if (mainPlayerWeaponsManager.isPlayerCarringWeapon ()) {
                            weaponEquiped = true;
                        }

                        checkIfAICarryingWeapon = true;
                    }
                }

                if (walkActive) {
                    if (Time.time > lastTimeWalkActive + 1) {
                        if (Time.time > lastTimeWalkActive + currentWalkDuration ||
                           mainFindObjectivesSystem.getRemainingPathDistanceToTarget () < 0.5f ||
                           mainFindObjectivesSystem.getCurrentDistanceToTarget () < 0.5f) {
                            resetRandomWalkState ();
                        }
                    }
                }

                if (searchingWeapon) {
                    bool weaponReached = false;

                    if (currentWeaponToGet != null) {
                        if (GKC_Utils.distance (mainAINavmeshManager.transform.position, currentWeaponToGet.transform.position) < 1) {
                            weaponPickup currentWeaponPickup = currentWeaponToGet.GetComponent<weaponPickup> ();

                            if (currentWeaponPickup != null) {
                                if (showDebugPrint) {
                                    print ("picking weapon " + currentWeaponToGet.name);
                                }

                                simpleActionButton currentSimpleActionButton = currentWeaponToGet.GetComponentInChildren<simpleActionButton> ();

                                if (currentSimpleActionButton != null) {
                                    mainFindObjectivesSystem.takeObject (currentSimpleActionButton.gameObject);
                                }

                                weaponReached = true;
                            }
                        }
                    } else {
                        weaponReached = true;
                    }

                    if (weaponReached) {
                        characterHasWeapons = mainPlayerWeaponsManager.checkIfWeaponsAvailable () ||
                        (!ignoreCheckIfUsableWeaponsPrefabListActive &&
                        mainPlayerWeaponsManager.checkIfUsableWeaponsPrefabListActive ());

                        mainFindObjectivesSystem.setSearchigWeaponState (false);

                        searchingWeapon = false;

                        checkIfSearchWeapon = false;

                        currentWeaponToGet = null;

                        mainFindObjectivesSystem.setIgnoreVisionRangeActiveState (true);

                        mainFindObjectivesSystem.resetAITargets ();

                        mainFindObjectivesSystem.setIgnoreVisionRangeActiveState (false);

                        mainPlayerWeaponsManager.repairObjectFully (currentWeaponNameToPick);

                        lastTimeWeaponPicked = Time.time;

                        if (showDebugPrint) {
                            print ("Weapon picked, resuming state on AI");
                        }
                    }
                }

                if (jumpToTargetOnRangeDistance) {
                    checkJumpState ();
                }
            }
        }
    }

    void checkJumpState ()
    {
        if (mainFindObjectivesSystem.isActionActive ()) {
            return;
        }

        if (!insideMinDistanceToAttack &&
            !searchingWeapon &&
            mainFindObjectivesSystem.isAIOnGround () &&
            mainFindObjectivesSystem.isOnSpotted () &&
            (lastTimeJump == 0 || Time.time > minTimeToJumpAgain + lastTimeJump)) {

            float distanceToTarget = mainFindObjectivesSystem.getDistanceToTarget ();

            if (distanceToTarget > rangeDistanceToJumpToTarget.x && distanceToTarget < rangeDistanceToJumpToTarget.y) {
                bool activateJumpResult = true;

                if (useJumpProbability) {
                    float currentProbability = Random.Range (0, 100);

                    if (currentProbability > jumpProbability) {
                        useJumpProbability = false;
                    }
                }

                if (activateJumpResult) {
                    bool setNewJumpForceResult = setNewJumpForce;

                    if (setNewJumpForceMinDistance) {
                        if (distanceToTarget < newJumpForceMinDistance) {
                            setNewJumpForceResult = false;
                        }
                    }

                    if (setNewJumpForceResult) {
                        mainFindObjectivesSystem.setJumpPower (newJumpForce);

                        mainFindObjectivesSystem.activateJumpExternally (0);
                    } else {
                        mainFindObjectivesSystem.setOriginalJumpPower ();

                        mainFindObjectivesSystem.activateJumpExternally (distanceToTarget);
                    }
                }

                lastTimeJump = Time.time;
            }
        }
    }

    public void resetRandomWalkState ()
    {
        mainFindObjectivesSystem.setRandomWalkPositionState (0);

        waitingWalkActive = false;

        walkActive = false;

        lastTimeWalkActive = Time.time;
    }

    public void resetRollState ()
    {
        waitingRollActive = false;

        lastTimeRollActive = Time.time;
    }

    public void resetStates ()
    {
        resetRandomWalkState ();

        resetRollState ();
    }

    public void checkIfResetStatsOnRandomWalk ()
    {
        if (walkActive) {
            resetStates ();
        }
    }

    public void checkRollState ()
    {
        if (ignoreOtherActionsToForceAttackActive) {
            return;
        }

        if (rollEnabled) {

            if (walkActive) {
                return;
            }

            if (!insideMinDistanceToAttack) {
                resetRollState ();

                lastTimeRollActive = 0;

                return;
            }

            if (waitingRollActive) {
                if (Time.time > lastTimeWaitRollActive + currentRollWaitTime) {

                    int randomRollMovementDirection = Random.Range (0, rollMovementDirectionList.Count);

                    Vector3 dashDirection = rollMovementDirectionList [randomRollMovementDirection];

                    if (mainFindObjectivesSystem.isLocked2_5dModeActive ()) {
                        randomRollMovementDirection = Random.Range (0, 2);

                        if (randomRollMovementDirection == 0) {
                            dashDirection = Vector3.forward;
                        } else {
                            dashDirection = -Vector3.forward;
                        }
                    }

                    mainDashSystem.activateDashStateWithCustomDirection (dashDirection);

                    resetRollState ();
                }
            } else {
                if (Time.time > lastTimeRollActive + randomWaitTime) {
                    currentRollWaitTime = Random.Range (randomRollWaitTime.x, randomRollWaitTime.y);

                    lastTimeWaitRollActive = Time.time;

                    waitingRollActive = true;

                    randomWaitTime = Random.Range (0.1f, 0.5f);
                }
            }
        }
    }

    public void checkWalkState ()
    {
        if (ignoreOtherActionsToForceAttackActive) {
            return;
        }

        if (randomWalkEnabled) {

            if (rollEnabled) {
                if (lastTimeRollActive != 0) {
                    rollCoolDownActive = Time.time < lastTimeRollActive + 0.7f;

                    if (rollCoolDownActive) {
                        return;
                    }
                }
            }

            if (waitingWalkActive) {
                if (!walkActive) {

                    if (Time.time > lastTimeWaitWalkActive + currentWalkTime) {
                        mainFindObjectivesSystem.setignoreRandomWalkInFrontDirectionEnabledState (ignoreRandomWalkInFrontDirectionEnabled);

                        mainFindObjectivesSystem.setRandomWalkPositionState (currentWalkRadius);

                        lastTimeWalkActive = Time.time;

                        walkActive = true;
                    }
                }
            } else {
                currentWalkTime = Random.Range (randomWalkWaitTime.x, randomWalkWaitTime.y);

                lastTimeWaitWalkActive = Time.time;

                waitingWalkActive = true;

                currentWalkDuration = Random.Range (randomWalkDuration.x, randomWalkDuration.y);

                currentWalkRadius = Random.Range (randomWalkRadius.x, randomWalkRadius.y);

                walkActive = false;
            }
        }
    }

    public void updateInsideMinDistance (bool newInsideMinDistanceToAttack)
    {
        insideMinDistanceToAttack = newInsideMinDistanceToAttack;

        if (mainFindObjectivesSystem.isAttackAlwaysOnPlace ()) {
            insideMinDistanceToAttack = true;
        }

        if (insideMinDistanceToAttack) {
            if (checkIfDrawWeaponActive) {
                if (!mainPlayerWeaponsManager.isPlayerCarringWeapon ()) {
                    setDrawOrHolsterWeaponState (true);
                }

                checkIfDrawWeaponActive = false;
            }
        } else {
            if (aimingWeapon && !mainFindObjectivesSystem.isAttackTargetDirectlyActive ()) {
                setAimWeaponState (false);
            }
        }
    }

    void updateLookAtTargetIfBehaviorPaused ()
    {
        cancelCheckAttackState = false;

        if (mainFindObjectivesSystem.isAttackTargetDirectlyActive ()) {
            mainFindObjectivesSystem.setLookingAtTargetPositionState (true);

            mainFindObjectivesSystem.lookAtCurrentPlaceToShoot ();
        } else {
            currentPathDistanceToTarget = mainFindObjectivesSystem.currentPathDistanceToTarget;
            minDistanceToAim = mainFindObjectivesSystem.minDistanceToAim;
            minDistanceToDraw = mainFindObjectivesSystem.minDistanceToDraw;
            minDistanceToShoot = mainFindObjectivesSystem.minDistanceToShoot;

            bool useHalfMinDistance = mainAINavmeshManager.useHalfMinDistance;

            if (useHalfMinDistance) {

                mainFindObjectivesSystem.setLookingAtTargetPositionState (false);

                cancelCheckAttackState = true;
            } else {

                if (currentPathDistanceToTarget <= minDistanceToAim) {
                    mainFindObjectivesSystem.setLookingAtTargetPositionState (true);

                    mainFindObjectivesSystem.lookAtCurrentPlaceToShoot ();
                } else {
                    if (currentPathDistanceToTarget >= minDistanceToAim + 1.5f) {
                        mainFindObjectivesSystem.setLookingAtTargetPositionState (false);

                        cancelCheckAttackState = true;
                    } else {
                        if (mainFindObjectivesSystem.isLookingAtTargetPosition ()) {
                            mainFindObjectivesSystem.lookAtCurrentPlaceToShoot ();
                        }
                    }
                }
            }
        }
    }

    public void updateMainFireWeaponsBehavior ()
    {
        if (!weaponsSystemActive) {
            return;
        }

        if (AIPaused) {
            return;
        }

        if (behaviorStatesPaused) {
            updateLookAtTargetIfBehaviorPaused ();

            return;
        }

        if (checkToForceAttackIfNotUsedLongTimeAgo) {
            if (lastTimeAttack > 0 && !ignoreOtherActionsToForceAttackActive) {
                if (Time.time > maxWaitTimeToForceAttackIfNotUsedLongTimeAgo + lastTimeAttack) {
                    ignoreOtherActionsToForceAttackActive = true;

                    resetRandomWalkState ();

                    resetRollState ();

                    if (showDebugPrint) {
                        print ("too much time since last attack active, reseting other states" +
                        " to force an attack");
                    }
                }
            }
        }

        checkWalkState ();

        bool ignoreRollStateResult = false;

        if (walkActive) {
            if (pauseFireWeaponDuringWalkEnabled) {
                return;
            } else {
                ignoreRollStateResult = true;
            }
        }

        if (!ignoreRollStateResult) {
            checkRollState ();

            if (rollEnabled) {
                if (Time.time < lastTimeRollActive + minWaitTimeAfterRollActive) {
                    return;
                }
            }
        }

        if (characterHasWeapons) {
            cancelCheckAttackState = false;

            if (mainFindObjectivesSystem.isAttackTargetDirectlyActive ()) {
                mainFindObjectivesSystem.setLookingAtTargetPositionState (true);

                mainFindObjectivesSystem.lookAtCurrentPlaceToShoot ();

                if (changeWeaponCheckActive) {
                    if (Time.time < lastTimeChangeWeapon + currentMinTimeToUseNewWeaponAfterChange) {
                        return;
                    } else {
                        changeWeaponCheckActive = false;
                    }
                }

                if (!weaponEquiped) {
                    setDrawOrHolsterWeaponState (true);
                } else {
                    if (!aimingWeapon) {
                        if (mainPlayerWeaponsManager.currentWeaponWithHandsInPosition () && mainPlayerWeaponsManager.isPlayerCarringWeapon () && !mainPlayerWeaponsManager.currentWeaponIsMoving ()) {
                            setAimWeaponState (true);
                        }
                    }

                    if (aimingWeapon) {
                        if (!mainPlayerWeaponsManager.currentWeaponIsMoving () &&
                            mainPlayerWeaponsManager.reloadingActionNotActive () &&
                            mainPlayerWeaponsManager.isPlayerCarringWeapon () &&
                            mainFindObjectivesSystem.checkIfMinimumAngleToAttack () &&
                            !mainPlayerWeaponsManager.isActionActiveInPlayer () &&
                            mainPlayerWeaponsManager.canPlayerMove ()) {

                            bool canShootResult = getCanShootResult ();

                            if (canShootResult) {
                                shootTarget ();
                            } else {
                                if (shootingWeapon) {
                                    setShootWeaponState (false);
                                }
                            }
                        }
                    }
                }
            } else {
                currentPathDistanceToTarget = mainFindObjectivesSystem.currentPathDistanceToTarget;
                minDistanceToAim = mainFindObjectivesSystem.minDistanceToAim;
                minDistanceToDraw = mainFindObjectivesSystem.minDistanceToDraw;
                minDistanceToShoot = mainFindObjectivesSystem.minDistanceToShoot;

                bool useHalfMinDistance = mainAINavmeshManager.useHalfMinDistance;

                if (useHalfMinDistance) {
                    if (!changeWeaponCheckActive) {
                        if (aimingWeapon) {
                            setAimWeaponState (false);
                        }
                    }

                    mainFindObjectivesSystem.setLookingAtTargetPositionState (false);

                    cancelCheckAttackState = true;
                } else {

                    if (currentPathDistanceToTarget <= minDistanceToAim) {
                        if (!changeWeaponCheckActive) {
                            if (!aimingWeapon) {
                                if (mainPlayerWeaponsManager.currentWeaponWithHandsInPosition () && mainPlayerWeaponsManager.isPlayerCarringWeapon () && !mainPlayerWeaponsManager.currentWeaponIsMoving ()) {
                                    setAimWeaponState (true);
                                }
                            }
                        }

                        mainFindObjectivesSystem.setLookingAtTargetPositionState (true);

                        mainFindObjectivesSystem.lookAtCurrentPlaceToShoot ();
                    } else {
                        if (currentPathDistanceToTarget >= minDistanceToAim + 1.5f) {
                            if (!changeWeaponCheckActive) {
                                if (aimingWeapon) {
                                    setAimWeaponState (false);
                                }
                            }

                            mainFindObjectivesSystem.setLookingAtTargetPositionState (false);

                            cancelCheckAttackState = true;
                        } else {
                            if (mainFindObjectivesSystem.isLookingAtTargetPosition ()) {
                                mainFindObjectivesSystem.lookAtCurrentPlaceToShoot ();
                            }
                        }
                    }

                    if (!changeWeaponCheckActive) {
                        if (!weaponEquiped && currentPathDistanceToTarget <= minDistanceToDraw) {
                            setDrawOrHolsterWeaponState (true);
                        }
                    }
                }

                if (changeWeaponCheckActive) {
                    if (Time.time < lastTimeChangeWeapon + currentMinTimeToUseNewWeaponAfterChange) {
                        return;
                    } else {
                        changeWeaponCheckActive = false;
                    }
                }
            }

            checkAttackState ();

            if (waitToActivateAttackActive) {
                if (activateRandomWalkIfWaitToActivateAttackActive) {
                    if (!walkActive) {
                        currentWalkTime = Random.Range (randomWalkWaitTime.x, randomWalkWaitTime.y);

                        currentWalkDuration = Random.Range (randomWalkDuration.x, randomWalkDuration.y);

                        currentWalkRadius = Random.Range (randomWalkRadius.x, randomWalkRadius.y);

                        mainFindObjectivesSystem.setignoreRandomWalkInFrontDirectionEnabledState (ignoreRandomWalkInFrontDirectionEnabled);

                        mainFindObjectivesSystem.setRandomWalkPositionState (currentWalkRadius);

                        lastTimeWalkActive = Time.time;

                        walkActive = true;
                    }
                }

                return;
            }

        } else {
            checkNoFireWeaponsAvailableState ();
        }
    }

    public void checkNoFireWeaponsAvailableState ()
    {
        if (!weaponsSystemActive) {
            return;
        }

        if (!searchingWeapon && !checkIfSearchWeapon) {
            if (lastTimeWeaponPicked > 0) {
                if (Time.time < lastTimeWeaponPicked + 0.75f) {
                    if (showDebugPrint) {
                        print ("NO; WAITING TO WEAPON PICKED\n\n");
                    }

                    return;
                } else {
                    lastTimeWeaponPicked = 0;
                }
            }

            characterHasWeapons = mainPlayerWeaponsManager.checkIfWeaponsAvailable () ||
            (!ignoreCheckIfUsableWeaponsPrefabListActive && mainPlayerWeaponsManager.checkIfUsableWeaponsPrefabListActive ());

            //seach for the closest weapon
            if (!characterHasWeapons) {
                if (!stealWeaponOnlyIfNotPickupsLocated) {
                    if (checkIfStealWeaponFromCurrentTarget ()) {
                        return;
                    }
                }

                if (!useEventsOnNoWeaponsAvailableAfterCheckWeaponsOnScene) {
                    checkEventOnNoWeaponsAvailable ();
                }

                bool weaponFound = false;

                if (searchWeaponsPickupsOnLevelIfNoWeaponsAvailable) {
                    checkIfSearchWeapon = true;

                    if (showDebugPrint) {
                        print ("checking if weapons found on scene");
                    }

                    weaponPickup [] weaponPickupList = FindObjectsOfType (typeof (weaponPickup)) as weaponPickup [];

                    List<GameObject> weaponObjectsDetected = new List<GameObject> ();

                    if (weaponPickupList.Length > 0) {
                        for (int i = 0; i < weaponPickupList.Length; i++) {
                            bool checkObjectResult = true;

                            if (useMaxDistanceToGetPickupWeapon) {
                                float distance = GKC_Utils.distance (weaponPickupList [i].transform.position, mainAINavmeshManager.transform.position);

                                if (distance > maxDistanceToGetPickupWeapon) {
                                    checkObjectResult = false;
                                }
                            }

                            if (checkObjectResult) {
                                weaponObjectsDetected.Add (weaponPickupList [i].gameObject);
                            }
                        }
                    }

                    if (weaponObjectsDetected.Count == 0) {
                        if (showDebugPrint) {
                            print ("no pickups detected, searching for physical objects");
                        }
                    }


                    if (weaponObjectsDetected.Count > 0) {
                        if (showDebugPrint) {
                            print ("weapons found on scene");
                        }

                        string currentWeaponName = "";

                        for (int i = 0; i < weaponObjectsDetected.Count; i++) {
                            if (!weaponFound) {
                                currentWeaponName = "";

                                weaponPickup currentWeaponPickup = weaponObjectsDetected [i].GetComponent<weaponPickup> ();

                                if (currentWeaponPickup != null) {
                                    currentWeaponName = currentWeaponPickup.weaponName;
                                }

                                currentWeaponNameToPick = currentWeaponName;

                                if (showDebugPrint) {
                                    print ("checking if weapon with name " + currentWeaponName + " can be used");
                                }

                                if (mainPlayerWeaponsManager.checkIfWeaponCanBePicked (currentWeaponPickup.weaponName)) {
                                    if (mainAINavmeshManager.updateCurrentNavMeshPath (weaponObjectsDetected [i].transform.position)) {
                                        mainFindObjectivesSystem.setSearchigWeaponState (true);

                                        currentWeaponToGet = weaponObjectsDetected [i];

                                        mainAINavmeshManager.setTarget (weaponObjectsDetected [i].transform);

                                        mainAINavmeshManager.setTargetType (false, true);

                                        weaponFound = true;

                                        mainAINavmeshManager.lookAtTaget (false);

                                        if (showDebugPrint) {
                                            print ("weapon to use located, setting searching weapon state on AI");
                                        }

                                        searchingWeapon = true;
                                    }
                                } else {
                                    if (showDebugPrint) {
                                        print ("weapon " + currentWeaponName + " can't be used");
                                    }
                                }
                            }
                        }
                    } else {
                        if (showDebugPrint) {
                            print ("no pickups or physical objects found");
                        }
                    }

                    if (!weaponFound) {
                        if (stealWeaponOnlyIfNotPickupsLocated) {
                            if (checkIfStealWeaponFromCurrentTarget ()) {
                                return;
                            }
                        }

                        if (useEventOnNoWeaponToPickFromScene) {
                            eventOnNoWeaponToPickFromScene.Invoke ();
                        }
                    }
                }

                if (!weaponFound) {
                    if (stealWeaponOnlyIfNotPickupsLocated) {
                        if (checkIfStealWeaponFromCurrentTarget ()) {
                            return;
                        }
                    }

                    if (showDebugPrint) {
                        print ("no weapons located or out of range, activating no weapons detected mode");
                    }

                    if (useEventsOnNoWeaponsAvailableAfterCheckWeaponsOnScene) {
                        checkEventOnNoWeaponsAvailable ();
                    }
                }

                //it will need to check if the weapon can be seen by the character and if it is can be reached by the navmesh
            }

            mainFindObjectivesSystem.setLookingAtTargetPositionState (false);
        }
    }

    void checkEventOnNoWeaponsAvailable ()
    {
        if (useEventsOnNoWeaponsAvailable) {
            eventOnNoWeaponsAvailable.Invoke ();
        }
    }

    bool checkIfStealWeaponFromCurrentTarget ()
    {
        if (stealWeaponFromCurrentTarget) {
            if (showDebugPrint) {
                print ("Checking probability to steal weapon");
            }

            float currentProbability = Random.Range (0, 100);

            if (currentProbability < probabilityToStealWeapon) {
                if (showDebugPrint) {
                    print ("AI can check if steal object, checking target state");
                }

                GameObject currentTarget = mainFindObjectivesSystem.getCurrentTargetToAttack ();

                if (currentTarget != null) {
                    if (useMaxDistanceToStealWeapon) {
                        float distance = GKC_Utils.distance (currentTarget.transform.position, mainAINavmeshManager.transform.position);

                        if (distance > maxDistanceToGetStealWeapon) {
                            return false;
                        }
                    }

                    playerComponentsManager currentPlayerComponentsManager = currentTarget.GetComponent<playerComponentsManager> ();

                    if (currentPlayerComponentsManager != null) {
                        playerController currentPlayerController = currentPlayerComponentsManager.getPlayerController ();

                        if (currentPlayerController.isPlayerUsingWeapons ()) {
                            if (showDebugPrint) {
                                print ("target is using fire weapons, checking weapon it self");
                            }

                            playerWeaponsManager currentPlayerWeaponsManager = currentPlayerComponentsManager.getPlayerWeaponsManager ();

                            if (currentPlayerWeaponsManager.canWeaponsBeStolenFromCharacter ()) {
                                string currentWeaponName = currentPlayerWeaponsManager.getCurrentWeaponName ();

                                if (showDebugPrint) {
                                    print (currentWeaponName + " is the weapon detected, checking if can be picked");
                                }

                                if (mainPlayerWeaponsManager.checkIfWeaponCanBePicked (currentWeaponName)) {
                                    if (currentPlayerController.isCharacterUsedByAI ()) {
                                        currentPlayerWeaponsManager.dropCurrentWeaponExternallyWithoutResultAndDestroyIt ();
                                    } else {
                                        inventoryManager currentInventoryManager = currentPlayerComponentsManager.getInventoryManager ();

                                        if (currentInventoryManager != null) {
                                            currentInventoryManager.removeObjectAmountFromInventoryByName (currentWeaponName, 1);
                                        }
                                    }

                                    mainPlayerWeaponsManager.equipWeapon (currentWeaponName, false, false, "", "");

                                    mainPlayerWeaponsManager.repairObjectFully (currentWeaponName);

                                    currentPlayerWeaponsManager.checkEventOnWeaponStolen ();

                                    if (showDebugPrint) {
                                        print ("weapon stolen from target");
                                    }

                                    if (useEventOnSteal) {
                                        eventOnStealWeapon.Invoke ();
                                    }

                                    return true;
                                } else {
                                    if (showDebugPrint) {
                                        print ("AI can't use weapon from target");
                                    }
                                }
                            }
                        } else {
                            if (showDebugPrint) {
                                print ("target is not using fire weapons");
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    public void checkAttackState ()
    {
        if (!attackEnabled) {
            return;
        }

        if (!insideMinDistanceToAttack) {
            return;
        }

        if (currentPauseAttackStateDuration > 0) {
            if (Time.time > currentPauseAttackStateDuration + lastTimeAttackPauseWithDuration) {

                attackStatePaused = false;

                currentPauseAttackStateDuration = 0;
            } else {
                return;
            }
        }


        if (!canUseAttackActive) {
            return;
        }

        if (!aimingWeapon && !cancelCheckAttackState) {
            setAimWeaponState (true);
        }

        //		if (showDebugPrint) {
        //			print ("check to fire");
        //		}

        if (mainFindObjectivesSystem.isOnSpotted ()) {
            if (!onSpotted) {
                onSpotted = true;
            }
        } else {
            if (onSpotted) {

                onSpotted = false;

                checkingToChangeWeaponActive = false;
            }
        }

        if (jumpToTargetOnRangeDistance && avoidAttackOnAir) {
            if (!mainFindObjectivesSystem.isAIOnGround ()) {
                return;
            }
        }

        if (onSpotted) {
            if (changeWeaponAfterTimeEnabled) {
                if (!checkingToChangeWeaponActive) {
                    if (!changeWeaponCheckActive) {
                        lastTimeChangeWeapon = Time.time;

                        checkingToChangeWeaponActive = true;

                        timeToChangeWeapon = Random.Range (randomChangeWeaponRate.x, randomChangeWeaponRate.y);
                    }
                } else {
                    if (Time.time > lastTimeChangeWeapon + timeToChangeWeapon) {
                        if (!mainPlayerWeaponsManager.currentWeaponIsMoving () &&
                            mainPlayerWeaponsManager.reloadingActionNotActive () &&
                            mainPlayerWeaponsManager.isPlayerCarringWeapon () &&
                            mainFindObjectivesSystem.checkIfMinimumAngleToAttack () &&
                            !mainPlayerWeaponsManager.isActionActiveInPlayer () &&
                            mainPlayerWeaponsManager.canPlayerMove ()) {

                            string previousWeaponName = "";

                            if (currentAIWeaponInfo != null) {
                                previousWeaponName = currentAIWeaponInfo.Name;
                            }

                            setNextWeapon ();

                            if (previousWeaponName != currentAIWeaponInfo.Name) {
                                setShootWeaponState (false);

                                aimingWeapon = false;

                                currentMinTimeToUseNewWeaponAfterChange = currentAIWeaponInfo.minTimeToUseNewWeaponAfterChange;

                                mainPlayerWeaponsManager.changeCurrentWeaponByName (currentAIWeaponInfo.Name);

                                changeWeaponCheckActive = true;

                                lastTimeChangeWeapon = Time.time;

                                resetSetWaitTimeOnFireWeaponBlastValues ();
                            }
                        }
                    }
                }
            }
        }

        if (changeWeaponCheckActive) {
            if (Time.time < lastTimeChangeWeapon + currentMinTimeToUseNewWeaponAfterChange) {
                return;
            } else {
                changeWeaponCheckActive = false;
            }
        }

        if (Time.time > fireWeaponAttackRate + lastTimeFireWeaponAttackAtDistance) {
            if (weaponEquiped &&
                aimingWeapon &&
                currentPathDistanceToTarget <= minDistanceToShoot &&
                !mainPlayerWeaponsManager.currentWeaponIsMoving () &&
                mainPlayerWeaponsManager.reloadingActionNotActive () &&
                mainPlayerWeaponsManager.isPlayerCarringWeapon () &&
                mainFindObjectivesSystem.checkIfMinimumAngleToAttack () &&
                !mainPlayerWeaponsManager.isActionActiveInPlayer () &&
                mainPlayerWeaponsManager.canPlayerMove ()) {

                bool canShootResult = getCanShootResult ();

                if (canShootResult) {
                    shootTarget ();
                } else {
                    if (shootingWeapon) {
                        setShootWeaponState (false);
                    }
                }
            }

            lastTimeFireWeaponAttackAtDistance = Time.time;
        }
    }

    bool getCanShootResult ()
    {
        bool canShootResult = true;

        if (setWaitTimeOnFireWeaponBlast) {
            if (waitingForNextFireWeaponBlastActive) {

                if (Time.time > lastTimeWaitingForNextFireWeaponBlastActive + currentWaitTimeRangeOnFireWeaponBlast) {
                    waitingForNextFireWeaponBlastActive = false;
                } else {
                    canShootResult = false;
                }
            } else {
                if (currentFireTimeRangeBeforeWaitForNextWeaponBlast == 0) {
                    currentFireTimeRangeBeforeWaitForNextWeaponBlast = Random.Range (fireTimeRangeBeforeWaitForNextWeaponBlast.x,
                        fireTimeRangeBeforeWaitForNextWeaponBlast.y);

                    lastTimeSettingFireWeaponBlast = Time.time;
                } else {
                    if (Time.time > currentFireTimeRangeBeforeWaitForNextWeaponBlast + lastTimeSettingFireWeaponBlast) {
                        lastTimeWaitingForNextFireWeaponBlastActive = Time.time;

                        waitingForNextFireWeaponBlastActive = true;

                        currentFireTimeRangeBeforeWaitForNextWeaponBlast = 0;

                        currentWaitTimeRangeOnFireWeaponBlast = Random.Range (waitTimeRangeOnFireWeaponBlast.x,
                            waitTimeRangeOnFireWeaponBlast.y);
                    }
                }
            }
        }

        //		print (canShootResult);

        return canShootResult;
    }

    void resetSetWaitTimeOnFireWeaponBlastValues ()
    {
        currentFireTimeRangeBeforeWaitForNextWeaponBlast = 0;
        waitingForNextFireWeaponBlastActive = false;
        lastTimeSettingFireWeaponBlast = 0;
    }

    public void updateMainFireWeaponsAttack (bool newCanUseAttackActiveState)
    {
        canUseAttackActive = newCanUseAttackActiveState;
    }

    public void setNextWeapon ()
    {
        bool newWeaponIndexFound = false;

        int newWeaponIndex = -1;

        if (changeWeaponRandomly) {
            int counter = 0;

            while (!newWeaponIndexFound) {

                newWeaponIndex = Random.Range (0, AIWeaponInfoList.Count);

                if (newWeaponIndex != currentWeaponIndex) {
                    if (AIWeaponInfoList [newWeaponIndex].weaponEnabled) {
                        newWeaponIndexFound = true;
                    }
                }

                counter++;

                if (counter > 100) {
                    if (showDebugPrint) {
                        print ("COUNTER LOOP");

                        newWeaponIndexFound = true;
                    }

                    newWeaponIndex = 0;
                }
            }
        } else {
            newWeaponIndex = currentWeaponIndex;

            while (!newWeaponIndexFound) {
                newWeaponIndex++;

                if (newWeaponIndex >= AIWeaponInfoList.Count) {
                    newWeaponIndex = 0;
                }

                if (AIWeaponInfoList [newWeaponIndex].weaponEnabled) {
                    newWeaponIndexFound = true;
                }
            }
        }

        if (newWeaponIndex > -1) {
            setNewWeaponByName (AIWeaponInfoList [newWeaponIndex].Name);

            if (showDebugPrint) {
                print ("changing weapon to " + AIWeaponInfoList [newWeaponIndex].Name);
            }
        }

        checkingToChangeWeaponActive = false;
    }

    public void setNewWeaponByName (string weaponName)
    {
        if (!weaponsSystemEnabled) {
            return;
        }

        int newWeaponIndex = AIWeaponInfoList.FindIndex (s => s.Name.Equals (weaponName));

        if (newWeaponIndex > -1) {
            if (currentAIWeaponInfo != null) {
                currentAIWeaponInfo.isCurrentWeapon = false;
            }

            currentAIWeaponInfo = AIWeaponInfoList [newWeaponIndex];

            currentAIWeaponInfo.isCurrentWeapon = true;

            currentWeaponIndex = newWeaponIndex;
        }
    }

    public void setWeaponsSystemActiveState (bool state)
    {
        if (!weaponsSystemEnabled) {
            return;
        }

        weaponsSystemActive = state;

        checkEventsOnCombatStateChange (weaponsSystemActive);

        if (weaponsSystemActive && drawWeaponWhenAttackModeSelected && !mainPlayerWeaponsManager.isPlayerCarringWeapon ()) {
            setDrawOrHolsterWeaponState (true);
        }

        onSpotted = false;

        changeWeaponCheckActive = false;

        lastTimeAttack = 0;
    }

    void checkEventsOnCombatStateChange (bool state)
    {
        if (useEventsOnCombatActive) {
            if (state) {
                eventOnCombatActive.Invoke ();
            } else {
                eventOnCombatDeactivate.Invoke ();
            }
        }
    }

    public void pauseAttackDuringXTime (float newDuration)
    {
        currentPauseAttackStateDuration = newDuration;

        lastTimeAttackPauseWithDuration = Time.time;

        attackStatePaused = true;
    }

    public void setWaitToActivateAttackActiveState (bool state)
    {
        if (!changeWaitToActivateAttackEnabled) {
            return;
        }

        waitToActivateAttackActive = state;

        if (activateRandomWalkIfWaitToActivateAttackActive) {
            if (state) {
                currentWalkTime = Random.Range (randomWalkWaitTime.x, randomWalkWaitTime.y);

                currentWalkDuration = Random.Range (randomWalkDuration.x, randomWalkDuration.y);

                currentWalkRadius = Random.Range (randomWalkRadius.x, randomWalkRadius.y);

                mainFindObjectivesSystem.setignoreRandomWalkInFrontDirectionEnabledState (ignoreRandomWalkInFrontDirectionEnabled);

                mainFindObjectivesSystem.setRandomWalkPositionState (currentWalkRadius);

                lastTimeWalkActive = Time.time;

                walkActive = true;
            } else {
                if (walkActive) {
                    resetRandomWalkState ();
                }
            }
        }
    }

    public void setUseRandomWalkEnabledState (bool state)
    {
        randomWalkEnabled = state;
    }

    public void setOriginalUseRandomWalkEnabledState ()
    {
        setUseRandomWalkEnabledState (originalRandomWalkEnabled);
    }

    public void resetBehaviorStates ()
    {
        resetStates ();

        waitingForAttackActive = false;

        checkIfDrawWeaponActive = true;

        if (keepWeaponsIfNotTargetsToAttack) {
            setDrawOrHolsterWeaponState (false);
        } else {
            setAimWeaponState (false);
        }

        insideMinDistanceToAttack = false;
    }

    public void setDrawOrHolsterWeaponState (bool state)
    {
        if (state) {
            mainPlayerWeaponsManager.drawCurrentWeaponWhenItIsReady (true);
        } else {
            mainPlayerWeaponsManager.drawOrKeepWeapon (false);
        }

        weaponEquiped = state;

        if (!weaponEquiped) {
            aimingWeapon = false;

            resetSetWaitTimeOnFireWeaponBlastValues ();
        }

        if (showDebugPrint) {
            print ("setting draw weapon state " + state);
        }
    }

    public void setAimWeaponState (bool state)
    {
        if (showDebugPrint) {
            print ("setting aim active state " + state);
        }

        if (state) {
            if (!aimingWeapon) {
                mainPlayerWeaponsManager.aimCurrentWeaponWhenItIsReady (true);
                lastTimeFireWeaponAttackAtDistance = Time.time;
            }
        } else {
            if (aimingWeapon) {
                mainPlayerWeaponsManager.stopAimCurrentWeaponWhenItIsReady (true);
            }
        }

        aimingWeapon = state;

        if (!aimingWeapon) {
            resetSetWaitTimeOnFireWeaponBlastValues ();
        }
    }

    public void setShootWeaponState (bool state)
    {
        mainPlayerWeaponsManager.shootWeapon (state);

        shootingWeapon = state;

        if (state) {
            lastTimeAttack = Time.time;

            ignoreOtherActionsToForceAttackActive = false;
        }
    }

    public void dropWeapon ()
    {
        if (!weaponsSystemActive) {
            return;
        }

        if (AIPaused) {
            return;
        }

        if (behaviorStatesPaused) {
            return;
        }

        if (!canDropWeaponExternallyEnabled) {
            return;
        }

        if (shootingWeapon) {
            setShootWeaponState (false);
        }

        if (mainPlayerWeaponsManager.dropWeaponCheckingMinDelay ()) {
            aimingWeapon = false;

            resetAttackState ();

            updateWeaponsAvailableState ();

            if (characterHasWeapons) {
                setDrawOrHolsterWeaponState (true);
            }

            resetSetWaitTimeOnFireWeaponBlastValues ();
        }
    }

    public void shootTarget ()
    {
        setShootWeaponState (true);
    }

    public void resetAttackState ()
    {
        weaponEquiped = false;
        aimingWeapon = false;

        resetSetWaitTimeOnFireWeaponBlastValues ();
    }

    public void checkIfDrawWeaponsWhenResumingAI ()
    {
        if (drawWeaponsWhenResumingAI && !weaponEquiped) {
            setDrawOrHolsterWeaponState (true);
        }
    }

    public void stopAim ()
    {
        if (aimingWeapon) {
            setAimWeaponState (false);
        }
    }

    public void disableOnSpottedState ()
    {

    }

    public void updateWeaponsAvailableState ()
    {
        characterHasWeapons = mainPlayerWeaponsManager.checkIfWeaponsAvailable ();
    }

    public void setBehaviorStatesPausedState (bool state)
    {
        behaviorStatesPaused = state;

        if (behaviorStatesPaused) {
            resetAttackState ();
        }
    }

    public bool isWeaponEquiped ()
    {
        return weaponEquiped;
    }

    public void updateIfCarryingWeapon ()
    {
        weaponEquiped = mainPlayerWeaponsManager.isPlayerCarringWeapon ();
    }

    public void inputWeaponMeleeAttack ()
    {
        if (weaponsSystemActive) {
            setShootWeaponState (false);

            mainPlayerWeaponsManager.inputWeaponMeleeAttack ();
        }
    }

    public void stopCurrentAttackInProcess ()
    {
        if (shootingWeapon || aimingWeapon) {
            setShootWeaponState (false);

            stopAim ();
        }
    }

    public void setTurnBasedCombatActionActiveState (bool state)
    {
        turnBasedCombatActionActive = state;
    }

    public void checkAIBehaviorStateOnCharacterSpawn ()
    {
        if (checkInitialWeaponsOnCharacterSpawnEnabled) {
            mainPlayerWeaponsManager.enableOrDisableWeaponsListInRuntime (true);

            mainPlayerWeaponsManager.resetInitialWeaponCheckedOnCharacterSpawn ();
        }
    }

    public void checkAIBehaviorStateOnCharacterDespawn ()
    {

    }

    public void setMaxWaitTimeToForceAttackIfNotUsedLongTimeAgoValue (float newValue)
    {
        maxWaitTimeToForceAttackIfNotUsedLongTimeAgo = newValue;
    }

    public void setCheckToForceAttackIfNotUsedLongTimeAgoState (bool state)
    {
        checkToForceAttackIfNotUsedLongTimeAgo = state;
    }
    public void checkIfDisableCurrentWeaponToChangeAttackMode (string newModeName)
    {
        changeAttackMode (newModeName);
    }

    public void changeCurrentAttackMode (string newModeName)
    {
        changeAttackMode (newModeName);
    }

    void changeAttackMode (string newModeName)
    {
        if (!weaponsSystemActive) {
            return;
        }

        stopAttackState ();

        eventToChangeAttackMode.Invoke (newModeName);
    }

    public void stopAttackState ()
    {
        if (!weaponsSystemActive) {
            return;
        }

        if (mainPlayerWeaponsManager.isPlayerCarringWeapon ()) {
            bool playerIsBusy = mainPlayerWeaponsManager.currentWeaponIsMoving () ||
                                mainPlayerWeaponsManager.reloadingActionNotActive () ||
                                mainPlayerWeaponsManager.isActionActiveInPlayer () ||
                                !mainPlayerWeaponsManager.canPlayerMove ();

            if (playerIsBusy) {
                //print ("player is busy");

                resetBehaviorStates ();

                mainPlayerWeaponsManager.checkIfDisableCurrentWeapon ();
            } else {
                //print ("player is not busy");
            }
        }
    }

    [System.Serializable]
    public class AIWeaponInfo
    {
        [Header ("Main Settings")]
        [Space]

        public string Name;

        public bool weaponEnabled;

        public bool isCurrentWeapon;

        public float minTimeToUseNewWeaponAfterChange = 1.8f;
    }
}