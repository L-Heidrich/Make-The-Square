using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

[System.Serializable]
public class ProfileData
{
    public string username;
    public int exp;
    public int level;
    public int character;
     
    public ProfileData(string n_name, int n_level, int n_exp )
    {
        this.username = n_name;
        this.exp = n_exp;
        this.level = n_level;
     }

    public ProfileData() {
        this.username = "";
        this.exp = 0;
        this.level = 1;
     }

  
}

[System.Serializable]
public class MapData
{
    public string name;
    public int scene;
}
public class MainMenu : MonoBehaviourPunCallbacks
{

    public GameObject main;
    public GameObject rooms;
    public GameObject create;
    public GameObject profileSection;
    public GameObject about;


    public Text mapValue;
    public MapData[] maps;
    private int currentMap = 0;
    public GameObject buttonRoom;

    public InputField roomNameTf;
    public Slider playerSlider;
    public Text maxPlayerText;


    private List<RoomInfo> roomList;

    public InputField usernametf;
    public static ProfileData profile = new ProfileData();

    public void Awake()
    {
        Debug.Log("Hello");

        PhotonNetwork.AutomaticallySyncScene = true;

        profile = Data.LoadProfile();
        if(usernametf.text == null)
        {
            usernametf.text = "username";
        }
        else
        {
            usernametf.text = profile.username;

        }
        Connect();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected");
        PhotonNetwork.JoinLobby();
        base.OnConnectedToMaster();
    }

    public override void OnJoinedRoom()
    {
        StartGame();
        base.OnJoinedRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Create();
        base.OnJoinRandomFailed(returnCode, message);
    }

    public void Connect()
        {
        Debug.Log("Connecting....");
        PhotonNetwork.GameVersion = "0.0.0";
        PhotonNetwork.ConnectUsingSettings();
        }

        public void Join()
        {
        PhotonNetwork.JoinRandomRoom();

        }

        public void Create()
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 2;


            options.CustomRoomPropertiesForLobby = new string[] { "map" };

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("map", currentMap);
            options.CustomRoomProperties = properties;

            PhotonNetwork.CreateRoom(roomNameTf.text, options);
        }

        public void CloseAll()
        {
        main.SetActive(false);
        rooms.SetActive(false);
        create.SetActive(false);
        about.SetActive(false);
        }   

    public void ChangeMap()
    {
        currentMap++;

        if(currentMap >= maps.Length)
        {
            currentMap = 0;
        }

        mapValue.text = "MAP: " + maps[currentMap].name;
    }

    public void ChangeMaxPlayerSlider(float value)
    {
        maxPlayerText.text = Mathf.RoundToInt(value).ToString();
    }
    public void OpenMain()
    {
        CloseAll();
        main.SetActive(true);
        
    }

    public void OpenRooms()
    {
        CloseAll();
        rooms.SetActive(true);

    }

    public void OpenProfile()
    {
        CloseAll();
        profileSection.SetActive(true);

    }

    public void OpenAbout()
    {
        CloseAll();
        about.SetActive(true);

    }

    public void OpenCreate()
    {
        CloseAll();
        create.SetActive(true);

        roomNameTf.text = "";

        currentMap = 0;
        mapValue.text = "MAP: " + maps[currentMap].name;

 
    }

    private void ClearRoomList()
    {
        Transform content = rooms.transform.Find("Scroll View/Viewport/Content");
        foreach (Transform a in content) Destroy(a.gameObject) ;
    }

    public override void OnRoomListUpdate(List<RoomInfo> t_roomList)
    {
        roomList = t_roomList;
        ClearRoomList();

        
        Transform content = rooms.transform.Find("Scroll View/Viewport/Content");

        foreach(RoomInfo a in roomList)
        {
            GameObject newRoomButton = Instantiate(buttonRoom, content) as GameObject;

            newRoomButton.transform.Find("Roomname").GetComponent<Text>().text = a.Name;
            newRoomButton.transform.Find("players").GetComponent<Text>().text = a.PlayerCount + " / " + a.MaxPlayers;

            if (a.CustomProperties.ContainsKey("map"))
            {
                newRoomButton.transform.Find("Map/Name").GetComponent<Text>().text = maps[(int)a.CustomProperties["map"]].name;

            }else
            {
                newRoomButton.transform.Find("Map/Name").GetComponent<Text>().text = "--------";
            }

             newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(newRoomButton.transform); });

        }

        base.OnRoomListUpdate(roomList);
        base.OnRoomListUpdate(roomList);
    }

    public void SetChar(int c)
    {
        profile.character = c;
    }

    public void JoinRoom(Transform button)
    {
        string roomName = button.transform.Find("Roomname").GetComponent<Text>().text;
        VerifyUsername();
        PhotonNetwork.JoinRoom(roomName);
    }
    public void StartGame()
        {
        VerifyUsername();   

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Data.SaveProfile(profile);
            PhotonNetwork.LoadLevel(maps[currentMap].scene);

        }

    }
    private void VerifyUsername()
    {

        if (string.IsNullOrEmpty(usernametf.text))
        {
            profile.username = "User " + Random.Range(100, 1000);
        }else
        {
            profile.username = usernametf.text;
        }

    }

}


