﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class taskCounterSystem : MonoBehaviour
{
	[Header ("Main Settings")]
	[Space]

	public bool taskCounterEnabled = true;

	public int numberOfTasks;

	public bool resetCounterOnTaskComplete;

	[Space]
	[Header ("Task Info List Settings")]
	[Space]

	public List<taskInfo> taskInfoList = new List<taskInfo> ();

	[Space]

	public bool makeTasksInOrderEnabled;

	public bool resetTaskCounterOnIncorrectOrder;

	[Space]
	[Header ("Debug")]
	[Space]

	public int currentTaskCounter;

	public bool taskComplete;

	public bool showDebugPrint;

	[Space]
	[Header ("Events Settings")]
	[Space]

	public UnityEvent eventOnTaskCounterComplete;

	[Space]

	public bool checkIfUndoTaskComplete;
	public UnityEvent eventOnUndoTaskComplete;


	public void increaseTaskCounter ()
	{
		increaseMultipleTaskCounter (1);
	}

	public void decreaseTaskCounter ()
	{
		increaseMultipleTaskCounter (-1);
	}

	public void completeTaskCounter ()
	{
		increaseMultipleTaskCounter (numberOfTasks);
	}

	public void addTaskCompleteState (string taskName)
	{
		numberOfTasks = taskInfoList.Count;

		for (int i = 0; i < taskInfoList.Count; i++) {
			if (!taskInfoList [i].taskComplete && taskInfoList [i].taskName.Equals (taskName)) {

				if (makeTasksInOrderEnabled) {
					if (currentTaskCounter != i) {
						if (resetTaskCounterOnIncorrectOrder) {
							resetTasksListCompleteState ();
						}

						return;
					}
				}

				taskInfoList [i].taskComplete = true;

				increaseTaskCounter ();

				return;
			}
		}
	}

	public void removeTaskCompleteState (string taskName)
	{
		numberOfTasks = taskInfoList.Count;

		for (int i = 0; i < taskInfoList.Count; i++) {
			if (taskInfoList [i].taskComplete && taskInfoList [i].taskName.Equals (taskName)) {

				taskInfoList [i].taskComplete = false;

				decreaseTaskCounter ();

				return;
			}
		}
	}

	public void resetTasksListCompleteState ()
	{
		numberOfTasks = taskInfoList.Count;

		for (int i = 0; i < taskInfoList.Count; i++) {
			taskInfoList [i].taskComplete = false;
		}

		currentTaskCounter = 0;
	}

	public void decreaseMultipleTaskCounter (int newValue)
	{
		increaseMultipleTaskCounter (-newValue);
	}

	public void increaseMultipleTaskCounter (int newValue)
	{
		if (!taskCounterEnabled || taskComplete) {
			return;
		}

		if (showDebugPrint) {
			if (newValue > 0) {
				print ("Increasing Task Counter In " + newValue);
			} else {
				print ("Decreasing Task Counter In " + Mathf.Abs (newValue));
			}
		}

		currentTaskCounter += newValue;

		if (currentTaskCounter < 0) {
			currentTaskCounter = 0;
		}

		if (currentTaskCounter >= numberOfTasks) {
			eventOnTaskCounterComplete.Invoke ();

			taskComplete = true;

			if (showDebugPrint) {
				print ("Task Complete");
			}

			if (resetCounterOnTaskComplete) {
				taskComplete = false;

				currentTaskCounter = 0;
			}
		} else {
			if (checkIfUndoTaskComplete) {
				if (newValue < 0 && taskComplete) {
					eventOnUndoTaskComplete.Invoke ();

					taskComplete = false;

					if (showDebugPrint) {
						print ("Undoing task Complete");
					}
				}
			}
		}
	}

	public void setTaskCounterEnabledState (bool state)
	{
		taskCounterEnabled = state;
	}

	public void resetTaskCounter ()
	{
		currentTaskCounter = 0;
	}

	public void setTaskCompleteState (bool state)
	{
		taskComplete = state;
	}

	public void resetTaskCounterAndDisableTaskComplete ()
	{
		resetTaskCounter ();

		setTaskCompleteState (false);
	}

	[System.Serializable]
	public class taskInfo
	{
		public string taskName;
		public bool taskComplete;
	}
}
