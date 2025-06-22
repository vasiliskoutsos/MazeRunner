using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float jumpForce = 45f;

    public float maxSlopeAngle = 60f;
    public float groundCheckDistance = 0.2f;
    public LayerMask whatIsGround;

    public float groundDrag = 6f;
    public float airDrag = 0.5f;
    public float brakingDeceleration = 20f;

    public Transform orientation;
    public Animation animator;
    [HideInInspector]
    public bool isAttacking = false;

    private Rigidbody rb;
    private float verticalInput;
    private Vector3 currentSlopeNormal;
    private bool isGrounded;
    private bool isJumping = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // ground and slope normal
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        RaycastHit hit;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.1f, whatIsGround))
        {
            isGrounded = true;
            currentSlopeNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            currentSlopeNormal = Vector3.up;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking)
            StartCoroutine(DoAttack());

        // only choose run or idle if not mid attack or midjump
        if (!isAttacking && !isJumping && animator)
        {
            if (verticalInput > 0f || verticalInput < 0f)
                animator.CrossFade("Run");
            else
                animator.CrossFade("idle");
        }

        // read input
        verticalInput = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isJumping)
            Jump();
        if (Input.GetKeyDown(KeyCode.Mouse0))
            isAttacking = true;

        if (!isAttacking && !isJumping && animator)
        {
            if (verticalInput > 0) animator.CrossFade("Run");
            else if (verticalInput < 0) animator.CrossFade("Run");
            else animator.CrossFade("idle");
        }
    }

    private IEnumerator DoAttack()
    {
        isAttacking = true;
        animator.Play("Attack");
        AnimationState state = animator["Attack"];
        if (state != null)
            yield return new WaitForSeconds(state.length);
        else
            yield return null;
        isAttacking = false;
    }

    private IEnumerator ResetJump()
    {
        AnimationState state = animator["Jump"];
        if (state != null)
            yield return new WaitForSeconds(state.length);
        else
            yield return null;
        isJumping = false;
    }

    void FixedUpdate()
    {
        ApplyDrag();
        MovePlayer();
    }

    void ApplyDrag()
    {
        // more drag on ground less in air
        rb.drag = isGrounded ? groundDrag : airDrag;
    }

    void MovePlayer()
    {
        if (isAttacking) return; // dont allow moving while the player attacks
        // build move direction project to slope
        Vector3 rawMove = orientation.forward * verticalInput;
        Vector3 projected = Vector3.ProjectOnPlane(rawMove, currentSlopeNormal).normalized;

        float slopeAngle = Vector3.Angle(Vector3.up, currentSlopeNormal);
        if (verticalInput != 0f && slopeAngle <= maxSlopeAngle)
        {
            // push forward
            rb.AddForce(projected * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            // brake when no input
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > 0.1f)
            {
                Vector3 brake = -flatVel.normalized * brakingDeceleration;
                rb.AddForce(brake, ForceMode.Acceleration);
            }
        }
    }

    void Jump()
    {
        if (isAttacking)
            return;
        // cancel vertical velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isJumping = true;
        animator.CrossFade("Jump");
        StartCoroutine(ResetJump());
    }
}
