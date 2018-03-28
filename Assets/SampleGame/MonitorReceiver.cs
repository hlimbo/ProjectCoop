using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;


public class MonitorReceiver : MonoBehaviour {

    public const int MAX_PLAYERS = 2;

    public PlayerController p1;
    public PlayerController p2;

    private void Awake()
    {
        AirConsole.instance.onMessage += OnMessage;
        AirConsole.instance.onConnect += OnConnect;
        AirConsole.instance.onDisconnect += OnDisconnect;
    }

    //helper function ~ returns active player number (where number > 0 on valid player otherwise player is invalid)
    int GetActivePlayer(int device_id)
    {
        return AirConsole.instance.ConvertDeviceIdToPlayerNumber(device_id);
    }

    void OnMessage(int device_id,JToken data)
    {
        // Debug.Log(data.ToString());
        int active_player = GetActivePlayer(device_id);
        if (active_player == -1)
            return;

        bool canMoveLeft = data[InputMap.LEFT] != null && (bool)data[InputMap.LEFT];
        bool canMoveRight = data[InputMap.RIGHT] != null && (bool)data[InputMap.RIGHT];
        bool hasEmitJump = data[InputMap.EMIT_JUMP] != null && (bool)data[InputMap.EMIT_JUMP];
        bool hasEmitDash = data[InputMap.EMIT_DASH] != null && (bool)data[InputMap.EMIT_DASH];

        if (active_player == 0)
        {
            p1.Input.SetKey(InputMap.LEFT, canMoveLeft);
            p1.Input.SetKey(InputMap.RIGHT, canMoveRight);
            p1.Input.SetKey(InputMap.EMIT_JUMP, hasEmitJump);
            p1.Input.SetKey(InputMap.EMIT_DASH, hasEmitDash);
            
            //send signal to other player ~ as instantaneous action
            if(hasEmitJump)
            {
                p2.Input.SetKey(InputMap.JUMP, true);
            }
            if(hasEmitDash)
            {
                p2.Input.SetKey(InputMap.DASH, true);
            }

            //TODO: radial signal transmission where circle collider expands over time at a fixed rate and distance


        }
        else if(active_player == 1)
        {
            p2.Input.SetKey(InputMap.LEFT, canMoveLeft);
            p2.Input.SetKey(InputMap.RIGHT, canMoveRight);
            p2.Input.SetKey(InputMap.EMIT_JUMP, hasEmitJump);
            p2.Input.SetKey(InputMap.EMIT_DASH, hasEmitDash);

            //send signal to other player ~ as instantaneous action
            if (hasEmitJump)
            {
                p1.Input.SetKey(InputMap.JUMP, true);
            }
            if (hasEmitDash)
            {
                p1.Input.SetKey(InputMap.DASH, true);
            }
        }
    }

    void OnConnect(int device_id)
    {
        Debug.Log(string.Format("Device {0} has connected!", device_id));

        if(AirConsole.instance.GetActivePlayerDeviceIds.Count == 0)
        {
            if(AirConsole.instance.GetControllerDeviceIds().Count >= MAX_PLAYERS)
            {
                AirConsole.instance.SetActivePlayers(MAX_PLAYERS);
            }
            else
            {
                Debug.Log("Need 2 players to start");
            }
        }
    }

    //if not enough players are available and a new player reconnects...
    //player numbers get rehashed (e.g. if I was formerly player 2, I could become player 1 when a new controller connects to the game)
    void OnDisconnect(int device_id)
    {
        Debug.Log(string.Format("Device {0} has disconnected", device_id));

        int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber(device_id);
        if(active_player != -1 && AirConsole.instance.GetControllerDeviceIds().Count < MAX_PLAYERS)
        {
            Debug.Log("A player left.. need more players to start!");
            AirConsole.instance.SetActivePlayers(0);
        }
    }

    void OnDestroy()
    {
        if (AirConsole.instance != null)
        {
            AirConsole.instance.onMessage -= OnMessage;
            AirConsole.instance.onConnect -= OnConnect;
            AirConsole.instance.onDisconnect -= OnDisconnect;
        }
    }
}
