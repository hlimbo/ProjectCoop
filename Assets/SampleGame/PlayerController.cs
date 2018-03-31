using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputMap))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleRenderer))]
public class PlayerController : MonoBehaviour {

    public enum Direction { LEFT = -1, RIGHT = 1 };
    public enum DashState
    {
        READY,
        DASHING,
        COOLDOWN
    };

    public enum AirState
    {
        FLOOR,
        JUMPING,
        FALLING
    };

    #region Player Properties
    public float moveAccel = 350f;
    public float maxMoveSpeed = 500f;
    public Direction direction = Direction.RIGHT;

    [SerializeField] DashState dashState = DashState.READY;
    public float dashAccel = 25f;
    public float dashActiveDuration = 0.15f;
    public float dashCDDuration = 1f;
    [SerializeField] float dashTimer = 0f;

    [SerializeField] AirState airState = AirState.FALLING;
    public float jumpHeight;
    public float timeToJumpApex;
    [SerializeField] float gravity;
    float beginJumpY;
    float beginJumpTime;
    [SerializeField] float recordedJumpHeight;
    [SerializeField] float jumpSpeed;
    [SerializeField] float timeElapsed;
    public LayerMask floorMask;
    #endregion

    #region Component References
    private InputMap input;
    public InputMap Input { get { return input; } }
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private CircleRenderer cr;
    #endregion

    // Use this for initialization
    void Start () {
        input = GetComponent<InputMap>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        cr = GetComponent<CircleRenderer>();

        //solve for gravity and jumpSpeed based on timeToJumpApex and jumpHeight
        //d = vi * t + (1/2) * a * t^2 where vi = initial jump velocity = 0
        //d = (1/2) * a * t^2
        gravity = (2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        Physics2D.gravity = new Vector2(Physics2D.gravity.x, -gravity);
        jumpSpeed = gravity * timeToJumpApex;
	}
	
	// Update is called once per frame
	void Update () {

        //movement related calculations
        Vector2 moveForce = Vector2.zero;
        if (input.GetKey(InputMap.LEFT))
        {
            direction = Direction.LEFT;
            moveForce.x = moveAccel * Time.deltaTime * (int)direction;
        }
        else if (input.GetKey(InputMap.RIGHT))
        {
            direction = Direction.RIGHT;
            moveForce.x = moveAccel * Time.deltaTime * (int)direction;
        }
        else if(!input.GetKey(InputMap.LEFT) && !input.GetKey(InputMap.RIGHT) && dashState != DashState.DASHING)
        {
            //instant falloff of x-axis movement speed
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }

        if (airState == AirState.FLOOR && input.GetKey(InputMap.JUMP))
        {
            Debug.Log(string.Format("{0} is jumping", name));
            input.SetKey(InputMap.JUMP, false);
            airState = AirState.JUMPING;
            beginJumpY = rb.position.y;
            beginJumpTime = Time.time;
        }

        //fixed jump height
        if(airState == AirState.JUMPING)
        {
            timeElapsed = Time.time - beginJumpTime;
            recordedJumpHeight = rb.position.y - beginJumpY;
            if(timeElapsed < timeToJumpApex)
            {
                float jumpModifier = 1.5f;
                float jumpVelPercent = Mathf.InverseLerp(0.0f, jumpHeight * jumpModifier, recordedJumpHeight);
                float newJumpVelocity = Mathf.Lerp(0.0f, jumpSpeed, 1.0f - jumpVelPercent);
                rb.velocity = new Vector2(rb.velocity.x, newJumpVelocity);
            }
            else
            {
                airState = AirState.FALLING;
            }

            recordedJumpHeight = rb.position.y - beginJumpY;
        }

        if(airState == AirState.FALLING)
        {
            Vector2 footPos = new Vector2(rb.position.x, rb.position.y - sr.bounds.extents.y);
            bool isOnFloor = Physics2D.OverlapCircle(footPos, 0.1f, floorMask);
            if (isOnFloor)
                airState = AirState.FLOOR;
        }

        Vector2 dashForce = Vector2.zero;
        if (dashState == DashState.READY && input.GetKey(InputMap.DASH))
        {
            Debug.Log(string.Format("{0} is dashing", name));
            input.SetKey(InputMap.DASH, false);
            dashState = DashState.DASHING;
            dashForce = new Vector2(dashAccel, 0f);
        }

        rb.AddForce(moveForce, ForceMode2D.Impulse);

        if (dashState != DashState.DASHING)
        {
            //clamp move speed
            rb.velocity = (Mathf.Abs(rb.velocity.x) > maxMoveSpeed) ? new Vector2(maxMoveSpeed * (int)direction, rb.velocity.y) : rb.velocity;
        }

        if (dashState == DashState.DASHING)
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashActiveDuration)
            {
                dashTimer = 0f;
                dashState = DashState.COOLDOWN;
            }
            else
            {
                rb.AddForce(dashForce * (int)direction, ForceMode2D.Impulse);
            }
        }

        if(dashState == DashState.COOLDOWN)
        {
            dashTimer += Time.deltaTime;
            if(dashTimer <= dashCDDuration)
            {
                dashTimer = 0f;
                dashState = DashState.READY;
            }
            else
            {
                rb.AddForce(dashForce * (int)direction * -1, ForceMode2D.Impulse);
            }
        }

        if(Input.GetKey(InputMap.EMIT_DASH) || Input.GetKey(InputMap.EMIT_JUMP))
        {
            cr.broadcastSignal();
        }

	}

    public void processSignal(GameObject sender)
    {
        PlayerController pc = sender.GetComponent<PlayerController>();
        Debug.Log("processing signal");
        if(pc.Input.GetKey(InputMap.EMIT_JUMP))
        {
            Input.SetKey(InputMap.JUMP, true);
            Debug.Log("stopping emit jump");
            pc.Input.SetKey(InputMap.EMIT_JUMP, false);
        }
        else if (pc.Input.GetKey(InputMap.EMIT_DASH))
        {
            Input.SetKey(InputMap.DASH, true);
            Debug.Log("stopping emit dash");
            pc.Input.SetKey(InputMap.EMIT_DASH, false);
        }
    }
}
