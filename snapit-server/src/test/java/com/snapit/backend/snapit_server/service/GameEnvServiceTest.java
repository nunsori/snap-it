package com.snapit.backend.snapit_server.service;

import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.dto.game.RoundStartMessage;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.Mockito;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.test.util.ReflectionTestUtils;

import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

import static org.mockito.Mockito.verify;

@ExtendWith(MockitoExtension.class)
class GameEnvServiceTest {

    @Mock
    private SimpMessagingTemplate messagingTemplate;

    @Mock
    private GeminiService geminiService;

    @InjectMocks
    private GameEnvService gameEnvService;


    @Test
    void testMakePlaceListAndSend() {
        // given
        UUID roomUUID = UUID.randomUUID();
        int roundNumber = 1;
        List<String> mockPlaceList = List.of("카페", "공원", "PC방");
        
        // Room 객체 생성 및 설정 - 필요한 인자를 전달
        Room room = new Room(
            roomUUID,
            "테스트 방",
            4, // 최대 인원
            GameType.PERSONAL
        );
        
        // ReflectionTestUtils를 사용하여 private roomInfo 맵에 접근하여 테스트용 Room 객체 추가
        Map<UUID, Room> roomInfoMap = new ConcurrentHashMap<>();
        roomInfoMap.put(roomUUID, room);
        ReflectionTestUtils.setField(gameEnvService, "roomInfo", roomInfoMap);

        // when
        Mockito.when(geminiService.generatePlaceList()).thenReturn(mockPlaceList);

        // execute
        gameEnvService.makePlaceListAndSend(roomUUID, roundNumber);

        // then
        RoundStartMessage expectedMessage =
                new RoundStartMessage(new RoundStartMessage.Body(roundNumber, mockPlaceList, GameType.PERSONAL));

        verify(messagingTemplate).convertAndSend(
                "/topic/room/" + roomUUID,
                expectedMessage
        );
    }

}