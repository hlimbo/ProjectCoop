using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum InputAction{
	up_autofire, 	up_keydown,		up_keyup,
	down_autofire, 	down_keydown, 	down_keyup,
	left_autofire, 	left_keydown, 	left_keyup,
	right_autofire, right_keydown, 	right_keyup,
	a1_autofire, 	a1_keydown, 	a1_keyup,
	a2_autofire, 	a2_keydown, 	a2_keyup,
	a3_autofire, 	a3_keydown, 	a3_keyup,
	a4_autofire, 	a4_keydown, 	a4_keyup,
	a5_autofire, 	a5_keydown, 	a5_keyup,
	a6_autofire, 	a6_keydown, 	a6_keyup
};

public enum InputType{
	none,
	leftKeyboard,
	rightKeyboard,
	airconsole,
	PS4,
	XBox
};

public class InputManager : MonoBehaviour {
	public Dictionary<string, InputAction> action_InputAction_mappings = new Dictionary<string, InputAction> {
		{ "up", InputAction.up_autofire },
		{ "down", InputAction.down_autofire },
		{ "left", InputAction.left_autofire },
		{ "right", InputAction.right_autofire },
		{ "a1", InputAction.a1_autofire },
		{ "a2", InputAction.a2_autofire },
		{ "a3", InputAction.a3_autofire },
		{ "a4", InputAction.a4_autofire },
		{ "a5", InputAction.a5_autofire },
		{ "a6", InputAction.a6_autofire }
	};


	//////////////////////////////////////////////////////////////////////////////////////////////////
	/// Config Key Mappings here    Config Key Mappings here    Config Key Mappings here    Config ///
	//////////////////////////////////////////////////////////////////////////////////////////////////

	public Dictionary<string, KeyCode> LKB_action_keycode_mappings = new Dictionary<string, KeyCode>
	{
		{ "up", KeyCode.W },
		{ "down", KeyCode.S },
		{ "left", KeyCode.A },
		{ "right", KeyCode.D },
		{ "a1", KeyCode.R },
		{ "a2", KeyCode.T },
		{ "a3", KeyCode.Y },
		{ "a4", KeyCode.F },
		{ "a5", KeyCode.G },
		{ "a6", KeyCode.H }
	};
	public Dictionary<string, KeyCode> RKB_action_keycode_mappings = new Dictionary<string, KeyCode>
	{
		{ "up", KeyCode.UpArrow },
		{ "down", KeyCode.DownArrow },
		{ "left", KeyCode.LeftArrow },
		{ "right", KeyCode.RightArrow },
		{ "a1", KeyCode.Keypad4 },
		{ "a2", KeyCode.Keypad5 },
		{ "a3", KeyCode.Keypad6 },
		{ "a4", KeyCode.Keypad1 },
		{ "a5", KeyCode.Keypad2 },
		{ "a6", KeyCode.Keypad3 }
	};

	//////////////////////////////////////////////////////////////////////////////////////////////////
	///  InputManager Class Variables
	//////////////////////////////////////////////////////////////////////////////////////////////////

	public Queue<InputAction> inputQueue;
	public InputType playerInputType = InputType.none;
	public List<string> autofire_AllowedActionSet, keydown_AllowedActionSet, keyup_AllowedActionSet;
	public Dictionary<string, bool> current_input;

	public void Init(string input_type = "", Dictionary<string, bool> current_input_ref = null, 
					List<string> autofire_AASet = null, List<string>  keydown_AASet = null, 
					List<string>  keyup_AASet = null){
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

		current_input = current_input_ref;



		autofire_AllowedActionSet = autofire_AASet;
		keydown_AllowedActionSet = keydown_AASet;
		keyup_AllowedActionSet = keyup_AASet;


		try{
		foreach (string action in keydown_AllowedActionSet)
			current_input.Add (action, false);
		foreach (string action in autofire_AllowedActionSet)
			current_input.Add (action, false);
		
		foreach (string action in keyup_AllowedActionSet)
			current_input.Add (action, false);
		} catch {}

		if(input_type == "LKB"){
			playerInputType = InputType.leftKeyboard;
		}
		else if(input_type == "RKB"){
			playerInputType = InputType.rightKeyboard;
		}

		inputQueue = new Queue<InputAction> ();



	}

	void leftKeyboard_input(){
		print("Using leftKeyboard");

		//////////////////////////////////////////
		//KeyPressType: *** keydown ***
		//////////////////////////////////////////

		foreach (string action in keydown_AllowedActionSet) {
			if (Input.GetKeyDown (LKB_action_keycode_mappings [action]))
				current_input [action] = true;
				//inputQueue.Enqueue ((InputAction)((int)action_InputAction_mappings [action] + 1));
		}


		//////////////////////////////////////////
		//KeyPressType: *** autofire ***
		//////////////////////////////////////////

		foreach (string action in autofire_AllowedActionSet) {
			if (Input.GetKey (LKB_action_keycode_mappings [action]))
				current_input [action] = true;
				//inputQueue.Enqueue (action_InputAction_mappings [action]);
		}


	}

	void rightKeyboard_input(){
		
		print("Using rightKeyboard");

		//////////////////////////////////////////
		//KeyPressType: *** keydown ***
		//////////////////////////////////////////

		foreach (string action in keydown_AllowedActionSet) {
			if (Input.GetKeyDown (RKB_action_keycode_mappings [action]))
				current_input [action] = true;
			//inputQueue.Enqueue ((InputAction)((int)action_InputAction_mappings [action] + 1));
		}


		//////////////////////////////////////////
		//KeyPressType: *** autofire ***
		//////////////////////////////////////////

		foreach (string action in autofire_AllowedActionSet) {
			if (Input.GetKey (RKB_action_keycode_mappings [action]))
				current_input [action] = true;
			//inputQueue.Enqueue (action_InputAction_mappings [action]);
		}

	}


	public void UpdateInput () {
		
		inputQueue.Clear ();

		if (playerInputType == InputType.leftKeyboard)
			leftKeyboard_input ();
		
		else if (playerInputType == InputType.rightKeyboard)
			rightKeyboard_input();
	}

}