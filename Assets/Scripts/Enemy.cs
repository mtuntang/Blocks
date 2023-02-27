using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent pathFinder;
    private Transform target; 
    
    void Start()
    {
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
            Vector3 targetPos = target.position;
            pathFinder.SetDestination(targetPos);
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
