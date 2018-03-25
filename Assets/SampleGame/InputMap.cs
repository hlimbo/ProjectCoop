using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//only works with c# 6.0 or higher :(
//using static Inputs.Keys;
//namespace Inputs
//{
//    public static class Keys
//    {
//        public const string LEFT = "left";
//        public const string RIGHT = "right";
//        public const string EMIT_JUMP = "emit_jump";
//        public const string EMIT_DASH = "emit_dash";
//    }
//}

public class InputMap : MonoBehaviour {

    //data labels being sent from client (controller) as json payloads to AirConsole.SCREEN
    public const string LEFT = "left";
    public const string RIGHT = "right";
    public const string JUMP = "jump";
    public const string DASH = "dash";

    public const string EMIT_JUMP = "emit_jump";
    public const string EMIT_DASH = "emit_dash";

    private Dictionary<string, bool> input_map;
    public bool GetKey(string key)
    {
        if (!input_map.ContainsKey(key))
        {
            string msg = string.Format("Key: '{0}' not defined in KeyInputs struct", key);
            throw new UnityException(msg);
        }

        return input_map[key];
    }

    public void SetKey(string key,bool value)
    {
        if(!input_map.ContainsKey(key))
        {
            string msg = string.Format("Key: '{0}' not defined in KeyInputs struct", key);
            throw new UnityException(msg);
        }

        input_map[key] = value;
    }

    void Awake()
    {
        input_map = new Dictionary<string, bool>()
        {
            { LEFT , false },
            { RIGHT, false },
            { JUMP, false },
            { DASH, false },
            { EMIT_JUMP, false },
            { EMIT_DASH, false }
        };
    }
}
