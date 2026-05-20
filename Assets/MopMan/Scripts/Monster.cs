using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Rooms;

public class MonsterPatrol : MonoBehaviour 
{
    [Header("Game Settings")]
    public int playerLives = 3;
    private int startingLives; 

    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float patrolSpeed = 2f;

    [Header("Chase Settings (Dynamic)")]
    public string vrPlayerTag = "Player"; 
    public float sightDistance = 15f;
    public float chaseSpeed = 4.5f;

    [Header("INSIDE Player Teleport (VR)")]
    public Transform mazeEntrancePoint;
    public Transform startingRoomVR;

    [Header("ABOVE Player Teleport (Desktop)")]
    public string topPlayerTag = "DesktopPlayer";
    public Transform startingRoomDesktop;

    private NavMeshAgent agent;
    private bool isChasing = false;
    
    private Transform currentPlayerTarget; 
    private float searchTimer = 0f;
    private float lastHitTime = 0f; 

    private List<int> pointsToVisit = new List<int>();
    private int currentWaypoint = -1;
    private int previousWaypoint = -1;

    // --- UBIQ NETWORK VARIABLES ---
    private NetworkContext context;
    private RoomClient roomClient;
    private bool isHost = false; 
    private bool isNetworkReady = false;

    private struct MonsterMsg
    {
        public bool isTransformUpdate;
        public Vector3 position;
        public Quaternion rotation;

        public bool isHitEvent;
        public int livesLeft;
    }

    void Start()
    {
        startingLives = playerLives; 
        agent = GetComponent<NavMeshAgent>();

        context = NetworkScene.Register(this);
        isNetworkReady = true;
        
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
        
        if (roomClient != null)
        {
            roomClient.OnPeerAdded.AddListener(OnPeerAdded);
            roomClient.OnPeerRemoved.AddListener(OnPeerRemoved);
            roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);
            
            CheckHost();
        }
        else
        {
            UpdateHostState(true);
        }

