using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * TODO: Platforms must be set to layer 8
 */

public class PlayerController : MonoBehaviour {
	public string BotName = "";
	public string InputType = ""; //LKB or RKB
	public Color transmission_color;

	public GameObject thisGO = null;
	public Rigidbody2D thisRB = null;
	public PolygonCollider2D thisCollider = null;
	public SpriteRenderer thisSpriteRenderer = null;
	public Vector2 colliderSize;
	public InputManager input_manager = null;


	//Player Movement Variables
	public float jump_speed;
	public float move_speed;

	//Player Input Variables
	public float horizontal_input, vertical_input, dash_input, shield_input, transactivate_input;
	public List<string> keydown_AllowedActionSet, autofire_AllowedActionSet, keyup_AllowedActionSet;
	//Player States
	public Dictionary<string, bool> current_input;
	public bool grounded = false;
	public bool dashing = false;

	public bool transmitting = false;
	public bool transmissionEnabled = false;

	public int facing = 1;
	public bool canMove = false;
	public int layerMask;

	public Color initColor;

	//Transmission Variables
	public float trans_signal_radius;
	public PlayerController otherPlayer;
	public Vector3 transmission_origin;
	public TransmissionDataTypes transmission_send;
	public TransmissionDataTypes transmission_receive;
	public float transmission_speed = 0.15f;

	public CircleRenderer transmissionRenderer;

	public enum TransmissionDataTypes {
		None,
		Jump,
		LeftMove,
		RightMove,
		Dash,
		Shield
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// START  START  START  START  START  START  START  START  START  START  START  START  START  START  START  START  START ///
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void Start () {

		thisGO = gameObject;
		thisRB = GetComponent<Rigidbody2D> ();//Rigidbody2D
		thisCollider = GetComponent<PolygonCollider2D>();//PolygonCollider2D
		thisSpriteRenderer = GetComponent<SpriteRenderer>();


		/*-----------------
		 * Physics Configs
		 * --------------*/
		//We need to standardize physics configs
		thisRB.gravityScale = 3f; 
		jump_speed = 2.5f;
		move_speed = 5f;

		//Player dimensions
		colliderSize = thisCollider.bounds.size; //Do we need this?
		initColor = thisSpriteRenderer.color;

		//Player transmission mechanics
		trans_signal_radius = 0f;


		//Player initialization
		initPlayer();

		transmissionRenderer = gameObject.AddComponent<CircleRenderer> ();

		//Player Actions

	}

	void initPlayer()
	{
		if (BotName == "")
		{
			Debug.Log ("Excuse me, but your player did not have a BotName for initialization");
			return;
		}

		if (BotName == "Alan") {
			otherPlayer = GameObject.FindGameObjectWithTag ("Harvey").GetComponent<PlayerController> ();

			// Bit shift the index of the layer (8) to get a bit mask
			layerMask = 1 << 8; //Alan's Game Object will be on layer 8
			layerMask = ~layerMask;

			transmission_color = Color.magenta;//set transmission color

			keydown_AllowedActionSet = new List<string> { "a2",  "a3",  "a4",  "a5",  "a6" };
			autofire_AllowedActionSet = new List<string> { "left", "right" };
			keyup_AllowedActionSet = new List<string> { };

		} else {
			otherPlayer = GameObject.FindGameObjectWithTag ("Alan").GetComponent<PlayerController> ();
			// Bit shift the index of the layer (8) to get a bit mask
			layerMask = 1 << 9; //Harvey's Game Object will be on layer 9
			layerMask = ~layerMask;

			transmission_color = Color.cyan; //set transmission color

			keydown_AllowedActionSet = new List<string> { "a2",  "a3",  "a4",  "a5",  "a6" };
			autofire_AllowedActionSet = new List<string> { "left", "right" };
			keyup_AllowedActionSet = new List<string> { };
		}

		input_manager = thisGO.AddComponent<InputManager> () as InputManager;
		current_input = new Dictionary<string, bool> ();
		input_manager.Init (InputType, current_input, autofire_AllowedActionSet, keydown_AllowedActionSet, keyup_AllowedActionSet);

		if (!otherPlayer)
			Debug.Log  ("Error: PlayerController.cs >> otherPlayer is null");
	}

	void recieveTransmission(){
		if (transmission_receive == TransmissionDataTypes.Jump) {
			transmission_receive = TransmissionDataTypes.None;
			jump ();
		}
		else if (transmission_receive == TransmissionDataTypes.Dash) {
			transmission_receive = TransmissionDataTypes.None;
			dash();
		}
		else if (transmission_receive == TransmissionDataTypes.Shield) {
			transmission_receive = TransmissionDataTypes.None;
			shield();
		}
	}


	///////////////////////////////////////////////////////////////////////
	/// 							Behaviours
	///////////////////////////////////////////////////////////////////////

