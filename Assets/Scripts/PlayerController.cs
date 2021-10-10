using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour

{
    #region Components
    public Rigidbody rb;

    public Transform playerCam;
    public Transform orientation;
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
    float x, y;
    bool jumping, sprinting, crouching;

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    //Wall running
    public LayerMask wallLayer;
    public float wallRunForce, maxWallRunTime, maxWallSpeed;
    bool isWallLeft, isWallRight;
    [SerializeField]
    bool isWallRunning;
    public float maxWallRunCamTilt, wallRunCamTilt;
    #endregion

    #region Functions

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        ReadInputs();
        Look();
        WallRunInput();
        CheckForWall();
    }

    //Gets all input values and assigns them appropriately
    void ReadInputs()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        crouching = Input.GetButton("Crouch");
        sprinting = Input.GetButton("Sprint");

        //Crouch
        if (Input.GetButtonDown("Crouch"))
            StartCrouch();
        if(Input.GetButtonUp("Crouch"))
            StopCrouch();

        if (sprinting)
            maxSpeed = sprintMaxSpeed;
        else
            maxSpeed = walkMaxSpeed;
    }

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

    void Movement()
    {
        //Added gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Get velocity relative to the direction the player is facing
        Vector2 mag = FindRelativeVelocity();
        float xMag = mag.x, yMag = mag.y;

        //Adding relative counter-impulses to make movement feel snappier
        Drag(x, y, mag);

        //Jump
        if (canJump && jumping) Jump();
        
        //Add force when sliding down a ramp so the player can build speed
        if(crouching && grounded && canJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }
        
        //Ensures the input won't exceed max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        float multiplier = 1f, multiplierV = 1f;

        //Air movement modifiers
        if(!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        if (grounded && crouching) multiplierV = 0;

        rb.AddForce(orientation.transform.forward * y * moveImpulse * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveImpulse * Time.deltaTime * multiplier);
    }

    void Jump()
    {
        if (grounded && canJump)
        {
            canJump = false;

            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //Reset Y velocity on air jump
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if(isWallRunning)
        {
            canJump = false;

            if(isWallLeft && !Input.GetKey(KeyCode.D) || isWallRight && !Input.GetKey(KeyCode.A))
            {
                rb.AddForce(Vector2.up * jumpForce * 1.5f);
                rb.AddForce(normalVector * jumpForce * 0.5f);
            }

            if (isWallRight || isWallLeft && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) rb.AddForce(-orientation.up * jumpForce);
            if (isWallRight && Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * jumpForce * 3.2f);
            if (isWallLeft && Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * jumpForce * 3.2f);

            rb.AddForce(orientation.forward * jumpForce);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void ResetJump()
    {
        canJump = true;
    }

    float desiredX;
    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90, 90);

        playerCam.transform.localRotation = Quaternion.Euler(xRot, desiredX, wallRunCamTilt);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);

        if (Mathf.Abs(wallRunCamTilt) < maxWallRunCamTilt && isWallRunning && isWallRight)
            wallRunCamTilt += Time.deltaTime * maxWallRunCamTilt * 2;
        if (Mathf.Abs(wallRunCamTilt) < maxWallRunCamTilt && isWallRunning && isWallLeft)
            wallRunCamTilt -= Time.deltaTime * maxWallRunCamTilt * 2;

        if (wallRunCamTilt > 0 && !isWallRight && !isWallLeft)
            wallRunCamTilt -= Time.deltaTime * maxWallRunCamTilt * 2;
        if (wallRunCamTilt < 0 && !isWallRight && !isWallLeft)
            wallRunCamTilt += Time.deltaTime * maxWallRunCamTilt * 2;
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

    void WallRunInput()
    {
        if (Input.GetKey(KeyCode.D) && isWallRight) StartWallRun();
        if (Input.GetKey(KeyCode.A) && isWallLeft) StartWallRun();
    }

    void StartWallRun()
    {
        rb.useGravity = false;
        isWallRunning = true;

        if(rb.velocity.magnitude <= maxWallSpeed)
        {
            rb.AddForce(orientation.forward * wallRunForce * Time.deltaTime);

            if (isWallRight)
                rb.AddForce(orientation.right * wallRunForce / 5 * Time.deltaTime);
            else
                rb.AddForce(-orientation.right * wallRunForce / 5 * Time.deltaTime);
        }
    }

    void StopWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true;
    }

    void CheckForWall()
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, 1f, wallLayer);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, 1f, wallLayer);

        if (!isWallLeft && !isWallRight) StopWallRun();
    }
    #endregion
}