package com.snapit.backend.snapit_server.controller;

import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.RoomCommand;
import com.snapit.backend.snapit_server.domain.enums.JoinResult;
import com.snapit.backend.snapit_server.dto.JoinMessage;
import com.snapit.backend.snapit_server.dto.RoomCreateRequestDto;
import com.snapit.backend.snapit_server.dto.RoomListMessage;
import com.snapit.backend.snapit_server.service.GameEnvService;
import com.snapit.backend.snapit_server.service.RoomService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.messaging.handler.annotation.DestinationVariable;
import org.springframework.messaging.handler.annotation.MessageMapping;
import org.springframework.messaging.handler.annotation.Payload;
import org.springframework.messaging.handler.annotation.SendTo;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.ResponseBody;

import java.security.Principal;
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

    // 방 입장 API (POST)
    @MessageMapping("/app/room/{roomUUID}/join")
    @SendTo("/topic/openrooms")
    public RoomListMessage joinRoom(
            @DestinationVariable UUID roomUUID,
            Principal principal) {

        String email = principal.getName();
        roomService.joinRoom(roomUUID, email);

        return roomService.getAllRooms();
    }

}