	void manageInput(){ 
		/*
			 * up action = up
			 * left action = left
			 * right action = right
			 * a1 action = None
			 * a2 action = dash
			 * a3 action = shield
			 * a4 action = jump_transmit
			 * a5 action = dash_transmit
			 * a6 action = shield_transmit
		 */

		//horizontal movement!
		if (current_input.ContainsKey("left") && current_input ["left"]) {
			thisRB.velocity = new Vector2 ( -move_speed, thisRB.velocity.y);
			facing = -1;
		} else if (current_input.ContainsKey("right") && current_input ["right"]) {
			thisRB.velocity = new Vector2 ( move_speed, thisRB.velocity.y);
			facing = 1;
		} 

		//self jump!!
		if (current_input.ContainsKey("up") && current_input["up"])
			jump ();

		//self dash!!
		if (current_input.ContainsKey("a2") && current_input["a2"])
			dash ();

		//self shield!!
		if (current_input.ContainsKey("a3") && current_input["a3"])
			shield ();

		//transmit jump!!
		if (current_input.ContainsKey("a4") && current_input["a4"])
			sendSignal (TransmissionDataTypes.Jump);

		//transmit dash!!
		if (current_input.ContainsKey("a5") && current_input["a5"])
			sendSignal (TransmissionDataTypes.Dash);

		//transmit shield!!
		if (current_input.ContainsKey("a6") && current_input["a6"])
			sendSignal (TransmissionDataTypes.Shield);
	
		foreach (string action in new List<string> (current_input.Keys)) {
			current_input [action] = false;
		}
	}

	void sendSignal(TransmissionDataTypes signal){

		//only transmit if not already transmitting
		if (!transmitting) {

			//init signal
			transmitting = true;
			transmission_send = signal;

			//init render signal
			trans_signal_radius = 0f;
			transmission_origin = transform.position;
			transmissionRenderer.xradius = transmissionRenderer.yradius = 0f;
			transmissionRenderer.SetOrigin (transmission_origin, transmission_color);

		}
	}

	void jump(){
		transmission_receive = TransmissionDataTypes.None;
		if (grounded) {
			thisRB.velocity = new Vector2 (thisRB.velocity.x, jump_speed * 5);
		}
	}

	void dash(){
		transmission_receive = TransmissionDataTypes.None;
		if (!dashing) {
			if (facing == 1) {
				thisRB.AddForce(Vector2.right * jump_speed * 100);
			}
			else if(facing == -1){
				thisRB.AddForce(Vector2.left * jump_speed * 100);
			}
		}
	}

	void shield(){
		transmission_receive = TransmissionDataTypes.None;

		if(thisSpriteRenderer.color == Color.white){
			thisSpriteRenderer.color = initColor;
		}
		else if(thisSpriteRenderer.color == initColor){
			thisSpriteRenderer.color = Color.white;
		}
	}


	///////////////////////////////////////////////////////////////////////
	/// 						State Checker Functions
	///////////////////////////////////////////////////////////////////////

	void isGrounded(){
		//always reset
		grounded = false;

		//RaycastHit2D hit = Physics2D.CircleCast (transform.position, Vector2.down, colliderSize.y/1.9f, layerMask);

		RaycastHit2D hit = Physics2D.CircleCast (transform.position, colliderSize.x / 4f, Vector2.down, colliderSize.y / 4f, layerMask);
		// Does the ray intersect any objects excluding the player layer
		if (hit)
		{
			grounded = true;
			Debug.DrawRay(transform.position, Vector2.down * hit.distance, Color.yellow);
			Debug.Log("Did Hit");
		}
		else
		{
			Debug.DrawRay(transform.position, Vector2.down * colliderSize.y/1.9f, Color.white);
			Debug.Log("Did not Hit");
		}
	}

	///////////////////////////////////////////////////////////////////////
	/// 						State Updater Functions
	///////////////////////////////////////////////////////////////////////

	// Update is called once per frame
	void FixedUpdate () {

		//update pre-state functions
		isGrounded ();

		//retrieve and process input
		input_manager.UpdateInput ();
		manageInput ();

		//update post-state functions
		recieveTransmission ();
		transmissionUpdate ();
	}


	void transmissionUpdate(){
		if (transmitting) {
			//increase radius of transmission signal
			trans_signal_radius += transmission_speed;
			transmissionRenderer.xradius = transmissionRenderer.yradius = trans_signal_radius;

			//get distance to other player
			float dist_to_other = Vector3.Distance (transmission_origin, otherPlayer.transform.position);

			if (trans_signal_radius >= dist_to_other + colliderSize.y/2f) {
				//transmission has reached other player, turn off "transmitting" and circle renderer
				transmitting = transmissionRenderer.line.enabled = false;
				otherPlayer.transmission_receive = transmission_send;

			}

			transmissionRenderer.CreatePoints ();
		}
	}
}