#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using OffScreenStuff;

public class Enemy : BaseEnemy
{
    //
    // Enemy
    // structs & classes
    [ReadOnly]
    public OffScreenIndicator OffScreenScript;

    public enum State
    {
        Init,
        Idle,
        Roam,
        Patrol,
        Follow,
        Attack,
        Dead
    }

    public enum Type
    {
        Human,
        Spider
    }

    public enum BehaviourType
    {
        Idle,
        Roam
        //Patrol
    }

    public static class AnimationName
    {
        public static string Idle = "idle";
        public static string Walk = "moveForward";
        public static string Run = "runNoWeap";
        public static string Shoot = "multiShot";
        public static string Fire = "pistolFire";
        public static string Aim = "pistolAim";
    }

    // • • • • • • • • • • • • • • • • • • • • //

    //
    // V a r i a b l e s
    //

    // • • • • • • • • • • • • • • • • • • • • //

    //
    // P r o p e r t i e s
    //

    // • • • • • • • • • • • • • • • • • • • • //

    //
    // U n i t y
    //

    private Vector3 randomPoint;
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // E x i t
        if (!EditorApplication.isPlaying) { return; }
        // Set Gizmos color
        Gizmos.color = Color.red;
        // do we have a random point?
        if (randomPoint == Vector3.zero) { randomPoint = position; }
        // Draw wire cube
        Gizmos.DrawWireCube(randomPoint + Vector3.up * (collider.bounds.size.y/2), collider.bounds.size);
    }
