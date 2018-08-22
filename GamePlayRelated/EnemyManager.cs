#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    //
    // EnemyManager
    // structs & classes

    // • • • • • • • • • • • • • • • • • • • • //

    //
    // V a r i a b l e s
    //

    private FPSPlayer player;
	public killstreak killstreakCounter;

    [SerializeField]
    private Enemy[] enemies;
    [SerializeField]
    private CubePickupManager cubeManager;

    // • • • • • • • • • • • • • • • • • • • • //

    //
    // P r o p e r t i e s
    //

    public int Children
    {
        get { return transform.childCount; }
    }

    public Enemy[] Enemies
    {
        get { return enemies; }
        set { enemies = value; }
    }

    public CubePickupManager CubeManager
    {
        get { return cubeManager; }
    }

    public Transform this[int i]
    {
        get
        {
            return transform.GetChild(i);
        }
    }

    // • • • • • • • • • • • • • • • • • • • • //

    //
    // U n i t y
    //

    void Awake()
    {
        Locator.Add(this, true);
    }

    void Start()
    {
        // Get the player from the service locator
        if (!player) player = Locator.Get<FPSPlayer>();
        // Find the cube manager if we don't have it
        if (null == cubeManager){ FindCubeManager(); }
        // Get
        if (null == enemies || 0 == enemies.Length)
        {
            FindEnemies();
        }

        SetEnemies();
    }

	// • • • • • • • • • • • • • • • • • • • • //

	//
	// U s e r
	//

    public void SetEnemies()
    {
#if UNITY_EDITOR
        if (!player) player = FindObjectOfType<FPSPlayer>();
        if (!player) 
        {
        	Debug.LogError("Could not find the player in this level");
        	return;
        }
#else
        // Get the player from the service locator
        if(!player) player = Locator.Get<FPSPlayer>();
#endif
        // E x i t
        if (null == enemies || 0 == enemies.Length) { return; }
        // Get the player collider
        Collider playerCollider = player ? player.GetComponent<Collider>() : null;

        foreach (Enemy enemy in enemies)
        {
            // C o n t i n u e
            if (enemy.set) { continue; }
            // Set the manager
            enemy.manager = this;
            // Set the target
            enemy.target = player.transform;
            // Set the target bounds
            enemy.targetBounds = (null != playerCollider) ? playerCollider.bounds : default(Bounds);
            // Debug
            DebugEnemy(enemy);
#if UNITY_EDITOR
            EditorSceneManagement.MarkSceneDirty(SceneManagement.Active);
            EditorUtility.SetDirty(enemy);
#endif
        }
    }

    public void FindEnemies()
    {
        enemies = GetComponentsInChildren<Enemy>();
#if UNITY_EDITOR
        EditorSceneManagement.MarkSceneDirty(SceneManagement.Active);
        EditorUtility.SetDirty(this);
#endif
    }

    public void FindCubeManager()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            cubeManager = FindObjectOfType<CubePickupManager>();
        }
        else
        {
#endif
        cubeManager = Locator.Get<CubePickupManager>();
#if UNITY_EDITOR
        }
        EditorSceneManagement.MarkSceneDirty(SceneManagement.Active);
        EditorUtility.SetDirty(this);
#endif
    }

    private void DebugEnemy(Enemy enemy)
    {
        //Debug.LogFormat("<color={0}> {1} set: {2} </color>", enemy.set ? "green" : "red", enemy, enemy.set);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EnemyManager))]
public class E_EnemyManager : Editor
{
    public EnemyManager I;

    void OnEnable()
    {
        if (!I) I = target as EnemyManager;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!I) return;

        if(null != I.Enemies && 0 < I.Enemies.Length)
        {
            if (GUILayout.Button("Set up enemies"))
                I.SetEnemies();
        }

        if (GUILayout.Button("Get enemy components on children"))
            I.FindEnemies();

        if (GUILayout.Button("Get Cube Manager"))
            I.FindCubeManager();
    }
}
#endif
