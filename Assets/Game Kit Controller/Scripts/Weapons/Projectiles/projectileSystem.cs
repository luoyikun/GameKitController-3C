﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using GameKitController.Audio;

public class projectileSystem : MonoBehaviour
{
	[Header ("Main Settings")]
	[Space]

	public bool setContinuousSpeculativeIngame;

	public string abilityFunctionNameAtStart;
	public string abilityFunctionName;
	public bool useCustomValues;
	public projectileInfo currentProjectileInfo;

	public string armorSurfaceLayer = "Armor Surface";

	int armorSurfaceLayerID = -1;

	public bool disableProjectileMeshOnImpact = true;

	[HideInInspector] public GameObject objectToDamage;
	[HideInInspector] public RaycastHit hit;
	[HideInInspector] public bool projectileUsed;

	[HideInInspector] public bool projectilePaused;

	[HideInInspector] public Vector3 projectileLocalRotation;

	[Space]
	[Space]
	[Space]

	public bool useLayerMaskImpact;
	public LayerMask layerMaskImpact;

	public bool useEventOnImpact;
	public UnityEvent eventOnImpact;

	public bool sendObjectDetectedOnImpactEvent;
	public eventParameters.eventToCallWithGameObject objectDetectedOnImpactEvent;

	public bool useLayerMaskCollision;
	public LayerMask layerMaskCollision;

	public bool useEventOnCollision;
	public UnityEvent eventOnCollision;

	public bool useEventOnExplosion;
	public UnityEvent evenOnExplosion;

	[Space]
	[Space]
	[Space]

	public TrailRenderer mainTrailRenderer;
	public Collider mainCollider;
	public MeshCollider secondaryBulletMeshCollider;

	public mapObjectInformation mainMapObjectInformation;
	public audioClipBip audioClipBipManager;
	public Rigidbody mainRigidbody;

	public GameObject bulletMesh;
	public GameObject secondaryBulletMesh;

	public projectileImpactSystem mainProjectileImpactSystem;

	public fakeProjectileTrailSystem fakeProjectileTrail;

	bool explosionActivated;

	float currentRotationSpeed;
	float rotationSpeed = 10;
	float targetPositionOffset = 0.1f;
	Quaternion currentRotation;
	Vector3 velocityDirection;
	Vector3 currentTargetDirection;
	Vector3 currentTargetPosition;
	float currentAngleWithTarget;
	float currentDistanceToTarget;
	float currentDistance;

	bool projectileCaptured;

	bool impactParticlesCreated;

	healthManagement mainHealthManagement;

	bool projectileAttachedToSurface;

	bool ignoreParentDestroyed;

	int currentNumberOfImpacts;
	List<GameObject> listOfGameObjectImpacts = new List<GameObject> ();
	List<Collider> listOfColliderImpacts = new List<Collider> ();

	float orbitingTargetTime;
	Vector3 currentProjectilePosition;

	Coroutine surfaceCoroutine;

	Vector3 originalProjectilePosition;

	Vector3 movementThisStep;
	float movementSqrMagnitude;
	float movementMagnitude;

	float minimumExtent;
	float sqrMinimumExtent;
	Vector3 previousPosition;

	float lastTimeRaycastCheckingActive;

	int loop;

	bool collisionDetectionChecked;

	bool pauseCheckTriggerActive;

	bool projectileRemovedPreviously;

	public bool projectileInitialized;

	Transform currentTargetTransform;

	Vector3 targetPosition;

	float lastTimeFired = -1;

	float noImpactDisableTimer;

	bool projectileCanBreakThroughObjectResult;

	bool mainMapObjectInformationAssigned;

	bool fakeProjectileTrailAssigned;

	bool audioClipBipManagerAssigned;

	bool mainTrailRendererAssigned;

	bool mainProjectileImpactSystemAssigned;

	bool impactParticlesAssigned;

	bool mainColliderAssigned;

	bool mainRigidbodyAssigned;

	bool projectileParticlesAssigned;

	bool projectileMeshesAssigned;

	GameObject currentTarget;


	void Update ()
	{
		if ((currentProjectileInfo.isHommingProjectile || currentProjectileInfo.isSeeker) && !mainRigidbody.isKinematic) {
			currentProjectilePosition = transform.position;

			mainRigidbody.velocity = currentProjectileInfo.projectileSpeed * transform.forward;

			if (currentTargetTransform != null) {
				targetPosition = currentTargetTransform.position;

				currentTargetPosition = targetPosition + (targetPositionOffset * currentTargetTransform.up);

				currentAngleWithTarget = Vector3.Angle (transform.up, currentTargetDirection);

				if (currentAngleWithTarget > 80 && currentAngleWithTarget < 100) {
					orbitingTargetTime += Time.deltaTime;
				}

				if (orbitingTargetTime > 0.8f) {
					orbitingTargetTime = 0;
					rotationSpeed *= 2;
				}

				currentDistanceToTarget = GKC_Utils.distance (targetPosition, currentProjectilePosition);

				currentRotationSpeed = rotationSpeed + (1 / currentDistanceToTarget);

				currentTargetDirection = currentTargetPosition - currentProjectilePosition;
				currentRotation = Quaternion.LookRotation (currentTargetDirection);
				transform.rotation = Quaternion.Lerp (transform.rotation, currentRotation, Time.deltaTime * currentRotationSpeed);
			}

			if (projectileCaptured) {
				if (currentTarget == null) {
					Collider[] colliders = Physics.OverlapSphere (currentProjectilePosition, currentProjectileInfo.explosionRadius, currentProjectileInfo.targetToDamageLayer);

					if (colliders.Length > 0) {
						float minDistance = Mathf.Infinity;

						for (int i = 0; i < colliders.Length; i++) {
							currentDistance = GKC_Utils.distance (colliders [i].transform.position, currentProjectilePosition);

							if (currentDistance < minDistance) {
								minDistance = currentDistance;
								currentTarget = colliders [i].gameObject;

								currentTargetTransform = currentTarget.transform;
							}
						}
					}
				}
			}
		}
			
		//destroy the projectile 
		if (currentProjectileInfo.useDisableTimer && !projectilePaused) {
			if (noImpactDisableTimer > 0) {
				noImpactDisableTimer -= Time.deltaTime;

				if (noImpactDisableTimer < 0) {
					if (!projectileUsed) {
						if (currentProjectileInfo.isExplosive || currentProjectileInfo.isImplosive) {
							setProjectileUsedState (true);

							checkExplodeProjectile ();

							return;
						}
					}

					destroyProjectile ();

					return;
				}
			}
		}

		if (currentProjectileInfo.launchProjectile) {
			if (mainRigidbody.velocity != Vector3.zero) {
				velocityDirection = mainRigidbody.velocity;
			}
		}

		if (projectileUsed) {
			if (projectileAttachedToSurface) {
				if (!ignoreParentDestroyed) {
					if (mainHealthManagement != null) {
						if (mainHealthManagement.checkIfDeadWithHealthManagement ()) {
//						print ("target dead");

							parentDestroyed ();
						}
					}
				}
			}
		}
	}

