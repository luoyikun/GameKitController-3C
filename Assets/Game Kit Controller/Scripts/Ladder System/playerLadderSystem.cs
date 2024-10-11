using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class playerLadderSystem : MonoBehaviour
{
	[Header ("Main Settings")]
	[Space]

	public float ladderMoovementSpeed = 5;
	public float ladderVerticalMovementAmount = 0.3f;
	public float ladderHorizontalMovementAmount = 0.1f;

	public float minAngleToInverseDirection = 100;

	public bool useAlwaysHorizontalMovementOnLadder;

	public bool useAlwaysLocalMovementDirection;

	public float minAngleVerticalDirection = 60;
	public float maxAngleVerticalDirection = 120;

	public LayerMask layerToCheckLadderEnd;

	public string climbLadderFootStepStateName = "Climb Ladders";

	[Space]
	[Header ("Jump Settings")]
	[Space]

	public bool jumpOnThirdPersonEnabled;

	public bool jumpOnFirstPersonEnabled;

	public Vector3 impulseOnJump;

	public Vector3 impulseOnJump2_5d = new Vector3 (0, 15, -30);

	public float jumpRotationSpeedThirdPerson = 1;
	public float jumpRotationSpeedFirstPerson = 0.5f;

	[Space]
	[Header ("Debug")]
	[Space]

	public bool showDebugPrint;

	public bool ladderLocated;

	public bool ladderFoundFirstPerson;

	public bool ladderFoundThirdPerson;

	public bool ladderEndDetected;
	public bool ladderStartDetected;

	public int movementDirection;
	public float ladderVerticalInput;
	public float ladderHorizontalInput;
	public float ladderAngle;
	public float ladderSignedAngle;

	public float currentVerticalInput;
	public float currentHorizontalInput;

	public Vector3 ladderMovementDirection;

	public bool movingOnLadder;
	public bool movingOnLadderPreviously;

	public Transform ladderDirectionTransform;
	public Transform ladderRaycastDirectionTransform;

	[Space]
	[Header ("Events Settings")]
	[Space]

	public bool useEventsOnThirdPerson;
	public UnityEvent eventOnLocatedLadderOnThirdPerson;
	public UnityEvent eventOnRemovedLadderOnThirdPerson;

	[Space]

	public bool useEventsOnJump;

	public UnityEvent eventOnJump;

	[Space]
	[Header ("Components")]
	[Space]

	public playerController playerControllerManager;
	public playerCamera playerCameraManager;
	public Transform playerCameraTransform;
	public Transform playerPivotTransform;
	public Rigidbody mainRigidbody;

	bool useLadderHorizontalMovement;
	bool moveInLadderCenter;
	bool useLocalMovementDirection;

	ladderSystem currentLadderSystem;
	ladderSystem previousLadderSystem;

	float verticalInput;
	float horizontalInput;

	Vector3 currentPlayerPosition;

	Vector3 currentPositionOffset;
	Vector3 lastPosition;

	Coroutine jumpCoroutine;


	void FixedUpdate ()
	{
		if (ladderFoundFirstPerson) {

			verticalInput = playerControllerManager.getVerticalInput ();
			horizontalInput = playerControllerManager.getHorizontalInput ();

			movementDirection = 1;

			ladderAngle = Vector3.Angle (playerCameraTransform.forward, ladderDirectionTransform.forward);

			if (ladderAngle > minAngleToInverseDirection) {
				movementDirection = (-1);
			}

			if (useLocalMovementDirection || useAlwaysLocalMovementDirection) {

				ladderSignedAngle = Vector3.SignedAngle (playerCameraTransform.forward, ladderDirectionTransform.forward, playerCameraTransform.up);

				if (ladderAngle < minAngleVerticalDirection || ladderAngle > maxAngleVerticalDirection) {
					currentVerticalInput = verticalInput;
					currentHorizontalInput = horizontalInput;
				} else {
					if (ladderSignedAngle < 0) {
						movementDirection = (-1);
					} else {
						movementDirection = 1;
					}

					currentVerticalInput = horizontalInput;
					currentHorizontalInput = -verticalInput;
				}
			} else {
				currentVerticalInput = verticalInput;
				currentHorizontalInput = horizontalInput;
			}
				
			ladderVerticalInput = currentVerticalInput * movementDirection;

			ladderMovementDirection = Vector3.zero;

			currentPlayerPosition = mainRigidbody.position;

			if (moveInLadderCenter) {
				if (useLadderHorizontalMovement || useAlwaysHorizontalMovementOnLadder) {
					if (Mathf.Abs (currentHorizontalInput) < 0.01f) {
						currentPlayerPosition = new Vector3 (ladderDirectionTransform.position.x, mainRigidbody.position.y, ladderDirectionTransform.position.z);
					}
				}
			}

			ladderMovementDirection += currentPlayerPosition + ladderDirectionTransform.up * (ladderVerticalMovementAmount * ladderVerticalInput);

			ladderEndDetected = !Physics.Raycast (mainRigidbody.position, ladderRaycastDirectionTransform.forward, 2, layerToCheckLadderEnd);

			ladderStartDetected = Physics.Raycast (mainRigidbody.position + playerCameraTransform.up * 0.1f, 
				-playerCameraTransform.up, 0.13f, layerToCheckLadderEnd);

			if (ladderEndDetected || (ladderStartDetected && ladderVerticalInput < 0)) {
				ladderMovementDirection = currentPlayerPosition + ladderRaycastDirectionTransform.forward * ladderVerticalInput;
			}

			if (useLadderHorizontalMovement || useAlwaysHorizontalMovementOnLadder) {

				ladderHorizontalInput = currentHorizontalInput * movementDirection;

				ladderMovementDirection += ladderDirectionTransform.right * (ladderHorizontalInput * ladderHorizontalMovementAmount);
			}

			mainRigidbody.position = Vector3.MoveTowards (mainRigidbody.position, ladderMovementDirection, Time.deltaTime * ladderMoovementSpeed);

			currentPositionOffset = mainRigidbody.position - lastPosition;
			if (currentPositionOffset.sqrMagnitude > 0.0001f) {
				lastPosition = mainRigidbody.position;
				movingOnLadder = true;
			} else {
				movingOnLadder = false;
			}

			if (movingOnLadder != movingOnLadderPreviously) {
				movingOnLadderPreviously = movingOnLadder;

				if (movingOnLadder) {
					playerControllerManager.stepManager.setFootStepState (climbLadderFootStepStateName);
				} else {
					playerControllerManager.stepManager.setDefaultGroundFootStepState ();
				}
			}
		}
	}

	public void setLadderFoundState (bool state, ladderSystem newLadderSystem)
	{
		if (playerControllerManager.isIgnoreExternalActionsActiveState () && state) {
			return;
		}

		bool cancelCheckLadderStatesResult = false;

		if (!playerControllerManager.isPlayerOnFirstPerson ()) {
			if (state) {
				checkLadderEventsOnThirdPerson (true);

				ladderFoundThirdPerson = true;

				cancelCheckLadderStatesResult = true;
			} else {
				checkLadderEventsOnThirdPerson (false);

				ladderFoundThirdPerson = false;
			}
		}

		if (previousLadderSystem != currentLadderSystem) {

			previousLadderSystem = currentLadderSystem;
		}

		ladderLocated = state;

		currentLadderSystem = newLadderSystem;

		if (cancelCheckLadderStatesResult) {
			return;
		}

		ladderFoundFirstPerson = state;

		playerControllerManager.setGravityForcePuase (ladderFoundFirstPerson);

		playerControllerManager.setPhysicMaterialAssigmentPausedState (ladderFoundFirstPerson);

		playerCameraManager.setCameraPositionMouseWheelEnabledState (!ladderFoundFirstPerson);

		playerControllerManager.setUpdate2_5dClampedPositionPausedState (ladderFoundFirstPerson);

		if (ladderFoundFirstPerson) {
			playerControllerManager.setRigidbodyVelocityToZero ();

			playerControllerManager.setOnGroundState (false);

			playerControllerManager.setZeroFrictionMaterial ();

			playerControllerManager.headBobManager.stopAllHeadbobMovements ();
			playerControllerManager.headBobManager.playOrPauseHeadBob (false);

			playerControllerManager.stepManager.setFootStepState (climbLadderFootStepStateName);

			playerControllerManager.setPauseAllPlayerDownForces (true);

			playerCameraManager.enableOrDisableChangeCameraView (false);

			playerControllerManager.setLadderFoundState (true);

			playerControllerManager.setIgnoreExternalActionsActiveState (true);
		} else {
			playerControllerManager.headBobManager.pauseHeadBodWithDelay (0.5f);
			playerControllerManager.headBobManager.playOrPauseHeadBob (true);

			playerControllerManager.setPauseAllPlayerDownForces (false);

			playerCameraManager.setOriginalchangeCameraViewEnabledValue ();

			playerControllerManager.setLadderFoundState (false);

			playerControllerManager.stepManager.setDefaultGroundFootStepState ();

			playerControllerManager.setIgnoreExternalActionsActiveState (false);
		}
	}

	public void setLadderDirectionTransform (Transform newLadderDirectionTransform, Transform newLadderRaycastDirectionTransform)
	{
		ladderDirectionTransform = newLadderDirectionTransform;
		ladderRaycastDirectionTransform = newLadderRaycastDirectionTransform;
	}

	public void setLadderHorizontalMovementState (bool state)
	{
		useLadderHorizontalMovement = state;
	}

	public void setMoveInLadderCenterState (bool state)
	{
		moveInLadderCenter = state;
	}

	public bool isLadderFound ()
	{
		return ladderFoundFirstPerson;
	}

	public void setUseLocalMovementDirectionState (bool state)
	{
		useLocalMovementDirection = state;
	}

	public void checkLadderEventsOnThirdPerson (bool state)
	{
		if (useEventsOnThirdPerson) {
			if (state) {
				eventOnLocatedLadderOnThirdPerson.Invoke ();
			} else {
				eventOnRemovedLadderOnThirdPerson.Invoke ();
			}
		}
	}

	public void rotateCharacterOnJump ()
	{
		stopRotateCharacterOnJumpCoroutine ();

		jumpCoroutine = StartCoroutine (rotateCharacterOnJumpCoroutine ());
	}

	void stopRotateCharacterOnJumpCoroutine ()
	{
		if (jumpCoroutine != null) {
			StopCoroutine (jumpCoroutine);
		}
	}

	public IEnumerator rotateCharacterOnJumpCoroutine ()
	{
		bool targetReached = false;

		float movementTimer = 0;

		float t = 0;

		float duration = 0;

		bool isFirstPersonActive = playerControllerManager.isPlayerOnFirstPerson ();

		if (isFirstPersonActive) {
			duration = 0.5f / jumpRotationSpeedFirstPerson;
		} else {
			duration = 0.5f / jumpRotationSpeedThirdPerson;
		}

		float angleDifference = 0;

		bool isFullBodyAwarenessActive = playerControllerManager.isFullBodyAwarenessActive ();

		Transform objectToRotate = playerControllerManager.transform;

		if (isFirstPersonActive || isFullBodyAwarenessActive) {
			objectToRotate = playerCameraTransform;
		}

		Quaternion targetRotation = Quaternion.LookRotation (-objectToRotate.forward, objectToRotate.up);

		while (!targetReached) {
			t += Time.deltaTime / duration; 

			objectToRotate.rotation = Quaternion.Slerp (objectToRotate.rotation, targetRotation, t);

			angleDifference = Quaternion.Angle (objectToRotate.rotation, targetRotation);

			movementTimer += Time.deltaTime;

			if (angleDifference < 0.2f || movementTimer > (duration + 1)) {
				targetReached = true;
			}
			yield return null;
		}
	}

	public void checkEventOnJump ()
	{
		if (useEventsOnJump) {
			eventOnJump.Invoke ();

			if (currentLadderSystem != null) {
				currentLadderSystem.checkEventOnJump ();
			}
		}
	}

	//INPUT FUNCTIONS
	public void inputJumpFromLadder ()
	{
		if (!ladderLocated) {
			return;
		}

		activeJumpOnLadder ();
	}

	public void activeJumpOnLadder ()
	{
		bool canJumpResult = false;

		if (ladderFoundThirdPerson && jumpOnThirdPersonEnabled) {
			if (currentLadderSystem.isJumpOnThirdPersonEnabled ()) {
				canJumpResult = true;
			}
		}

		if (ladderFoundFirstPerson && jumpOnFirstPersonEnabled) {
			if (currentLadderSystem.isJumpOnFirstPersonEnabled ()) {
				canJumpResult = true;
			}
		}

		if (canJumpResult) {
			if (showDebugPrint) {
				print ("activate jump on ladder");
			}

			Vector3 newImpulseOnJump = impulseOnJump;

			if (!playerControllerManager.isPlayerMovingOn3dWorld ()) {
				newImpulseOnJump = impulseOnJump2_5d;
			} 

			currentLadderSystem.checkEventOnJump ();

			currentLadderSystem.checkTriggerInfo (playerControllerManager.getMainCollider (), false);

			playerControllerManager.stopAllActionsOnActionSystem ();

			Vector3 totalForce = newImpulseOnJump.y * playerControllerManager.transform.up + newImpulseOnJump.z * playerControllerManager.transform.forward;

			playerControllerManager.resetLastMoveInputOnJumpValue ();

			playerControllerManager.useJumpPlatform (totalForce, ForceMode.Impulse);

			rotateCharacterOnJump ();

			if (!playerControllerManager.isPlayerMovingOn3dWorld ()) {
				playerControllerManager.resetPlayerControllerInput ();

				playerControllerManager.setMoveInputPausedWithDuration (true, 0.2f);

				playerControllerManager.setCurrentVelocityValue (Vector3.zero);
			}
		}
	}
}
