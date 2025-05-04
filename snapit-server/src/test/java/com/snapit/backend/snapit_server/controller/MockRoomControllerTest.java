package com.snapit.backend.snapit_server.controller;

import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.RoomCommand;
import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.domain.enums.JoinResult;
import com.snapit.backend.snapit_server.dto.RoomCreateRequestDto;
import com.snapit.backend.snapit_server.dto.RoomDto;
import com.snapit.backend.snapit_server.dto.RoomListMessage;
import com.snapit.backend.snapit_server.service.GameEnvService;
import com.snapit.backend.snapit_server.service.RoomService;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.security.core.Authentication;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class MockRoomControllerTest {

    @Mock
    private SimpMessagingTemplate messagingTemplate;

    @Mock
    private RoomService roomService;

    @Mock
    private GameEnvService gameEnvService;

    @InjectMocks
    private WebSocketController webSocketController;

    @InjectMocks
    private HttpController httpController;

    @BeforeEach
    public void setup() {
        MockitoAnnotations.openMocks(this);
    }

    @Test
    public void testGetRoomsList() {
        // 가짜 방 목록 메시지 생성
        UUID roomId = UUID.randomUUID();
        List<String> userList = new ArrayList<>();
        userList.add("test@example.com");
        List<RoomDto> rooms = Collections.singletonList(
            new RoomDto(roomId, "Test Room", 1, 4, "PERSONAL", userList)
        );
        RoomListMessage expectedResponse = RoomListMessage.of(rooms);
        
        // RoomService의 getAllRooms 메서드가 가짜 방 목록을 반환하도록 설정
        when(roomService.getAllRooms()).thenReturn(expectedResponse);
        
        // WebSocketController의 getRoomsList 메서드 호출
        RoomListMessage actualResponse = webSocketController.getRoomsList();
        
        // 반환값 검증
        assertNotNull(actualResponse);
        assertEquals(expectedResponse, actualResponse);
    }

    @Test
    public void testCreateRoom() {
        // 테스트 데이터 준비
        UUID roomId = UUID.randomUUID();
        RoomCreateRequestDto dto = new RoomCreateRequestDto(roomId, "Test Room", 4, GameType.PERSONAL);
        
        // 가짜 방 목록 메시지 생성
        List<String> userList = new ArrayList<>();
        userList.add("test@example.com");
        List<RoomDto> rooms = Collections.singletonList(
            new RoomDto(roomId, "Test Room", 1, 4, "PERSONAL", userList)
        );
        RoomListMessage expectedResponse = RoomListMessage.of(rooms);
        
        // Principal 객체 모킹
        Authentication principal = mock(Authentication.class);
        when(principal.getName()).thenReturn("test@example.com");
        
        // RoomService의 createRoom 메서드가 가짜 방 목록을 반환하도록 설정
        when(roomService.createRoom(any(RoomCommand.class), anyString())).thenReturn(expectedResponse);
        
        // WebSocketController의 createRoom 메서드 호출
        RoomListMessage actualResponse = webSocketController.createRoom(dto, principal);
        
        // 반환값 검증
        assertNotNull(actualResponse);
        assertEquals(expectedResponse, actualResponse);
    }

    @Test
    public void testJoinRoom() {
        // 테스트 데이터 준비
        UUID roomId = UUID.randomUUID();
        
        // Principal 객체 모킹
        Authentication authentication = mock(Authentication.class);
        when(authentication.getName()).thenReturn("test@example.com");
        
        // RoomService의 joinRoom 메서드가 성공 결과를 반환하도록 설정
        when(roomService.joinRoom(eq(roomId), anyString())).thenReturn(JoinResult.SUCCESS);
        
        // HttpController의 joinRoom 메서드 호출
        var response = httpController.joinRoom(roomId, authentication);
        
        // 반환값 검증
        assertEquals(200, response.getStatusCode().value());
    }

    @Test
    public void testLeaveRoom() {
        // 테스트 데이터 준비
        UUID roomId = UUID.randomUUID();
        
        // 가짜 방 목록 메시지 생성
        List<RoomDto> rooms = Collections.emptyList(); // 퇴장 후 빈 목록 가정
        RoomListMessage expectedResponse = RoomListMessage.of(rooms);
        
        // Principal 객체 모킹
        Authentication principal = mock(Authentication.class);
        when(principal.getName()).thenReturn("test@example.com");
        
        // RoomService의 getAllRooms 메서드가 가짜 방 목록을 반환하도록 설정
        when(roomService.getAllRooms()).thenReturn(expectedResponse);
        
        // WebSocketController의 leaveRoom 메서드 호출
        RoomListMessage actualResponse = webSocketController.leaveRoom(roomId, principal);
        
        // 반환값 검증
        assertNotNull(actualResponse);
        assertEquals(expectedResponse, actualResponse);
    }

    @Test
    public void testStartRoom() {
        // 테스트 데이터 준비
        UUID roomId = UUID.randomUUID();
        
        // 가짜 방 객체 및 방 목록 메시지 생성
        Room mockRoom = new Room(roomId, "Test Room", 4, GameType.PERSONAL);
        List<RoomDto> rooms = Collections.emptyList(); // 게임 시작 후 빈 목록 가정
        RoomListMessage expectedResponse = RoomListMessage.of(rooms);
        
        // RoomService의 gameStartandDeleteRoom과 getAllRooms 메서드 설정
        when(roomService.gameStartandDeleteRoom(roomId)).thenReturn(mockRoom);
        when(roomService.getAllRooms()).thenReturn(expectedResponse);
        
        // WebSocketController의 startRoom 메서드 호출
        RoomListMessage actualResponse = webSocketController.startRoom(roomId);
        
        // 반환값 검증
        assertNotNull(actualResponse);
        assertEquals(expectedResponse, actualResponse);
    }
} 