using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputMap))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {

    public enum Direction { LEFT = -1, RIGHT = 1 };
    public enum DashState
    {
        READY,
        DASHING,
        COOLDOWN
    };

    #region Player Properties
    public float moveAccel = 350f;
    public float maxMoveSpeed = 500f;
    public Direction direction = Direction.RIGHT;

    public DashState dashState = DashState.READY;
    public float dashAccel = 25f;
    public float dashActiveDuration = 0.15f;
    public float dashCDDuration = 1f;
    [SerializeField]
    float dashTimer = 0f;
    #endregion

    #region Component References
    private InputMap input;
    public InputMap Input { get { return input; } }
    private Rigidbody2D rb;
    #endregion

    // Use this for initialization
    void Start () {
        input = GetComponent<InputMap>();
        rb = GetComponent<Rigidbody2D>();
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

        if (input.GetKey(InputMap.JUMP))
        {
            Debug.Log(string.Format("{0} is jumping", name));
            input.SetKey(InputMap.JUMP, false);
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

	}
}
