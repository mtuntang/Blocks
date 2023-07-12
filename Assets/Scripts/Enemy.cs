using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State
    {
        Idle,
        Chasing,
        Attacking
    }
    State currentState;

    public NavMeshAgent pathFinder;
    private Transform target;
    private Material skinMaterial;
    private Color originalColour;
    private LivingEntity targetEntity;

    float attackDistanceThreshold = .5f;
    float timeBetweenAttacks = 1;
    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadius;
    float damage = 1;

    bool hasTarget;
    
    protected override void Start()
    {
        base.Start();
        pathFinder= GetComponent<NavMeshAgent>();
        skinMaterial = GetComponent<Renderer>().material;
        originalColour = skinMaterial.color;

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            currentState = State.Chasing;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
            StartCoroutine(UpdatePath());
        }
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasTarget)
        { 
            return; 
        }

        if (Time.time > nextAttackTime)
        {
            // Need to calculate distance between enemy-target but not using the actual distance, don't need to find root, just compare because cheaper
            // Note to future self: if you give up just use Vector3.distance lmaooo
            float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;
            if (sqrDistanceToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
            {
                nextAttackTime = Time.time + timeBetweenAttacks;
                StartCoroutine(Attack());
            }
        }
    }

    private IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;
        while (hasTarget)
        { 
            if (currentState == State.Chasing)
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius);

                if (alive)
                {
                    pathFinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

    private IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathFinder.enabled = false;

        Vector3 originalPos = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 attackPos = target.position - directionToTarget * myCollisionRadius;
        
        float attackSpeed = 3;
        float percent = 0;

        skinMaterial.color = Color.red;
        Transform bagpackTransform = transform.Find("Bagpack");
        if (bagpackTransform != null)
        {
            Renderer[] bagpackRenderers = bagpackTransform.GetComponentsInChildren<Renderer>();
            foreach (Renderer bagpackRenderer in bagpackRenderers)
            {
                bagpackRenderer.material.color = Color.red;
            }
        }

        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            if (percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPos, attackPos, interpolation);
            yield return null;
        }

        skinMaterial.color = originalColour;
        if (bagpackTransform != null)
        {
            Renderer[] bagpackRenderers = bagpackTransform.GetComponentsInChildren<Renderer>();
            foreach (Renderer bagpackRenderer in bagpackRenderers)
            {
                bagpackRenderer.material.color = originalColour;
            }
        }
        currentState = State.Chasing;
        pathFinder.enabled = true;
    }
}