	void FixedUpdate ()
	{
		if (currentProjectileInfo.useRaycastCheckingOnRigidbody && !projectilePaused && !projectileUsed) {
			if (previousPosition != Vector3.zero) {
				movementThisStep = mainRigidbody.position - previousPosition; 
				movementSqrMagnitude = movementThisStep.sqrMagnitude;

				if (movementSqrMagnitude > sqrMinimumExtent ||
				    (currentProjectileInfo.customRaycastCheckingRate > 0 && Time.time > currentProjectileInfo.customRaycastCheckingRate + lastTimeRaycastCheckingActive)) { 
					movementMagnitude = Mathf.Sqrt (movementSqrMagnitude);

//					Debug.DrawLine (previousPosition, (previousPosition + movementThisStep * (movementMagnitude + currentProjectileInfo.customRaycastCheckingDistance)), Color.red, 5);

					if (Physics.Raycast (previousPosition, movementThisStep, out hit, movementMagnitude + currentProjectileInfo.customRaycastCheckingDistance, currentProjectileInfo.targetToDamageLayer)) {
						checkSurface (hit.collider);
					}

					lastTimeRaycastCheckingActive = Time.time;
				} 
			}

			previousPosition = mainRigidbody.position;
		}
	}

	public void parentDestroyed ()
	{
		if (secondaryBulletMeshCollider != null) {
			setProjectileParent (null);

			enableOrDisableProjectileCollider (true);
		} else {
			destroyProjectile ();
		}
	}

	public void setEnemy (GameObject obj)
	{
		StartCoroutine (missileWaiting (obj));
	}

	IEnumerator missileWaiting (GameObject obj)
	{
		WaitForSeconds delay = new WaitForSeconds (currentProjectileInfo.waitTimeToSearchTarget);

		yield return delay;

		currentTarget = obj;

		currentTargetTransform = applyDamage.getPlaceToShoot (currentTarget);

		if (currentTargetTransform == null) {
			currentTargetTransform = obj.transform;
		}
	}
		
	//when the bullet touchs a surface, then
	void OnTriggerEnter (Collider col)
	{
		if (pauseCheckTriggerActive) {
			return;
		}

		checkSurface (col);
	}

	bool ignoreProjectileWithAbilityState;

	public void setIgnoreProjectileWithAbilityState (bool state)
	{
		ignoreProjectileWithAbilityState = state;
	}

