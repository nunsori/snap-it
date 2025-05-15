package com.snapit.backend.snapit_server;

import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.dto.RoomCreateRequestDto;
import com.snapit.backend.snapit_server.dto.RoomListMessage;
import com.snapit.backend.snapit_server.service.MockRoomService;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.client.TestRestTemplate;
import org.springframework.boot.test.web.server.LocalServerPort;
import org.springframework.context.annotation.Import;
import org.springframework.http.HttpHeaders;
import org.springframework.messaging.converter.MappingJackson2MessageConverter;
import org.springframework.messaging.simp.stomp.StompFrameHandler;
import org.springframework.messaging.simp.stomp.StompHeaders;
import org.springframework.messaging.simp.stomp.StompSession;
import org.springframework.messaging.simp.stomp.StompSessionHandlerAdapter;
import org.springframework.security.test.context.support.WithMockUser;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.web.socket.WebSocketHttpHeaders;
import org.springframework.web.socket.client.standard.StandardWebSocketClient;
import org.springframework.web.socket.messaging.WebSocketStompClient;
import com.snapit.backend.snapit_server.config.TestSecurityConfig;
import com.snapit.backend.snapit_server.config.TestWebSocketConfig;
import com.snapit.backend.snapit_server.config.SocketTestConfig;

import java.lang.reflect.Type;
import java.util.UUID;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

import static org.assertj.core.api.Assertions.assertThat;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;

@SpringBootTest(webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT)
@ActiveProfiles("test")
@Import({TestSecurityConfig.class, TestWebSocketConfig.class, SocketTestConfig.class, MockRoomService.class})
@WithMockUser(username = "test@example.com")
public class RoomIntegrationTest {

    @LocalServerPort
    private int port;

    @Autowired
    private TestRestTemplate restTemplate;
    
    @Autowired
    private MockRoomService mockRoomService;

    private String wsUrl;
    private WebSocketStompClient stompClient;
    private StompSession stompSession;

    @BeforeEach
    public void setup() throws Exception {
        this.wsUrl = "ws://localhost:" + port + "/ws";
        
        // 웹소켓 클라이언트 설정
        this.stompClient = new WebSocketStompClient(new StandardWebSocketClient());
        this.stompClient.setMessageConverter(new MappingJackson2MessageConverter());
        
        // 동기화를 위한 래치 생성
        final CountDownLatch latch = new CountDownLatch(1);
        
        try {
            // 웹소켓 연결에 필요한 헤더 설정
            WebSocketHttpHeaders handshakeHeaders = new WebSocketHttpHeaders();
            StompHeaders connectHeaders = new StompHeaders();
            
            // 인증 헤더 추가
            handshakeHeaders.add("Authorization", "Bearer test-token");
            connectHeaders.add("Authorization", "Bearer test-token");
            
            // 웹소켓 연결 시도
            this.stompSession = stompClient.connect(
                wsUrl, 
                handshakeHeaders,  // 핸드셰이크 헤더
                connectHeaders,   // STOMP 연결 헤더
                new StompSessionHandlerAdapter() {
                    @Override
                    public void afterConnected(StompSession session, StompHeaders connectedHeaders) {
                        latch.countDown();
                    }
                }
            ).get(5, TimeUnit.SECONDS);
            
            // 연결이 완료될 때까지 대기
            assertTrue(latch.await(5, TimeUnit.SECONDS));
        } catch (Exception e) {
            System.err.println("WebSocket 연결 실패: " + e.getMessage());
            throw e;
        }
    }
    
    @Test
    public void testCreateRoomAndGetRoomList() throws ExecutionException, InterruptedException, TimeoutException {
        // 테스트 코드를 단순화: 설정(Setup) - 실행(Act) - 검증(Assert) 패턴을 사용
        
        // 1. 설정(Setup): 방 생성
        UUID roomId = UUID.randomUUID();
        RoomCreateRequestDto roomDto = new RoomCreateRequestDto(roomId, "테스트 방", 4, GameType.PERSONAL);
        
        // 2. 실행(Act): MockRoomService를 사용하여 방 생성
        mockRoomService.createRoom(roomDto, "test@example.com");
        
        // 3. 검증(Assert): 방이 생성되었는지 확인
        RoomListMessage response = mockRoomService.getRoomList();
        assertNotNull(response);
        assertThat(response.body().roomList()).isNotEmpty();
        
        boolean roomExists = response.body().roomList().stream()
                .anyMatch(room -> room.roomUUID().toString().equals(roomId.toString()) && room.title().equals("테스트 방"));
        assertThat(roomExists).isTrue();
    }
    
    @Test
    public void testJoinAndLeaveRoom() throws ExecutionException, InterruptedException, TimeoutException {
        // 테스트 코드를 단순화: 설정(Setup) - 실행(Act) - 검증(Assert) 패턴을 사용
        
        // 1. 설정(Setup): MockRoomService를 사용하여 방 생성
        UUID roomId = UUID.randomUUID();
        mockRoomService.createRoom(
            new RoomCreateRequestDto(roomId, "참여 테스트 방", 4, GameType.PERSONAL), 
            "test@example.com"
        );
        
        // 방이 생성되었는지 확인
        boolean roomExists = mockRoomService.getRoomList().body().roomList().stream()
                .anyMatch(room -> room.roomUUID().equals(roomId));
        assertThat(roomExists).isTrue();
        
        // 2. 실행(Act): MockRoomService를 사용하여 방 떠나기
        mockRoomService.leaveRoom(roomId, "test@example.com");
        
        // 3. 검증(Assert): 방이 삭제되었는지 확인
        boolean roomExistsAfterLeave = mockRoomService.getRoomList().body().roomList().stream()
                .anyMatch(room -> room.roomUUID().equals(roomId));
        assertThat(roomExistsAfterLeave).isFalse();
    }
} 