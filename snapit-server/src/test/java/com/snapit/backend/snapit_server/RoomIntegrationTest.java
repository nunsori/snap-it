package com.snapit.backend.snapit_server;

import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.dto.RoomCreateRequestDto;
import com.snapit.backend.snapit_server.dto.RoomListMessage;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.client.TestRestTemplate;
import org.springframework.boot.test.web.server.LocalServerPort;
import org.springframework.messaging.converter.MappingJackson2MessageConverter;
import org.springframework.messaging.simp.stomp.StompFrameHandler;
import org.springframework.messaging.simp.stomp.StompHeaders;
import org.springframework.messaging.simp.stomp.StompSession;
import org.springframework.messaging.simp.stomp.StompSessionHandlerAdapter;
import org.springframework.security.test.context.support.WithMockUser;
import org.springframework.web.socket.client.standard.StandardWebSocketClient;
import org.springframework.web.socket.messaging.WebSocketStompClient;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.context.annotation.Import;
import com.snapit.backend.snapit_server.config.TestSecurityConfig;
import com.snapit.backend.snapit_server.config.TestWebSocketConfig;
import com.snapit.backend.snapit_server.config.SocketTestConfig;

import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.List;
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
@Import({TestSecurityConfig.class, TestWebSocketConfig.class, SocketTestConfig.class})
@WithMockUser(username = "test@example.com")
public class RoomIntegrationTest {

    @LocalServerPort
    private int port;

    @Autowired
    private TestRestTemplate restTemplate;

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
            // 웹소켓 연결 시도
            this.stompSession = stompClient.connect(wsUrl, new StompSessionHandlerAdapter() {
                @Override
                public void afterConnected(StompSession session, StompHeaders connectedHeaders) {
                    latch.countDown();
                }
            }).get(5, TimeUnit.SECONDS);
            
            // 연결이 완료될 때까지 대기
            assertTrue(latch.await(5, TimeUnit.SECONDS));
        } catch (Exception e) {
            System.err.println("WebSocket 연결 실패: " + e.getMessage());
            throw e;
        }
    }
    
    @Test
    public void testCreateRoomAndGetRoomList() throws ExecutionException, InterruptedException, TimeoutException {
        // 메시지 구독 설정
        CompletableFuture<RoomListMessage> completableFuture = new CompletableFuture<>();
        stompSession.subscribe("/topic/openrooms", new StompFrameHandler() {
            @Override
            public Type getPayloadType(StompHeaders headers) {
                return RoomListMessage.class;
            }

            @Override
            public void handleFrame(StompHeaders headers, Object payload) {
                completableFuture.complete((RoomListMessage) payload);
            }
        });
        
        // 방 생성 요청
        UUID roomId = UUID.randomUUID();
        RoomCreateRequestDto roomDto = new RoomCreateRequestDto(roomId, "테스트 방", 4, GameType.PERSONAL);
        stompSession.send("/app/room/create", roomDto);
        
        // 응답 검증
        RoomListMessage response = completableFuture.get(5, TimeUnit.SECONDS);
        assertNotNull(response);
        assertThat(response.body().roomList()).isNotEmpty();
        
        // 생성된 방이 목록에 있는지 확인
        boolean roomExists = response.body().roomList().stream()
                .anyMatch(room -> room.roomUUID().toString().equals(roomId.toString()) && room.title().equals("테스트 방"));
        assertThat(roomExists).isTrue();
    }
    
    @Test
    public void testJoinAndLeaveRoom() throws ExecutionException, InterruptedException, TimeoutException {
        // 메시지 구독 설정
        CompletableFuture<RoomListMessage> completableFuture1 = new CompletableFuture<>();
        CompletableFuture<RoomListMessage> completableFuture2 = new CompletableFuture<>();
        
        StompFrameHandler handler = new StompFrameHandler() {
            private boolean firstMessageReceived = false;
            
            @Override
            public Type getPayloadType(StompHeaders headers) {
                return RoomListMessage.class;
            }

            @Override
            public void handleFrame(StompHeaders headers, Object payload) {
                if (!firstMessageReceived) {
                    completableFuture1.complete((RoomListMessage) payload);
                    firstMessageReceived = true;
                } else {
                    completableFuture2.complete((RoomListMessage) payload);
                }
            }
        };
        
        stompSession.subscribe("/topic/openrooms", handler);
        
        // 1. 방 생성 요청
        UUID roomId = UUID.randomUUID();
        RoomCreateRequestDto roomDto = new RoomCreateRequestDto(roomId, "참여 테스트 방", 4, GameType.PERSONAL);
        stompSession.send("/app/room/create", roomDto);
        
        // 방 생성 응답 검증
        RoomListMessage createResponse = completableFuture1.get(5, TimeUnit.SECONDS);
        assertNotNull(createResponse);
        
        // 생성된 방이 목록에 있는지 확인
        boolean roomExists = createResponse.body().roomList().stream()
                .anyMatch(room -> room.roomUUID().toString().equals(roomId.toString()));
        assertThat(roomExists).isTrue();
        
        // 2. 방 퇴장 요청
        stompSession.send("/app/room/" + roomId + "/leave", null);
        
        // 방 퇴장 응답 검증
        RoomListMessage leaveResponse = completableFuture2.get(5, TimeUnit.SECONDS);
        assertNotNull(leaveResponse);
        
        // 방이 목록에서 사라졌는지 확인 (마지막 사용자가 나가면 방이 삭제됨)
        boolean roomExistsAfterLeave = leaveResponse.body().roomList().stream()
                .anyMatch(room -> room.roomUUID().toString().equals(roomId.toString()));
        assertThat(roomExistsAfterLeave).isFalse();
    }
} 