	public void checkSurface (Collider col)
	{
		if (currentProjectileInfo.projectileWithAbility && !ignoreProjectileWithAbilityState) {
			if (abilityFunctionName != "") {
				SendMessage (abilityFunctionName, col, SendMessageOptions.DontRequireReceiver);
			}

			return;
		}

		//if the layer of the object found is in the layers list, then

		bool canActivateEffectResult = canActivateEffect (col);

		if (canActivateEffectResult) {
			objectToDamage = col.GetComponent<Collider> ().gameObject;

			if (currentProjectileInfo.projectileCanBeDeflected) {
				if (armorSurfaceLayerID == -1) {
					armorSurfaceLayerID = LayerMask.NameToLayer (armorSurfaceLayer);
				}

				if (objectToDamage.layer == armorSurfaceLayerID) {
					if (!projectileCaptured) {
						armorSurfaceSystem currentArmorSurfaceSystem = objectToDamage.GetComponent<armorSurfaceSystem> ();

						if (currentArmorSurfaceSystem != null) {
							if (currentArmorSurfaceSystem.isArmorEnabled ()) {
								enableOrDisableMainTrailRenderer (false);

								setProjectileParent (objectToDamage.gameObject.transform.parent);

								checkCollisionDetectionMode ();

								setKinematicState (true);

								projectilePaused = true;
								projectileCaptured = true;

								currentArmorSurfaceSystem.addProjectile (this);
							} else {
								return;
							}
						}
					}

					if (projectileCaptured) {
						return;
					}
				}
			}

			bool isNewGameObjectImpact = false;

			projectileCanBreakThroughObjectResult = false;

			if (currentProjectileInfo.breakThroughObjects) {
				projectileCanBreakThroughObjectResult = true;

				if (!currentProjectileInfo.canBreakThroughArmorSurface) {
					if (armorSurfaceLayerID == -1) {
						armorSurfaceLayerID = LayerMask.NameToLayer (armorSurfaceLayer);
					}

					if (objectToDamage.layer == armorSurfaceLayerID) {
						armorSurfaceSystem currentArmorSurfaceSystem = objectToDamage.GetComponent<armorSurfaceSystem> ();

						if (currentArmorSurfaceSystem != null) {
							if (currentArmorSurfaceSystem.isArmorEnabled ()) {

								if (currentArmorSurfaceSystem.isBlockProjectilesFromBreakingThroughSurfaceEnabled ()) {

									if (currentProjectileInfo.breakThroughArmorSurfacePriorityValue <= currentArmorSurfaceSystem.getArmorSurfacePriorityValue ()) {
										projectileCanBreakThroughObjectResult = false;
									}
								}
							}
						}
					}
				}
			}

			if (projectileCanBreakThroughObjectResult) {
				GameObject character = applyDamage.getCharacterOrVehicle (objectToDamage);

				if (character == null) {
					character = objectToDamage;
				}

				if (!listOfGameObjectImpacts.Contains (character) || currentProjectileInfo.canDamageSameObjectMultipleTimes) {
					listOfGameObjectImpacts.Add (character);
					currentNumberOfImpacts++;
					isNewGameObjectImpact = true;
				}

				if (currentProjectileInfo.stopBreakThroughObjectsOnLayer) {
					if ((1 << objectToDamage.layer & currentProjectileInfo.layerToStopBreahtThroughObjects.value) == 1 << objectToDamage.layer) {
						currentNumberOfImpacts = currentProjectileInfo.numberOfImpacts;
					}
				}

				if (!listOfColliderImpacts.Contains (col)) {
					listOfColliderImpacts.Add (col);
				}

				if (!currentProjectileInfo.infiniteNumberOfImpacts) {
					if (currentNumberOfImpacts >= currentProjectileInfo.numberOfImpacts) {
						setProjectileUsedState (true);
					}
				}

				if (currentNumberOfImpacts == 1 && !currentProjectileInfo.useRayCastShoot) {
					if (!currentProjectileInfo.ignoreNewRotationOnProjectileImpact) {
						transform.rotation = Quaternion.LookRotation (currentProjectileInfo.forwardDirection);
					}

					mainRigidbody.velocity = currentProjectileInfo.projectileSpeed * transform.forward;
				}
			} else {
				setProjectileUsedState (true);

				isNewGameObjectImpact = true;
			}

			objectToDamage = col.GetComponent<Collider> ().gameObject;

			Rigidbody objectToDamageRigidbody = objectToDamage.GetComponent<Rigidbody> ();

			if (projectileUsed) {
				//set the bullet kinematic
				if (objectToDamageRigidbody == null && !currentProjectileInfo.useGravityOnLaunch && !currentProjectileInfo.useGraivtyOnImpact &&
				    !currentProjectileInfo.useExplosionDelay) {
					checkCollisionDetectionMode ();

					setKinematicState (true);
				}

				if (currentProjectileInfo.useGravityOnLaunch || currentProjectileInfo.useGraivtyOnImpact || currentProjectileInfo.useExplosionDelay) {
					if (secondaryBulletMesh != null) {
						secondaryBulletMesh.SetActive (true);
					}

					enableOrDisableSecondaryMeshCollider (true);
				}
			}

			checkCollisionType currentCheckCollisionType = objectToDamage.GetComponent<checkCollisionType> ();

			if (currentCheckCollisionType != null) {
				currentCheckCollisionType.checkTriggerWithGameObject (gameObject, true);
			}

			if (isNewGameObjectImpact) {
				//damage the surface where the proectile impacts
				if (currentProjectileInfo.killInOneShot) {
					Vector3 damageDirection = transform.forward;

					if (currentProjectileInfo.launchProjectile) {
						damageDirection = objectToDamage.transform.position - transform.position;

						damageDirection = damageDirection / damageDirection.magnitude;
					}

					applyDamage.killCharacter (gameObject, objectToDamage, -damageDirection, transform.position, 
						currentProjectileInfo.owner, false);
				} else {
					bool damageSurfaceMultipleTimes = false;

					if (projectileCanBreakThroughObjectResult && currentProjectileInfo.canDamageSameObjectMultipleTimes) {
						damageSurfaceMultipleTimes = true;
					}

					if (currentProjectileInfo.projectileDamage > 0 || currentProjectileInfo.damageTypeID > -1) {
						applyDamage.checkHealth (gameObject, objectToDamage, currentProjectileInfo.projectileDamage, -transform.forward, transform.position, 
							currentProjectileInfo.owner, damageSurfaceMultipleTimes, true, currentProjectileInfo.ignoreShield, false, currentProjectileInfo.damageCanBeBlocked,
							currentProjectileInfo.canActivateReactionSystemTemporally, currentProjectileInfo.damageReactionID, currentProjectileInfo.damageTypeID);
					}

					if (currentProjectileInfo.damageTargetOverTime) {
						applyDamage.setDamageTargetOverTimeState (objectToDamage, currentProjectileInfo.damageOverTimeDelay, currentProjectileInfo.damageOverTimeDuration, 
							currentProjectileInfo.damageOverTimeAmount,	currentProjectileInfo.damageOverTimeRate, currentProjectileInfo.damageOverTimeToDeath,
							currentProjectileInfo.damageTypeID);
					}

					if (currentProjectileInfo.removeDamageOverTimeState) {
						applyDamage.removeDamagetTargetOverTimeState (objectToDamage);
					}

					if (currentProjectileInfo.sedateCharacters) { 
						applyDamage.sedateCharacter (objectToDamage, transform.position, currentProjectileInfo.sedateDelay, currentProjectileInfo.useWeakSpotToReduceDelay, 
							currentProjectileInfo.sedateUntilReceiveDamage, currentProjectileInfo.sedateDuration);
					}

					if (currentProjectileInfo.pushCharacter) { 
						applyDamage.pushAnyCharacter (objectToDamage, currentProjectileInfo.pushCharacterForce, currentProjectileInfo.pushCharacterRagdollForce, transform.forward);
					}
				}

				if (currentProjectileInfo.useRemoteEventOnObjectsFound) {
					checkRemoteEvents (objectToDamage);
				}

				//apply force if the surface has a rigidbody
				if (currentProjectileInfo.applyImpactForceToVehicles) {
					Rigidbody objectToDamageMainRigidbody = applyDamage.applyForce (objectToDamage);

					if (objectToDamageMainRigidbody != null) {
						Vector3 force = currentProjectileInfo.impactForceApplied * transform.forward;

						bool vehicleDetected = applyDamage.isVehicle (objectToDamage);

						if (vehicleDetected) {
							force = currentProjectileInfo.impactForceToVehiclesMultiplier * force;
						}

						float massForce = objectToDamageMainRigidbody.mass;

						if (currentProjectileInfo.forceMassMultiplier == 0) {
							massForce = 1;
						} else {
							massForce *= currentProjectileInfo.forceMassMultiplier;
						}

						objectToDamageMainRigidbody.AddForce (massForce * force, currentProjectileInfo.forceMode);
					}
				} else {
					if (applyDamage.canApplyForce (objectToDamage)) {
						Vector3 force = currentProjectileInfo.impactForceApplied * transform.forward;

						if (objectToDamageRigidbody == null) {
							objectToDamageRigidbody = objectToDamage.GetComponent<Rigidbody> ();
						}

						float massForce = objectToDamageRigidbody.mass;

						if (currentProjectileInfo.forceMassMultiplier == 0) {
							massForce = 1;
						} else {
							massForce *= currentProjectileInfo.forceMassMultiplier;
						}

						objectToDamageRigidbody.AddForce (massForce * force, currentProjectileInfo.forceMode);
					}
				}

				externalDamageProperties currentExternalDamageProperties = objectToDamage.GetComponent<externalDamageProperties> ();

				if (currentExternalDamageProperties != null) {
					currentExternalDamageProperties.setDamageOwner (currentProjectileInfo.owner);
				}
			}

			if (currentProjectileInfo.isExplosive || currentProjectileInfo.isImplosive) {
				checkExplodeProjectile ();
			}

			if (currentProjectileInfo.launchProjectile) {
				transform.rotation = Quaternion.LookRotation (velocityDirection);
			}

			if (currentProjectileInfo.adhereToSurface) {
				attachProjectileToSurface ();

			} else {
				if (currentProjectileInfo.useGraivtyOnImpact) {
					setUseGravityState (true);
				}
			}

			if (isNewGameObjectImpact) {
				if (!currentProjectileInfo.isExplosive && !currentProjectileInfo.isImplosive) {
					setProjectileScorch ();

					disableBullet (currentProjectileInfo.impactDisableTimer);

					checkMapIcon ();
				}
			}
				
			if (projectileCanBreakThroughObjectResult && isNewGameObjectImpact && !projectileUsed && currentProjectileInfo.useRayCastShoot) {
				RaycastHit[] hits;

				hits = Physics.RaycastAll (transform.position, transform.forward, Mathf.Infinity, currentProjectileInfo.targetToDamageLayer);

				float maxDistance = Mathf.Infinity;
				int elementIndex = -1;
				Vector3 hitPoint = Vector3.zero;
				Vector3 currentPosition = transform.position;

				for (int i = 0; i < hits.Length; i++) {
					if (!listOfColliderImpacts.Contains (hits [i].collider)) {
						CharacterJoint boneJoint = hits [i].collider.GetComponent<CharacterJoint> ();

						if (boneJoint == null) {
							currentDistance = GKC_Utils.distance (currentPosition, hits [i].point);

							if (currentDistance < maxDistance) {
								maxDistance = currentDistance;
								elementIndex = i;

								hitPoint = hits [i].point;
							}
						}
					}
				}

				if (elementIndex > -1) {
					rayCastShoot (hits [elementIndex].collider, hitPoint, transform.forward);
				}
			}

			loop++;

			if (loop > 100) {
				print ("WARNING: loop in projectile system, stopping" + loop);

				return;
			}

			projectimeImpactState (objectToDamage);

			if (currentProjectileInfo.sliceObjectsDetected) {
				bool activateSliceResult = true;

				if (currentProjectileInfo.useGeneralProbabilitySliceObjects) {
					float randomProbability = UnityEngine.Random.Range (0, 101);

					if (randomProbability > currentProjectileInfo.generalProbabilitySliceObjects) {
						activateSliceResult = false;
					}
				}

				if (activateSliceResult) {
					Vector3 slicePosition = transform.position;
			
					if (currentProjectileInfo.useBodyPartsSliceList) {
						if (applyDamage.characterHasWeakSpotList (objectToDamage)) {
							activateSliceResult = applyDamage.checkIfDamagePositionIsCloseEnoughToWeakSpotByName (objectToDamage, transform.position,
								currentProjectileInfo.bodyPartsSliceList, currentProjectileInfo.maxDistanceToBodyPart);

							if (activateSliceResult) {
								Vector3 newSlicePosition = applyDamage.getClosestWeakSpotPositionToPosition (objectToDamage, slicePosition, 
									                           currentProjectileInfo.bodyPartsSliceList, true, currentProjectileInfo.maxDistanceToBodyPart);

								if (newSlicePosition != -Vector3.one) {
									slicePosition = newSlicePosition;
								}
							}
						}
					}

					if (activateSliceResult) {
						sliceSystemUtils.processObjectToSlice (objectToDamage, col.GetComponent<Collider> (), 
							slicePosition, transform.rotation, 0.1f, transform.up, transform.right, transform.forward,
							10, currentProjectileInfo.layerToSlice, currentProjectileInfo.randomSliceDirection,
							currentProjectileInfo.showSliceGizmo, currentProjectileInfo.activateRigidbodiesOnNewObjects);
					}
				}
			}
		}
	}

