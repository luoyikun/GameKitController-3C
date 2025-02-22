﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class abilitiesAIBehavior : AIBehaviorInfo
{
	[Header ("Custom Settings")]
	[Space]

	public AIAbilitiesSystemBrain mainAIAbilitiesSystemBrain;

	public override void updateAI ()
	{
		if (!behaviorEnabled) {
			return;
		}

		mainAIAbilitiesSystemBrain.updateAI ();
	}

	public override void updateAIBehaviorState ()
	{
		if (!behaviorEnabled) {
			return;
		}

		mainAIAbilitiesSystemBrain.updateBehavior ();
	}

	public override void updateAIAttackState (bool canUseAttack)
	{
		if (!behaviorEnabled) {
			return;
		}

		mainAIAbilitiesSystemBrain.updateAIAttackState (canUseAttack);
	}

	public override void setSystemActiveState (bool state)
	{
		if (!behaviorEnabled) {
			return;
		}

		mainAIAbilitiesSystemBrain.setSystemActiveState (state);
	}

	public override void setWaitToActivateAttackActiveState (bool state)
	{
		mainAIAbilitiesSystemBrain.setWaitToActivateAttackActiveState (state);
	}

	public override void stopCurrentAttackInProcess ()
	{
		mainAIAbilitiesSystemBrain.stopCurrentAttackInProcess ();
	}

	public override void setTurnBasedCombatActionActiveState (bool state)
	{
		mainAIAbilitiesSystemBrain.setTurnBasedCombatActionActiveState (state);
	}

	public override void checkAIBehaviorStateOnCharacterSpawn ()
	{
		mainAIAbilitiesSystemBrain.checkAIBehaviorStateOnCharacterSpawn ();
	}

	public override void checkAIBehaviorStateOnCharacterDespawn ()
	{
		mainAIAbilitiesSystemBrain.checkAIBehaviorStateOnCharacterDespawn ();
	}
}
