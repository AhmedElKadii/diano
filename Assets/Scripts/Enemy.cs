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
    
    [Header("Performance Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    
    [Header("Attack Settings")]
    [SerializeField] private float lungeForce = 10f;
    [SerializeField] private float attackWindupTime = 0.2f;
    [SerializeField] private float attackStrikeTime = 0.15f;
    [SerializeField] private float attackRecoveryTime = 0.2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float retreatDistance = 3f;
    [SerializeField] private float damageRange = 2.5f;
    
    [Header("Attack Visual Settings")]
    [SerializeField] private float windupScale = 0.7f;
    [SerializeField] private float strikeScale = 1.8f;

    [Header("Cosmetics")]
	[SerializeField] private GhostHat ghostHat;
	[SerializeField] private List<GameObject> skins;
    
    private NavMeshAgent agent;
    private Rigidbody rb;
    
    private Transform playerTransform;
    private GameManager gameManager;
    
    private bool isChasing;
    private bool isAttacking;
    private bool canAttack = true;
    private float nextUpdateTime;
    private Vector3 originalScale;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        
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
    }
    
    private bool IsInitialized()
    {
        return playerTransform != null && gameManager != null && agent != null;
    }
    
    private void UpdateBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool canSeePlayer = CanSeePlayer(distanceToPlayer);
        
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
        if (distance > detectionRange) return false;
        
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
        
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        Vector3 lungeDirection = (playerTransform.position - transform.position).normalized;
        lungeDirection.y = 0;
        
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        
        float elapsed = 0f;
        while (elapsed < attackWindupTime)
        {
            float t = elapsed / attackWindupTime;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * windupScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (rb != null)
        {
            rb.AddForce(lungeDirection * lungeForce, ForceMode.Impulse);
        }
        
        elapsed = 0f;
        while (elapsed < attackStrikeTime)
        {
            float t = elapsed / attackStrikeTime;
            transform.localScale = Vector3.Lerp(originalScale * windupScale, originalScale * strikeScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (IsPlayerInDamageRange())
        {
            playerController.TakeDamage(35);
            Debug.Log("Player hit by enemy attack!");
        }
        else
        {
            Debug.Log("Player dodged the attack!");
        }
        
        elapsed = 0f;
        while (elapsed < attackRecoveryTime)
        {
            float t = elapsed / attackRecoveryTime;
            transform.localScale = Vector3.Lerp(originalScale * strikeScale, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = originalScale;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        if (agent != null)
        {
            agent.enabled = true;
            
            Vector3 directionAwayFromPlayer = (transform.position - playerTransform.position).normalized;
            Vector3 retreatPosition = transform.position + directionAwayFromPlayer * retreatDistance;
            
            if (NavMesh.SamplePosition(retreatPosition, out NavMeshHit hit, retreatDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        
        isAttacking = false;
        
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void Die()
    {
        agent.enabled = false;
        if (rb != null) { rb.isKinematic = false; }

		// TODO: Fix the damn hat not dropping issue
		ghostHat = GetComponentInChildren<GhostHat>();
		if (ghostHat != null && ghostHat.hat != null)
		{
			Debug.Log("Enemy dropped its hat.");
			ghostHat.hat.transform.SetParent(null);
			ghostHat.hat.GetComponent<Rigidbody>().isKinematic = false;
			Debug.Log("Enemy hat detached from parent.");
			Debug.Log("Enemy hat has " + ghostHat.hat.GetComponentsInChildren<Collider>().Length + " colliders.");
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

        // Destroy(gameObject, 5f);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, damageRange);
        
        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle * 0.5f, 0) * transform.forward * detectionRange;
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
