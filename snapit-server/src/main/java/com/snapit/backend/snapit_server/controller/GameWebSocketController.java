package com.snapit.backend.snapit_server.controller;

import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.dto.game.GameScoreInfoMessage;
import com.snapit.backend.snapit_server.dto.game.ScoreMessage;
import com.snapit.backend.snapit_server.dto.game.TimeOverMessage;
import com.snapit.backend.snapit_server.dto.game.VoteMessage;
import com.snapit.backend.snapit_server.service.GameEnvService;
import com.snapit.backend.snapit_server.service.GamePlayService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.messaging.handler.annotation.DestinationVariable;
import org.springframework.messaging.handler.annotation.MessageMapping;
import org.springframework.messaging.handler.annotation.Payload;
import org.springframework.messaging.handler.annotation.SendTo;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.stereotype.Controller;

import java.security.Principal;
import java.util.List;
import java.util.UUID;

@Controller
public class GameWebSocketController {

    private final GameEnvService gameEnvService;
    private final GamePlayService gamePlayService;

    @Autowired
    public GameWebSocketController(GameEnvService gameEnvService, GamePlayService gamePlayService) {
        this.gameEnvService = gameEnvService;
        this.gamePlayService = gamePlayService;
    }

    // 라운드별 장소 투표
    @MessageMapping("/room/{roomUUID}/vote")
    public void vote(@DestinationVariable UUID roomUUID,
                     @Payload VoteMessage voteMessage) {

        gameEnvService.voteWithUUID(roomUUID, voteMessage);

    }

    // 점수 획득 시
    @MessageMapping("/room/{roomUUID}/score")
    public void score(@DestinationVariable UUID roomUUID,
                      @Payload ScoreMessage scoreMessage,
                      Principal principal) {
        if (GameType.PERSONAL.equals(scoreMessage.gameType())) {
            gamePlayService.addScore(roomUUID, principal.getName(), scoreMessage);
        } else if (GameType.COOPERATE.equals(scoreMessage.gameType())) {
            gamePlayService.addCount(roomUUID,scoreMessage);
        }

    }

    // 시간 경과 완료
    @MessageMapping("/room/{roomUUID}/end")
    public void end(@DestinationVariable UUID roomUUID,
                    @Payload TimeOverMessage timeOverMessage,
                    Principal principal) {

        gamePlayService.timeOver(roomUUID, principal.getName(), timeOverMessage);

    }

}
