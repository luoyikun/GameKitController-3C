﻿using UnityEngine;
using System.Collections;
using GameKitController.Audio;
using UnityEngine.Events;

public class quickTravelStationSystem : MonoBehaviour
{
	[Header ("Main Settings")]
	[Space]

	public Transform quickTravelTransform;
	public LayerMask layer;
	public float maxDistanceToCheck = 4;

	public string animationName;
	public AudioClip enterAudioSound;
	public AudioElement enterAudioElement;
	public bool activateAtStart;

	public bool setGravityDirection;

	[Space]
	[Header ("Other Settings")]
	[Space]

	public string mainMapCreatorManagerName = "Map Creator";

	[Space]
	[Header ("Events Settings")]
	[Space]

	public bool callEventOnTeleport;
	public UnityEvent eventOnTeleport;
	public bool callEventOnEveryTeleport;

    [Space]
    [Header ("Debug")]
    [Space]

    public bool stationActivated;

    [Space]
	[Header ("Components")]
	[Space]

	public setGravity setGravityManager;
	public AudioSource audioSource;
	public Animation stationAnimation;
	public mapObjectInformation mapObjectInformationManager;
	public mapCreator mapCreatorManager;

	bool eventCalled;

	RaycastHit hit;


	private void InitializeAudioElements ()
	{
		if (audioSource == null) {
			audioSource = GetComponent<AudioSource> ();
		}

		if (audioSource != null) {
			enterAudioElement.audioSource = audioSource;
		}

		if (enterAudioSound != null) {
			enterAudioElement.clip = enterAudioSound;
		}
	}

	void Start ()
	{
		if (stationAnimation == null) {
			stationAnimation = GetComponent<Animation> ();
		}

		InitializeAudioElements ();

		if (mapObjectInformationManager == null) {
			mapObjectInformationManager = GetComponent<mapObjectInformation> ();
		}

		if (activateAtStart) {
			activateStation ();
		}

		bool mapCreatorManagerAssigned = (mapCreatorManager != null);

		if (!mapCreatorManagerAssigned) {
			mapCreatorManager = mapCreator.Instance;

			mapCreatorManagerAssigned = mapCreatorManager != null;
		}

		if (!mapCreatorManagerAssigned) {
			GKC_Utils.instantiateMainManagerOnSceneWithTypeOnApplicationPlaying (mapCreator.getMainManagerName (), typeof(mapCreator), true);

			mapCreatorManager = mapCreator.Instance;

			mapCreatorManagerAssigned = (mapCreatorManager != null);
		}

		if (!mapCreatorManagerAssigned) {
			mapCreatorManager = FindObjectOfType<mapCreator> ();

			mapCreatorManagerAssigned = mapCreatorManager != null;
		} 
	}

	public void travelToThisStation (Transform currentPlayer)
	{
		Vector3 positionToTravel = quickTravelTransform.position;

		if (Physics.Raycast (quickTravelTransform.position, -transform.up, out hit, maxDistanceToCheck, layer)) {
			positionToTravel = hit.point + transform.up * 0.3f;
		}

		currentPlayer.position = positionToTravel;
		currentPlayer.rotation = quickTravelTransform.rotation;

		if (mapCreatorManager != null) {
			mapCreatorManager.changeCurrentBuilding (mapObjectInformationManager.getBuildingIndex (), currentPlayer.gameObject);
		}

		if (setGravityDirection && setGravityManager != null) {
			Collider currentCollider = currentPlayer.GetComponent<Collider> ();

			if (currentCollider != null) {
				setGravityManager.checkTriggerType (currentCollider, true);
			}
		}

		if (callEventOnTeleport) {
			if (!eventCalled || callEventOnEveryTeleport) {
				eventOnTeleport.Invoke ();

				eventCalled = true;
			}
		}
	}

	public void OnTriggerEnter (Collider col)
	{
		if (!stationActivated && col.CompareTag ("Player")) {
			AudioPlayer.PlayOneShot (enterAudioElement, gameObject);

			activateStation ();
		}
	}

	public void activateStation ()
	{
		if (stationAnimation != null && animationName != "") {
			stationAnimation [animationName].speed = 1;
			stationAnimation.Play (animationName);
		}

		mapObjectInformationManager.createMapIconInfo ();

		stationActivated = true;
	}
}