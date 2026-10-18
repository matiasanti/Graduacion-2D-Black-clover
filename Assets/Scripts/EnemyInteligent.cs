using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class EnemyInteligent : MonoBehaviour
{
    private static readonly int MovingParameter = Animator.StringToHash("Moving");
    private static readonly int ChasingParameter = Animator.StringToHash("Chasing");

    [SerializeField] private Transform player;
    [SerializeField, Min(0f)] private float detectionRange = 5f;
    [SerializeField, Min(0f)] private float maxVerticalDetectionDifference = 0.8f;
    [SerializeField, Min(0f)] private float stoppingDistance = 0.6f;
    [SerializeField, Min(0f)] private float patrolDistance = 2f;
    [SerializeField, Min(0f)] private float patrolSpeed = 1.5f;
    [SerializeField, Min(0f)] private float chaseSpeed = 3.5f;

    private Rigidbody2D body;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float patrolCenterX;
    private int patrolDirection = 1;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        patrolCenterX = transform.position.x;

        if (player == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player == null)
        {
            Debug.LogError(
                "EnemyInteligent necesita una referencia al jugador. Asigna el campo Player en el Inspector.",
                this);
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        float horizontalDifference = player.position.x - transform.position.x;
        float verticalDifference = Mathf.Abs(player.position.y - transform.position.y);

        if (Mathf.Abs(horizontalDifference) <= detectionRange
            && verticalDifference <= maxVerticalDetectionDifference)
        {
            ChasePlayer(horizontalDifference);
            return;
        }

        Patrol();
    }

    private void ChasePlayer(float horizontalDifference)
    {
        if (Mathf.Abs(horizontalDifference) <= stoppingDistance)
        {
            SetHorizontalVelocity(0f);
            SetAnimationState(isMoving: false, isChasing: false);
            return;
        }

        float direction = Mathf.Sign(horizontalDifference);
        FaceDirection(direction);
        SetHorizontalVelocity(direction * chaseSpeed);
        SetAnimationState(isMoving: true, isChasing: true);
    }

    private void Patrol()
    {
        float leftBoundary = patrolCenterX - patrolDistance;
        float rightBoundary = patrolCenterX + patrolDistance;

        if (transform.position.x <= leftBoundary)
        {
            patrolDirection = 1;
        }
        else if (transform.position.x >= rightBoundary)
        {
            patrolDirection = -1;
        }

        FaceDirection(patrolDirection);
        SetHorizontalVelocity(patrolDirection * patrolSpeed);
        SetAnimationState(isMoving: patrolSpeed > 0f, isChasing: false);
    }

    private void FaceDirection(float direction)
    {
        if (spriteRenderer != null && direction != 0f)
        {
            spriteRenderer.flipX = direction < 0f;
        }
    }

    private void SetHorizontalVelocity(float horizontalVelocity)
    {
        body.velocity = new Vector2(horizontalVelocity, body.velocity.y);
    }

    private void SetAnimationState(bool isMoving, bool isChasing)
    {
        animator.SetBool(MovingParameter, isMoving);
        animator.SetBool(ChasingParameter, isChasing);
    }
}
