package com.snapit.backend.snapit_server.controller;

import com.snapit.backend.snapit_server.domain.enums.JoinResult;
import com.snapit.backend.snapit_server.service.RoomService;
import org.springframework.security.core.Authentication;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.Map;
import java.util.UUID;

@RestController
public class HttpController {

    private final RoomService roomService;

    @Autowired
    public HttpController(RoomService roomService) {
        this.roomService = roomService;
    }

    // 방 입장 API
    @GetMapping("/app/room/{roomUUID}/join")
    @ResponseBody
    public ResponseEntity<?> joinRoom(
            @PathVariable UUID roomUUID,
            Authentication authentication) {
        String email = authentication.getName();
        JoinResult joinResult = roomService.joinRoom(roomUUID, email);
        return switch (joinResult) {// 200, 409, 404, 500
            case SUCCESS -> ResponseEntity.ok(Map.of("message", "방에 입장했습니다."));
            case FULL_CAPACITY -> ResponseEntity.status(HttpStatus.CONFLICT)
                    .body(Map.of("error", "FULL_CAPACITY", "message", "방이 가득 찼습니다."));
            case ROOM_NOT_FOUND -> ResponseEntity.status(HttpStatus.NOT_FOUND)
                    .body(Map.of("error", "ROOM_NOT_FOUND", "message", "해당 방을 찾을 수 없습니다."));
            case UNEXPECTED_ERROR -> ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body(Map.of("error", "UNEXPECTED_ERROR", "message", "예상치 못한 오류가 발생했습니다."));
        };

    }
}
