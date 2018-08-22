using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.Serialization;

// We don't care about storing these items in hash tables, so don't plan to implement GetHashCode
#pragma warning disable 659

public class BaseEnemy : MonoBehaviour
{
    //
    // BaseEnemy
    // structs & classes

    [System.Serializable]
    public class Properties
    {
        public FloatGroupRange moveRange    = new FloatGroupRange(1, 2, 3);
        public FloatGroupRange lookRange    = new FloatGroupRange(1.5f, 2);
        public FloatGroupRange attackRange  = new FloatGroupRange(0.5f, 1f, 2f);
        public Enemy.Type type = Enemy.Type.Human;
        public LayerMask sightMask;
		
		public override bool Equals (object obj)
		{
			return Equals((Properties)obj);
		}
		
		public bool Equals(Properties other)
		{
			if (other == null) return false;
			
			return moveRange.Equals(other.moveRange)
			    && lookRange.Equals(other.lookRange)
			    && attackRange.Equals(other.attackRange)
			    && type.Equals(other.type)
			    && sightMask.Equals(other.sightMask);
		}
    }

    [System.Serializable]
    public class Behaviour
    {
        private int currentWaypoint;
        //private int patrolDirection = 1;

        public bool attack = true;
        public bool explode = false;
        public Enemy.BehaviourType type;
        //public Vector3[] waypoint;

        //public void SetPatrol(int direction)
        //{
        //    // E x i t
        //    if (null == waypoint) return;
        //}

		public override bool Equals(object obj)
		{
			return Equals((Behaviour)obj);
		}

		public bool Equals(Behaviour other)
		{
			if (other == null) return false;
			
			return attack.Equals(other.attack)
			    && explode.Equals(other.explode)
			    && type.Equals(other.type);
		}
    }

    [System.Serializable]
    public class Speed
    {
        public float walk = 1;
        public float run = 6;
        [ReadOnly]
        public float rotation = 5;
        
		public override bool Equals(object obj)
		{
			return Equals((Speed)obj);
		}
		
		public bool Equals(Speed other)
		{
			if (other == null) return false;

            return walk == other.walk
                && run == other.run
			    && rotation == other.rotation; 
		}
		
    }

    [System.Serializable]
    public struct Audio
    {
        // The clips that the audio source will play
        public AudioClip deadClip;
        
		public override bool Equals(object obj)
		{
			return Equals((Audio)obj);
		}
		
		public bool Equals(Audio other)
		{
			//if (other == null) return false;	// structs are never null
			
			return deadClip.Equals(other.deadClip);
		}
		
	}
	
	[System.Serializable]
    public class Distance
    {
        public float stop = 2;
        public float attack = 5;
        public float follow = 20;
        public float listen = 30;

        public float pow2Stop
        {
            get { return stop * stop; }
        }

        public float pow2Attack
        {
            get { return attack * attack; }
        }

        public float pow2Follow
        {
            get { return follow * follow; }
        }

        public float pow2Listen
        {
			get { return listen * listen; }
        }
        
		public override bool Equals(object obj)
		{
			return Equals((Distance)obj);
		}
		
		public bool Equals(Distance other)
		{
			if (other == null) return false;
			
			return attack == other.attack && follow == other.follow && stop == other.stop && listen == other.listen;
		}
	}
	
	[System.Serializable]
    public struct Dependencies
    {
        public Transform eyePoint;
        public GameObject ragdollPrefab;
		public GameObject ragdollFlyPrefab;
        [ReadOnly]
        public Animation animation;
        [ReadOnly]
        #if UNITY_5_6
        public UnityEngine.AI.NavMeshAgent navigation;
        #else
        public UnityEngine.AI.NavMeshAgent navigation;
        #endif
        [ReadOnly]
        public Stats stats;
        [ReadOnly]
        public EnemyWeapon weapon;
        public EnemyManager manager;
        public ExplosiveObject explosive;
        [ReadOnly]
        public EnemyStateMachine stateMachine;
        
        public Renderer renderer;
        
        
		public override bool Equals(object obj)
		{
			return Equals((Dependencies)obj);
		}
		
		public bool Equals(Dependencies other)
		{
			//if (other == null) return false; // structs are never null
			
			return eyePoint.Equals(other.eyePoint)
			    && ragdollPrefab.Equals(other.ragdollPrefab)
			    && animation.Equals(other.animation)
			    && navigation.Equals(other.navigation)
			    && stats.Equals(other.stats)
			    && manager.Equals(other.manager)
			    && stateMachine.Equals(other.stateMachine)
			    && renderer.Equals(other.renderer);
		}
	}
	