	public void checkMapIcon ()
	{
		getProjectileMapInformation ();

		if (mainMapObjectInformationAssigned) {
			mainMapObjectInformation.removeMapObject ();
		}
	}

	public void createMapIconInfo ()
	{
		getProjectileMapInformation ();

		if (mainMapObjectInformationAssigned) {
			mainMapObjectInformation.createMapIconInfo ();
		}
	}

	Coroutine explosionCoroutine;

	public void checkExplodeProjectile ()
	{
		if (currentProjectileInfo == null) {
			return;
		}

		if (currentProjectileInfo.useExplosionDelay) {
			if (explosionCoroutine != null) {
				StopCoroutine (explosionCoroutine);
			}

			explosionCoroutine = StartCoroutine (explodeProjectileCoroutine ());
		} else {
			explodeProjectileFunction ();
		}
	}

	bool projectileWaitingToExplodeActive;

	IEnumerator explodeProjectileCoroutine ()
	{
		projectileWaitingToExplodeActive = true;

		noImpactDisableTimer = currentProjectileInfo.explosionDelay + 0.1f;

		getProjectileBipManager ();

		if (audioClipBipManagerAssigned) {
			audioClipBipManager.increasePlayTime (currentProjectileInfo.explosionDelay);
		}
			
		WaitForSeconds delay = new WaitForSeconds (currentProjectileInfo.explosionDelay);

		yield return delay;

		explodeProjectileFunction ();

		projectileWaitingToExplodeActive = false;
	}

	public void explodeProjectileFunction ()
	{
		if (!currentProjectileInfo.isExplosive && !currentProjectileInfo.isImplosive) {
			return;
		}

		Transform lastProjectileParent = transform.parent;

		if (lastProjectileParent != null) {
			setProjectileParent (null);
		}

		explodeProjectile ();

		if (lastProjectileParent != null) {
//			print ("IS ACTIVE RESULT " + (lastProjectileParent.gameObject.activeInHierarchy && lastProjectileParent.gameObject.activeSelf));

			if (lastProjectileParent.gameObject.activeInHierarchy && lastProjectileParent.gameObject.activeSelf) {
				setProjectileParent (lastProjectileParent);
			}
		}

		setProjectileScorch ();

		checkMapIcon ();

		disableBullet (currentProjectileInfo.impactDisableTimer);
	}

	void explodeProjectile ()
	{
		//explosion or implosion
		//get all the objects inside a radius in the impact position, applying to them an explosion force
		if (!explosionActivated) {
			creatImpactParticles ();

			Vector3 currentPosition = transform.position;

			Transform projectileOwner = transform;

			if (currentProjectileInfo.owner != null) {
				projectileOwner = currentProjectileInfo.owner.transform;
			}

			applyDamage.setExplosion (currentPosition, currentProjectileInfo.explosionRadius, true, currentProjectileInfo.targetToDamageLayer, currentProjectileInfo.owner, 
				currentProjectileInfo.canDamageProjectileOwner, gameObject, currentProjectileInfo.killInOneShot, currentProjectileInfo.isExplosive, currentProjectileInfo.isImplosive, 
				currentProjectileInfo.explosionDamage, currentProjectileInfo.pushCharacters, currentProjectileInfo.applyExplosionForceToVehicles, 
				currentProjectileInfo.explosionForceToVehiclesMultiplier, currentProjectileInfo.explosionForce, currentProjectileInfo.forceMode, false, 
				projectileOwner, currentProjectileInfo.ignoreShield, currentProjectileInfo.useRemoteEventOnObjectsFoundOnExplosion,
				currentProjectileInfo.remoteEventNameOnExplosion, currentProjectileInfo.damageTypeID,
				currentProjectileInfo.damageTargetOverTime, currentProjectileInfo.damageOverTimeDelay, currentProjectileInfo.damageOverTimeDuration, 
				currentProjectileInfo.damageOverTimeAmount, currentProjectileInfo.damageOverTimeRate, currentProjectileInfo.damageOverTimeToDeath, 
				currentProjectileInfo.removeDamageOverTimeState, currentProjectileInfo.damageCanBeBlocked, currentProjectileInfo.searchClosestWeakSpot);

			explosionActivated = true;

			if (useEventOnExplosion) {
				evenOnExplosion.Invoke ();
			}
		}
	}

