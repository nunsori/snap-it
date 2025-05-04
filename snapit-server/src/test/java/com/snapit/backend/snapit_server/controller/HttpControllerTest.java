package com.snapit.backend.snapit_server.controller;

import com.snapit.backend.snapit_server.domain.enums.JoinResult;
import com.snapit.backend.snapit_server.service.RoomService;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;

import java.util.Map;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertInstanceOf;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

@ExtendWith(MockitoExtension.class)
public class HttpControllerTest {

    @Mock
    private RoomService roomService;

    @InjectMocks
    private HttpController httpController;

    @Test
    public void testJoinRoom_Success() {
        // 준비
        UUID roomUUID = UUID.randomUUID();
        Authentication authentication = mock(Authentication.class);
        when(authentication.getName()).thenReturn("test@example.com");
        when(roomService.joinRoom(any(UUID.class), anyString())).thenReturn(JoinResult.SUCCESS);

        // 실행
        ResponseEntity<?> response = httpController.joinRoom(roomUUID, authentication);

        // 검증
        assertEquals(HttpStatus.OK, response.getStatusCode());
        assertInstanceOf(Map.class, response.getBody());
        Map<String, String> responseBody = (Map<String, String>) response.getBody();
        assertEquals("방에 입장했습니다.", responseBody.get("message"));
    }

    @Test
    public void testJoinRoom_FullCapacity() {
        // 준비
        UUID roomUUID = UUID.randomUUID();
        Authentication authentication = mock(Authentication.class);
        when(authentication.getName()).thenReturn("test@example.com");
        when(roomService.joinRoom(any(UUID.class), anyString())).thenReturn(JoinResult.FULL_CAPACITY);

        // 실행
        ResponseEntity<?> response = httpController.joinRoom(roomUUID, authentication);

        // 검증
        assertEquals(HttpStatus.CONFLICT, response.getStatusCode());
        assertInstanceOf(Map.class, response.getBody());
        Map<String, String> responseBody = (Map<String, String>) response.getBody();
        assertEquals("FULL_CAPACITY", responseBody.get("error"));
        assertEquals("방이 가득 찼습니다.", responseBody.get("message"));
    }

    @Test
    public void testJoinRoom_RoomNotFound() {
        // 준비
        UUID roomUUID = UUID.randomUUID();
        Authentication authentication = mock(Authentication.class);
        when(authentication.getName()).thenReturn("test@example.com");
        when(roomService.joinRoom(any(UUID.class), anyString())).thenReturn(JoinResult.ROOM_NOT_FOUND);

        // 실행
        ResponseEntity<?> response = httpController.joinRoom(roomUUID, authentication);

        // 검증
        assertEquals(HttpStatus.NOT_FOUND, response.getStatusCode());
        assertInstanceOf(Map.class, response.getBody());
        Map<String, String> responseBody = (Map<String, String>) response.getBody();
        assertEquals("ROOM_NOT_FOUND", responseBody.get("error"));
        assertEquals("해당 방을 찾을 수 없습니다.", responseBody.get("message"));
    }

    @Test
    public void testJoinRoom_UnexpectedError() {
        // 준비
        UUID roomUUID = UUID.randomUUID();
        Authentication authentication = mock(Authentication.class);
        when(authentication.getName()).thenReturn("test@example.com");
        when(roomService.joinRoom(any(UUID.class), anyString())).thenReturn(JoinResult.UNEXPECTED_ERROR);

        // 실행
        ResponseEntity<?> response = httpController.joinRoom(roomUUID, authentication);

        // 검증
        assertEquals(HttpStatus.INTERNAL_SERVER_ERROR, response.getStatusCode());
        assertInstanceOf(Map.class, response.getBody());
        Map<String, String> responseBody = (Map<String, String>) response.getBody();
        assertEquals("UNEXPECTED_ERROR", responseBody.get("error"));
        assertEquals("예상치 못한 오류가 발생했습니다.", responseBody.get("message"));
    }
} 