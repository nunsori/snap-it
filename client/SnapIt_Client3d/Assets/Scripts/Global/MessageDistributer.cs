using UnityEngine;
using System;
using System.Text.RegularExpressions;

public static class MessageDistributer
{
    public static async void DistributeMessage(string message)
    {
        string dest = ExtractDestination(message);


        if (dest == "/topic/openrooms")
        {
            MainUIController.RoomListUpdateInvoke(JsonUtility.FromJson<RoomListResponse>(ExtractJsonBody(message)));
            //GameController.UpdatePlayingRoomInvoke(JsonUtility.FromJson<RoomListResponse>(ExtractJsonBody(message)));
        }
        else if (dest == "/topic/room/" + GameController.Instance.cur_uuid)
        {


            string body = ExtractJsonBody(message);


            BaseHeader baseHeader = JsonUtility.FromJson<BaseHeader>(body);

            if (baseHeader == null || string.IsNullOrEmpty(baseHeader.header))
            {
                Debug.LogError("Invalid message received: " + body);
                return;
            }

            // 2단계: header에 따라 분기 처리
            switch (baseHeader.header)
            {
                case "start":
                    StartMessage startMsg = JsonUtility.FromJson<StartMessage>(body);
                    Debug.Log("StartMessage received. gameType: " + startMsg.gameType);
                    GameController.Instance.cur_game_type = JsonUtility.FromJson<StartMessage>(body).gameType;
                    //TODO : 투표 시작하기
                    GameController.GameStartEventInvoke();


                    break;

                case "roundStart":
                    RoundStartMessage roundStartMsg = JsonUtility.FromJson<RoundStartMessage>(body);
                    Debug.Log("RoundStartMessage received. Round: " + roundStartMsg.body.round);

                    if (GameController.Instance.startTrigger)
                    {
                        Debug.Log("Game Start");
                        GameController.Instance.startTrigger = false;
                        //GameController.Instance.cur_game_type = JsonUtility.FromJson<StartMessage>(body).gameType;
                        //TODO : setActive false of interact btn;
                        GameController.Instance.cur_round = roundStartMsg.body.round;
                        RoundStartMessage tmp_msg = JsonUtility.FromJson<RoundStartMessage>(body);
                        int randomValue = UnityEngine.Random.Range(0, tmp_msg.body.placeList.Length);
                        VoteRequest vote = new VoteRequest(tmp_msg.body.placeList[randomValue], GameController.Instance.cur_game_type, tmp_msg.body.round);
                        WebSocketService.Instance.SendMessage("/app/room/" + GameController.Instance.cur_uuid + "/vote", JsonUtility.ToJson(vote));
                    }
                    else
                    {
                        GameController.Instance.roundStartMessage = roundStartMsg;
                    }

                    

                    break;

                case "voteResult":
                    VoteResultResponse voteResult = JsonUtility.FromJson<VoteResultResponse>(body);
                    Debug.Log("vote result here : " + voteResult.body.place);

                    UIController.Instance.InitUserColor();

                    //TODO : send gemini and get word
                    GoogleApiController.Instance.send_gemini(voteResult.body.place);
                    
                    break;

                case "gameInfo":
                    //TODO : user점수 업데이트하기
                    GameInfoResponse gameInfo = JsonUtility.FromJson<GameInfoResponse>(body);

                    UIController.Instance.UpdateUserScore(gameInfo);



                    if (gameInfo.body.isEnd)
                    {
                        //게임 끝난것
                        //터치막고 Result popup on, 점수확인
                        //승리자 정보가져오기
                        UserInfo topScorer = null;
                        int maxScore = int.MinValue;

                        int userScore = int.MinValue;

                        foreach (UserInfo user in gameInfo.body.userInfoList)
                        {
                            int totalScore = user.score + user.score2;
                            if (totalScore > maxScore)
                            {
                                maxScore = totalScore;
                                topScorer = user;
                            }

                            if (user.email == GameController.Instance.cur_email)
                            {
                                userScore = user.score + user.score2;
                            }
                        }

                        //게임종료 팝업
                        UIController.Instance.resultPopup.StartPopup(!gameInfo.body.isEnd, topScorer.email, maxScore.ToString(), userScore.ToString(), gameInfo.body.round);
                    }
                    else
                    {
                        //게임 끝나지 않은것
                        //UIController.Instance.resultPopup.StartPopup(gameInfo.body.isEnd, GameController.Instance.cur_email,GameController.Instance.cur_score.ToString());
                    }

                    break;

                case "similarity":
                    //TODO : websocket에 바로 유사도점수보내기, 타이머종료, scan 불가능하도록 조정
                    SimilarityMessage similarityMessage = JsonUtility.FromJson<SimilarityMessage>(body);
                    Debug.Log("similarity get : " + similarityMessage.body.similarity + " / " + similarityMessage.body.email);

                    if (similarityMessage.body.email != GameController.Instance.cur_email)
                    {
                        break;
                    }

                    UIController.ScanInvoke(false);
                    StateTester.checkState = false;

                    GameData data = new GameData
                    {
                        round = GameController.Instance.cur_round,
                        score = (int)(similarityMessage.body.similarity * 100),
                        gameType = GameController.Instance.cur_game_type,
                        stuff = "diohfoiwe(필요없는부분)"
                    };

                    // JSON string 으로 변환

                    string jsonString = JsonUtility.ToJson(data);
                    Debug.Log("json string is : " + jsonString);


                    //점수보내기
                    await WebSocketService.Instance.SendMessage("/app/room/" + GameController.Instance.cur_uuid + "/score", jsonString);

                    //타이머종료하기
                    UIController.CountDownEventInvoke(3);
                    TimeOverInfo overInfo = new TimeOverInfo
                    {
                        timeOver = true,
                        gameType = GameController.Instance.cur_game_type,
                        round = GameController.Instance.cur_round
                    };
                    //타이머 종료 웹소켓 보내기
                    await WebSocketService.Instance.SendMessage("/app/room/" + GameController.Instance.cur_uuid + "/end", JsonUtility.ToJson(overInfo));


                    break;

                case "userList":
                    UserListResponse gameinfoMessage = JsonUtility.FromJson<UserListResponse>(body);

                    GameController.UpdatePlayingRoomInvoke(gameinfoMessage);
                    break;

                default:
                    Debug.LogWarning("Unknown header type: " + baseHeader.header);
                    break;
            }



            if (JsonUtility.FromJson<RoundStartMessage>(body) != null)
            {

            }
            else if (JsonUtility.FromJson<StartMessage>(body) != null)
            {

            }
        }
        else
        {
            Debug.LogWarning("MessageDistributer->DistributeMessage : no destination detected");
        }

    }