	public void setProjectileScorch ()
	{
		bool scorchCreatedCorrectly = false;

		bool objectToDamageNotNull = objectToDamage != null;

		if (objectToDamageNotNull) {
			scorchCreatedCorrectly = decalManager.setImpactDecal (decalManager.checkIfHasDecalImpact (objectToDamage), 
				transform, objectToDamage, currentProjectileInfo.scorchRayCastDistance,
				currentProjectileInfo.targetForScorchLayer, true, currentProjectileInfo.projectilesPoolEnabled);
		} 

//		print (scorchCreatedCorrectly);
			
		if (!scorchCreatedCorrectly) {
//			print (currentProjectileInfo != null);

			if (currentProjectileInfo.impactAudioElement != null) {
				createImpactSound (currentProjectileInfo.impactAudioElement);
			}

			if (currentProjectileInfo.scorch != null) {
				if (objectToDamageNotNull) {
					//the bullet fired is a simple bullet or a greanade, check the hit point with a raycast to set in it a scorch
					if (Physics.Raycast (transform.position - 0.4f * transform.forward, transform.forward, out hit, 
						    currentProjectileInfo.scorchRayCastDistance, currentProjectileInfo.targetForScorchLayer)) {

						decalManager.setScorch (transform.rotation, currentProjectileInfo.scorch, hit, objectToDamage, currentProjectileInfo.projectilesPoolEnabled);
					}
				}
			}

			creatImpactParticles ();
		} else {
			if (currentProjectileInfo.isExplosive || currentProjectileInfo.isImplosive) {
				createImpactSound (currentProjectileInfo.impactAudioElement);
			}
		}
	}

	public void createImpactSound (AudioElement impactSoundEffect)
	{
		getMainProjectileImpactSystem ();

		if (mainProjectileImpactSystemAssigned) {
			if (currentProjectileInfo.projectilesPoolEnabled) {
				GameObject newProjectileImpactGameObject = GKC_PoolingSystem.Spawn (mainProjectileImpactSystem.gameObject, transform.position, transform.rotation, 30);

				projectileImpactSystem newProjectileImpactSystem = newProjectileImpactGameObject.GetComponent<projectileImpactSystem> ();

				newProjectileImpactSystem.setSendObjectToPoolSystemToDisableState (true);

				newProjectileImpactSystem.activateImpactElements (transform.position, impactSoundEffect);
			} else {

				if (currentProjectileInfo.projectilesPoolEnabled) {
					mainProjectileImpactSystem.changeDestroyForSetActiveFunction (true);
				}

				mainProjectileImpactSystem.activateImpactElements (transform.position, impactSoundEffect);
			}
		}
	}

	public bool canActivateEffect (Collider col)
	{
		if (col == null) {
			return false;
		}

		if (projectileUsed) {
			return false;
		}

		if (projectilePaused) {
			return false;
		}

		if ((1 << col.gameObject.layer & currentProjectileInfo.targetToDamageLayer.value) != 1 << col.gameObject.layer) {
			return false;
		}

		if (currentProjectileInfo.useCustomIgnoreTags && currentProjectileInfo.customTagsToIgnoreList.Contains (col.gameObject.tag)) {
			return false;
		}
			
		if (col.gameObject == currentProjectileInfo.owner) {
			if (!currentProjectileInfo.allowDamageForProjectileOwner) {
				if (!projectileCaptured) {
					return false;
				}
			}
		}

		if (currentProjectileInfo.launchProjectile && !currentProjectileInfo.useRayCastShoot && Time.time < lastTimeFired + 0.05f) {
			return false;
		}

		return true;
	}

	public void creatImpactParticles ()
	{
		if (!impactParticlesAssigned) {
			impactParticlesAssigned = currentProjectileInfo.impactParticles != null;
		}

		if (impactParticlesAssigned && !impactParticlesCreated) {
			if (currentProjectileInfo.projectilesPoolEnabled) {
				GKC_PoolingSystem.Spawn (currentProjectileInfo.impactParticles, transform.position, transform.rotation, currentProjectileInfo.maxAmountOfPoolElementsOnWeapon);
			} else {
				Instantiate (currentProjectileInfo.impactParticles, transform.position, transform.rotation);
			}

			impactParticlesCreated = true;
		}
	}

	//destroy the bullet according to the time value
	public void disableBullet (float time)
	{
		if (currentProjectileInfo.impactAudioElement.clip) {
			if (currentProjectileInfo.impactAudioElement.clip.length > time) {
				noImpactDisableTimer = currentProjectileInfo.impactAudioElement.clip.length;
			} else {
				noImpactDisableTimer = time;
			}
		} else {
			noImpactDisableTimer = time;
		}

		if (!currentProjectileInfo.breakThroughObjects) {
			checkCollisionDetectionMode ();

			setKinematicState (true);

			if (bulletMesh != null && disableProjectileMeshOnImpact) {
				bulletMesh.SetActive (false);
			}
		}
	}

	public void setKinematicState (bool state)
	{
		if (mainRigidbodyAssigned) {
			mainRigidbody.isKinematic = state;
		}
	}
		
	//get the info of the current weapon selected, so the projectile has the correct behaviour
	public void setProjectileInfo (projectileInfo info)
	{
		if (useCustomValues) {
			return;
		}

		currentProjectileInfo = new projectileInfo (info);

		noImpactDisableTimer = currentProjectileInfo.noImpactDisableTimer;

		lastTimeFired = Time.time;
	}

	public void setProjectileOnwer (GameObject owner)
	{
		currentProjectileInfo.owner = owner;
	}

	public void setTargetToDamageLayer (LayerMask layer)
	{
		currentProjectileInfo.targetToDamageLayer = layer;
	}

	public void setUseCustomIgnoreTags (bool state, List<string> newCustomTagsToIgnoreList)
	{
		currentProjectileInfo.useCustomIgnoreTags = state;

		currentProjectileInfo.customTagsToIgnoreList = newCustomTagsToIgnoreList;
	}

	//if the projectiles is placed directly in the raycast hit point, place the projectile in the correct position
	public void rayCastShoot (Collider surface, Vector3 position, Vector3 forwardDirection)
	{
		originalProjectilePosition = transform.position;

		transform.position = position;

		getProjectileCollider ();

		currentProjectileInfo.useRayCastShoot = true;

		initializeProjectile ();

		transform.rotation = Quaternion.LookRotation (forwardDirection);

		if (currentProjectileInfo.useRaycastShootDelay) {
			if (bulletMesh != null && disableProjectileMeshOnImpact) {
				bulletMesh.SetActive (false);
			}

			pauseCheckTriggerActive = true;

			checkSurfaceWithDelay (surface);
		} else {
			checkSurface (surface);
		}
	}

	public void setProjectileLocalRotation (Vector3 newProjectileLocalRotation)
	{
		projectileLocalRotation = newProjectileLocalRotation;
	}

