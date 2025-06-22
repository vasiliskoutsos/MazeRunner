using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public LayerMask groundMask;
    public LayerMask playerMask;
    public int damage = 20;

    public float walkPointRange = 1f;
    public float sightRange = 3.3f;
    public float attackRange = 0.4f;
    public float timeBetweenAttacks = 4f;

    private Vector3 walkPoint;
    private bool walkPointSet;
    private float attackCooldownTimer;
    
    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;

    private Transform player;
    //private Transform enemy;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
            player = go.transform;
        else
            Debug.LogError("EnemyAi player not found");

        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("No animator found ");
    }

    void Update()
    {
        // timers
        attackCooldownTimer += Time.deltaTime;

        // distance checks
        Vector3 flatEnemy  = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatPlayer = new Vector3(player.position.x, 0, player.position.z);
        float distToPlayer = Vector3.Distance(flatEnemy, flatPlayer);
        
        // state transitions
        if (distToPlayer <= attackRange) currentState = State.Attack;
        else if (distToPlayer <= sightRange) currentState = State.Chase;
        else currentState = State.Patrol;
        
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                animator.CrossFade("Anim_Hyeobdo_Walk_Solider", 0.1f);
                break;
            case State.Chase:
                Chase();
                animator.CrossFade("Anim_Hyeobdo_Run_Solider", 0.1f);
                break;
            case State.Attack:
                Attack();
                animator.CrossFade("Anim_Hyeobdo_Attack1_Solider", 0.1f);
                break;
        }

    }

    private void Patrol()
    {
        if (!walkPointSet) FindWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        // arrived?
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            walkPointSet = false;
    }

    private void FindWalkPoint()
    {
        // pick random point in sphere
        Vector3 randomDir = Random.insideUnitSphere * walkPointRange;
        randomDir += transform.position;
        NavMeshHit hit;
        // sample closest on navmesh
        if (NavMesh.SamplePosition(randomDir, out hit, 2f, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
    }

    private void Chase()
    {
        agent.SetDestination(player.position);
        LookAt(player.position);
    }

    private void Attack()
    {
        agent.ResetPath();
        LookAt(player.position);

        if (attackCooldownTimer >= timeBetweenAttacks)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage);
                Debug.Log($"takedamage on {player.name}");
            }
            else
                Debug.LogError("no playerStats");
            attackCooldownTimer = 0f;
        }
    }

    private void LookAt(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, agent.angularSpeed * Time.deltaTime);
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

}
