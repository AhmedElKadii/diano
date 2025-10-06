using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private LayerMask visionObstructionLayers;
    
    [Header("Aggro Settings")]
    [SerializeField] private float aggroDetectionRange = 25f;
    [SerializeField] private float aggroChaseRange = 15f;
    
    [Header("Performance Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    
    [Header("Attack Settings")]
    [SerializeField] private float lungeDistance = 2f;
    [SerializeField] private float attackWindupTime = 0.2f;
    [SerializeField] private float attackStrikeTime = 0.15f;
    [SerializeField] private float attackRecoveryTime = 0.2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float retreatDistance = 4f;
    [SerializeField] private float retreatSpeed = 8f;
    [SerializeField] private float damageRange = 2.5f;
    
    [Header("Attack Visual Settings")]
    [SerializeField] private float windupScale = 0.7f;
    [SerializeField] private float strikeScale = 1.8f;

    [Header("Cosmetics")]
    [SerializeField] private GhostHat ghostHat;
    [SerializeField] private List<GameObject> skins;

    private bool dead = false;
    private bool isAggroed = false;
    
    private NavMeshAgent agent;
    private Rigidbody rb;

    [HideInInspector]
    public EnemySpawner enemySpawner;
    
    private Transform playerTransform;
    private GameManager gameManager;
    
    private bool isChasing;
    private bool isAttacking;
    private bool canAttack = true;
    private float nextUpdateTime;
    private Vector3 originalScale;
    private Collider triggerCollider;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        triggerCollider = GetComponentInChildren<Collider>();
        
        originalScale = transform.localScale;
        
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (skins != null && skins.Count > 0)
        {
            int randomIndex = Random.Range(0, skins.Count);
            GameObject selectedSkin = Instantiate(skins[randomIndex], transform);
            selectedSkin.transform.localPosition = Vector3.zero;
            selectedSkin.transform.localRotation = Quaternion.identity;
			GetComponent<Entity>().dc = selectedSkin.GetComponent<DissolveController>();
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
        if (!IsInitialized()) return;
        
        if (Time.time < nextUpdateTime) return;
        nextUpdateTime = Time.time + updateInterval;
        
        if (!isAttacking)
        {
            UpdateBehavior();
        }

		transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));
    }
    
    private bool IsInitialized()
    {
        return playerTransform != null && gameManager != null && agent != null;
    }
    
    private void UpdateBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        float currentDetectionRange = isAggroed ? aggroDetectionRange : detectionRange;
        float currentChaseRange = isAggroed ? aggroChaseRange : chaseRange;
        
        bool canSeePlayer = CanSeePlayer(distanceToPlayer, currentDetectionRange);
        
        if (isAggroed && !canSeePlayer)
        {
            isAggroed = false;
            Debug.Log($"{name} lost aggro - no line of sight!");
        }
        
        bool shouldChase = (canSeePlayer && distanceToPlayer <= currentDetectionRange) || 
                          distanceToPlayer <= currentChaseRange;
        
        if (shouldChase)
        {
            StartChasing();
        }
        else
        {
            StopChasing();
        }
    }
    
    private bool CanSeePlayer(float distance, float range)
    {
        if (distance > range) return false;
        
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > visionAngle * 0.5f) return false;
        
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
        
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
    
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!canAttack || isAttacking) return;
        
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            StartCoroutine(AttackPlayer(playerController));
        }
    }
    
    private IEnumerator AttackPlayer(PlayerController playerController)
    {
        isAttacking = true;
        canAttack = false;
        
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
        
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        Vector3 lungeDirection = (playerTransform.position - transform.position).normalized;
        lungeDirection.y = 0;
        
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + lungeDirection * lungeDistance;
        
        // Windup
        float elapsed = 0f;
        while (elapsed < attackWindupTime)
        {
            float t = elapsed / attackWindupTime;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * windupScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Quick lunge forward
        elapsed = 0f;
        bool hitPlayer = false;
        
        while (elapsed < attackStrikeTime)
        {
            float t = elapsed / attackStrikeTime;
            
            transform.localScale = Vector3.Lerp(originalScale * windupScale, originalScale * strikeScale, t);
            
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, t);
            
            float distanceToPlayer = Vector3.Distance(newPosition, playerTransform.position);
            if (distanceToPlayer <= damageRange && !hitPlayer)
            {
                hitPlayer = true;
                playerController.TakeDamage(35);
                Debug.Log("Player hit by enemy attack!");
                break;
            }
            
            transform.position = newPosition;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!hitPlayer)
        {
            Debug.Log("Player dodged the attack!");
        }
        
        // Recovery
        elapsed = 0f;
        while (elapsed < attackRecoveryTime)
        {
            float t = elapsed / attackRecoveryTime;
            transform.localScale = Vector3.Lerp(originalScale * strikeScale, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = originalScale;
        
        if (agent != null)
        {
            agent.enabled = true;
        }
        
        isAttacking = false;
        
        Vector3 retreatDirection = (transform.position - playerTransform.position).normalized;
        retreatDirection.y = 0;
        
        float distanceTraveled = 0f;
        while (distanceTraveled < retreatDistance)
        {
            float step = retreatSpeed * Time.deltaTime;
            
            if (agent != null && agent.isOnNavMesh)
            {
                agent.Move(retreatDirection * step);
            }
            else
            {
                transform.position += retreatDirection * step;
            }
            
            distanceTraveled += step;
            yield return null;
        }
        
        yield return new WaitForSeconds(attackCooldown);
        
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
        
        canAttack = true;
    }
    
    public void TakeDamage(float damage)
    {
        if (dead) return;
        
        isAggroed = true;
        Debug.Log($"{name} is now aggroed!");
        
        if (!isChasing && !isAttacking)
        {
            StartChasing();
        }
    }

    public void Die()
    {
        if (dead) return;

		GameManager.Instance.score++;

        dead = true;
        agent.enabled = false;
        if (rb != null) { rb.isKinematic = false; }

        if (enemySpawner != null) enemySpawner.currentEnemies--;

        GetComponentInChildren<Collider>().enabled = false;

        ghostHat = GetComponentInChildren<GhostHat>();
        if (ghostHat != null && ghostHat.hat != null)
        {
            GameManager.Instance.trash.Add(ghostHat.hat);

            ghostHat.hat.transform.SetParent(null);

            Rigidbody hatRb = ghostHat.hat.GetComponent<Rigidbody>();
            if (hatRb != null) hatRb.isKinematic = false;

            foreach (Rigidbody rb in ghostHat.hat.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = false;
            }
        }

        foreach (DissolveController dc in GetComponentsInChildren<DissolveController>())
        {
            if (dc != null)
            {
                dc.AnimateDissolve(1f, 0.5f);
            }
        }

        Destroy(gameObject, 2f);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, damageRange);
        
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, aggroDetectionRange);
        
        Gizmos.color = new Color(1f, 0.2f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, aggroChaseRange);
        
        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle * 0.5f, 0) * transform.forward * detectionRange;
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
