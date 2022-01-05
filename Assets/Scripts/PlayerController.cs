using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IEntity

{
    #region Components
    public Rigidbody rb;

    public Transform playerCam;
    public Transform orientation;

    public PlayerInputs inputs;

    public float maxHealth { get; set; } = 200f;
    public float currentHealth { get; set; }
    #endregion

    #region Controls
    //Looking and rotation
    private float xRot;
    private float sensitivity = 20f;
    private float sensMultiplier = 1f;

    //Movement
    public float moveImpulse = 5000;
    public float walkMaxSpeed = 20;
    public float sprintMaxSpeed = 30;
    float maxSpeed = 20;
    public bool grounded;
    public LayerMask groundLayer;

    public float drag = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //Crouching and Sliding
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideDrag = 0.2f;

    //Jump
    private bool canJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;

    //Inputs
    bool jumping, sprinting, crouching;

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    //Wallrunning
    [SerializeField]
    private bool isWallLeft, isWallRight, isWallrunning;
    public LayerMask wallLayer;
    public float wallJumpForce = 5000f;

    private RaycastHit wallHit;
    #endregion

    #region Functions

    void Awake()
    {
        inputs = new PlayerInputs();
        rb = GetComponent<Rigidbody>();

        inputs.InGame.Jump.started += _ => jumping = true;
        inputs.InGame.Jump.canceled += _ => jumping = false;


        inputs.InGame.Sprint.started += _ => maxSpeed = sprintMaxSpeed;
        inputs.InGame.Sprint.canceled += _ => maxSpeed = walkMaxSpeed;

        inputs.InGame.Crouch.started += _ => StartCrouch();
        inputs.InGame.Crouch.started += _ => crouching = true;
        inputs.InGame.Crouch.canceled += _ => StopCrouch();
        inputs.InGame.Crouch.canceled += _ => crouching = false;
    }

    private void OnEnable()
    {
        inputs.Enable();
    }

    private void OnDisable()
    {
        inputs.Disable();
    }

    private void Start()
    {
        playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        Movement(inputs.InGame.Move.ReadValue<Vector2>());
    }

    private void Update()
    {
        Look(inputs.InGame.Look.ReadValue<Vector2>());

        CheckForWall();
    }

    //Gets all input values and assigns them appropriately


    void StartCrouch()
    {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);

        if(rb.velocity.magnitude > 0.5f && grounded && sprinting)
        {
            rb.AddForce(orientation.transform.forward * slideForce);
        }
    }

    void StopCrouch()
    {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    public void Movement(Vector2 input)
    {
        //Get velocity relative to the direction the player is facing
        Vector2 mag = FindRelativeVelocity();
        float xMag = mag.x, yMag = mag.y;

        if (isWallrunning)
        {
            input = ClampSpeed(input, xMag, yMag);

            rb.AddForce(orientation.transform.forward * input.y * moveImpulse * Time.deltaTime);

            if (isWallLeft) rb.AddForce(-orientation.right * 1.25f * Time.deltaTime);
            if (isWallRight) rb.AddForce(orientation.right * 1.25f * Time.deltaTime);

            if (canJump && jumping) Jump();

            return;
        }

        //Added gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Adding relative counter-impulses to make movement feel snappier
        Drag(input.x, input.y, mag);

        //Jump
        if (canJump && jumping) Jump();
        
        //Add force when sliding down a ramp so the player can build speed
        if(crouching && grounded && canJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        input = ClampSpeed(input, xMag, yMag);

        float multiplier = 1f, multiplierV = 1f;

        //Air movement modifiers
        if(!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        //Reduce forward movement when crouching
        if (grounded && crouching) multiplierV = 0.25f;

        //Add input forces
        rb.AddForce(orientation.transform.forward * input.y * moveImpulse * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * input.x * moveImpulse * Time.deltaTime * multiplier);
    }

    Vector2 ClampSpeed(Vector2 input, float xMag, float yMag)
    {
        //Ensures the input won't exceed max speed
        if (input.x > 0 && xMag > maxSpeed) input.x = 0;
        if (input.x < 0 && xMag < -maxSpeed) input.x = 0;
        if (input.y > 0 && yMag > maxSpeed) input.y = 0;
        if (input.y < 0 && yMag < -maxSpeed) input.y = 0;

        return input;
    }

    void Jump()
    {
        if(isWallrunning)
        {
            WallJump();
            return;
        }

        if (grounded && canJump)
        {
            canJump = false;

            rb.AddForce(Vector2.up * jumpForce * 1.5f);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void ResetJump()
    {
        canJump = true;
    }

    float desiredX;
    void Look(Vector2 input)
    {
        float mouseX = input.x * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = input.y * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90, 90);

        playerCam.transform.localRotation = Quaternion.Euler(xRot, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    void Drag(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        if(crouching)
        {
            rb.AddForce(moveImpulse * Time.deltaTime * -rb.velocity.normalized * slideDrag);
            return;
        }

        if (Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
            rb.AddForce(moveImpulse * orientation.transform.right * Time.deltaTime * -mag.x * drag);
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
            rb.AddForce(moveImpulse * orientation.transform.forward * Time.deltaTime * -mag.y * drag);

        if(Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallSpeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallSpeed, n.z);
        }
    }

    public Vector2 FindRelativeVelocity()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitude = rb.velocity.magnitude;
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancelGrounded;

    private void OnCollisionStay(Collision collision)
    {
        int layer = collision.gameObject.layer;
        if (groundLayer != (groundLayer | (1 << layer))) return;

        for(int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.contacts[i].normal;

            if(IsFloor(normal))
            {
                grounded = true;
                cancelGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        float delay = 3f;
        if(!cancelGrounded)
        {
            cancelGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    void StopGrounded()
    {
        grounded = false;
    }

    //Wallrun Implementation
    void CheckForWall()
    {
        //Check if wall left
        if (Physics.Raycast(transform.position, -orientation.right, out wallHit, 0.75f, wallLayer))
        { 
            isWallLeft = true;
            isWallrunning = true;
            rb.useGravity = false;
            return;
        }
        else
        { 
            isWallLeft = false; 
            isWallrunning = false;
            rb.useGravity = true;
        }
        //Check if wall right
        if (Physics.Raycast(transform.position, orientation.right, out wallHit, 0.75f, wallLayer))
        { 
            isWallRight = true; 
            isWallrunning = true;
            rb.useGravity = false;
            return;
        }
        else
        {
            isWallRight = false; 
            isWallrunning = false;
            rb.useGravity = true;
        }
    }

    void WallJump()
    {
        if(isWallLeft)
        {
            rb.AddForce(orientation.forward * wallJumpForce * Time.deltaTime);
            rb.AddForce(orientation.right * wallJumpForce * Time.deltaTime);
            rb.AddForce(orientation.up * wallJumpForce * Time.deltaTime);
        }

        if(isWallRight)
        {
            rb.AddForce(orientation.forward * wallJumpForce * Time.deltaTime);
            rb.AddForce(-orientation.right * wallJumpForce * Time.deltaTime);
            rb.AddForce(orientation.up * wallJumpForce * Time.deltaTime);
        }
    }



    //Entity Implementation
    public void Initialize()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
            KillEntity();
    }

    public void KillEntity()
    {
        Debug.Log("Player ded");
    }
    #endregion
    #region Debugs
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 leftRay = -orientation.right * .75f;
        Vector3 rightRay = orientation.right * .75f;
        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, orientation.forward);


        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(wallHit.point, 1f);
    }
    #endregion
}