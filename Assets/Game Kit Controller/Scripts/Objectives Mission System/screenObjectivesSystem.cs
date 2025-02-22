﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class screenObjectivesSystem : MonoBehaviour
{
	public bool showObjectivesActive = true;

	public List<objectiveIconElement> objectiveIconList = new List<objectiveIconElement> ();
	public List<objectiveInfo> objectiveList = new List<objectiveInfo> ();

	public List<playerScreenObjectivesSystem> playerScreenObjectivesList = new List<playerScreenObjectivesSystem> ();

	public string mainMapCreatorManagerName = "Map Creator";

	mapCreator mapCreatorManager;

	int currentID = 0;



	public const string mainManagerName = "Screen Objectives Manager";

	public static string getMainManagerName ()
	{
		return mainManagerName;
	}

	private static screenObjectivesSystem _screenObjectivesSystemInstance;

	public static screenObjectivesSystem Instance { get { return _screenObjectivesSystemInstance; } }

	bool instanceInitialized;

	public void getComponentInstance ()
	{
		if (instanceInitialized) {
//			print ("already initialized manager");

			return;
		}

		if (_screenObjectivesSystemInstance != null && _screenObjectivesSystemInstance != this) {
			Destroy (this.gameObject);

			return;
		} 

		_screenObjectivesSystemInstance = this;

		instanceInitialized = true;
	}

	void Awake ()
	{
		getComponentInstance ();
		
		if (!showObjectivesActive) {
			return;
		}

		checkGetMapCreatorManager ();
	}

	public void addNewPlayer (playerScreenObjectivesSystem newPlayer)
	{
		if (!showObjectivesActive) {
			return;
		}

		playerScreenObjectivesList.Add (newPlayer);


		int objectiveListCount = objectiveList.Count;
	
		if (objectiveListCount > 0) {

			for (int i = 0; i < objectiveListCount; i++) {
				newPlayer.addElementToList (objectiveList [i], objectiveList [i].iconPrefab, objectiveList [i].ID);
			}
		}
	}

	//get the renderer parts of the target to set its colors with the objective color, to see easily the target to reach
	public void addElementToScreenObjectiveList (GameObject obj, bool useCloseDistance, float radiusDistance, bool showOffScreen, bool showDistanceInfo,
	                                             bool showDistanceOffScreen, string objectiveIconName, bool useCustomObjectiveColor, Color objectiveColor,
	                                             bool removeCustomObjectiveColor, float iconOffset)
	{
		if (!showObjectivesActive) {
			return;
		}

		objectiveInfo newObjective = new objectiveInfo ();
		newObjective.Name = obj.name;
		newObjective.mapObject = obj;
		newObjective.iconOffset = iconOffset;
		newObjective.ID = currentID;

		int currentObjectiveIconIndex = -1;

		int objectiveIconListCount = objectiveIconList.Count;

		for (int i = 0; i < objectiveIconListCount; i++) {
			if (currentObjectiveIconIndex == -1 && objectiveIconList [i].name.Equals (objectiveIconName)) {
				currentObjectiveIconIndex = i;
			}
		}

		if (currentObjectiveIconIndex != -1) {

			objectiveIconElement currentObjectiveIconElement = objectiveIconList [currentObjectiveIconIndex];

			if (radiusDistance < 0) {
				radiusDistance = currentObjectiveIconElement.minDefaultDistance;
			}

			newObjective.useCloseDistance = useCloseDistance;
			newObjective.closeDistance = radiusDistance;
			newObjective.showOffScreenIcon = showOffScreen;
			newObjective.showDistance = showDistanceInfo;
			newObjective.showDistanceOffScreen = showDistanceOffScreen;

			int propertyNameID = Shader.PropertyToID ("_Color");

			if (currentObjectiveIconElement.changeObjectiveColors && !removeCustomObjectiveColor) {
				Component[] components = obj.GetComponentsInChildren (typeof(Renderer));
				foreach (Renderer child in components) {
					if (child.material.HasProperty (propertyNameID)) {
						int materialsLength = child.materials.Length;

						for (int j = 0; j < materialsLength; j++) {
							Material currentMaterial = child.materials [j];

							newObjective.materials.Add (currentMaterial);
							newObjective.originalColor.Add (currentMaterial.color);

							if (useCustomObjectiveColor) {
								currentMaterial.color = objectiveColor;
							} else {
								currentMaterial.color = currentObjectiveIconElement.objectiveColor;
							}
						}
					}
				}
			}

			newObjective.iconPrefab = currentObjectiveIconElement.iconInfoElement.gameObject;
	
			objectiveList.Add (newObjective);

			int playerScreenObjectivesListCount = playerScreenObjectivesList.Count;

			for (int i = 0; i < playerScreenObjectivesListCount; i++) {
				playerScreenObjectivesList [i].addElementToList (newObjective, currentObjectiveIconElement.iconInfoElement.gameObject, currentID);
			}
		} else {
			print ("Element not found in objective icon list");
		}

		currentID++;
	}

	public int getCurrentID ()
	{
		return currentID;
	}

	public void increaseCurrentID ()
	{
		currentID++;
	}

	public void addElementFromPlayerList (objectiveInfo newObjectiveInfo, objectiveIconElement currentObjectiveIconElement, Transform currentPlayer, bool addIconOnRestOfPlayers)
	{
		if (!showObjectivesActive) {
			return;
		}

		objectiveList.Add (newObjectiveInfo);

		if (addIconOnRestOfPlayers) {
			int playerScreenObjectivesListCount = playerScreenObjectivesList.Count;

			for (int i = 0; i < playerScreenObjectivesListCount; i++) {
				if (playerScreenObjectivesList [i].player != currentPlayer) {
					playerScreenObjectivesList [i].addElementToList (newObjectiveInfo, currentObjectiveIconElement.iconInfoElement.gameObject, newObjectiveInfo.ID);
				}
			}
		}
	}

	//if the target is reached, disable all the parameters and clear the list, so a new objective can be added in any moment
	public void removeElementFromList (objectiveInfo objectiveListElement)
	{
		if (!showObjectivesActive) {
			return;
		}

		if (objectiveListElement.iconRemoved) {
			return;
		}

		checkObjectiveInfoOnMapSystem (objectiveListElement);

		if (objectiveListElement.materials.Count > 0) {
			StartCoroutine (changeObjectColors (objectiveListElement, true));
		} else {
			removeElementFromObjectiveList (objectiveListElement);
		}
	}

	public void removeGameObjectFromList (GameObject objectToSearch)
	{
		if (!showObjectivesActive) {
			return;
		}

		int objectiveListCount = objectiveList.Count;

		for (int j = objectiveListCount - 1; j >= 0; j--) {
			if (objectiveList [j].mapObject == objectToSearch) {
				removeElementFromList (objectiveList [j]);
			}
		}
	}

	public void removeGameObjectListFromList (List<GameObject> list)
	{
		if (!showObjectivesActive) {
			return;
		}

		bool mapCreatorLocated = mapCreatorManager != null;

		for (int i = 0; i < list.Count; i++) {
			for (int j = 0; j < objectiveList.Count; j++) {
				if (objectiveList [j].mapObject == list [i]) {
					if (mapCreatorLocated) {
						mapCreatorManager.removeMapObject (objectiveList [j].mapObject, true);
					}

					removeElementFromObjectiveList (objectiveList [j]);
				}
			}
		}
	}

	IEnumerator changeObjectColors (objectiveInfo objectiveListElement, bool removeElement)
	{
		if (objectiveListElement.materials.Count != 0) {
			for (float t = 0; t < 1;) {
				
				t += Time.deltaTime;

				int materialsCount = objectiveListElement.materials.Count;

				for (int k = 0; k < materialsCount; k++) {
					Material currentMaterial = objectiveListElement.materials [k];

					currentMaterial.color = Color.Lerp (currentMaterial.color, objectiveListElement.originalColor [k], t / 3);
				}

				yield return null;
			}
		}

		if (removeElement) {
			removeElementFromObjectiveList (objectiveListElement);
		}
	}

	public void removeElementFromObjectiveList (objectiveInfo objectiveListElement)
	{
		if (!showObjectivesActive) {
			return;
		}

		int playerScreenObjectivesListCount = playerScreenObjectivesList.Count;

		for (int i = 0; i < playerScreenObjectivesListCount; i++) {
			playerScreenObjectivesList [i].removeElementFromList (objectiveListElement.ID);
		}

		int objectiveListIndex = objectiveList.FindIndex (s => s.ID == objectiveListElement.ID);

		if (objectiveListIndex > -1) {

			objectiveList.RemoveAt (objectiveListIndex);
		}
	}

	public void removeElementFromObjectiveListCalledByPlayer (int objectId, GameObject currentPlayer)
	{
		if (!showObjectivesActive) {
			return;
		}

		for (int i = 0; i < objectiveList.Count; i++) {
			if (objectiveList [i].ID == objectId) {

				checkObjectiveInfoOnMapSystem (objectiveList [i]);

				if (i < objectiveList.Count && objectiveList [i].ID == objectId) {
	
					if (objectiveList [i].materials.Count > 0) {
						StartCoroutine (changeObjectColors (objectiveList [i], true));
					} else {
						objectiveList.Remove (objectiveList [i]);
					}
//					print ("Screen Icon removed without the map system");
				} else {
//					print ("Screen Icon removed with the map system");
				}

				int playerScreenObjectivesListCount = playerScreenObjectivesList.Count;

				for (int j = 0; j < playerScreenObjectivesListCount; j++) {
					if (playerScreenObjectivesList [j].player != currentPlayer) {
						playerScreenObjectivesList [j].removeElementFromList (objectId);
					}
				}

				return;
			}
		}
	}

	public void checkObjectiveInfoOnMapSystem (objectiveInfo objectiveListElement)
	{
		if (!showObjectivesActive) {
			return;
		}

		if (objectiveListElement.mapObject != null) {

			objectiveListElement.iconRemoved = true;

			bool isPathElement = false;

			mapObjectInformation currentMapObjectInformation = objectiveListElement.mapObject.GetComponent<mapObjectInformation> ();

			if (currentMapObjectInformation != null) {
				if (currentMapObjectInformation.typeName.Equals ("Path Element")) {
					isPathElement = true;
				}

				currentMapObjectInformation.checkPointReachedEvent ();
			}

			if (mapCreatorManager != null) {
				mapCreatorManager.removeMapObject (objectiveListElement.mapObject, isPathElement);
			}
		}
	}

	public void setScreenObjectiveVisibleState (bool state, GameObject mapObject)
	{
		if (!showObjectivesActive) {
			return;
		}

		int objectiveListCount = objectiveList.Count;

		for (int j = 0; j < objectiveListCount; j++) {
			if (objectiveList [j].mapObject == mapObject) {
				objectiveList [j].showIconPaused = state;
			}
		}
	}

	public void checkGetMapCreatorManager ()
	{
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
		} 
	}

	[System.Serializable]
	public class objectiveInfo
	{
		public string Name;
		public GameObject mapObject;
		public RectTransform iconTransform;
		public GameObject onScreenIcon;
		public GameObject offScreenIcon;
		public Text iconText;

		public int ID;

		public bool useCloseDistance;
		public float closeDistance;
		public bool showOffScreenIcon;
		public bool showDistance;
		public bool showDistanceOffScreen;

		public float iconOffset;
		public bool showIconPaused;

		public float lastDistance;

		public bool iconRemoved;
		[HideInInspector] public List<Material> materials = new List<Material> ();
		[HideInInspector] public List<Color> originalColor = new List<Color> ();


		public GameObject iconPrefab;
	}

	[System.Serializable]
	public class objectiveIconElement
	{
		public string name;
		public objectiveIconInfo iconInfoElement;
		public bool changeObjectiveColors = true;
		public Color objectiveColor;
		public float minDefaultDistance = 10;
	}
}