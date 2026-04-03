using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敵キャラクターがプレイヤーを追跡するAI。
/// NavMeshAgent を使用するため、シーンにNavMeshをBakeしてください。
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("追跡設定")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 15f;   // この範囲内でプレイヤーを検知
    [SerializeField] private float attackRange    = 1.5f;  // この範囲内で攻撃
    [SerializeField] private float chaseSpeed    = 4f;
    [SerializeField] private float patrolSpeed   = 2f;

    [Header("パトロール設定")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;

    private NavMeshAgent _agent;
    private EnemyState   _state = EnemyState.Patrol;
    private int          _patrolIndex;
    private float        _waitTimer;

    private enum EnemyState { Patrol, Chase, Attack }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        // プレイヤーが未設定なら自動検索
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (_state)
        {
            case EnemyState.Patrol: UpdatePatrol(distToPlayer); break;
            case EnemyState.Chase:  UpdateChase(distToPlayer);  break;
            case EnemyState.Attack: UpdateAttack(distToPlayer); break;
        }
    }

    // ---------------------------------------------------------------- Patrol

    private void UpdatePatrol(float dist)
    {
        if (dist <= detectionRange)
        {
            EnterChase();
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (_agent.remainingDistance < 0.3f)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
                _agent.SetDestination(patrolPoints[_patrolIndex].position);
                _waitTimer = patrolWaitTime;
            }
        }
    }

    private void EnterChase()
    {
        _state = EnemyState.Chase;
        _agent.speed = chaseSpeed;
    }

    // ----------------------------------------------------------------- Chase

    private void UpdateChase(float dist)
    {
        if (dist > detectionRange * 1.2f)   // 少し余裕を持たせてパトロールに戻る
        {
            EnterPatrol();
            return;
        }

        if (dist <= attackRange)
        {
            _state = EnemyState.Attack;
            _agent.ResetPath();
            return;
        }

        _agent.SetDestination(player.position);
    }

    private void EnterPatrol()
    {
        _state = EnemyState.Patrol;
        _agent.speed = patrolSpeed;
        _waitTimer = patrolWaitTime;
        if (patrolPoints != null && patrolPoints.Length > 0)
            _agent.SetDestination(patrolPoints[_patrolIndex].position);
    }

    // ---------------------------------------------------------------- Attack

    private void UpdateAttack(float dist)
    {
        // プレイヤーの方を向く
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

        if (dist > attackRange)
        {
            EnterChase();
            return;
        }

        // 攻撃処理: Animatorがあればトリガー発火
        var anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Attack");
    }

    // -------------------------------------------------------------- Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