	public void checkSurfaceWithDelay (Collider surface)
	{
		stopCheckSurfaceWithDelay ();

		surfaceCoroutine = StartCoroutine (checkSurfaceWithDelayCoroutine (surface));
	}

	void stopCheckSurfaceWithDelay ()
	{
		if (surfaceCoroutine != null) {
			StopCoroutine (surfaceCoroutine);
		}

		if (checkSurfaceWithDelayActive) {
			if (fakeProjectileTrailAssigned) {
				fakeProjectileTrail.stopTrailMovement ();
			}
		}

		checkSurfaceWithDelayActive = false;
	}

	bool checkSurfaceWithDelayActive;

	IEnumerator checkSurfaceWithDelayCoroutine (Collider surface)
	{
		checkSurfaceWithDelayActive = true;

		if (currentProjectileInfo.useFakeProjectileTrails) {
			getFakeProjectileTrail ();
		}

		if (currentProjectileInfo.getDelayWithDistance) {

			float raycastShootDelay = GKC_Utils.distance (transform.position, originalProjectilePosition) / currentProjectileInfo.delayWithDistanceSpeed;

			if (currentProjectileInfo.maxDelayWithDistance > 0) {
				raycastShootDelay = Mathf.Clamp (raycastShootDelay, 0, currentProjectileInfo.maxDelayWithDistance);
			}

			if (currentProjectileInfo.useFakeProjectileTrails) {
				if (fakeProjectileTrailAssigned) {
					fakeProjectileTrail.setSpeedMultiplier (raycastShootDelay);
				}
			}

			WaitForSeconds delay = new WaitForSeconds (raycastShootDelay);

			yield return delay;
		} else {
			if (currentProjectileInfo.useFakeProjectileTrails) {
				if (fakeProjectileTrailAssigned) {
					fakeProjectileTrail.setSpeedMultiplier (currentProjectileInfo.raycastShootDelay);
				}
			}

			WaitForSeconds delay = new WaitForSeconds (currentProjectileInfo.raycastShootDelay);

			yield return delay;
		}

		pauseCheckTriggerActive = false;

		checkSurface (surface);

//		print ("1 " + fakeProjectileTrailAssigned + " " + currentProjectileInfo.useFakeProjectileTrails);

		if (currentProjectileInfo.useFakeProjectileTrails) {
			if (fakeProjectileTrailAssigned) {
				fakeProjectileTrail.stopTrailMovement ();
		
//				print ("stop ");
			}
		}

		checkSurfaceWithDelayActive = false;
	}

	public void checkRemoteEvents (GameObject objectToCheck)
	{
		if (objectToCheck == null) {
			objectToCheck = objectToDamage;
		}

		if (objectToCheck == null) {
			return;
		}

		remoteEventSystem currentRemoteEventSystem = objectToCheck.GetComponent<remoteEventSystem> ();

		if (currentRemoteEventSystem != null) {
			int remoteEventNameListCount = currentProjectileInfo.remoteEventNameList.Count;

			for (int i = 0; i < remoteEventNameListCount; i++) {
				currentRemoteEventSystem.callRemoteEvent (currentProjectileInfo.remoteEventNameList [i]);
			}
		}
	}

	public void checkRemoteEventOnProjectile (string remoteEventName)
	{
		remoteEventSystem currentRemoteEventSystem = GetComponent<remoteEventSystem> ();

		if (currentRemoteEventSystem != null) {
			currentRemoteEventSystem.callRemoteEvent (remoteEventName);
		}
	}

	public void checkRemoteEventsOnProjectile (List<string> remoteEventNameList)
	{
		remoteEventSystem currentRemoteEventSystem = GetComponent<remoteEventSystem> ();

		if (currentRemoteEventSystem != null) {
			int remoteEventNameListCount = remoteEventNameList.Count;

			for (int i = 0; i < remoteEventNameListCount; i++) {
				currentRemoteEventSystem.callRemoteEvent (remoteEventNameList [i]);
			}
		}
	}

	public void creatFakeProjectileTrail (Vector3 targetPosition)
	{
		getFakeProjectileTrail ();

		if (fakeProjectileTrailAssigned) {
			fakeProjectileTrail.resetFakeProjectileTrail ();

			fakeProjectileTrail.transform.position = transform.position;

			fakeProjectileTrail.instantiateFakeProjectileTrail (targetPosition);

			if (currentProjectileInfo.projectilesPoolEnabled) {
				fakeProjectileTrail.changeDestroyForSetActiveFunction (true);
			}
		}
	}

	public void setDestroyTrailAfterTimeState (bool state)
	{
		getFakeProjectileTrail ();

		if (fakeProjectileTrailAssigned) {
			fakeProjectileTrail.setDestroyTrailAfterTimeState (state);
		}
	}

	public void setFakeProjectileTrailSpeedMultiplier (float newValue)
	{
		getFakeProjectileTrail ();

		if (fakeProjectileTrailAssigned) {
			fakeProjectileTrail.setSpeedMultiplier (newValue);
		}
	}

	public void destroyProjectile ()
	{
		checkMapIcon ();

		projectileInitialized = false;

		if (currentProjectileInfo.projectilesPoolEnabled) {
			GKC_PoolingSystem.Despawn (gameObject);

			transform.position = 1000 * (-Vector3.up);

			resetProjectile ();

			setKinematicState (false);

			transform.position = 3000 * Vector3.up;
		
			projectileRemovedPreviously = true;
		} else {
			Destroy (gameObject);
		}
	}

	void OnDisable ()
	{
		if (!projectileRemovedPreviously) {
			if (projectileWaitingToExplodeActive) {
//				print ("ON DISABLE PROJECTILE");

				destroyProjectile ();
			}
		}
	}

	public bool isProjectileUsed ()
	{
		return projectileUsed;
	}

	public void setProjectileUsedState (bool state)
	{
		projectileUsed = state;
	}

	public void initializeProjectile ()
	{
		if (projectileInitialized) {
			return;
		}

		projectileInitialized = true;

		getProjectileRigidbody ();

		getFakeProjectileTrail ();

		getProjectileMeshes ();

		if (currentProjectileInfo.setProjectileMeshRotationToFireRotation && bulletMesh != null) {
			bulletMesh.transform.localEulerAngles = projectileLocalRotation;
		}

		enableOrDisableMainTrailRenderer (true);

		if (currentProjectileInfo.launchProjectile) {
			setUseGravityState (true);
		} else {
			//the bullet moves on the camera direction
			if (!currentProjectileInfo.useRayCastShoot) {
				if (currentProjectileInfo.useGravityOnLaunch) {
					setUseGravityState (true);
				}

				mainRigidbody.velocity = currentProjectileInfo.projectileSpeed * transform.forward;
			}
		}

		if (!projectileParticlesAssigned) {
			projectileParticlesAssigned = currentProjectileInfo.projectileParticles != null;
		}

		if (projectileParticlesAssigned) {
			GameObject projectileParticles = (GameObject)Instantiate (currentProjectileInfo.projectileParticles, transform.position, transform.rotation);

			projectileParticles.transform.SetParent (transform);
		}

		if (currentProjectileInfo.projectileWithAbility) {
			if (abilityFunctionNameAtStart != "") {
				SendMessage (abilityFunctionNameAtStart, SendMessageOptions.DontRequireReceiver);
			}
		}

		if (currentProjectileInfo.useRaycastCheckingOnRigidbody) {
			getProjectileCollider ();

			previousPosition = Vector3.zero; 

			minimumExtent = Mathf.Min (Mathf.Min (mainCollider.bounds.extents.x, mainCollider.bounds.extents.y), mainCollider.bounds.extents.z); 
			sqrMinimumExtent = minimumExtent * minimumExtent; 
		}
	}