    public static string ExtractDestination(string message)
    {
        var match = Regex.Match(message, @"^destination:(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    public static string ExtractJsonBody(string message)
    {
        // 메시지 끝에 \0 (null terminator)가 있을 수 있으므로 제거
        int jsonStartIndex = message.IndexOf("{");
        if (jsonStartIndex >= 0)
        {
            string json = message.Substring(jsonStartIndex).TrimEnd('\0');
            return json;
        }
        return null;
    }

    public static void startRoundTrigger()
    {
        RoundStartMessage roundStartMsg = GameController.Instance.roundStartMessage;
        Debug.Log("Game Start");
        //GameController.Instance.cur_game_type = JsonUtility.FromJson<StartMessage>(body).gameType;
        //TODO : setActive false of interact btn;
        GameController.Instance.cur_round = roundStartMsg.body.round;
        RoundStartMessage tmp_msg = roundStartMsg;
        int randomValue = UnityEngine.Random.Range(0, tmp_msg.body.placeList.Length);
        VoteRequest vote = new VoteRequest(tmp_msg.body.placeList[randomValue], GameController.Instance.cur_game_type, tmp_msg.body.round);
        WebSocketService.Instance.SendMessage("/app/room/" + GameController.Instance.cur_uuid + "/vote", JsonUtility.ToJson(vote));

        GameController.Instance.roundStartMessage = null;
    }
}
