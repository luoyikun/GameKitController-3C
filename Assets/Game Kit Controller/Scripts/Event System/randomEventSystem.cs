using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class randomEventSystem : MonoBehaviour
{
    [Header ("Main Settings")]
    [Space]

    public bool randomEventsEnabled = true;

    public bool useSameIndexValue;
    public int sameIndexValueToUse;

    [Space]
    [Header ("Debug")]
    [Space]

    public bool showDebugPrint;
    public bool ignoreRandomEventActive;

    [Space]
    [Header ("Random Events Settings")]
    [Space]

    public List<randomEventInfo> randomEventInfoList = new List<randomEventInfo> ();


    randomEventInfo currentEventInfo;

    public void callRandomEventByName (string eventName)
    {
        if (ignoreRandomEventActive) {
            return;
        }

        if (!randomEventsEnabled) {
            return;
        }

        int eventIndex = randomEventInfoList.FindIndex (s => s.Name == eventName);

        if (eventIndex > -1) {
            activateEvent (eventIndex);
        }
    }

    public void callRandomEvent ()
    {
        if (ignoreRandomEventActive) {
            return;
        }

        if (!randomEventsEnabled) {
            return;
        }

        int randomIndex = Random.Range (0, randomEventInfoList.Count);

        if (useSameIndexValue) {
            randomIndex = sameIndexValueToUse;
        }

        activateEvent (randomIndex);
    }

    void activateEvent (int currentIndex)
    {
        if (currentIndex <= randomEventInfoList.Count - 1) {
            currentEventInfo = randomEventInfoList [currentIndex];

            if (currentEventInfo.eventEnabled) {
                if (showDebugPrint) {
                    print (currentEventInfo.Name);
                }

                bool activateEventResult = true;

                if (currentEventInfo.useProbabilityToActivateEvent) {
                    float currentProbability = Random.Range (0, 100);

                    if (currentProbability > currentEventInfo.probabilityToActivateEvent) {
                        activateEventResult = false;
                    }

                    if (showDebugPrint) {
                        print ("probability result " + currentProbability + " " + activateEventResult);
                    }
                }

                if (activateEventResult) {
                    currentEventInfo.eventToActive.Invoke ();

                    if (currentEventInfo.disableEventAfterActivation) {
                        currentEventInfo.eventEnabled = false;
                    }

                    if (showDebugPrint) {
                        print (currentEventInfo.Name + " event activated properly");
                    }
                }
            }
        }
    }

    public void setRandomEventsEnabledState (bool state)
    {
        randomEventsEnabled = state;
    }

    public void setIgnoreRandomEventActiveState (bool state)
    {
        ignoreRandomEventActive = state;
    }

    [System.Serializable]
    public class randomEventInfo
    {
        [Header ("Main Settings")]
        [Space]

        public string Name;

        public bool eventEnabled = true;

        public bool disableEventAfterActivation;

        [Space]
        [Header ("Probability Settings")]
        [Space]

        public bool useProbabilityToActivateEvent;

        [Range (0, 100)] public float probabilityToActivateEvent = 0;

        [Space]
        [Header ("Event Settings")]
        [Space]

        public UnityEvent eventToActive;
    }
}
