using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputMap))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {

    #region Player Properties
    public float speed = 350f;
    public bool isFacingRight = true;
    public Vector2 dashForce = new Vector2(800f, 0f);
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

        if (input.GetKey(InputMap.LEFT))
        {
            rb.velocity = new Vector2(-speed * Time.deltaTime, rb.velocity.y);
            isFacingRight = false;
        }
        else if (input.GetKey(InputMap.RIGHT))
        {
            rb.velocity = new Vector2(speed * Time.deltaTime, rb.velocity.y);
            isFacingRight = true;
        }
        else
            rb.velocity = Vector2.zero;

        if(input.GetKey(InputMap.JUMP))
        {
            Debug.Log(string.Format("{0} is jumping", name));
            input.SetKey(InputMap.JUMP, false);
        }

        if (input.GetKey(InputMap.DASH))
        {
            Debug.Log(string.Format("{0} is dashing", name));
            if (isFacingRight)
                rb.MovePosition(rb.position + dashForce * Time.deltaTime);
            else
                rb.MovePosition(rb.position - dashForce * Time.deltaTime);
            input.SetKey(InputMap.DASH, false);
        }

	}
}
