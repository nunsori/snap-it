package com.snapit.backend.snapit_server.controller;

import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.RoomCommand;
import com.snapit.backend.snapit_server.domain.enums.JoinResult;
import com.snapit.backend.snapit_server.dto.JoinMessage;
import com.snapit.backend.snapit_server.dto.RoomCreateRequestDto;
import com.snapit.backend.snapit_server.dto.RoomListMessage;
import com.snapit.backend.snapit_server.dto.UserListMessage;
import com.snapit.backend.snapit_server.dto.game.SimilarityResponseMessage;
import com.snapit.backend.snapit_server.dto.game.SimilarityResultMessage;
import com.snapit.backend.snapit_server.service.GameEnvService;
import com.snapit.backend.snapit_server.service.RoomService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.messaging.handler.annotation.*;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.ResponseBody;
import org.springframework.web.client.RestTemplate;

import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.security.Principal;
import java.util.List;
import java.util.Map;
import java.util.UUID;

@Controller
public class WebSocketController {

    private final SimpMessagingTemplate messagingTemplate;
    private final RoomService roomService;
    private final GameEnvService gameEnvService;

    @Autowired
    public WebSocketController(SimpMessagingTemplate messagingTemplate, RoomService roomService, GameEnvService gameEnvService) {
        this.messagingTemplate = messagingTemplate;
        this.roomService = roomService;
        this.gameEnvService = gameEnvService;
    }

    // 방 목록 요청 처리
    @MessageMapping("/room/list")
    @SendTo("/topic/openrooms")
    public RoomListMessage getRoomsList() {
        return roomService.getAllRooms();
    }

    // 방 생성
    @MessageMapping("/room/create")
    @SendTo("/topic/openrooms")
    public RoomListMessage createRoom(@Payload RoomCreateRequestDto dto,
                                      Principal principal) {
        // DTO → Command 변환
        RoomCommand cmd = RoomCommand.fromRequest(dto);

        // 서비스 호출 및 반환
        return roomService.createRoom(cmd, principal.getName());
    }

    // 방 퇴장
    @MessageMapping("/room/{roomUUID}/leave")
    @SendTo("/topic/openrooms")
    public RoomListMessage leaveRoom(@DestinationVariable UUID roomUUID,
                                     Principal principal) {
        roomService.leaveRoom(roomUUID, principal.getName());
        Room room = roomService.getRoom(roomUUID);
        if(room != null) {
            List<String> userList = room.getUserList();
            messagingTemplate.convertAndSend(" /topic/room/"+roomUUID,
                    new UserListMessage(userList));

        }
        return roomService.getAllRooms();
    }

    // 게임 시작
    @MessageMapping("/room/{roomUUID}/start")
    @SendTo("/topic/openrooms")
    public RoomListMessage startRoom(@DestinationVariable UUID roomUUID) {
        Room room = roomService.gameStartandDeleteRoom(roomUUID);
        gameEnvService.gameInitiate(roomUUID, room);
        return roomService.getAllRooms();
    }

    // 방 입장 (MessageMapping)
    @MessageMapping("/room/{roomUUID}/join")
    @SendTo("/topic/openrooms")
    public RoomListMessage joinRoom(
            @DestinationVariable UUID roomUUID,
            Principal principal) {

        String email = principal.getName();
        roomService.joinRoom(roomUUID, email);
        Room room = roomService.getRoom(roomUUID);
        if(room != null) {
            List<String> userList = room.getUserList();
            messagingTemplate.convertAndSend(" /topic/room/"+roomUUID,
                    new UserListMessage(userList));

        }
        return roomService.getAllRooms();
    }

    private final RestTemplate restTemplate = new RestTemplate();

    @MessageMapping("/room/{roomUUID}/similarity")
    public void getSimilarity(@DestinationVariable UUID roomUUID,
                              @Header("first_word") String first_word,
                              @Header("second_word") String second_word,
                              Principal principal) {
        String email = principal.getName();
        System.out.println("[유사도 계산] 이메일, UUID, first_word, second_word = "
                + email + ", " + roomUUID + ", " + first_word + ", " + second_word);

        // 1. HTTP GET 요청 보내기
        String encodedFirst = URLEncoder.encode(first_word, StandardCharsets.UTF_8);
        String encodedSecond = URLEncoder.encode(second_word, StandardCharsets.UTF_8);

        String url = "http://snap-it-word2vec.snapit-word2voc.svc.cluster.local:8000/similarity?first_word=" + encodedFirst + "&second_word=" + encodedSecond;
        SimilarityResponseMessage similarityResult = restTemplate.getForObject(url, SimilarityResponseMessage.class);
        // 2. WebSocket으로 결과 보내기

        messagingTemplate.convertAndSend("/topic/room/" + roomUUID,
                new SimilarityResultMessage(
                        new SimilarityResultMessage.Body(
                                email,
                                first_word,
                                second_word,
                                similarityResult.similarity()
                        )));
    }
}