	public void returnBullet (Vector3 direction, GameObject owner, bool ignoreProjectileOwner)
	{
		if (currentProjectileInfo.owner != null && !ignoreProjectileOwner) {
			Transform placeToShoot = applyDamage.getPlaceToShoot (currentProjectileInfo.owner);

			if (placeToShoot != null) {
				currentTarget = placeToShoot.gameObject;

				currentTargetTransform = currentTarget.transform;

				transform.LookAt (currentTargetTransform.position);
			}
		} else {
			transform.LookAt (direction);
		}

		//now the owner of the projectile is the player
		currentProjectileInfo.owner = owner;
		currentProjectileInfo.projectileSpeed = 15;

		enableOrDisableMainTrailRenderer (true);

		currentProjectileInfo.useRayCastShoot = false;

		getProjectileCollider ();

		mainCollider.enabled = true;

		projectilePaused = false;

		setProjectileParent (null);

		checkCollisionDetectionMode ();

		setKinematicState (false);

		mainRigidbody.velocity = currentProjectileInfo.projectileSpeed * transform.forward;
	}

	void setProjectileParent (Transform newParent)
	{
		transform.SetParent (newParent);
	}

	public void attachProjectileToSurface ()
	{
		bool surfaceDetected = false;

		if (Physics.Raycast (transform.position - 0.7f * transform.forward, transform.forward, out hit, currentProjectileInfo.scorchRayCastDistance, currentProjectileInfo.targetToDamageLayer)) {
			if (secondaryBulletMesh != null) {
				secondaryBulletMesh.SetActive (true);
			}

			surfaceDetected = true;

		}

		if (!surfaceDetected) {
			Ray ray = new Ray (transform.position, transform.forward);

			if (Physics.SphereCast (ray, 0.3f, out hit, 0.35f, currentProjectileInfo.targetToDamageLayer)) {
				surfaceDetected = true;

//				Debug.DrawLine (transform.position, hit.point + transform.forward * 6, Color.red, 5);
			}

			if (!surfaceDetected) {
				ray = new Ray (transform.position, -transform.up);

				if (Physics.SphereCast (ray, 0.3f, out hit, 0.35f, currentProjectileInfo.targetToDamageLayer)) {
					surfaceDetected = true;

//					Debug.DrawLine (transform.position, hit.point - transform.up * 6, Color.red, 5);
				}
			}

			if (!surfaceDetected) {
				ray = new Ray (transform.position, transform.up);

				if (Physics.SphereCast (ray, 0.3f, out hit, 0.35f, currentProjectileInfo.targetToDamageLayer)) {
					surfaceDetected = true;

//					Debug.DrawLine (transform.position, hit.point + transform.up * 6, Color.red, 5);
				}
			}
		}

		if (surfaceDetected) {

			bool attachedCorrectly = decalManager.placeProjectileInSurface (transform.rotation, transform, hit, objectToDamage, 
				                         currentProjectileInfo.adhereToLimbs, currentProjectileInfo.ignoreSetProjectilePositionOnImpact);

			if (!attachedCorrectly) {
				enableOrDisableProjectileCollider (true);
			} else {
				checkProjectilesParent ();

				enableOrDisableProjectileCollider (false);

				if (useEventOnAdhereToSurface) {
					eventOnAdhereToSurface.Invoke (objectToDamage);
				}
			}
		}
	}

	public void checkEventOnProjectileFiredExternally ()
	{
		if (useEventOnProjectileFiredExternally) {
			eventOnProjectileFiredExternally.Invoke ();
		}
	}

	public bool useEventOnAdhereToSurface;
	public eventParameters.eventToCallWithGameObject eventOnAdhereToSurface;

	public bool useEventOnProjectileFiredExternally;
	public UnityEvent eventOnProjectileFiredExternally;

	public void enableOrDisableProjectileCollider (bool state)
	{
		checkCollisionDetectionMode ();

		setKinematicState (!state);

		setUseGravityState (state);

		if (secondaryBulletMesh != null) {
			enableOrDisableSecondaryMeshCollider (state);
		}
	}

	void setUseGravityState (bool state)
	{
		mainRigidbody.useGravity = state;
	}

	public void checkProjectilesParent ()
	{
		projectileAttachedToSurface = true;

		Transform transformParent = transform.parent;

		if (transformParent != null) {
			mainHealthManagement = transformParent.GetComponent<healthManagement> ();
		}
	}

	public void setIgnoreParentDestroyedState (bool state)
	{
		ignoreParentDestroyed = state;
	}

	public void enableOrDisableSecondaryMeshCollider (bool state)
	{
		if (secondaryBulletMeshCollider != null) {
			secondaryBulletMeshCollider.enabled = state;
		}
	}

	public void enableOrDisableMainTrailRenderer (bool state)
	{
		getProjectileTrailRenderer ();

		if (mainTrailRendererAssigned) {
			mainTrailRenderer.enabled = state;
		}
	}

	public void projectimeImpactState (GameObject objectToDamage)
	{
		if (useEventOnImpact) {
			if (useLayerMaskImpact) {
				if ((1 << objectToDamage.layer & currentProjectileInfo.targetToDamageLayer.value) == 1 << objectToDamage.layer) {
					callEventOnImpact ();
				}
			} else {
				callEventOnImpact ();
			}
		}
	}

	public void callEventOnImpact ()
	{
		if (sendObjectDetectedOnImpactEvent && objectToDamage != null) {
			objectDetectedOnImpactEvent.Invoke (objectToDamage);
		}

		eventOnImpact.Invoke ();
	}

	public void checkCollisionDetectionMode ()
	{
		if (setContinuousSpeculativeIngame) {
			if (collisionDetectionChecked) {
				return;
			}

			if (mainRigidbody != null) {
				string[] collisionDetectionModeList = Enum.GetNames (typeof(CollisionDetectionMode));

				if (collisionDetectionModeList.Length > 3) {
					for (int i = 0; i < collisionDetectionModeList.Length; i++) {
						if (collisionDetectionModeList [i].Equals ("ContinuousSpeculative")) {
							CollisionDetectionMode currentMode = (CollisionDetectionMode)Enum.GetValues (typeof(CollisionDetectionMode)).GetValue (i);

							if (currentMode != 0) {
								mainRigidbody.collisionDetectionMode = currentMode;
							}

							collisionDetectionChecked = true;

							return;
						}
					}
				}
			}

			collisionDetectionChecked = true;
		}
	}

