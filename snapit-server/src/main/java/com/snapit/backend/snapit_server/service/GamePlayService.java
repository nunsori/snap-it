package com.snapit.backend.snapit_server.service;

import com.snapit.backend.snapit_server.domain.GameScore;
import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.dto.game.FoundStuffRemainMessage;
import com.snapit.backend.snapit_server.dto.game.GameScoreInfoMessage;
import com.snapit.backend.snapit_server.dto.game.ScoreMessage;
import com.snapit.backend.snapit_server.dto.game.TimeOverMessage;
import lombok.RequiredArgsConstructor;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

@Service
@RequiredArgsConstructor
public class GamePlayService {

    private final Map<UUID, List<GameScore>> gameScoreInfo = new ConcurrentHashMap<>();
    private final Map<UUID, List<String>> endCount = new ConcurrentHashMap<>();
    private final Map<UUID, Integer> counts = new ConcurrentHashMap<>();
    // 60% 이상 달성했나 확인하기 위한 자료형에 roomUUID 기반 초기화. 단순 count용. 라운드 종료시 timeOver 위치에서 초기화

    private final SimpMessagingTemplate messagingTemplate;
    private final GameEnvService gameEnvService;


    // [PERSONAL] UUID 기준으로 gameInfo에서 GameScore를 찾아서 해당 유저 점수를 더함. 없으면 생성 후 더함.
    public void addScore(UUID roomUUID, String email, ScoreMessage scoreMessage) {
        System.out.println("[점수 획득 실행]-email,roomUUID="+email+","+roomUUID);
        int round = scoreMessage.round();
        int score = scoreMessage.score();
        System.out.println("[점수 획득 실행]-round,score="+round+","+score);

        List<GameScore> scores = gameScoreInfo.get(roomUUID);
        if (scores == null) {
            System.out.println("[점수 획득 실행]-scores가 null이므로 초기화");
            addGameInfo(roomUUID);
            endCount.put(roomUUID, new ArrayList<>());
            scores = gameScoreInfo.get(roomUUID);
            System.out.println("[점수 획득 실행]- gameScoreInfo 초기화 후 값 확인 = "+gameScoreInfo.get(roomUUID));
            for (GameScore gs : scores) {
                System.out.println("[점수 획득 실행]- gameScoreInfo 초기화 후 값 확인 = "+gs.getEmail()+","+gs.getScore()+","+gs.getScore2());
            }

        }

        // UUID가 키로 존재한다면, 모든 유저가 등록되어있음.
        GameScore gameScore = scores.stream()
                .filter(gs -> gs.getEmail().equals(email))
                .findFirst()
                .orElse(null);

        System.out.println("[점수 획득 실행]-UUID, gameScore Null 여부 = "+roomUUID+","+gameScore);

        if (round == 1) {
            System.out.println("[점수 획득 실행]-round가 1이므로 점수 업데이트");
            gameScore.setScore(Math.max(score, gameScore.getScore()));
            System.out.println("[점수 획득 실행]-획득 점수 score, 현재 최고 점수 = "+score+","+gameScore.getScore());
        } else if (round == 2) {
            System.out.println("[점수 획득 실행]-round가 2이므로 점수 업데이트");
            gameScore.setScore2(Math.max(score, gameScore.getScore2()));
            System.out.println("[점수 획득 실행]-획득 점수 score, 현재 최고 점수 = "+score+","+gameScore.getScore2());
        }

        broadcastGameInfo(roomUUID, round, false, convertGameScoreListToUserInfoList(scores));
    }

    // [COOPERATE] count를 1 올리고, 해당 stuff를 해결했다고 broadcast (협력전이므로 email 필요없다고 판단)
    public void addCount(UUID roomUUID, ScoreMessage scoreMessage) {
        counts.compute(roomUUID, (key, val) -> val == null ? 1 : val + 1);
        broadcastFoundStuffAndRemain(roomUUID, scoreMessage.round(), false, scoreMessage.stuff());
    }


