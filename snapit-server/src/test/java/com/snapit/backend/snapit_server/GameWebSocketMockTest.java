package com.snapit.backend.snapit_server;

import com.snapit.backend.snapit_server.controller.GameWebSocketController;
import com.snapit.backend.snapit_server.controller.WebSocketController;
import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.RoomCommand;
import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.dto.RoomCreateRequestDto;
import com.snapit.backend.snapit_server.dto.RoomListMessage;
import com.snapit.backend.snapit_server.dto.game.ScoreMessage;
import com.snapit.backend.snapit_server.dto.game.TimeOverMessage;
import com.snapit.backend.snapit_server.dto.game.VoteMessage;
import com.snapit.backend.snapit_server.service.GameEnvService;
import com.snapit.backend.snapit_server.service.GamePlayService;
import com.snapit.backend.snapit_server.service.RoomService;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.messaging.simp.SimpMessagingTemplate;

import java.security.Principal;
import java.util.UUID;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
public class GameWebSocketMockTest {

    @Mock
    private SimpMessagingTemplate messagingTemplate;

    @Mock
    private RoomService roomService;
    
    @Mock
    private GameEnvService gameEnvService;
    
    @Mock
    private GamePlayService gamePlayService;

    @Mock
    private Principal principal;

    @InjectMocks
    private WebSocketController roomController;

    @InjectMocks
    private GameWebSocketController gameController;

    private UUID roomId;
    private String userEmail = "test@example.com";

    @BeforeEach
    public void setup() {
        roomId = UUID.randomUUID();
        lenient().when(principal.getName()).thenReturn(userEmail);
    }

    @Test
    public void testRoomCreation() {
        // 방 생성 요청 설정
        RoomCreateRequestDto requestDto = new RoomCreateRequestDto(roomId, "테스트 방", 4, GameType.PERSONAL);
        RoomListMessage roomListMessage = mock(RoomListMessage.class);
        
        // roomService.createRoom이 호출되었을 때의 동작 모킹
        when(roomService.createRoom(any(RoomCommand.class), eq(userEmail))).thenReturn(roomListMessage);
        
        // 방 생성 요청 실행
        roomController.createRoom(requestDto, principal);
        
        // 메서드 호출 확인
        verify(roomService).createRoom(any(RoomCommand.class), eq(userEmail));
    }

    @Test
    public void testStartGame() {
        // 각 테스트마다 mocking 재설정
        RoomListMessage roomListMessage = mock(RoomListMessage.class);
        lenient().when(roomService.getAllRooms()).thenReturn(roomListMessage);
        
        // 방 객체 생성
        Room room = new Room(roomId, "테스트 방", 4, GameType.PERSONAL);
        lenient().when(roomService.gameStartandDeleteRoom(any(UUID.class))).thenReturn(room);
        
        // 게임 시작 메서드 실행
        roomController.startRoom(roomId);
        
        // 메서드 호출 확인만 수행
        verify(roomService).gameStartandDeleteRoom(roomId);
        verify(gameEnvService).gameInitiate(any(UUID.class), any(Room.class));
    }

    @Test
    public void testVotePlace() {
        // 장소 투표 메시지
        VoteMessage voteMessage = new VoteMessage("테스트 장소", GameType.PERSONAL, 1);
        
        // 메서드 실행
        gameController.vote(roomId, voteMessage);
        
        // 메서드 호출 확인만 수행
        verify(gameEnvService).voteWithUUID(any(UUID.class), any(VoteMessage.class));
    }

    @Test
    public void testAddScore() {
        // 점수 추가 메시지
        ScoreMessage scoreMessage = new ScoreMessage(10, 1,GameType.PERSONAL);
        
        // 메서드 실행
        gameController.score(roomId, scoreMessage, principal);
        
        // 메서드 호출 확인
        verify(gamePlayService).addScore(any(UUID.class), anyString(), any(ScoreMessage.class));
    }

    @Test
    public void testTimeOver() {
        // 시간 종료 메시지
        TimeOverMessage timeOverMessage = new TimeOverMessage(true, GameType.PERSONAL, 1);
        
        // 메서드 실행
        gameController.end(roomId, timeOverMessage, principal);
        
        // 메서드 호출 확인
        verify(gamePlayService).timeOver(any(UUID.class), anyString(), any(TimeOverMessage.class));
    }
} 