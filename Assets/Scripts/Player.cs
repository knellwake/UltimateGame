using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isBusy { get; private set; }
    [Header("攻击信息")] public Vector2[] attackMovement;

    [Header("移动参数")] public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("冲刺信息")] [SerializeField] private float dashCooldown;
    private float dashUsageTimer;
    public float dashSpeed;
    public float dashDuration;
    
    public float dashDir { get; private set; }
    public Vector2 sizeNormal;
    public Vector2 offsetNormal;

    [Header("碰撞信息")] [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;

    public int facingDir { get; private set; } = 1;
    private bool facingRight = true;

    #region Components

    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public CapsuleCollider2D coll { get; private set; }

    #endregion

    #region States

    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerWallSlideState wallSlide { get; private set; }
    public PlayerWallJumpState wallJump { get; private set; }
    public PlayerPrimaryAttackState primaryAttack { get; private set; }
    public PlayerCrouchDashState crouchDash { get; private set; }

    #endregion

    private void Awake()
    {
        stateMachine = new PlayerStateMachine();

        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        airState = new PlayerAirState(this, stateMachine, "Jump");
        dashState = new PlayerDashState(this, stateMachine, "Dash");
        wallSlide = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        wallJump = new PlayerWallJumpState(this, stateMachine, "Jump");
        primaryAttack = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        crouchDash = new PlayerCrouchDashState(this, stateMachine, "Crouch");
    }

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        stateMachine.currentState.Update();
        CheckForDashInput();
    }

    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;
        yield return new WaitForSeconds(_seconds);
        isBusy = false;
    }

    public void AnimationTrigger() => stateMachine.currentState.AnimatorFinishTrigger();

    private void CheckForDashInput()
    {
        if (IsWallDetected())
            return;

        dashUsageTimer -= Time.deltaTime;


        if (dashUsageTimer < 0)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                dashUsageTimer = dashCooldown;
                dashDir = Input.GetAxisRaw("Horizontal");

                if (dashDir == 0)
                    dashDir = facingDir;
                stateMachine.ChangeState(dashState);
            }
        }
    }

    #region Velocity

    public void ZeroVelocity() => rb.velocity = new Vector2(0, 0);

    /// <summary>
    /// 设置移动速度
    /// </summary>
    /// <param name="_xVelocity">X轴</param>
    /// <param name="_yVelocity">Y轴</param>
    public void SetVelocity(float _xVelocity, float _yVelocity)
    {
        rb.velocity = new Vector2(_xVelocity, _yVelocity);
        FlipController(_xVelocity);
    }

    #endregion

    #region Collision

    public bool IsGroundDetected() =>
        Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);

    public bool IsWallDetected() =>
        Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position,
            new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position,
            new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));
    }

    #endregion

    #region Flip

    public void Flip()
    {
        facingDir *= -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    public void FlipController(float _x)
    {
        if (_x > 0 && !facingRight)
            Flip();
        else if (_x < 0 && facingRight)
            Flip();
    }

    #endregion
}