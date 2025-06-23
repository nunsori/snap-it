using System.Collections.Generic;

[System.Serializable]
public class RoomListResponse
{
    public string header;
    public Body body;
}

[System.Serializable]
public class Body
{
    public List<RoomInfo> roomList;
}

[System.Serializable]
public class RoomInfo
{
    public string roomUUID;
    public string title;
    public int currentCapacity;
    public int maxCapacity;
    public string gameType;
    public List<string> userList;
}


[System.Serializable]
public class StartMessage
{
    public string header;
    public string gameType;
}

[System.Serializable]
public class RoundStartBody
{
    public int round;
    public string[] placeList;
    public string gameType;
}

[System.Serializable]
public class RoundStartMessage
{
    public string header;
    public RoundStartBody body;
}


[System.Serializable]
public class VoteRequest
{
    public string place;
    public string gameType;
    public int round;

    public VoteRequest(string place, string gameType, int round)
    {
        this.place = place;
        this.gameType = gameType;
        this.round = round;
    }
}

[System.Serializable]
public class BaseHeader
{
    public string header;
}



[System.Serializable]
public class VoteResultResponse
{
    public string header;
    public VoteResultBody body;
}

[System.Serializable]
public class VoteResultBody
{
    public int round;
    public string place;
    public string gameType;
}

[System.Serializable]
public class GameData
{
    public int round;
    public int score;
    public string gameType;
    public string stuff;
}



[System.Serializable]
public class GameInfoResponse
{
    public string header;
    public GameInfoBody body;
}

[System.Serializable]
public class GameInfoBody
{
    public int round;
    public bool isEnd;
    public List<UserInfo> userInfoList;
}

[System.Serializable]
public class UserInfo
{
    public string email;
    public int score;
    public int score2;
}

[System.Serializable]
public class SimilarityMessage
{
    public string header;
    public SimilarityBody body;
}

[System.Serializable]
public class SimilarityBody
{
    public string email;
    public string firstWord;
    public string secondWord;
    public float similarity;
}

[System.Serializable]
public class TimeOverInfo
{
    public bool timeOver;
    public string gameType;
    public int round;
}



[System.Serializable]
public class UserListResponse
{
    public string header;
    public UserListBody body;
}

[System.Serializable]
public class UserListBody
{
    public List<string> userList;
}