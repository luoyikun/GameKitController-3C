﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setMapOrientationSystem : MonoBehaviour
{
	[Header ("Main Settings")]
	[Space]

	public mapCameraMovement mapCameraMovementType;

	public Transform mapOrientationTransform;

	public enum mapCameraMovement
	{
		XY,
		XZ,
		YZ
	}

	GameObject currentPlayer;


	public void setMapOrientation ()
	{
		if (currentPlayer != null) {
			playerComponentsManager mainPlayerComponentsManager = currentPlayer.GetComponent<playerComponentsManager> ();

			mapSystem currentMapSystem = mainPlayerComponentsManager.getMapSystem ();

			if (currentMapSystem != null) {
				currentMapSystem.setMapOrientation ((int)mapCameraMovementType, mapOrientationTransform);
			}
		}
	}

	public void setCurrentPlayer (GameObject newPlayer)
	{
		currentPlayer = newPlayer;
	}
}
