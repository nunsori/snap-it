package com.snapit.backend.snapit_server.service;

import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.dto.game.*;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.function.Function;
import java.util.stream.Collectors;

@Service
public class GameEnvService {

    private final SimpMessagingTemplate messagingTemplate;
    private final GeminiService geminiService;

    private final Map<UUID, CopyOnWriteArrayList<String>> votes = new ConcurrentHashMap<>();
    private final Map<UUID, Room> roomInfo = new ConcurrentHashMap<>();

    public GameEnvService(SimpMessagingTemplate messagingTemplate, GeminiService geminiService) {
        this.messagingTemplate = messagingTemplate;
        this.geminiService = geminiService;
    }

    // 게임 시작
    public void gameInitiate(UUID roomUUID, Room room) {
        // 게임 시작 알림
        GameStartMessage startMessage = new GameStartMessage("start", room.getGameType());
        roomInfo.put(roomUUID, room);
        messagingTemplate.convertAndSend("/topic/room/" + roomUUID, startMessage);
        makePlaceListAndSend(roomUUID, 1);
    }

    // 라운드 시작 + 장소 투표 후보 발송
    public void makePlaceListAndSend(UUID roomUUID, int round) {
        List<String> placeList = geminiService.generatePlaceList();
        messagingTemplate.convertAndSend("/topic/room/" + roomUUID,
                new RoundStartMessage(new RoundStartMessage.Body(round, placeList,
                        roomInfo.get(roomUUID).getGameType())));
    }

    // UUID 기반 투표 + 결과 전송/협동전도 한번에 처리
    public void voteWithUUID(UUID roomUUID, VoteMessage voteMessage) {
        String place = voteMessage.place();
        int round = voteMessage.round();
        GameType gameType = voteMessage.gameType();

        votes.computeIfAbsent(roomUUID, k -> new CopyOnWriteArrayList<>()).add(place);
        if (GameType.PERSONAL.equals(gameType)) {

            if (round == 1 && votes.get(roomUUID).size() == roomInfo.get(roomUUID).getCurrentCapacity()) {
                String mostVoted = calculateVoteResultAndSend(roomUUID, round);
                sendVoteResult(roomUUID, round, mostVoted);
            } else if (round == 2) {
                String mostVoted = calculateVoteResultAndSend(roomUUID, round);
                sendVoteResult(roomUUID, round, mostVoted);
                roomInfo.remove(roomUUID);// 2라운드 투표 끝나면 제거
                votes.remove(roomUUID);
            }
        } else if (GameType.COOPERATE.equals(gameType)) {

            if (round == 1 && votes.get(roomUUID).size() == roomInfo.get(roomUUID).getCurrentCapacity()) {
                String mostVoted = calculateVoteResultAndSend(roomUUID, round);
                List<String> stuffList = geminiService.generateStuffList(mostVoted);
                sendVoteResultAndStuffList(roomUUID, round, mostVoted, stuffList);

                // 60% 이상 달성했나 확인하기 위한 자료형에 roomUUID 기반 초기화.
                // 단순 count용. 다음 라운드 시작하면 초기화할것. 이 자료형은 GamePlayService에 존재.
            } else if (round == 2) {
                String mostVoted = calculateVoteResultAndSend(roomUUID, round);
                List<String> stuffList = geminiService.generateStuffList(mostVoted);
                sendVoteResultAndStuffList(roomUUID, round, mostVoted, stuffList);

                roomInfo.remove(roomUUID);// 2라운드 투표 끝나면 제거
                votes.remove(roomUUID);
            }
        }
    }

    // 투표 결과 계산 후 전송
    public String calculateVoteResultAndSend(UUID roomUUID, int round) {
        return votes.get(roomUUID).stream()
                .collect(Collectors.groupingBy(Function.identity(), Collectors.counting()))
                .entrySet().stream()
                .max(Map.Entry.comparingByValue())
                .map(Map.Entry::getKey)
                .orElse(null);
    }

    private void sendVoteResult(UUID roomUUID, int round, String mostVoted) {
        messagingTemplate.convertAndSend("/topic/room/" + roomUUID,
                new VoteResultMessage(new VoteResultMessage.Body(round,
                        mostVoted, roomInfo.get(roomUUID).getGameType())));
    }

    private void sendVoteResultAndStuffList(UUID roomUUID, int round, String mostVoted, List<String> stuffList) {
        messagingTemplate.convertAndSend("/topic/room/" + roomUUID,
                new VoteResultWithStuffListMessage(new VoteResultWithStuffListMessage.Body(round,
                        mostVoted, stuffList, roomInfo.get(roomUUID).getGameType())));
    }

    public Room getRoom(UUID roomUUID) {
        return roomInfo.get(roomUUID);
    }


}
