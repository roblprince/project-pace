//Robert Prince, Full Sail University (2017)
//Inherits EnemyStateMachine.cs

using UnityEngine;
using System.Collections;

public class SeekerStateMachine : EnemyStateMachine {

	public override void Start(){
		base.Start();

		pursueMinRange = lungeDetectionRange;

		SetState( States.IDLESTATE );
	}
	
	void FixedUpdate() {
		UpdateLoop();
	}

	protected override void IdleState() {
		DeathCheck();
		CountCooldown();

		if( DetectPlayerByLOS() ) { SetState( States.PURSUESTATE ); }
	}

	protected override void PursueState() {
		DeathCheck();
		PursuePlayer();
		CountCooldown();

        if ( DetectPlayerByRange( lungeDetectionRange ) &&
		   LungeIsOnCooldown() == false ) {

			SetState( States.PREATTACKSTATE );
		}
	}

	protected override void LungeState() {
		DeathCheck();
		Lunge();
        if ( IsInAttackState == false ) { IsInAttackState = true; }
		EndLungeState();
	}

	protected override void PreAttackState() {
		PreAttackFlicker();
		DeathCheck();
		PursuePlayer();
		
		if( TimeSinceLastStateChange > preAttackDuration ) { SetState( States.LUNGESTATE ); }
	}
}