#endif

    void Awake()
    {
        collider = GetComponent<Collider>();
        // Initialize the animation, this code was improve from C10.2
        Initialize_Animations();
        Initialize_Navigation();
    }

    void Start()
    {
        // Set the spawn point as our current position
        spawnPoint = transform.position;
        // Initalize state machine
        state = State.Init;
        stateMachine.enter = true;
        // E x i t
        if (!stats) return;
        // Set the stats
        stats.onDamageTaken.AddListener(Damage);
        stats.onNoMoreHealth.AddListener(Dead);

        if (!SceneManagement.Name.Contains("Sniper"))
        {
            OffScreenScript = GameObject.FindObjectOfType<OffScreenIndicator>();
        }
    }

    void OnEnable()
    {
        stats.onDamage += OnDamage;
    }

    void OnDisable()
    {
        StopCoroutine(SetFollowByDistance());

        stats.onDamage -= OnDamage;
    }

    // • • • • • • • • • • • • • • • • • • • • //

    //
    // U s e r
    //
    
    public IEnumerator SetFollowByDistance()
    {
        // E x i t: if we are not hostile
        if (!attack || !target || isFollowing) { yield break; }
        
        while(!isFollowing)
        {
            // Calculate the magnitud
            float magnitud = Vector3.SqrMagnitude(target.position - position);
            
            if (magnitud < distance.pow2Listen)
            {
                // Calculate the end line position
                Vector3 end = target.position;
                // Calculate the direction
                Vector3 direction = (end - eyePointPosition).normalized;
                //Debug.DrawLine(eyePointPosition, eyePointPosition + direction * distance.listen, Color.red);
                // The ray cast hit info
                RaycastHit hit;
                // Raycast
                Physics.Raycast(eyePointPosition, direction, out hit, distance.listen, properties.sightMask);
                // Check if we can see the target
                if (hit.collider && hit.collider.CompareTag("Player"))
                {
                    // Pause the ed itor
                    //EditorApplication.isPaused = true;
                    // Draw a line to the out hit
                    //Debug.DrawLine(eyePointPosition, hit.point, Color.cyan);
                    // Set flag
                    isFollowing = true;
                }
				else
				{
				
							StartCoroutine ("ForceEnemiesToFollowPlayers");
					
				}
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

	IEnumerator ForceEnemiesToFollowPlayers()
	{
		isFollowing = true;
		yield return new WaitForSeconds (0.5f);
		state = State.Follow;
		yield return new WaitForSeconds (0.5f);
		state = State.Patrol;
		yield return new WaitForSeconds (0.5f);
		state = State.Follow;
	}
    
    public void OriginalSetFollow()
    {
        // E x i t: if we are not hostile
        if (!attack || !target || isFollowing) { return; }
        
        // Calculate the magnitud
        float magnitud = Vector3.SqrMagnitude(target.position - position);
        
        if (magnitud < distance.pow2Listen)
        {
            // Calculate the end line position
            Vector3 end = target.position;
            // Calculate the direction
            Vector3 direction = (end - eyePointPosition).normalized;
            //Debug.DrawLine(eyePointPosition, eyePointPosition + direction * distance.listen, Color.red);
            // The ray cast hit info
            RaycastHit hit;
            // Raycast
            Physics.Raycast(eyePointPosition, direction, out hit, distance.listen, properties.sightMask);
            // Check if we can see the target
            if (!hit.collider || !hit.collider.CompareTag("Player"))
            {
                return;
            }
            // Pause the editor
            //EditorApplication.isPaused = true;
            // Draw a line to the out hit
            //Debug.DrawLine(eyePointPosition, hit.point, Color.cyan);
            // Set flag
            isFollowing = true;
        }
    }

    public void SetFollow()
    {
        // E x i t: if we are not hostile
        if (!attack || !target || isFollowing) { return; }
        // Set flag
        isFollowing = true;
    }

    private void Initialize_Animations()
    {
        // E x i t
        if (!animation) return;

        // Set all animations to loop
        animation.wrapMode = WrapMode.Loop;
        // Except our action animations, Dont loop those
        animation[AnimationName.Shoot].wrapMode = WrapMode.Once;
        // Set the wrap mode for: pistol fire
        animation[AnimationName.Fire].wrapMode = WrapMode.Once;

        // State: idle
        AnimationState idle = animation[AnimationName.Idle];
        // Put idle and run in a lower layer. They will only animate if our action animations are not playing
        if(idle) idle.layer = -1;

        // State: Walk
        AnimationState walk = animation[AnimationName.Walk];

        if(walk)
        {
            // Set the layer: move forward
            walk.layer = -1;
            // Set the speed: walk animation speed
            walk.speed = 1;
        }

        // State: Run
        AnimationState run = animation[AnimationName.Run];

        if (run)
        {
            // Set the layer: run animation speed
            run.layer = -1;
            // Set the speed: run animation speed
            run.speed = 1;
        }
        
        // Set the speed: shoot animation speed
        animation[AnimationName.Shoot].speed = 1;
        // set the idle to cross fade
        animation.CrossFade(AnimationName.Idle, 0.3f);
        // S t o p
        animation.Stop();
    }

    private void Initialize_Navigation()
    {
        navigation.speed = speed.walk;
    }

    public void Attack(UnityAction call)
    {
        if (isAttacking) return;

        onAttackOver = call;

        StartCoroutine(IE_Attack());
    }

    private IEnumerator IE_Attack()
    {
        // Set flag to attack
        isAttacking = true;
        // Start shoot animation
        animation.CrossFade(AnimationName.Aim, 0.3f);
        // length of the animation
        float length = animation[AnimationName.Aim].length / 2;
        // Wait until half the animation has played
        yield return new WaitForSeconds(length);
        // Fire gun
        if (!behaviour.explode)
        {
            // fire the weapon
            weapon.Fire();
            // Play animation
            animation.Play(AnimationName.Fire);
            // Get the length animation
            length = animation[AnimationName.Fire].length;
            // Wait for the rest of the animation to finish
            yield return new WaitForSeconds(length);
        }
        // Set flag to attack
        isAttacking = false;
        // if we have an event
        if (null != onAttackOver) onAttackOver.Invoke();
    }

    private void Damage()
    {
        // E x i t
        if (null != damageCoroutine) { StopCoroutine(damageCoroutine); }
        // Start coroutine and store it
        damageCoroutine = StartCoroutine(IE_Damage());
    }

    private IEnumerator IE_Damage()
    {
        if (renderer)
        {
            renderer.material.color = Color.red;

            yield return new WaitForSeconds(0.5f);

            renderer.material.color = Color.white;
        }
    }

    private void OnDamage(float amount, Vector3 direction, Vector3 position)
    {
        // Change to follow
        SetFollow();
    }

    public void Dead()
    {


       // if (SceneManagement.Name.Contains("Sniper")) 
       // {
       //     Debug.LogFormat("Remove Arrow offscreen indicator for: {0}", this.gameObject.name);
       //    // OffScreenScript.RemoveIndicator(this.gameObject.transform);
       // }

        //UnityEditor.EditorApplication.isPaused = true;

        // Play a dying audio clip
        if (audio.deadClip)
        {
            AudioSource.PlayClipAtPoint(audio.deadClip, position);
        }
        // E x i t
        if (manager && manager.CubeManager)
        {
            // Spawn pickable blocks
            manager.CubeManager.SpawnBlocks(position + Vector3.up * 1, renderer.material);
        }
        // E x i t: if we don't have  dommy prefab
        if (ragdollPrefab)
        {
            // Get the remove body component
            RemoveBody body = ragdollPrefab.GetComponent<RemoveBody>();

            // Can we spawn the gun
            if (body && body.GunPickup)
            {
                GameObject pickup = Instantiate(body.GunPickup, position + Vector3.up * 1.5f, body.GunPickup.transform.rotation) as GameObject;

                // For balancing the amount of ammo in each group (b)
                if (BalanceCentral.instance != null)
                {
                    AmmoPickupC17 ammoPickup = pickup.GetComponent<AmmoPickupC17>();
                    if (ammoPickup != null)
                    {
                        //ammoPickup.ammoToAdd += (int)(BalanceCentral.instance.AmmoDrops * (ammoPickup.ammoToAdd / 100.0f));
						ammoPickup.ammoToAdd += (int)(BalanceCentral.instance.GetAmmoDrops() * (ammoPickup.ammoToAdd / 100.0f));
						//Debug.Log ("Ammo Drops " + BalanceCentral.instance.GetAmmoDrops ());
                    }
                }
            }
            
            // Make sure the skin on our dead body is the same as the one on our living body
			//SkinnedMeshRenderer skinRenderer = body.GetComponentInChildren<SkinnedMeshRenderer>();
			//if (skinRenderer != null)
			//{
			//	skinRenderer.material.mainTexture = _dependencies.renderer.material.mainTexture;;
			//}
        }
			
		if (ragdollFlyPrefab) 
		{
			GameObject ragdoll = Instantiate(ragdollFlyPrefab, position + Vector3.up * 0.8f, ragdollFlyPrefab.transform.rotation) as GameObject;

			SkinnedMeshRenderer skinRenderer = ragdoll.GetComponentInChildren<SkinnedMeshRenderer>();
			//if (skinRenderer != null)
			//{
			skinRenderer.material.mainTexture = _dependencies.renderer.sharedMaterial.mainTexture;
			//}
		}

		if (Locator.Get<EnemyManager> ().killstreakCounter) {
			Locator.Get<EnemyManager> ().killstreakCounter.enemiesKilled += 1;
			Locator.Get<EnemyManager> ().killstreakCounter.countdown = Locator.Get<EnemyManager> ().killstreakCounter.killTimer;
		}

			
    }

    //
    // H e l p e r s
    //

    public void Slerp(Quaternion target, float time)
    {
        rotation = Quaternion.Slerp(rotation, target, time);
    }

    public float Rotate(Vector3 targetDelta)
    {
        // Calculate the angles
        float angle = Vector3.Angle(targetDelta, transform.forward);
        // Rotate
        if (0 != angle && Vector3.zero != targetDelta)
        {
            Slerp(Quaternion.LookRotation(targetDelta), Time.deltaTime * speed.rotation);
        }

        return angle;
    }

    public bool RandomPoint(out Vector3 result)
    {
        Vector3 randomPoint = spawnPoint + Random.insideUnitSphere * 5;

        UnityEngine.AI.NavMeshHit hit;
        // The max distance of the sample position (Is recommended to use 2 times the height of the agent)
        float maxDistance = navigation.height * navigation.height;
        // https://docs.unity3d.com/ScriptReference/NavMesh.SamplePosition.html
        for (int i = 0; i < 5; i++)
        {
            if(UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, maxDistance, UnityEngine.AI.NavMesh.AllAreas))
            {
                result = hit.position;
                this.randomPoint = result;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }

    // Plays a crossfade animation for the human, but not for the spider
    public void PlayAnimationByType(string name, float fade)
    {
        // E x i t
        if (!animation) { return; }
        // E x i t
        // if (animation.IsPlaying(name)) { return; }
        // Switch between types
        switch(type)
        {
            case Type.Human:
                // Play the animation by name
                animation.CrossFade(name, fade);
                break;
            case Type.Spider:
                if(AnimationName.Idle == name)
                {
                    // Stop the animation for the spider
                    animation.Stop();
                    // E x i t
                    return;
                }
                // Play the animation by name
                animation.Play(name);
                break;
        }
    }

#if UNITY_EDITOR
    //
    // O n l y  E d i t o r
    //
    public void FindDependencies()
    {
        _dependencies.animation = GetComponentInChildren<Animation>();
        // Find the enemy manager on scene
        _dependencies.manager = FindObjectOfType<EnemyManager>();
        // Get the nav mesh agent
        _dependencies.navigation = GetComponent<UnityEngine.AI.NavMeshAgent>();
        // Get the the skin renderer on our children
        _dependencies.renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        // Get the state machine component
        _dependencies.stateMachine = GetComponent<EnemyStateMachine>();
        // Get the stats component
        _dependencies.stats = GetComponent<Stats>();
        // Get the weapon
        _dependencies.weapon = GetComponent<EnemyWeapon>();

        EditorUtility.SetDirty(gameObject);
    }

    // We have a spreadsheet with a bunch of values in it; this'll get them for us
    public void OutputSpreadsheetValues()
    {
    	string[] column = new string[12];
    	column[0] = name;									// Prefab's name
    	column[1] = distance.stop.ToString();				// Don't come closer range
		column[2] = distance.attack.ToString();				// shoot range
		column[3] = distance.follow.ToString();				// attack range
		column[4] = distance.listen.ToString();	            // listen range
		column[5] = stats.MaxHealth.ToString();				// hit points
		column[6] = weapon.range.ToString();				// range
		column[7] = weapon.fireRate.ToString();				// fire rate
		column[8] = weapon.burstShots.ToString();			// burst shots
		column[9] = weapon.randomShots.ToString();			// random shots
		column[10] = weapon.force.ToString();				// force
		column[11] = weapon.damage.ToString();				// damage

		Debug.Log(string.Join("\t", column));
    }

#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Enemy)), CanEditMultipleObjects]
public class E_Enemy : Editor
{
    public Enemy I;

    void OnEnable()
    {
        if (!I) I = target as Enemy;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!I) return;
        // H O R I Z O N T A L  group
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Find Dependencies"))
            {
                I.FindDependencies();
            }

            if (GUILayout.Button("Output Spreadsheet Values"))
            {
                foreach (var t in targets)
                {
                    (t as Enemy).OutputSpreadsheetValues();
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif
