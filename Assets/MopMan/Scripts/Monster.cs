using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Rooms;

public class MonsterPatrol : MonoBehaviour
{
    [Header("Game Settings")]
    public int playerLives = 3;
    private int startingLives; // To remember max lives when resetting

    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float patrolSpeed = 2f;

    [Header("Chase Settings (Dynamic)")]
    [Tooltip("Tag assigned to the VR player inside the maze")]
    public string vrPlayerTag = "Player"; // <--- NO MORE DRAG & DROP
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
    
    // Internal reference to the current VR Player
    private Transform currentPlayerTarget; 
    private float searchTimer = 0f;

    // Variables for intelligent random movement
    private List<int> pointsToVisit = new List<int>();
    private int currentWaypoint = -1;
    private int previousWaypoint = -1;

    NetworkContext context;
    RoomClient roomClient;
    bool isHost = false;
    void Start()
    {
        startingLives = playerLives; // Save initial lives for resets
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        context = NetworkScene.Register(this);
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
        
        roomClient.OnPeerAdded.AddListener(OnPeerAdded);
        roomClient.OnPeerRemoved.AddListener(OnPeerRemoved);
        roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);
        

        if (waypoints.Length >= 3)
        {
            GoToNextWaypoint();
        }
    }

void CheckHost()
{
    var allUuids = new List<string>();
    allUuids.Add(roomClient.Me.uuid);
    foreach (var p in roomClient.Peers) allUuids.Add(p.uuid);
    
    allUuids.Sort();
    isHost = allUuids[0] == roomClient.Me.uuid;
    
    Debug.Log($"Sono host? {isHost}");
}
void OnJoinedRoom(IRoom room) => CheckHost();
void OnPeerAdded(IPeer peer) => CheckHost();
void OnPeerRemoved(IPeer peer) => CheckHost();

    public void ProcessMessage (ReferenceCountedSceneGraphMessage msg)
    {
        
    }

    void Update()
    {
       var found = FindObjectsOfType<MonsterPatrol>();
Debug.Log($"Trovati: {found.Length}");

        // 1. DYNAMIC PLAYER SEARCH 
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            FindDynamicPlayer();
            searchTimer = 1f;
        }

        // 2. Critical Stop
        if (playerLives <= 0 || waypoints.Length < 3) return;

        // 3. Check if we have a target AND we can see it
        bool playerIsVisible = (currentPlayerTarget != null) && SeesPlayer();

        // --- PRIORITY LOGIC: CHASE VS PATROL ---
        if (playerIsVisible)
        {
            isChasing = true;
            agent.speed = chaseSpeed;
            
            // --- THE STUTTER FIX ---
            // Only update the destination if the player has moved at least 0.5 meters.
            // This prevents the NavMeshAgent from resetting its path 60 times a second,
            // allowing it to smoothly calculate curves around corners!
            if (Vector3.Distance(agent.destination, currentPlayerTarget.position) > 0.5f)
            {
                agent.SetDestination(currentPlayerTarget.position);
            }
            
            Debug.DrawLine(transform.position, currentPlayerTarget.position, Color.yellow);
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                agent.speed = patrolSpeed;
                
                // Return to the last waypoint
                if (currentWaypoint != -1) agent.SetDestination(waypoints[currentWaypoint].position);
            }

            // Normal patrol
            if (!isChasing && !agent.pathPending && agent.remainingDistance < 0.2f)
            {
                GoToNextWaypoint();
            }
        }
    }


    // --- NEW FUNCTION: Dynamically find who is playing VR right now ---
    void FindDynamicPlayer()
    {
        GameObject vrObj = GameObject.FindGameObjectWithTag(vrPlayerTag);
        
        if (vrObj != null)
        {
            currentPlayerTarget = vrObj.transform;
            // Uncomment the line below if you want continuous confirmation in the console
            // Debug.Log("Scanner: FOUND! Now tracking: " + vrObj.name);
        }
        else
        {
            // If the player left or swapped, clear the target and stop chasing
            currentPlayerTarget = null;
            
            if (isChasing)
            {
                isChasing = false;
                agent.speed = patrolSpeed;
                if (currentWaypoint != -1) agent.SetDestination(waypoints[currentWaypoint].position);
            }
        }
    }

    // --- FLAWLESS 360 DEGREE VISION ---
    bool SeesPlayer()
    {
        // Safety check
        if (currentPlayerTarget == null) return false;

        // 1. Raise the aim to chest level (1.0f) so it doesn't hit the floor
        Vector3 monsterEyes = transform.position + Vector3.up * 1.0f;
        Vector3 playerChest = currentPlayerTarget.position + Vector3.up * 1.0f;
        
        // 2. Calculate distance and direction dynamically (this creates 360-degree awareness)
        float distanceToPlayer = Vector3.Distance(monsterEyes, playerChest);
        Vector3 directionToPlayer = (playerChest - monsterEyes).normalized;

        // 3. Check if player is within range
        if (distanceToPlayer <= sightDistance)
        {
            RaycastHit hit;
            // SphereCast acts like a thick laser (radius 0.4f). It's very accurate and ignores small bumps.
            if (Physics.SphereCast(monsterEyes, 0.4f, directionToPlayer, out hit, sightDistance))
            {
                // If the thick laser hits the player before a wall, the monster sees you!
                if (hit.collider.CompareTag(vrPlayerTag) || hit.collider.CompareTag("MainCamera"))
                {
                    // Draw a red line in the Scene view to prove it sees you
                    Debug.DrawLine(monsterEyes, hit.point, Color.red);
                    return true;
                }
            }
        }
        
        return false; // Player is too far or hidden behind a wall
    }

    void GoToNextWaypoint()
    {
        // Refill the bag of points if it's empty
        if (pointsToVisit.Count == 0)
        {
            for (int i = 0; i < waypoints.Length; i++) pointsToVisit.Add(i);
        }

        List<int> validChoices = new List<int>(pointsToVisit);

        // Rules: Do not return to current or previous waypoint
        if (validChoices.Contains(currentWaypoint)) validChoices.Remove(currentWaypoint);
        if (validChoices.Count > 1 && validChoices.Contains(previousWaypoint)) validChoices.Remove(previousWaypoint);

        // Pick a random valid point
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

        // Safely disable CharacterController during teleport to prevent physics glitches
        CharacterController cc = playerToMove.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; 
        
        playerToMove.position = destination.position;
        playerToMove.rotation = destination.rotation;
        
        if (cc != null) cc.enabled = true; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerLives <= 0) return;

        if (other.CompareTag(vrPlayerTag) || other.CompareTag("MainCamera"))
        {
            playerLives--;

            if (playerLives > 0)
            {
                Debug.Log("Player caught! Lives remaining: " + playerLives);
                
                ExecuteTeleport(other.transform.root, mazeEntrancePoint);

                // Stop chasing and go back to patrol
                isChasing = false;
                agent.speed = patrolSpeed;
                GoToNextWaypoint();
            }
            else
            {
                Debug.LogWarning("GAME OVER!");

                // Teleport VR Player
                ExecuteTeleport(other.transform.root, startingRoomVR);

                // Dynamically find and teleport Desktop Player
                GameObject topPlayer = GameObject.FindGameObjectWithTag(topPlayerTag);
                if (topPlayer != null)
                {
                    ExecuteTeleport(topPlayer.transform.root, startingRoomDesktop);
                }

                // Stop the monster completely
                agent.isStopped = true;
            }
        }
    }

    // --- NEW PUBLIC FUNCTION: CALL THIS WHEN A NEW GAME STARTS ---
    public void ResetMonster()
    {
        playerLives = startingLives;
        agent.isStopped = false;
        isChasing = false;
        agent.speed = patrolSpeed;
        FindDynamicPlayer();
        GoToNextWaypoint();
        Debug.Log("Monster Reset! Ready for a new game.");
    }
}