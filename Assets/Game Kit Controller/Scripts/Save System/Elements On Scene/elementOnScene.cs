﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class elementOnScene : MonoBehaviour
{
	[Header ("Main Settings")]
	[Space]

	public bool saveElementEnabled = true;

	public int elementScene;
	public int elementID;

	public bool elementActiveState = true;

	[Space]
	[Header ("Transform Settings")]
	[Space]

	public bool savePositionValues;

	public bool saveRotationValues;

	[Space]
	[Space]

	public bool useCustomTransform;

	public Transform customTransform;

	[Space]
	[Header ("Element Prefab Settings")]
	[Space]

	public int elementPrefabID = -1;

	[Space]
	[Header ("Element Stats Settings")]
	[Space]

	public bool useStats;

	public float loadStatsDelay;

	public List<statInfo> statInfoList = new List<statInfo> ();

	[Space]
	[Header ("Events Settings")]
	[Space]

	public bool useEventOnObjectActive;
	public UnityEvent eventOnObjectActive;

	public bool useEventOnObjectInactive = true;
	public UnityEvent eventOnObjectInactive;

	public bool useDelayForEvent;
	public float delayForEvent;

	[Space]
	[Space]

	public bool useEventObjectObjectActiveWithoutDelay;
	public UnityEvent eventObjectObjectActiveWithoutDelay;

	public bool useEventObjectObjectInactiveWithoutDelay;
	public UnityEvent eventObjectObjectInactiveWithoutDelay;

	[Space]
	[Header ("Debug")]
	[Space]

	public bool showDebugPrint;
	public bool useElementPrefabID;
	public bool objectOriginallyOnScene;

	Vector3 newPosition;
	Vector3 newRotation;

	string currentStatNameToCheck;

	elementOnSceneManager mainElementOnSceneManager;

	bool mainElementOnSceneManagerLocated;


	public void setElementScene (int newValue)
	{
		elementScene = newValue;
	}

	public void setElementID (int newValue)
	{
		elementID = newValue;
	}

	public void setNewPositionValues (Vector3 newValue)
	{
		newPosition = newValue;
	}

	public void setNewRotationValues (Vector3 newValue)
	{
		newRotation = newValue;
	}

	public void setUseEventOnObjectActiveState (bool state)
	{
		useEventOnObjectActive = state;
	}

	public void setUseEventOnObjectInactive (bool state)
	{
		useEventOnObjectInactive = state;
	}

	public void setElementActiveState (bool state)
	{
		elementActiveState = state;

		if (showDebugPrint) {
			print ("\n");
			print ("SETTING ELEMENT STATE ON " + gameObject.name + " as " + state);

			print ("\n");
		}
	}

	public void setElementPrefabIDValue (int newValue)
	{
		elementPrefabID = newValue;
	}

	public void setNewInstantiatedElementOnSceneManagerIngame ()
	{
		if (!saveElementEnabled) {
			return;
		}
		
		setElementActiveState (true);

		if (!mainElementOnSceneManagerLocated) {
			getMainElementOnSceneManager ();
		}

		if (mainElementOnSceneManagerLocated) {
			mainElementOnSceneManager.setNewInstantiatedElementOnSceneManagerIngame (this);
		}
	}

	public void setElementActiveStateToMainElementOnSceneManager (bool state)
	{
		if (!saveElementEnabled) {
			return;
		}
		
		setElementActiveState (state);

		if (!mainElementOnSceneManagerLocated) {
			getMainElementOnSceneManager ();
		}

		if (mainElementOnSceneManagerLocated) {
			if (objectOriginallyOnScene) {
				mainElementOnSceneManager.setTemporalElementActiveState (elementID, elementScene, elementActiveState);
			} else {
				mainElementOnSceneManager.removeElementFromSceneList (this);

				Destroy (this);
			}
		}
	}

	public void setNewInstantiatedElementOnSceneManagerIngameWithInfo ()
	{
		if (!mainElementOnSceneManagerLocated) {
			getMainElementOnSceneManager ();
		}

		if (mainElementOnSceneManagerLocated) {
			mainElementOnSceneManager.setNewInstantiatedElementOnSceneManagerIngameWithInfo (this);
		}
	}

	public void addNewElementOnSceneManager ()
	{
		if (!saveElementEnabled) {
			return;
		}

		if (!mainElementOnSceneManagerLocated) {
			getMainElementOnSceneManager ();
		}

		if (mainElementOnSceneManagerLocated) {
			mainElementOnSceneManager.addNewElementOnScene (this);
		}

	}

	public void checkStateOnLoad ()
	{
		if (!saveElementEnabled) {
			return;
		}

		if (useDelayForEvent) {
			activateStateOnLoadWithDelay ();
		} else {
			activateStateOnLoad ();
		}

		checkActivateStateOnLoadWithoutDelay ();
	}

	void checkActivateStateOnLoadWithoutDelay ()
	{
		if (saveElementEnabled) {
			if (elementActiveState) {
				if (useEventObjectObjectActiveWithoutDelay) {
					eventObjectObjectActiveWithoutDelay.Invoke ();
				}
			} else {
				if (useEventObjectObjectInactiveWithoutDelay) {
					eventObjectObjectInactiveWithoutDelay.Invoke ();
				}
			}
		}
	}

	public void activateStateOnLoadWithDelay ()
	{
		StartCoroutine (activateStateOnLoadCoroutine ());
	}

	IEnumerator activateStateOnLoadCoroutine ()
	{
		WaitForSeconds delay = new WaitForSeconds (delayForEvent);

		yield return delay;
	
		activateStateOnLoad ();
	}

	public void activateStateOnLoad ()
	{
		if (saveElementEnabled) {
			if (elementActiveState) {
				if (useEventOnObjectActive) {
					eventOnObjectActive.Invoke ();
				}

				if (savePositionValues) {
					if (useCustomTransform) {
						customTransform.position = newPosition;
					} else {
						transform.position = newPosition;
					}
				}

				if (saveRotationValues) {
					if (useCustomTransform) {
						customTransform.eulerAngles = newRotation;
					} else {
						transform.eulerAngles = newRotation;
					}
				}
			} else {
				if (useEventOnObjectInactive) {
					eventOnObjectInactive.Invoke ();
				}
			}
		}
	}

	public Vector3 getElementPosition ()
	{
		if (useCustomTransform) {
			return customTransform.position;
		} else {
			return transform.position;
		}
	}

	public Vector3 getElementRotation ()
	{
		if (useCustomTransform) {
			return customTransform.eulerAngles;
		} else {
			return transform.eulerAngles;
		}
	}

	public bool isSaveElementEnabled ()
	{
		return saveElementEnabled;
	}

	public void setSaveElementEnabledState (bool state)
	{
		saveElementEnabled = state;
	}

	public void setObjectOriginallyOnSceneState (bool state)
	{
		objectOriginallyOnScene = state;
	}


	//STATS FUNCTIONS
	public void setStatsSearchingByInfo (int currentElementScene, int currentElementID)
	{
		if (!mainElementOnSceneManagerLocated) {
			getMainElementOnSceneManager ();
		}

		if (mainElementOnSceneManagerLocated) {
			mainElementOnSceneManager.setStatsSearchingByInfo (currentElementScene, currentElementID, this);
		}
	}

	public void checkStatsStateOnLoad ()
	{
		if (!saveElementEnabled) {
			return;
		}

		if (useStats) {
			if (loadStatsDelay > 0) {
				setStatsOnLoadWithDelay ();
			} else {
				setStatsOnLoad ();
			}
		}
	}

	public void setStatsOnLoadWithDelay ()
	{
		StartCoroutine (setStatsOnLoadCoroutine ());
	}

	IEnumerator setStatsOnLoadCoroutine ()
	{
		WaitForSeconds delay = new WaitForSeconds (loadStatsDelay);

		yield return delay;

		setStatsOnLoad ();
	}

	public void setStatsOnLoad ()
	{
		if (saveElementEnabled) {
			if (elementActiveState) {
				if (useStats) {
					if (showDebugPrint) {
						print ("\n");
						print ("SETTING STATS INFO ON " + gameObject.name);

						print ("\n");
					}

					for (int i = 0; i < statInfoList.Count; i++) {
						statInfo currentStatInfo = statInfoList [i];

						if (currentStatInfo.statIsAmount) {
							currentStatInfo.eventToInitializeFloatStat.Invoke (currentStatInfo.currentFloatValue);
						} else {
							currentStatInfo.eventToInitializeBoolStat.Invoke (currentStatInfo.currentBoolState);
						}
					}
				}
			}
		}
	}

	public void setCurrentStatNameToSave (string newValue)
	{
		currentStatNameToCheck = newValue;
	}

	public void setCurrentFloatValueToSave (float newValue)
	{
		if (saveElementEnabled) {
			if (useStats) {
				for (int i = 0; i < statInfoList.Count; i++) {
					statInfo currentStatInfo = statInfoList [i];

					if (currentStatInfo.Name.Equals (currentStatNameToCheck)) {
						currentStatInfo.currentFloatValue = newValue;

						return;
					}
				}
			}
		}
	}

	public void setCurrentBoolValueToSave (bool newValue)
	{
		if (saveElementEnabled) {
			if (useStats) {
				for (int i = 0; i < statInfoList.Count; i++) {
					statInfo currentStatInfo = statInfoList [i];

					if (currentStatInfo.Name.Equals (currentStatNameToCheck)) {
						currentStatInfo.currentBoolState = newValue;

						return;
					}
				}
			}
		}
	}

	public void checkEventOnStatsSave ()
	{
		if (saveElementEnabled) {
			if (useStats) {
				for (int i = 0; i < statInfoList.Count; i++) {
					statInfo currentStatInfo = statInfoList [i];

					currentStatNameToCheck = currentStatInfo.Name;

					if (currentStatInfo.statIsAmount) {
						currentStatInfo.eventToGetFloatStat.Invoke ();
					} else {
						currentStatInfo.eventToGetBoolStat.Invoke ();
					}
				}
			}
		}
	}

	public void setUseElementPrefabIDState (bool state)
	{
		useElementPrefabID = state;
	}

	public void addSingleElementOnSceneToManager ()
	{
		if (!mainElementOnSceneManagerLocated) {
			getMainElementOnSceneManager ();
		}

		if (mainElementOnSceneManagerLocated) {
			mainElementOnSceneManager.addSingleElementOnSceneToManager (this);
		}
	}

	void getMainElementOnSceneManager ()
	{
		mainElementOnSceneManagerLocated = mainElementOnSceneManager != null;

		if (!mainElementOnSceneManagerLocated) {
			mainElementOnSceneManager = elementOnSceneManager.Instance;

			mainElementOnSceneManagerLocated = mainElementOnSceneManager != null;
		}

		if (!mainElementOnSceneManagerLocated) {
			GKC_Utils.instantiateMainManagerOnSceneWithTypeOnApplicationPlaying (elementOnSceneManager.getMainManagerName (), typeof(elementOnSceneManager), true);

			mainElementOnSceneManager = elementOnSceneManager.Instance;

			mainElementOnSceneManagerLocated = mainElementOnSceneManager != null;
		}

		if (!mainElementOnSceneManagerLocated) {

			mainElementOnSceneManager = FindObjectOfType<elementOnSceneManager> ();

			mainElementOnSceneManagerLocated = mainElementOnSceneManager != null;
		} 
	}

	[System.Serializable]
	public class statInfo
	{
		public string Name;

		public bool statIsAmount = true;

		[Space]

		public float currentFloatValue;

		public eventParameters.eventToCallWithAmount eventToInitializeFloatStat;

		public UnityEvent eventToGetFloatStat;

		[Space]
		[Space]

		public bool currentBoolState;

		public eventParameters.eventToCallWithBool eventToInitializeBoolStat;

		public UnityEvent eventToGetBoolStat;
	}
}
