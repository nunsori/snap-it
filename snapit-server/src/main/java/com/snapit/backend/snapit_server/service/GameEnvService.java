package com.snapit.backend.snapit_server.service;

import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.dto.game.GameStartMessage;
import com.snapit.backend.snapit_server.dto.game.RoundStartMessage;
import com.snapit.backend.snapit_server.dto.game.VoteResultMessage;
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
        GameStartMessage startMessage = new GameStartMessage("start");
        roomInfo.put(roomUUID, room);
        messagingTemplate.convertAndSend("/topic/room/" + roomUUID, startMessage);
    }

    // 라운드 시작 + 장소 투표 후보 발송
    public void makePlaceListAndSend(UUID roomUUID, int round) {
        List<String> placeList = geminiService.generatePlaceList();
        messagingTemplate.convertAndSend("/topic/room/" + roomUUID,
                new RoundStartMessage(new RoundStartMessage.Body(round, placeList)));
    }

    // UUID 기반 투표
    public void voteWithUUID(UUID roomUUID, String place, int round) {
        votes.computeIfAbsent(roomUUID, k -> new CopyOnWriteArrayList<>())
                .add(place);

        if (round == 1 && votes.get(roomUUID).size() == roomInfo.get(roomUUID).getCurrentCapacity()) {
            calculateVoteResultAndSend(roomUUID, round);
        }else if (round == 2) {
            calculateVoteResultAndSend(roomUUID, round);
            roomInfo.remove(roomUUID);// 2라운드 투표 끝나면 제거
            votes.remove(roomUUID);
        }
    }

    // 투표 결과 계산 후 전송
    public void calculateVoteResultAndSend(UUID roomUUID, int round) {
        String mostVoted = votes.get(roomUUID).stream()
                .collect(Collectors.groupingBy(Function.identity(), Collectors.counting()))
                .entrySet().stream()
                .max(Map.Entry.comparingByValue())
                .map(Map.Entry::getKey)
                .orElse(null);

        messagingTemplate.convertAndSend("/topic/room/" + roomUUID,
                new VoteResultMessage(new VoteResultMessage.Body(round, mostVoted)));
    }

    public Room getRoom(UUID roomUUID) {
        return roomInfo.get(roomUUID);
    }



}
