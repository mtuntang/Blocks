using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public NavMeshAgent pathFinder;
    private Transform target; 
    
    protected override void Start()
    {
        base.Start();
        pathFinder= GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(UpdatePath());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;
        while (target != null)
        { 
            Vector3 targetPosition = new Vector3(target.position.x, 0, target.position.z);
            if (alive)
            {
                pathFinder.SetDestination(targetPosition);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
