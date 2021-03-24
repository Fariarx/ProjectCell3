using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersSystem : MonoBehaviour
{
    private List<Player> players;

    private void Awake()
    {
        players = new List<Player>();
        Globals.playerSystem = this;
    }

    public Player AddPlayer(int pid)
    {
        Player newPlayer;

        if(pid == Globals.defaultLocalPlayerId)
        {
            newPlayer = new LocalPlayer(pid, this.gameObject);
        }
        else
        {
            newPlayer = new BotPlayer(pid, this.gameObject);
        }

        players.Add(newPlayer);

        return newPlayer;
    }
    public bool DeletePlayerFromList(int pid)
    { 
        for (var i = 0; i < players.Count; i++)
        {
            if (players[i].id == pid)
            {
                players.RemoveAt(i);
                return true;
            }
        } 

        return false;
    }
    public Player GetPlayerById(int pid)
    {
        foreach(var player in players)
        {
            if(player.id == pid)
            {
                return player;
            }
        }

        return null;
    }
    public LocalPlayer GetLocalPlayer()
    {
        foreach (var player in players)
        {
            if (player.IsLocalPlayer())
            {
                return (LocalPlayer) player;
            }
        }

        return null;
    }
}
