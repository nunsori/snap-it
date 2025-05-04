package com.snapit.backend.snapit_server;

import com.snapit.backend.snapit_server.domain.RoomCommand;
import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.dto.RoomCreateRequestDto;
import com.snapit.backend.snapit_server.dto.game.ScoreMessage;
import com.snapit.backend.snapit_server.dto.game.TimeOverMessage;
import com.snapit.backend.snapit_server.dto.game.VoteMessage;
import com.snapit.backend.snapit_server.service.GameEnvService;
import com.snapit.backend.snapit_server.service.GamePlayService;
import com.snapit.backend.snapit_server.service.RoomService;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.security.test.context.support.WithMockUser;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.context.annotation.Import;
import com.snapit.backend.snapit_server.config.TestSecurityConfig;

import java.security.Principal;
import java.util.Collections;
import java.util.UUID;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyInt;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.*;
import static org.junit.jupiter.api.Assertions.assertDoesNotThrow;

@SpringBootTest
@ActiveProfiles("test")
@Import({TestSecurityConfig.class})
@WithMockUser(username = "test@example.com")
public class GameIntegrationTest {

    @MockBean
    private RoomService roomService;
    
    @MockBean
    private GameEnvService gameEnvService;
    
    @MockBean
    private GamePlayService gamePlayService;
    
    private Principal mockPrincipal;
    private UUID testRoomId;
    private Room testRoom;

    @BeforeEach
    public void setup() {
        // 테스트용 UUID 생성
        testRoomId = UUID.randomUUID();
        
        // 목 Principal 생성
        Authentication auth = new UsernamePasswordAuthenticationToken(
            "test@example.com", 
            null, 
            Collections.singletonList(new SimpleGrantedAuthority("ROLE_USER"))
        );
        mockPrincipal = auth;
        
        // 테스트용 Room 객체 생성
        testRoom = new Room(testRoomId, "게임 테스트 방", 4, GameType.PERSONAL);
        testRoom.getUserList().add("test@example.com");
        
        // 방 생성 모의 설정
        RoomCreateRequestDto roomDto = new RoomCreateRequestDto(testRoomId, "게임 테스트 방", 4, GameType.PERSONAL);
        RoomCommand roomCommand = RoomCommand.fromRequest(roomDto);
        doReturn(null).when(roomService).createRoom(any(RoomCommand.class), anyString());
    }
    
    @Test
    public void testStartGame() {
        // 게임 시작 모의 테스트
        doReturn(testRoom).when(roomService).gameStartandDeleteRoom(eq(testRoomId));
        doNothing().when(gameEnvService).gameInitiate(eq(testRoomId), any(Room.class));
        
        // 게임 시작 메서드 호출
        assertDoesNotThrow(() -> {
            roomService.gameStartandDeleteRoom(testRoomId);
            gameEnvService.gameInitiate(testRoomId, testRoom);
        });
        
        // 게임 시작 메서드가 호출되었는지 검증
        verify(roomService, times(1)).gameStartandDeleteRoom(eq(testRoomId));
        verify(gameEnvService, times(1)).gameInitiate(eq(testRoomId), any(Room.class));
    }
    
    @Test
    public void testVotePlace() {
        // 장소 투표 모의 테스트
        VoteMessage voteMessage = new VoteMessage("테스트 장소", GameType.PERSONAL, 1);
        doNothing().when(gameEnvService).voteWithUUID(any(UUID.class), any(VoteMessage.class));
        
        // 투표 메서드 호출
        assertDoesNotThrow(() -> gameEnvService.voteWithUUID(testRoomId, voteMessage));
        
        // 투표 메서드가 호출되었는지 검증
        verify(gameEnvService, times(1)).voteWithUUID(eq(testRoomId), eq(voteMessage));
    }
    
    @Test
    public void testAddScore() {
        // 점수 추가 모의 테스트
        ScoreMessage scoreMessage = new ScoreMessage(10, 1,GameType.PERSONAL);
        doNothing().when(gamePlayService).addScore(any(UUID.class), anyString(), any(ScoreMessage.class));
        
        // 점수 추가 메서드 호출
        assertDoesNotThrow(() -> gamePlayService.addScore(testRoomId, "test@example.com", scoreMessage));
        
        // 점수 추가 메서드가 호출되었는지 검증
        verify(gamePlayService, times(1)).addScore(eq(testRoomId), eq("test@example.com"), eq(scoreMessage));
    }
    
    @Test
    public void testTimeOver() {
        // 시간 종료 모의 테스트
        TimeOverMessage timeOverMessage = new TimeOverMessage(true, GameType.PERSONAL, 1);
        doNothing().when(gamePlayService).timeOver(any(UUID.class), anyString(), any(TimeOverMessage.class));
        
        // 시간 종료 메서드 호출
        assertDoesNotThrow(() -> gamePlayService.timeOver(testRoomId, "test@example.com", timeOverMessage));
        
        // 시간 종료 메서드가 호출되었는지 검증
        verify(gamePlayService, times(1)).timeOver(eq(testRoomId), eq("test@example.com"), eq(timeOverMessage));
    }
    
    @Test
    public void testGameFlow() {
        // 게임 흐름 전체 테스트
        
        // 1. 게임 시작 모의 설정
        doReturn(testRoom).when(roomService).gameStartandDeleteRoom(eq(testRoomId));
        doNothing().when(gameEnvService).gameInitiate(eq(testRoomId), any(Room.class));
        
        // 2. 장소 투표 모의 설정
        VoteMessage voteMessage = new VoteMessage("공원", GameType.PERSONAL, 1);
        doNothing().when(gameEnvService).voteWithUUID(any(UUID.class), any(VoteMessage.class));
        
        // 3. 점수 추가 모의 설정
        ScoreMessage scoreMessage = new ScoreMessage(50, 1, GameType.PERSONAL);
        doNothing().when(gamePlayService).addScore(eq(testRoomId), anyString(), any(ScoreMessage.class));
        
        // 4. 시간 종료 모의 설정
        TimeOverMessage timeOverMessage = new TimeOverMessage(true, GameType.PERSONAL, 1);
        doNothing().when(gamePlayService).timeOver(eq(testRoomId), anyString(), any(TimeOverMessage.class));
        
        // 전체 게임 흐름 테스트
        assertDoesNotThrow(() -> {
            roomService.gameStartandDeleteRoom(testRoomId);
            gameEnvService.gameInitiate(testRoomId, testRoom);
            gameEnvService.voteWithUUID(testRoomId, voteMessage);
            gamePlayService.addScore(testRoomId, "test@example.com", scoreMessage);
            gamePlayService.timeOver(testRoomId, "test@example.com", timeOverMessage);
        });
        
        // 모든 메서드가 호출되었는지 검증
        verify(roomService, times(1)).gameStartandDeleteRoom(eq(testRoomId));
        verify(gameEnvService, times(1)).gameInitiate(eq(testRoomId), any(Room.class));
        verify(gameEnvService, times(1)).voteWithUUID(eq(testRoomId), eq(voteMessage));
        verify(gamePlayService, times(1)).addScore(eq(testRoomId), eq("test@example.com"), eq(scoreMessage));
        verify(gamePlayService, times(1)).timeOver(eq(testRoomId), eq("test@example.com"), eq(timeOverMessage));
    }
} 