	public Rigidbody getProjectileRigibody ()
	{
		return mainRigidbody;
	}

	public void setNewProjectileDamage (float newValue)
	{
		currentProjectileInfo.projectileDamage = newValue;
	}

	public void setProjectileDamageMultiplier (float newValue)
	{
		currentProjectileInfo.projectileDamage *= newValue;
	}


	//INITIALIZE PROJECTILE FUNCTIONS
	public void getProjectileComponents (bool calledFromEditor)
	{
		getProjectileMapInformation ();

		getProjectileCollider ();

		getProjectileTrailRenderer ();

		getProjectileRigidbody ();

		getProjectileMeshes ();

		getProjectileBipManager ();

		getMainProjectileImpactSystem ();

		getFakeProjectileTrail ();

		if (calledFromEditor) {
			GKC_Utils.updateComponent (this);
		}
	}

	void getProjectileMeshes ()
	{
		if (projectileMeshesAssigned) {
			return;
		}

		int childCount = transform.childCount;

		if (childCount > 0) {
			if (bulletMesh == null) {
				bulletMesh = transform.GetChild (0).gameObject;
			}

			if (childCount > 1) {
				if (secondaryBulletMesh == null) {
					secondaryBulletMesh = transform.GetChild (1).gameObject;
				}

				if (secondaryBulletMesh != null) {
					if (secondaryBulletMeshCollider == null) {
						secondaryBulletMeshCollider = secondaryBulletMesh.GetComponentInChildren<MeshCollider> ();
					}
				}
			}
		}

		projectileMeshesAssigned = true;
	}

	void getProjectileCollider ()
	{
		if (!mainColliderAssigned) {
			mainColliderAssigned = mainCollider != null;

			if (!mainColliderAssigned) {
				mainCollider = GetComponent<Collider> ();

				mainColliderAssigned = mainCollider != null;
			}

			if (!mainColliderAssigned) {
				mainCollider = GetComponentInChildren<Collider> ();

				mainColliderAssigned = mainCollider != null;
			}
		}
	}

	void getProjectileRigidbody ()
	{
		if (!mainRigidbodyAssigned) {
			mainRigidbodyAssigned = mainRigidbody != null;

			if (!mainRigidbodyAssigned) {
				mainRigidbody = GetComponent<Rigidbody> ();

				mainRigidbodyAssigned = mainRigidbody != null;
			}
		}
	}

	void getProjectileMapInformation ()
	{
		if (!mainMapObjectInformationAssigned) {
			mainMapObjectInformationAssigned = mainMapObjectInformation != null;
			
			if (!mainMapObjectInformationAssigned) {
				mainMapObjectInformation = GetComponent<mapObjectInformation> ();

				mainMapObjectInformationAssigned = mainMapObjectInformation != null;
			}
		}
	}

	void getProjectileTrailRenderer ()
	{
		if (!mainTrailRendererAssigned) {
			mainTrailRendererAssigned = mainTrailRenderer != null;

			if (!mainTrailRendererAssigned) {
				mainTrailRenderer = GetComponent<TrailRenderer> ();

				mainTrailRendererAssigned = mainTrailRenderer != null;
			}
		}
	}

	void getProjectileBipManager ()
	{
		if (!audioClipBipManagerAssigned) {
			audioClipBipManagerAssigned = audioClipBipManager != null;

			if (!audioClipBipManagerAssigned) {
				audioClipBipManager = GetComponentInChildren<audioClipBip> ();

				audioClipBipManagerAssigned = audioClipBipManager != null;
			}
		}
	}

	void getMainProjectileImpactSystem ()
	{
		if (!mainProjectileImpactSystemAssigned) {
			mainProjectileImpactSystemAssigned = mainProjectileImpactSystem != null;
		
			if (!mainProjectileImpactSystemAssigned) {
				mainProjectileImpactSystem = GetComponentInChildren<projectileImpactSystem> ();

				mainProjectileImpactSystemAssigned = mainProjectileImpactSystem != null;
			}
		}
	}

	void getFakeProjectileTrail ()
	{
		if (!fakeProjectileTrailAssigned) {
			fakeProjectileTrailAssigned = fakeProjectileTrail != null;

			if (!fakeProjectileTrailAssigned) {
				fakeProjectileTrail = GetComponent<fakeProjectileTrailSystem> ();

				fakeProjectileTrailAssigned = fakeProjectileTrail != null;
			}
		}
	}

	public virtual void resetProjectile ()
	{
		if (bulletMesh != null && disableProjectileMeshOnImpact) {
			if (!bulletMesh.activeSelf) {
				bulletMesh.SetActive (true);
			}
		}

		if (secondaryBulletMesh != null) {
			if (secondaryBulletMesh.activeSelf) {
				secondaryBulletMesh.SetActive (false);
			}
		}

		projectileInitialized = false;

		currentProjectileInfo = null;

		objectToDamage = null;

		ignoreProjectileWithAbilityState = false;

		hit = new RaycastHit ();

		setProjectileUsedState (false);

		projectilePaused = false;

		explosionActivated = false;

		rotationSpeed = 10;

		projectileCaptured = false;

		impactParticlesCreated = false;

		mainHealthManagement = null;

		projectileAttachedToSurface = false;

		currentNumberOfImpacts = 0;

		if (listOfGameObjectImpacts.Count > 0) {
			listOfGameObjectImpacts.Clear ();
		}

		if (listOfColliderImpacts.Count > 0) {
			listOfColliderImpacts.Clear ();
		}

		orbitingTargetTime = 0;

		if (checkSurfaceWithDelayActive) {
			stopCheckSurfaceWithDelay ();
		}

		loop = 0;

		collisionDetectionChecked = false;

		pauseCheckTriggerActive = false;

		projectileRemovedPreviously = false;

		previousPosition = Vector3.zero;

		currentTargetTransform = null;

		if (transform.parent != null) {
			setProjectileParent (null);
		}

		if (mainTrailRendererAssigned) {
			mainTrailRenderer.Clear ();
		}

		impactParticlesAssigned = false;

		projectileParticlesAssigned = false;
	
		projectileCanBreakThroughObjectResult = false;

		projectileWaitingToExplodeActive = false;

		currentTarget = null;
	}

	public void checkToResetProjectile ()
	{
		if (projectileRemovedPreviously) {
			resetProjectile ();

			projectileRemovedPreviously = false;

			createMapIconInfo ();
		} else {
			projectileCaptured = false;
		}
	}

	public virtual void setProjectileSpecialActionActiveState (bool state)
	{

	}
}