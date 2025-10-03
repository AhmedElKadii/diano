using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private LayerMask visionObstructionLayers;
    
    [Header("Performance Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    
    [Header("Attack Settings")]
    [SerializeField] private float lungeForce = 10f;
    [SerializeField] private float attackWindupTime = 0.2f; // Quick windup
    [SerializeField] private float attackStrikeTime = 0.15f; // Fast strike
    [SerializeField] private float attackRecoveryTime = 0.2f; // Recovery
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float retreatDistance = 3f; // How far to back up after attack
    [SerializeField] private float damageRange = 2.5f; // Range to check if player is close enough to damage

    
    [Header("Attack Visual Settings")]
    [SerializeField] private float windupScale = 0.7f; // Shrink before attack
    [SerializeField] private float strikeScale = 1.8f; // Pop big on attack

    [Header("Cosmetics")]
	[SerializeField] private GhostHat ghostHat;
    
    // Components
    private NavMeshAgent agent;
    private Rigidbody rb;
    
    // Cached references
    private Transform playerTransform;
    private GameManager gameManager;
    
    // State tracking
    private bool isChasing;
    private bool isAttacking;
    private bool canAttack = true;
    private float nextUpdateTime;
    private Vector3 originalScale;
    
    private void Awake()
    {
        // Get components in Awake for better initialization order
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        
        // Store original scale
        originalScale = transform.localScale;
        
        // Setup rigidbody if it exists
        if (rb != null)
        {
            rb.isKinematic = true; // NavMeshAgent controls movement normally
        }
    }
    
    private void Start()
    {
        InitializeReferences();
    }
    
    private void InitializeReferences()
    {
        gameManager = GameManager.Instance;
        
        if (gameManager != null && gameManager.player != null)
        {
            playerTransform = gameManager.player.transform;
        }
        else
        {
            Debug.LogWarning($"Enemy '{name}' couldn't find player reference in GameManager");
        }
    }
    
    private void Update()
    {
        // Early exit if references aren't valid
        if (!IsInitialized()) return;
        
        // Update at intervals instead of every frame for performance
        if (Time.time < nextUpdateTime) return;
        nextUpdateTime = Time.time + updateInterval;
        
        if (!isAttacking)
        {
            UpdateBehavior();
        }
    }
    
    private bool IsInitialized()
    {
        return playerTransform != null && gameManager != null && agent != null;
    }
    
    private void UpdateBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool canSeePlayer = CanSeePlayer(distanceToPlayer);
        
        // Determine if we should chase
        bool shouldChase = (canSeePlayer && distanceToPlayer <= detectionRange) || 
                          distanceToPlayer <= chaseRange;
        
        if (shouldChase)
        {
            StartChasing();
        }
        else
        {
            StopChasing();
        }
    }
    
    private bool CanSeePlayer(float distance)
    {
        // Outside detection range
        if (distance > detectionRange) return false;
        
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        // Check if player is within vision cone
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > visionAngle * 0.5f) return false;
        
        // Raycast to check for obstructions
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f; // Eye level
        
        if (Physics.Raycast(rayOrigin, directionToPlayer, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.CompareTag("Player");
        }
        
        return false;
    }
    
    private void StartChasing()
    {
        if (!isChasing)
        {
            isChasing = true;
        }
        
        // Update destination
        if (agent.isOnNavMesh && agent.isActiveAndEnabled)
        {
            agent.SetDestination(playerTransform.position);
        }
    }
    
    private void StopChasing()
    {
        if (isChasing)
        {
            isChasing = false;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!canAttack || isAttacking) return;
        
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            StartCoroutine(AttackPlayer(playerController));
        }
    }
    
    private bool IsPlayerInDamageRange()
    {
        if (playerTransform == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= damageRange;
    }
    
    private IEnumerator AttackPlayer(PlayerController playerController)
    {
        isAttacking = true;
        canAttack = false;
        
        // Disable NavMeshAgent during attack
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        // Calculate lunge direction
        Vector3 lungeDirection = (playerTransform.position - transform.position).normalized;
        lungeDirection.y = 0; // Keep it horizontal
        
        // Enable physics for lunge
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        
        // Phase 1: Quick shrink (windup/anticipation)
        float elapsed = 0f;
        while (elapsed < attackWindupTime)
        {
            float t = elapsed / attackWindupTime;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * windupScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Phase 2: STRIKE - Pop big and lunge!
        if (rb != null)
        {
            rb.AddForce(lungeDirection * lungeForce, ForceMode.Impulse);
        }
        
        elapsed = 0f;
        while (elapsed < attackStrikeTime)
        {
            float t = elapsed / attackStrikeTime;
            // Quick snap to big size
            transform.localScale = Vector3.Lerp(originalScale * windupScale, originalScale * strikeScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Check if player is still in range before dealing damage
        if (IsPlayerInDamageRange())
        {
            playerController.TakeDamage(35);
            Debug.Log("Player hit by enemy attack!");
        }
        else
        {
            Debug.Log("Player dodged the attack!");
        }
        
        // Phase 3: Quick recovery back to normal
        elapsed = 0f;
        while (elapsed < attackRecoveryTime)
        {
            float t = elapsed / attackRecoveryTime;
            transform.localScale = Vector3.Lerp(originalScale * strikeScale, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we're back to original scale
        transform.localScale = originalScale;
        
        // Re-enable NavMeshAgent
        if (rb != null)
        {
            // Stop velocity BEFORE making it kinematic
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        if (agent != null)
        {
            agent.enabled = true;
            
            // Calculate retreat position - back away from player
            Vector3 directionAwayFromPlayer = (transform.position - playerTransform.position).normalized;
            Vector3 retreatPosition = transform.position + directionAwayFromPlayer * retreatDistance;
            
            // Make sure the retreat position is on the NavMesh
            if (NavMesh.SamplePosition(retreatPosition, out NavMeshHit hit, retreatDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        
        isAttacking = false;
        
        // Cooldown before next attack
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void Die()
    {
        agent.enabled = false;
        if (rb != null) { rb.isKinematic = false; }

		if (ghostHat != null)
		{
			ghostHat.transform.parent = null;
			ghostHat.GetComponent<Rigidbody>().isKinematic = false;
		}

        GetComponent<DissolveController>().AnimateDissolve(1f, 0.5f);
        Destroy(gameObject, 1f);
    }
    
    // Optional: Visualize detection ranges in editor
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Chase range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Damage range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, damageRange);
        
        // Vision cone
        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle * 0.5f, 0) * transform.forward * detectionRange;
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