        if (isHost && waypoints.Length >= 3)
        {
            GoToNextWaypoint();
        }
    }

    void CheckHost()
    {
        if (roomClient.Me == null || string.IsNullOrEmpty(roomClient.Me.uuid))
        {
            UpdateHostState(true);
            return;
        }

        var allUuids = new List<string> { roomClient.Me.uuid };
        
        foreach (var p in roomClient.Peers) 
        {
            allUuids.Add(p.uuid);
        }
        
        allUuids.Sort();
        bool amIHost = (allUuids[0] == roomClient.Me.uuid);
        
        UpdateHostState(amIHost);
    }

    void UpdateHostState(bool hostStatus)
    {
        isHost = hostStatus;
        
        if (agent != null)
        {
            agent.enabled = isHost;
        }
    }

    void OnJoinedRoom(IRoom room) => CheckHost();
    void OnPeerAdded(IPeer peer) => CheckHost();
    void OnPeerRemoved(IPeer peer) => CheckHost();

    // --- UBIQ MESSAGE RECEIVING ---
    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<MonsterMsg>();

        if (data.isTransformUpdate && !isHost)
        {
            transform.position = data.position;
            transform.rotation = data.rotation;
        }

        if (data.isHitEvent)
        {
            if (playerLives > data.livesLeft)
            {
                playerLives = data.livesLeft;
                lastHitTime = Time.time; 
                HandleLifeLossLocally();
            }
        }
    }

    // --- UBIQ MESSAGE SENDING ---
    private void FixedUpdate()
    {
        if (isHost && isNetworkReady) 
        {
            context.SendJson(new MonsterMsg 
            { 
                isTransformUpdate = true, 
                position = transform.position, 
                rotation = transform.rotation 
            });
        }
    }

    // --- AI LOGIC ---
    void Update()
    {
        if (!isHost) return;

        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            FindDynamicPlayer();
            searchTimer = 1f;
        }

        if (playerLives <= 0 || waypoints.Length < 3) return;

        bool playerIsVisible = (currentPlayerTarget != null) && SeesPlayer();

        if (playerIsVisible)
        {
            isChasing = true;
            agent.speed = chaseSpeed;

            agent.stoppingDistance = 0f;
            
            if (Vector3.Distance(agent.destination, currentPlayerTarget.position) > 0.1f)
            {
                agent.SetDestination(currentPlayerTarget.position);
            }
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                agent.speed = patrolSpeed;
                if (currentWaypoint != -1) agent.SetDestination(waypoints[currentWaypoint].position);
            }

            if (!isChasing && !agent.pathPending && agent.remainingDistance < 0.2f)
            {
                GoToNextWaypoint();
            }
        }
    }

    void FindDynamicPlayer()
    {
        GameObject[] possiblePlayers = GameObject.FindGameObjectsWithTag(vrPlayerTag);
        
        foreach (GameObject obj in possiblePlayers)
        {
            if (obj != this.gameObject)
            {
                currentPlayerTarget = obj.transform;
                return;
            }
        }

        currentPlayerTarget = null;
        if (isChasing)
        {
            isChasing = false;
            agent.speed = patrolSpeed;
            if (currentWaypoint != -1) agent.SetDestination(waypoints[currentWaypoint].position);
        }
    }

    bool SeesPlayer()
    {
        if (currentPlayerTarget == null) return false;

        Vector3 monsterEyes = transform.position + Vector3.up * 1.0f;
        Vector3 playerChest = currentPlayerTarget.position + Vector3.up * 1.0f;
        
        float distanceToPlayer = Vector3.Distance(monsterEyes, playerChest);
        Vector3 directionToPlayer = (playerChest - monsterEyes).normalized;

        if (distanceToPlayer <= sightDistance)
        {
            RaycastHit hit;
            if (Physics.SphereCast(monsterEyes, 0.4f, directionToPlayer, out hit, sightDistance))
            {
                if (hit.collider.CompareTag(vrPlayerTag) || hit.collider.CompareTag("MainCamera"))
                {
                    return true;
                }
            }
        }
        return false; 
    }

    void GoToNextWaypoint()
    {
        if (pointsToVisit.Count == 0)
        {
            for (int i = 0; i < waypoints.Length; i++) pointsToVisit.Add(i);
        }

        List<int> validChoices = new List<int>(pointsToVisit);

        if (validChoices.Contains(currentWaypoint)) validChoices.Remove(currentWaypoint);
        if (validChoices.Count > 1 && validChoices.Contains(previousWaypoint)) validChoices.Remove(previousWaypoint);

        int chosenIndex = Random.Range(0, validChoices.Count);
        int nextPoint = validChoices[chosenIndex];

        previousWaypoint = currentWaypoint;
        currentWaypoint = nextPoint;

        pointsToVisit.Remove(nextPoint);
        agent.SetDestination(waypoints[currentWaypoint].position);
    }

    void ExecuteTeleport(Transform playerToMove, Transform destination)
    {
        if (destination == null || playerToMove == null) return;

        CharacterController cc = playerToMove.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; 
        
        playerToMove.position = destination.position;
        playerToMove.rotation = destination.rotation;
        
        if (cc != null) cc.enabled = true; 
    }

    void TeleportLocalPlayer(Transform vrDestination, Transform desktopDestination)
    {
        if (Camera.main == null) return;

        Transform myRoot = Camera.main.transform.root;

        if (myRoot.CompareTag(vrPlayerTag) && vrDestination != null)
        {
            ExecuteTeleport(myRoot, vrDestination);
        }
        else if (myRoot.CompareTag(topPlayerTag) && desktopDestination != null)
        {
            ExecuteTeleport(myRoot, desktopDestination);
        }
    }

    // --- LOCAL HIT LOGIC ---
    void HandleLifeLossLocally()
    {
        if (playerLives > 0)
        {
            Debug.Log("Player caught! Lives remaining: " + playerLives);
            
            TeleportLocalPlayer(mazeEntrancePoint, null);

            if (isHost)
            {
                isChasing = false;
                agent.speed = patrolSpeed;
                GoToNextWaypoint();
            }
        }
        else
        {
            Debug.LogWarning("GAME OVER!");

            TeleportLocalPlayer(startingRoomVR, startingRoomDesktop);

            if (isHost && agent != null) agent.isStopped = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerLives <= 0) return;

        if (Time.time - lastHitTime < 2.0f) return;

        if (other.CompareTag(vrPlayerTag) || other.CompareTag("MainCamera"))
        {
            lastHitTime = Time.time;
            playerLives--;
            
            HandleLifeLossLocally();

            if (isNetworkReady)
            {
                context.SendJson(new MonsterMsg 
                { 
                    isHitEvent = true, 
                    livesLeft = playerLives 
                });
            }
        }
    }

    public void ResetMonster()
    {
        playerLives = startingLives;
        lastHitTime = 0f; 
        
        if (isHost)
        {
            if (agent != null) agent.isStopped = false;
            isChasing = false;
            agent.speed = patrolSpeed;
            FindDynamicPlayer();
            GoToNextWaypoint();
        }
        Debug.Log("Monster Reset! Ready for a new game.");
    }
}