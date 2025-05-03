package com.snapit.backend.snapit_server.controller;

import com.snapit.backend.snapit_server.dto.game.VoteMessage;
import com.snapit.backend.snapit_server.service.GameEnvService;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.util.UUID;

import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;

@ExtendWith(MockitoExtension.class)
class GameWebSocketControllerTest {

    @Mock
    private GameEnvService gameEnvService;

    @InjectMocks
    private GameWebSocketController controller;

    @Test
    void voteRoom_shouldInvokeServiceWithCorrectParameters() {
        // given
        UUID roomUUID = UUID.randomUUID();
        VoteMessage voteMessage = new VoteMessage("cafeteria", "",1);

        // when
        controller.vote(roomUUID, voteMessage);

        // then
        verify(gameEnvService, times(1)).voteWithUUID(roomUUID, "cafeteria",1);
    }
}