# project-pace
Student project files

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class EnemyStateMachine : MonoBehaviour {

	protected enum States {
		
		IDLESTATE,
		PURSUESTATE,
		LUNGESTATE,
		DODGESTATE,
		PREATTACKSTATE,
		SPREADSTATE,
		COMBINESTATE
		
	}
	protected float TimeSinceLastStateChange = 0;
	[SerializeField] protected States curState = States.IDLESTATE;
	protected Dictionary<States, Action> fsm = new Dictionary<States, Action>();

	//-------------------- BEHAVIOR VARIABLES --------------------------------------
	[SerializeField] private bool IsInvincible = false;
	[SerializeField] private float contactDamage = 10;
	[SerializeField] GameObject invincibleParticle;
	//private float particleTimer = 0;
	[SerializeField] GameObject guardPoint;
	//private bool atGuardPoint = false;
	//private int guardPointID = 0;
	protected float guardDistanceLimit = 5;
	protected bool tookDamage = false;
	protected float damageFlashTimer = 0;
	protected bool isMag = false;

	//-------------------- PURSUE VARIABLES ----------------------------------------
	[SerializeField] protected float pursuitDetectionRange = 1;
	[SerializeField] protected float pursueForce = 50;
	[SerializeField] protected float pursueMaxSpeed = 10;
	[SerializeField] protected float pursueMinRange = 2;

	//-------------------- DODGE VARIABLES -----------------------------------------
	private float dodgeChance = 0;
	private float dodgeRange = 2;
	private float dodgeForce = 50;
	private float dodgeDuration = 0.25f;
	private bool didDodge = false;

	//-------------------- LUNGE VARIABLES -----------------------------------------
	protected bool IsInAttackState = false;
	private bool Confused = false;
	[SerializeField] protected float preAttackDuration = 1;
	protected bool preAttackSwitch = false;
	[SerializeField] protected float defaultAttackCooldown = 2;
	protected float modifiedAttackCooldown;
	[SerializeField] public float lungeDetectionRange = 1;
	[SerializeField] private float lungeForce = 50;
	[SerializeField] protected float lungeHitPush = 20;
	[SerializeField] protected float lungeHitRecoil = 20;
	[SerializeField] protected float lungeDamage = 10;
	[SerializeField] private float lungeDuration = 0.25f;
    [SerializeField] protected float lungeMaxSpeed = 60;
	protected bool didLunge = false;
	[SerializeField] private bool trackingLunge = false;
    private Vector3 targetVector = Vector3.zero;

	//------------------- SPINATTACK VARIABLES -----------------------------------

	protected float spinRate = 1;
	protected float spinTimer = 0;

	//-------------------- DESTRUCTION VARIABLES -----------------------------------
	[SerializeField] protected GameObject[] destructionDebris;
	[SerializeField] protected float[] pickupChance;
	[SerializeField] protected float debrisArea;
	[SerializeField] protected int debrisAmount;
	protected bool didDestruction = false;

	//-------------------- HEALTH VARIABLES ----------------------------------------
	[SerializeField] float maxHealth = 100;
	[SerializeField] float damageResist = 0;
	[SerializeField] protected float curHealth = 0;

	//-------------------- COMPONENT VARIABLES -------------------------------------
	protected Rigidbody rb;
	protected Transform playerTransform;
	protected Renderer ren;
	protected PlayerStateMachine playerCtrl;
	protected Rigidbody playerBody;
	[SerializeField] protected EnemyFloorSticker floorStick;
	protected Transform enemyParent;
	protected Transform myTransform;
	protected GameObject enemyParentObject;
	[SerializeField] GameObject healthBarObject;
	[SerializeField] GameObject newHealthBar;
	private AudioSource audioSource;

	//-------------------- AUDIO VARIABLES -----------------------------------------

	[SerializeField] private AudioClip EnemyHitPlayer;
	[SerializeField] private AudioClip ContactDamaging;
	[SerializeField] private AudioClip SpinAttacking;
	[SerializeField] private AudioClip PreAttack;
	[SerializeField] private AudioClip EnemyDefeat;
	[SerializeField] private AudioClip EnemyDamaged;
	[SerializeField] private AudioClip EnemyAttack;

	//--------------------COLOR VARIABLES ------------------------------------------
	[SerializeField] public Color idleColor = Color.green;
	[SerializeField] public Color pursueColor = Color.yellow;
	[SerializeField] public Color lungeColor = Color.magenta;

	public virtual void Start () {
		fsm.Add( States.IDLESTATE, new Action( IdleState ) );
		fsm.Add( States.PURSUESTATE, new Action( PursueState ) );
		fsm.Add( States.LUNGESTATE, new Action( LungeState ) );
		fsm.Add( States.DODGESTATE, new Action( DodgeState ) );
		fsm.Add( States.PREATTACKSTATE, new Action ( PreAttackState ) );

		//----------------------- VARIABLE INITIALIZATION --------------------------
		curHealth = maxHealth;
		modifiedAttackCooldown = defaultAttackCooldown;

		//----------------------- COMPONENT INITIALIZATION -------------------------
		audioSource = gameObject.GetComponent<AudioSource>();
		enemyParent = transform.parent;
		enemyParentObject = enemyParent.gameObject;
		playerCtrl = GameObject.Find("Player").GetComponent<PlayerStateMachine>();
		playerTransform = GameObject.Find("Player").GetComponent<Transform>();
		rb = GetComponent<Rigidbody>();
		ren = GetComponent<Renderer>();
		playerBody = GameObject.Find("Player").GetComponent<Rigidbody>();
		SetState( States.IDLESTATE );
		floorStick = transform.parent.gameObject.GetComponent<EnemyFloorSticker>();

		myTransform = GetComponent<Transform>();
		newHealthBar = Instantiate( healthBarObject, transform.position, transform.rotation ) as GameObject;
		newHealthBar.GetComponent<EnemyHealthBar>().SetTransform(myTransform);
		//wallBreaker.GetComponent<WallBreaker> ().FlashColor();
	}

	protected void UpdateLoop() {
		fsm[curState].Invoke();
		TimeSinceLastStateChange += Time.deltaTime;

	}
	
	//-------------------------------- COLLISION FUNCTIONS -------------------------

	protected virtual void OnCollisionEnter( Collision _contact ) {
		if( _contact.gameObject.tag == "Player" ) {

			if( IsInAttackState == true ) {
				playerCtrl.ChangePlayerHealth( -lungeDamage );
				playerBody.AddForce( FindPlayerDirection() * lungeHitPush, ForceMode.Impulse );
				rb.AddForce( -FindPlayerDirection() * lungeHitRecoil, ForceMode.Impulse );

				if( EnemyHitPlayer != null ) {
					audioSource.PlayOneShot(EnemyHitPlayer);
				}
			}
		}
	}

	public void OnCollisionStay( Collision _contact ) {
		if( contactDamage > 0 && _contact.gameObject.tag == "Player" ) {
			bool doDamage = false;
			if( !playerCtrl.GetIsPlayerInAttackState() || IsInvincible ) { doDamage = true; }
			if( doDamage ) {

				if( ContactDamaging != null ) {
					audioSource.PlayOneShot(ContactDamaging);
				}
				playerCtrl.ChangePlayerHealth( -contactDamage ); }
		}
	}
	
	//-------------------------------- STATE MACHINE FUNCTIONS ---------------------
	virtual protected void IdleState() {}
	
	virtual protected void PursueState() {}
	
	virtual protected void LungeState() {}
	
	virtual protected void DodgeState() {
		DoDodge();
		EndDodgeState();
	}

	virtual protected void PreAttackState() {

		PreAttackFlicker();

		if( TimeSinceLastStateChange > preAttackDuration ) { SetState( States.LUNGESTATE ); }
	}

    //-------------------------------- HELPER FUNCTIONS ----------------------------

    protected void SpeedRegulator( float _maxSpeed )
    {
        if (rb.velocity.magnitude > _maxSpeed )
        {

            rb.velocity = Vector3.zero;
        }
    }

    protected void SpinAttack() {
		spinTimer += Time.deltaTime;
		gameObject.transform.Rotate( Vector3.up * Time.deltaTime * 720 );

		if( spinTimer > SpinAttacking.length ) {
			if( SpinAttacking != null ) {
				audioSource.PlayOneShot(SpinAttacking);
			}
			spinTimer = 0;
		}
	}

	protected void ReturnToGuard() {
		Vector3 VectorToGuardPoint = guardPoint.transform.position - transform.position;
		Vector3 DirectionToGuardPoint = VectorToGuardPoint.normalized;
		float RangeToGuardPoint = VectorToGuardPoint.magnitude;

		if( RangeToGuardPoint > 0.5f ) {
			rb.AddForce( DirectionToGuardPoint * pursueForce, ForceMode.Force );
		}

        SpeedRegulator(pursueMaxSpeed);
    }

	protected void PreAttackFlicker() {
		if( IsInAttackState == false ) {

			if( PreAttack != null ) {
				audioSource.PlayOneShot( PreAttack );
			}

			IsInAttackState = true; }
		if( preAttackSwitch ) {
			ren.material.color = pursueColor;
			preAttackSwitch = false;
		}
		else if (!preAttackSwitch ) {
			ren.material.color = lungeColor;
			preAttackSwitch = true;
		}
	}

	protected bool LungeIsOnCooldown() {
		bool onCooldown = false;

		if( modifiedAttackCooldown > 0 ) { onCooldown = true; }
		else if ( modifiedAttackCooldown <= 0 ) { onCooldown = false; }

		return onCooldown;
	}

	public void EndLungeState() {
		if( TimeSinceLastStateChange >= lungeDuration ) {
			floorStick.ResumeSticking();
			rb.velocity = Vector3.zero;
			didLunge = false;
			IsInAttackState = false;
            //Debug.Log("Lunge State Ended.");
			SetState( States.IDLESTATE );
		}
	}

	public void EndDodgeState() {
		if( TimeSinceLastStateChange >= dodgeDuration ) {
			SetState( States.IDLESTATE );
			didDodge = false;
		}
	}

	public void DoDodge() {
		if( didDodge == false ) {
			didDodge = true;
			Vector3 dodgeVector = transform.position - playerTransform.position;
			dodgeVector = dodgeVector.normalized;
			//Debug.Log( dodgeVector );
			rb.AddForce(dodgeVector * dodgeForce, ForceMode.Impulse);
		}
	}
	
	public void PursuePlayer() {
		//Debug.Log("Pursuing Player");
		Vector3 dirToPlayer = FindPlayerDirection();
		if( GetPlayerRange() > pursueMinRange ) {
			rb.AddForce( dirToPlayer * pursueForce, ForceMode.Force );
		}
        SpeedRegulator(pursueMaxSpeed);
    }

	protected void ColorSetter() {
		if( IsInAttackState == true ) { ren.material.color = lungeColor; }
		else if( IsInAttackState == false ) { ren.material.color = idleColor; }
	}

	protected bool DetectDodge() {
		Collider[] contacts = Physics.OverlapSphere(transform.position, dodgeRange);
		bool detectedPlayer = false;

		for( int i = 0; i < contacts.Length; i++ ) {

			if( contacts[i].gameObject.tag == "Player" &&
			   contacts[i].gameObject.GetComponent<Renderer>() != null &&
			   playerCtrl.GetIsPlayerInAttackState() ) {

				float dodgeRoll = UnityEngine.Random.Range( 0, 100 );

				if( dodgeRoll <= dodgeChance ) {
					detectedPlayer = true;
				} else { detectedPlayer = false; }
			}
		}

		return detectedPlayer;
	}

    protected void CountCooldown()
    {
        if (Confused)
        {
            //Some sort of visual effect goes here
            modifiedAttackCooldown *= 2;
            Confused = false;
        }

        modifiedAttackCooldown -= Time.deltaTime;
    }

    public void Lunge() {
		if( didLunge == false ) {
			IsInAttackState = true;
			ColorSetter();
			if( modifiedAttackCooldown < 0 ) { modifiedAttackCooldown = 0; }
			modifiedAttackCooldown += defaultAttackCooldown;
			didLunge = true;

			if( EnemyAttack != null ) {
				audioSource.PlayOneShot(EnemyAttack);
			}

            targetVector = playerTransform.position - transform.position;
            targetVector = targetVector.normalized;
            //Debug.Log("LungeSetup: " + didLunge);
        }

        if (trackingLunge) {
            targetVector = playerTransform.position - transform.position;
            //targetVector += playerBody.velocity;
            targetVector = targetVector.normalized;
        }

        rb.AddForce(targetVector * lungeForce, ForceMode.Force);

        SpeedRegulator(lungeMaxSpeed);
    }

	public bool DetectPlayerByLOS() {
		RaycastHit hit;

		if ( Physics.Raycast(transform.position, FindPlayerDirection(), out hit) ) {
			Debug.DrawLine(transform.position, hit.point);
			if( hit.transform.gameObject.tag == "Player" &&
			   hit.transform.gameObject.GetComponent<Renderer>() != null ) {
				
				return true;
			} else { return false; }
		} else { return false; }
	}

	public bool DetectPlayerByRange( float _range ) {

		Collider[] contacts = Physics.OverlapSphere(transform.position, _range);
		bool detectedPlayer = false;
		
		for( int i = 0; i < contacts.Length; i++ ) {
			if( contacts[i].gameObject.tag == "Player" &&
			   contacts[i].gameObject.GetComponent<Renderer>() != null ) {

				detectedPlayer = true;
			}
		}
		//Debug.Log( detectedPlayer );
		return detectedPlayer;

	}
	
	public Vector3 FindPlayerDirection() {
		Vector3 DirectionToPlayer = playerTransform.position - transform.position;
		return DirectionToPlayer.normalized;
	}

	public float GetPlayerRange() {
		Vector3 playerVector = playerTransform.position - transform.position;
		return playerVector.magnitude;
	}

	public float GetGuardPointRange() {
		Vector3 guardVector = guardPoint.transform.position - transform.position;
		return guardVector.magnitude;
	}

	protected void SelectPickup( float _amount ) {
		for( int i = 0; i < _amount; i++ ) {

			int pickupRoll = UnityEngine.Random.Range(0, 100);
			
			for( int x = 0; x < pickupChance.Length; x++ ) {
				if( pickupRoll <= pickupChance[x] ) {
					SpawnPickup( destructionDebris[x] );
					
					break;
				}
			}
			
			
		}
	}

	protected void SpawnPickup( GameObject _pickup ) {
		float debrisRoll = UnityEngine.Random.Range(0, 360);
		Quaternion debrisDirection = transform.rotation;
		debrisDirection.z = debrisRoll;
		
		Vector3 debrisPosition = transform.position;
		float debrisPosRollx = UnityEngine.Random.Range(-debrisArea, debrisArea);
		float debrisPosRolly = UnityEngine.Random.Range(1, debrisArea);
		float debrisPosRollz = UnityEngine.Random.Range(-debrisArea, debrisArea);
		debrisPosition.x = debrisPosition.x + debrisPosRollx;
		debrisPosition.y = debrisPosition.y + debrisPosRolly;
		debrisPosition.z = debrisPosition.z + debrisPosRollz;

		Instantiate( _pickup,
		            debrisPosition,
		            transform.rotation );

	}

	public void DeathCheck() {
		if( curHealth <= 0 && didDestruction == false ) {

			SelectPickup(debrisAmount);

			didDestruction = true;

			if( EnemyDefeat != null ) { audioSource.PlayOneShot(EnemyDefeat); }
			Destroy(enemyParentObject);
		}

		if( tookDamage ) {
			damageFlashTimer += Time.deltaTime;
			
			if( damageFlashTimer < 0.25f ) {
				if( isMag ) {
					transform.GetComponent<Renderer>().material.color = Color.cyan;
					isMag = false;
				} else if( !isMag ) {
					transform.GetComponent<Renderer>().material.color = Color.magenta;
					isMag = true;
				}
			} else if ( damageFlashTimer >= 0.25f ) {
				ColorSetter();
				tookDamage = false;
				damageFlashTimer = 0;
			}
		}

	
	}
	

	//-------------------------------- ACCESSOR FUNCTIONS --------------------------

	public bool GetIsEnemyInAttackState() {
		return IsInAttackState;
	}

	public float GetEnemyHealthPerc() {
		float healthPerc = curHealth / maxHealth;
		return healthPerc;
	}

	public bool IsEnemyHurt() {
		bool isHurt = false;

		if( curHealth < maxHealth ) { isHurt = true; }
		else if( curHealth >= maxHealth ) { isHurt = false; }

		return isHurt;
	}

	//-------------------------------- MUTATOR FUNCTIONS ---------------------------

	public void ConfuseEnemy() {
		Confused = true;
	}

	public void ModifyEnemyHealth(float _value) {

		float damageToDeal = 0;

		if( _value < 0 ) {
			damageToDeal = _value + damageResist;

			if( damageToDeal > 0 ) {
				damageToDeal = 0;
			}
		}

		if( !IsInvincible ) {
			curHealth += damageToDeal;
		}

		if( _value < 0 ) {

			SelectPickup(1);
			tookDamage = true;

			if( EnemyDamaged != null ) { audioSource.PlayOneShot(EnemyDamaged); }
		}

        if( _value > 0 )
        {
            curHealth += _value;
        }
	}

	//-------------------------------- STATE MACHINE FUNCTIONS ---------------------

	protected void SetState( States _newState ) {
		curState = _newState;
		ColorSetter();
		TimeSinceLastStateChange = 0;
		//Debug.Log( "Current enemy state: " + curState );
	}
}
