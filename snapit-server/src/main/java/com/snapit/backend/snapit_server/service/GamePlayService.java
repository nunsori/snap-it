package com.snapit.backend.snapit_server.service;

import com.snapit.backend.snapit_server.domain.GameScore;
import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.dto.game.GameScoreInfoMessage;
import com.snapit.backend.snapit_server.dto.game.ScoreMessage;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

@Service
public class GamePlayService {

    private final Map<UUID, List<GameScore>> gameInfo = new ConcurrentHashMap<>();
    private final Map<UUID, List<String>> endCount = new ConcurrentHashMap<>();

    private final SimpMessagingTemplate messagingTemplate;
    private final GameEnvService gameEnvService;

    public GamePlayService(SimpMessagingTemplate messagingTemplate, GameEnvService gameEnvService) {
        this.messagingTemplate = messagingTemplate;
        this.gameEnvService = gameEnvService;
    }

    // UUID 기준으로 gameInfo에서 GameScore를 찾아서 해당 유저 점수를 더함. 없으면 생성 후 더함.
    public void addScore(UUID roomUUID, String email, ScoreMessage scoreMessage) {
        int round = scoreMessage.round();
        int score = scoreMessage.score();


        List<GameScore> scores = gameInfo.get(roomUUID);
        if (scores == null) {
            addGameInfo(roomUUID);
            endCount.put(roomUUID, new ArrayList<>());
        }

        // UUID가 키로 존재한다면, 모든 유저가 등록되어있음.
        GameScore gameScore = scores.stream()
                .filter(gs -> gs.getEmail().equals(email))
                .findFirst()
                .orElse(null);

        if (round == 1) {
            gameScore.setScore(Math.max(score, gameScore.getScore()));
        } else if (round == 2) {
            gameScore.setScore2(Math.max(score, gameScore.getScore2()));
        }
        broadcastGameInfo(roomUUID,round,false, convertGameScoreListToUserInfoList(scores));

    }


    // 라운드 종료 + 모든 유저 라운드 종료 시
    public void timeOver(UUID roomUUID, String email, int round) {
        endCount.get(roomUUID).add(email);
        // 1라운드 종료시, 게임결과 발송 후 2번째 게임 투표 발송
        if (round == 1 && endCount.get(roomUUID).size() == gameInfo.get(roomUUID).size()) {
            broadcastGameInfo(roomUUID,round,true, convertGameScoreListToUserInfoList(gameInfo.get(roomUUID)));
            gameEnvService.makePlaceListAndSend(roomUUID,2);
        }
        // 2라운드 종료시, 게임결과 발송 후 키-벨류 정리
        if (round == 2 && endCount.get(roomUUID).size() == 2 * gameInfo.get(roomUUID).size()) {
            broadcastGameInfo(roomUUID,round,true, convertGameScoreListToUserInfoList(gameInfo.get(roomUUID)));
            gameInfo.remove(roomUUID);
            endCount.remove(roomUUID);
        }
    }


    // 특정 UUID로 gameInfo broadcast
    private void broadcastGameInfo(UUID roomUUID, int round, boolean isEnd,
                                   List<GameScoreInfoMessage.UserInfo> userInfoList) {
        messagingTemplate.convertAndSend("/room/" + roomUUID, new GameScoreInfoMessage(
                new GameScoreInfoMessage.Body(round, isEnd, userInfoList)
        ));
    }


    // scors->UserInfos
    private List<GameScoreInfoMessage.UserInfo> convertGameScoreListToUserInfoList(List<GameScore> scores) {
        return scores.stream()
                .map(gs -> new GameScoreInfoMessage.UserInfo(
                        gs.getEmail(), gs.getScore(), gs.getScore2()))
                .toList();
    }

    // UUID로 Room 정보 가져와서 gameInfo에 추가
    private void addGameInfo(UUID roomUUID) {
        Room room = gameEnvService.getRoom(roomUUID);
        List<GameScore> scores = new ArrayList<>();
        for (String s : room.getUserList()) {
            scores.add(new GameScore(s));
        }
        gameInfo.put(roomUUID, scores);
    }
}
