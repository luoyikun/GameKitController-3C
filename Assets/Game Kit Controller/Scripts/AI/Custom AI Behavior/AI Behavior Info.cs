﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviorInfo : MonoBehaviour
{
    [Header ("Main Settings")]
    [Space]

    public bool behaviorEnabled = true;

    public virtual void activateAIBehavior ()
    {

    }

    public virtual void deactivateAIBehavior ()
    {

    }

    public virtual void updateAI ()
    {

    }

    public virtual void updateAIBehaviorState (bool state)
    {

    }

    public virtual void updateAIAttackState (bool state)
    {

    }

    public virtual void updateInsideRangeDistance (bool state)
    {

    }

    public virtual void updateAIBehaviorState ()
    {

    }

    public virtual void setDrawOrHolsterWeaponState (bool state)
    {

    }

    public virtual void setAimWeaponState (bool state)
    {

    }

    public virtual void setShootWeaponState (bool state)
    {

    }

    public virtual void setHoldShootWeaponState (bool state)
    {

    }

    public virtual void dropWeapon ()
    {

    }

    public virtual void checkIfDrawWeaponsWhenResumingAI ()
    {

    }

    public virtual void resetBehaviorStates ()
    {

    }

    public virtual void resetAttackState ()
    {

    }

    public virtual void stopAim ()
    {

    }

    public virtual void disableOnSpottedState ()
    {

    }

    public virtual void updateWeaponsAvailableState ()
    {

    }

    public virtual void setBehaviorStatesPausedState (bool state)
    {

    }

    public virtual void setSystemActiveState (bool state)
    {

    }

    public virtual bool carryingWeapon ()
    {

        return false;
    }

    public virtual void updateIfCarryingWeapon ()
    {

    }

    public virtual void setWaitToActivateAttackActiveState (bool state)
    {


    }

    public virtual void setUseRandomWalkEnabledState (bool state)
    {


    }

    public virtual void setOriginalUseRandomWalkEnabledState ()
    {


    }

    public virtual bool isAIBehaviorAttackInProcess ()
    {


        return false;
    }

    public virtual void checkNoWeaponsAvailableState ()
    {

    }

    public virtual void checkIfDisableCurrentWeaponToChangeAttackMode (string newModeName)
    {

    }

    public virtual void changeCurrentAttackMode (string newModeName)
    {

    }

    public virtual void setIsCurrentCharacterTurn (bool state)
    {

    }

    public virtual void setAIOnPlayerTeamState (bool state)
    {

    }

    public virtual void setCurrentCharacterTurnTarget (GameObject newTarget)
    {

    }

    public virtual void stopCurrentAttackInProcess ()
    {

    }

    public virtual void setTurnBasedCombatActionActiveState (bool state)
    {

    }

    public virtual void checkStateOnCharacterDead ()
    {

    }

    public virtual void checkAIBehaviorStateOnCharacterSpawn ()
    {

    }

    public virtual void checkAIBehaviorStateOnCharacterDespawn ()
    {

    }

    public virtual void stopAttackState ()
    {

    }
}
