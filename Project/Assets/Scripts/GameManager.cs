using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class PlayerInfo
{
    public ProfileData profile;
    public int actor;
    public short kills;
    public short deaths;

    public PlayerInfo(ProfileData p, int a, short k, short d)
    {
        this.profile = p;
        this.actor = a;
        this.kills = k;
        this.deaths = d;
    }
}

public enum State
{
    Waiting = 0,
    Starting = 1,
    Playing = 2,
    Ending = 3
}

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public int mainmenu = 0;
    public int killcount = 9;

 
    private string playerPrefab = "LightBandit";
    public Transform[] spawnpositions;

    public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
    public int myInd ;

    private Text myKills;
    private Text myDeaths;
    public bool repeat = false;

    private Transform leaderBoard;
    public Transform endgame;
    public Transform preGame;

    private bool spawned = false;
    private State state = State.Waiting;

    public enum codes : byte
    {
        NewPlayer,
        UpdatePlayer,
        ChangeStat,
        NewMatch
    }

  
    private void Start()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            spawned = false;
            InitializeUI();
            NewPlayerSend(MainMenu.profile);
            Debug.Log("waiting for more players....");
            preGame.gameObject.SetActive(true);
            playerPrefab = "LightBandit";
        }
        else
        {
            InitializeUI();
            NewPlayerSend(MainMenu.profile);
            spawned = true;
            preGame.gameObject.SetActive(false);
            playerPrefab = "HeavyBandit";

            Spawn();
        }
        
    }
    private void InitializeUI()
    {
        myKills = GameObject.Find("HUD/KD/Kills/Text").GetComponent<Text>();
        myDeaths = GameObject.Find("HUD/KD/Deaths/Text").GetComponent<Text>();

        leaderBoard = GameObject.Find("HUD").transform.Find("Scoreboard").transform;
        endgame = GameObject.Find("Canvas").transform.Find("Endgame").transform;

        RefreshStats();
    }

    private void Leaderboard(Transform lb)
    {
        for(int i = 2; i < lb.childCount; i++)
        {
            Destroy(lb.GetChild(i).gameObject);
        }

 
        GameObject playercard = lb.GetChild(1).gameObject;
        playercard.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(playerInfo);

        

        foreach(PlayerInfo a in sorted)
        {
            GameObject n_card = Instantiate(playercard, lb) as GameObject;

            n_card.transform.Find("Name").GetComponent<Text>().text = a.profile.username;
            n_card.transform.Find("Kills").GetComponent<Text>().text = a.kills.ToString();
            n_card.transform.Find("Deaths").GetComponent<Text>().text = a.deaths.ToString();

            n_card.SetActive(true);
        }
        lb.gameObject.SetActive(true);
    }

    public List<PlayerInfo> SortPlayers(List<PlayerInfo> info)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while (sorted.Count< info.Count)
        {

            short highest = -1;
            PlayerInfo n = info[0];

            foreach(PlayerInfo a in info)
            {
                if(sorted.Contains(a))
                {
                    continue;
                }if(a.kills > highest)
                {
                    n = a;
                    highest = a.kills;
                }

            }
            sorted.Add(n);
        }
        return sorted;
    }
    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    private void RefreshStats()
    {
        if(playerInfo.Count > myInd)
        {
            myKills.text = $"{playerInfo[myInd].kills} kills";
            myDeaths.text = $"{playerInfo[myInd].deaths} deaths";

        }
        else
        {
            myKills.text = "0 kills";
            myDeaths.text = "0 deaths";
        }
    }

    public void Spawn()
    {
   
        Transform temp_spawn = spawnpositions[Random.Range(0, spawnpositions.Length)];
        PhotonNetwork.Instantiate(playerPrefab,temp_spawn.position, temp_spawn.rotation );
    }

    public void Update()
    {

        if(state == State.Ending)
        {
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2 && spawned == false)
        {
            Spawn();
            spawned = true;
            preGame.gameObject.SetActive(false);
 
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (leaderBoard.gameObject.activeSelf)
            {
                leaderBoard.gameObject.SetActive(false);
            }
            else
            {
                Leaderboard(leaderBoard);
            }
        }

      }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(mainmenu);
    }
    private void ValidateConnection()
    {
        if (PhotonNetwork.IsConnected)
        {
            return;
        }
        SceneManager.LoadScene(mainmenu);
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code >= 200)
        {
            return;
        }

        codes e = (codes)photonEvent.Code;
        object[] o = (object[])photonEvent.CustomData;

        switch (e)
        {
            case codes.NewPlayer:
                 NewPlayerReceive(o);
                break;
            case codes.UpdatePlayer:
 
                UpdatePlayersReceive(o);
                break;
            case codes.ChangeStat:
                Debug.Log("ChangeStatReceive");

                ChangeStatReceive(o);
                break;

        }
        
    }

    private void StateCheck()
    {
        if(state == State.Ending)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        state = State.Ending;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();

            if (!repeat)
            {
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;

            }
            
        }


        endgame.gameObject.SetActive(true);
        Leaderboard(endgame.Find("Scoreboard"));

        StartCoroutine(End(4f));
    }
    private void ScoreCheck()
    {
        bool win = false;

        foreach (PlayerInfo a in playerInfo)
        {
            if(a.kills >= 3)
            {
                win = true;
                break;
            }
        }

        if (win)
        {
            if(PhotonNetwork.IsMasterClient && state != State.Ending)
            {
                UpdatePlayersSend((int)State.Ending, playerInfo);
            }
        }
    }

    public void NewMatchSend()
    {
        PhotonNetwork.RaiseEvent(
            (byte)codes.NewMatch,
            null,
            new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            },
            new SendOptions
            {
                Reliability = true
            });            
        }
    
    public void NewMatchReceive()
    {
        state = State.Waiting;

 
        endgame.gameObject.SetActive(false);

        foreach(PlayerInfo a in playerInfo)
        {
            a.kills = 0;
            a.deaths = 0;

        }

        RefreshStats();

        Spawn();
    }
    public void NewPlayerSend(ProfileData p)
    {
        object[] package = new object[7];

        package[0] = p.username;
        package[1] = p.level;
        package[2] = p.exp;
        package[3] = p.character;
        package[4] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[5] = (short)0;
        package[6] = (short)0;
 
        PhotonNetwork.RaiseEvent(
            (byte)codes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    public void NewPlayerReceive(object[] data)
    {
        PlayerInfo p = new PlayerInfo(
            new ProfileData(
                (string)data[0],
                (int)data[1],
                (int)data[2] 

            ),
            (int)data[4],
            (short)data[5],
            (short)data[6]
         );

        playerInfo.Add(p);
        UpdatePlayersSend((int)state,playerInfo);
    }

    public void UpdatePlayersSend(int state, List<PlayerInfo> info)
    {
        object[] package = new object[info.Count + 1];

        package[0] = state;
         for (int i = 0; i < info.Count; i++)
        {
            object[] piece = new object[7];

            piece[0] = info[i].profile.username;
            piece[1] = info[i].profile.level;
            piece[2] = info[i].profile.exp;
 
            piece[4] = info[i].actor;
            piece[5] = info[i].kills;
            piece[6] = info[i].deaths;
 
            package[i+1] = piece;
        }

        PhotonNetwork.RaiseEvent(
            (byte)codes.UpdatePlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }


    public void UpdatePlayersReceive(object[] data)
    {
        state = (State)data[0];
        playerInfo = new List<PlayerInfo>();

        for (int i = 1; i < data.Length; i++)
        {
            object[] extract = (object[])data[i];

            PlayerInfo p = new PlayerInfo(
                new ProfileData(
                    (string)extract[0],
                    (int)extract[1],
                    (int)extract[2]
 
                ),
                (int)extract[4],
                (short)extract[5],
                (short)extract[6]
             );

            playerInfo.Add(p);

            if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor)
            {
                myInd = i-1;
            }
        }
        StateCheck();
    }

    public void ChangeStatReceive(object[] data)
    {
        int actor = (int)data[0];
        byte stat = (byte)data[1];
        byte amt = (byte)data[2];

        for (int i = 0; i < playerInfo.Count; i++)
        {
            if (playerInfo[i].actor == actor)
            {
                switch (stat)
                {
                    case 0: //kills
                        playerInfo[i].kills += amt;
                        Debug.Log($"Player {playerInfo[i].profile.username} : kills = {playerInfo[i].kills}");
                        break;

                    case 1: //deaths
                        playerInfo[i].deaths += amt;
                        Debug.Log($"Player {playerInfo[i].profile.username} : deaths = {playerInfo[i].deaths}");
                        break;
                }

                if (i == myInd)
                {
                    RefreshStats();
                }
            

                break;
            }
        }
        ScoreCheck();
     }

    public void ChangeStatSend(int actor, byte stat, byte amt)
    {
        object[] package = new object[] { actor, stat, amt };

        PhotonNetwork.RaiseEvent(
            (byte)codes.ChangeStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    private IEnumerator End(float wait)
    {
        yield return new WaitForSeconds(wait);

        if (repeat)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NewMatchSend();
            }
        }
        else
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        

    }


}