    // 라운드 종료 + 모든 유저 라운드 종료 시
    public void timeOver(UUID roomUUID, String email, TimeOverMessage timeOverMessage) {
        int round = timeOverMessage.round();
        GameType gameType = timeOverMessage.gameType();
        System.out.println("[라운드 종료 실행]-round,email,gameType="+round+","+email+","+gameType);
        endCount.get(roomUUID).add(email);
        if (GameType.PERSONAL.equals(gameType)) {

            // 1라운드 종료시, 게임결과 발송 후 2번째 게임 투표 발송
            if (round == 1 && endCount.get(roomUUID).size() == gameScoreInfo.get(roomUUID).size()) {
                System.out.println("[라운드 종료 실행]-종료된 사람, 총 인원 = "+endCount.get(roomUUID).size()+","+gameScoreInfo.get(roomUUID).size());
                broadcastGameInfo(roomUUID, round, true, convertGameScoreListToUserInfoList(gameScoreInfo.get(roomUUID)));
                gameEnvService.makePlaceListAndSend(roomUUID, 2);
            }
            // 2라운드 종료시, 게임결과 발송 후 키-벨류 정리
            if (round == 2 && endCount.get(roomUUID).size() == 2 * gameScoreInfo.get(roomUUID).size()) {
                System.out.println("[라운드 종료 실행]-종료된 사람, 총 인원 = "+endCount.get(roomUUID).size()+","+gameScoreInfo.get(roomUUID).size());
                broadcastGameInfo(roomUUID, round, true, convertGameScoreListToUserInfoList(gameScoreInfo.get(roomUUID)));
                gameScoreInfo.remove(roomUUID);
                endCount.remove(roomUUID);
            }

        } else if (GameType.COOPERATE.equals(gameType)) {

            // 1라운드 종료시, 게임결과 발송 후 counts 초기화 후 2번째 게임 투표 발송
            if (round == 1 && endCount.get(roomUUID).size() == gameScoreInfo.get(roomUUID).size()) {
                System.out.println("[라운드 종료 실행]-종료된 사람, 총 인원 = "+endCount.get(roomUUID).size()+","+gameScoreInfo.get(roomUUID).size());
                broadcastFoundStuffAndRemain(roomUUID, round, true, "gameEnd");
                counts.put(roomUUID, 0);
                gameEnvService.makePlaceListAndSend(roomUUID, 2);
            }
            // 2라운드 종료시, 게임결과 발송 후 키-벨류 정리. counts 초기화
            if (round == 2 && endCount.get(roomUUID).size() == 2 * gameScoreInfo.get(roomUUID).size()) {
                System.out.println("[라운드 종료 실행]-종료된 사람, 총 인원 = "+endCount.get(roomUUID).size()+","+gameScoreInfo.get(roomUUID).size());
                broadcastFoundStuffAndRemain(roomUUID, round, true, "gameEnd");
                counts.remove(roomUUID);
                gameScoreInfo.remove(roomUUID);
                endCount.remove(roomUUID);
            }

        }

    }

    // [COOPERATE] 특정 UUID로 찾은 물건 + 남은 갯수 broadcast
    private void broadcastFoundStuffAndRemain(UUID roomUUID, int round, boolean isEnd, String stuff) {
        messagingTemplate.convertAndSend("/topic/room/" + roomUUID, new FoundStuffRemainMessage(
                new FoundStuffRemainMessage.Body(round, isEnd, stuff, 10 - counts.get(roomUUID))
        ));
    }

    // [PERSONAL] 특정 UUID로 gameScoreInfo broadcast
    private void broadcastGameInfo(UUID roomUUID, int round, boolean isEnd,
                                   List<GameScoreInfoMessage.UserInfo> userInfoList) {
        System.out.println("[게임 정보 발송 실행]-roomUUID,round,isEnd="+roomUUID+","+round+","+isEnd);
        messagingTemplate.convertAndSend("/topic/room/" + roomUUID, new GameScoreInfoMessage(
                new GameScoreInfoMessage.Body(round, isEnd, userInfoList)
        ));
    }


    // [PERSONAL]scors->UserInfos
    private List<GameScoreInfoMessage.UserInfo> convertGameScoreListToUserInfoList(List<GameScore> scores) {
        System.out.println("[게임 정보 발송 실행]-score에서 UserInfo로 변환");
        return scores.stream()
                .map(gs -> new GameScoreInfoMessage.UserInfo(
                        gs.getEmail(), gs.getScore(), gs.getScore2()))
                .toList();
    }

    // [PERSONAL]UUID로 Room 정보 가져와서 gameInfo에 추가
    private void addGameInfo(UUID roomUUID) {
        Room room = gameEnvService.getRoom(roomUUID);
        List<GameScore> scores = new ArrayList<>();
        for (String s : room.getUserList()) {
            scores.add(new GameScore(s));
        }
        gameScoreInfo.put(roomUUID, scores);
    }
}