	// • • • • • • • • • • • • • • • • • • • • //

    //
    // V a r i a b l e s
    //

    [HideInInspector]
    public bool isFollowing;
    [HideInInspector]
    public bool isAttacking;
    [HideInInspector]
    public bool isRoaming;
    [HideInInspector]
    public bool onFollowRange;
    [HideInInspector]
    public bool onAttackRange;
    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public Vector3 alert;
    [HideInInspector]
    public Vector3 targetPosition;
    [HideInInspector]
    public Bounds targetBounds;

    protected Vector3 spawnPoint;
    protected Collider _collider;
    protected UnityAction onAttackOver;
    protected Coroutine damageCoroutine;

    [SerializeField]
    protected Enemy.State _state;
    [SerializeField, ReadOnly]
    protected Properties _properties = new Properties();
    [SerializeField]
    protected Behaviour _behaviour = new Behaviour();
    [SerializeField]
    protected Speed _speed = new Speed();
    [SerializeField]
    protected Audio _audio;
    [SerializeField]
    protected Distance _distance = new Distance();
    [SerializeField]
    protected Dependencies _dependencies;

    // • • • • • • • • • • • • • • • • • • • • //

    //
    // P r o p e r t i e s
    //

    public bool set
    {
        get { return target && default(Bounds) != targetBounds && manager; }
    }

    public bool attack
    {
        get { return _behaviour.attack; }
    }

    public string CurrentAnimation
    {
        get
        {
            foreach (AnimationState state in animation)
            {
                if (animation.IsPlaying(state.name)) { return state.name; }
            }

            return "";
        }
    }

    public Vector3 position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public Vector3 SpawnPoint
    {
        get { return spawnPoint; }
    }

    public Vector3 velocity
    {
        get { return navigation.velocity; }
        set { navigation.velocity = value; }
    }

    public new Collider collider
    {
        get { return _collider; }
        protected set { _collider = value; }
    }

    public Quaternion rotation
    {
        get { return transform.rotation; }
        set { transform.rotation = value; }
    }

    public Enemy.State state
    {
        get { return _state; }
        set { _state = value; }
    }

    public Properties properties
    {
        get { return _properties; }
    }

    public GameObject ragdollPrefab
    {
        get { return _dependencies.ragdollPrefab; }
        set { _dependencies.ragdollPrefab = value; }
    }

	public GameObject ragdollFlyPrefab
	{
		get { return _dependencies.ragdollFlyPrefab; }
		set { _dependencies.ragdollFlyPrefab = value; }
	}

    public EnemyManager manager
    {
        get { return _dependencies.manager; }
        set { _dependencies.manager = value; }
    }

    public EnemyStateMachine stateMachine
    {
        get { return _dependencies.stateMachine; }
    }

    public new Audio audio
    {
        get { return _audio; }
    }

    public new Animation animation
    {
        get { return _dependencies.animation; }
    }
#if UNITY_5_6
    public UnityEngine.AI.NavMeshAgent navigation
#else
    public UnityEngine.AI.NavMeshAgent navigation
#endif
    {
        get { return _dependencies.navigation; }
    }

    public new Renderer renderer
    {
        get { return _dependencies.renderer; }
    }

    public ExplosiveObject explosion
    {
        get { return _dependencies.explosive; }
    }
    
    public Stats stats
    {
        get { return _dependencies.stats; }
    }

    public Speed speed
    {
        get { return _speed; }
    }

    public float walk
    {
        get { return _speed.walk; }
        set { _speed.walk = value; }
    }

    public float run 
    {
        get { return _speed.run; }
        set { _speed.run = value; }
    }

    public Distance distance
    {
        get { return _distance; }
    }

    public Behaviour behaviour
    {
        get { return _behaviour; }
    }

    public FloatGroupRange moveRange
    {
        get { return _properties.moveRange; }
    }

    public FloatGroupRange lookRange
    {
        get { return _properties.lookRange; }
    }

    public FloatGroupRange attackRange
    {
        get { return _properties.attackRange; }
    }

    public Enemy.Type type
    {
        get { return _properties.type; }
    }

    public EnemyWeapon weapon
    {
        get { return _dependencies.weapon; }
    }

    public Transform eyePoint
    {
        get { return _dependencies.eyePoint; }
    }

    public Vector3 eyePointPosition
    {
        get { return _dependencies.eyePoint.position; }
    }

    // • • • • • • • • • • • • • • • • • • • • //

    //
    // U n i t y
    //

    // • • • • • • • • • • • • • • • • • • • • //

    //
    // U s e r
    //
}
