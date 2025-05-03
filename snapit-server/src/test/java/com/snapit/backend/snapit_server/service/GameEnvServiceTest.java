package com.snapit.backend.snapit_server.service;

import com.snapit.backend.snapit_server.dto.game.RoundStartMessage;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.Mockito;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.messaging.simp.SimpMessagingTemplate;

import java.util.List;
import java.util.UUID;

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

        // when
        Mockito.when(geminiService.generatePlaceList()).thenReturn(mockPlaceList);

        // execute
        gameEnvService.makePlaceListAndSend(roomUUID, roundNumber);

        // then
        RoundStartMessage expectedMessage =
                new RoundStartMessage(new RoundStartMessage.Body(roundNumber, mockPlaceList));

        verify(messagingTemplate).convertAndSend(
                "/topic/room/" + roomUUID,
                expectedMessage
        );
    }

}