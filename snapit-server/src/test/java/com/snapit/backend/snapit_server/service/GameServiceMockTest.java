package com.snapit.backend.snapit_server.service;

import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.dto.game.ScoreMessage;
import com.snapit.backend.snapit_server.dto.game.TimeOverMessage;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.util.UUID;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyInt;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.doNothing;
import static org.mockito.Mockito.verify;

@ExtendWith(MockitoExtension.class)
public class GameServiceMockTest {

    @Mock
    private GameEnvService gameEnvService;
    
    @Mock
    private GamePlayService gamePlayService;

//    @Test
//    public void testVotePlace() {
//        // 모킹
//        UUID roomId = UUID.randomUUID();
//        String place = "테스트 장소";
//        int round = 1;
//
//        // void 메서드 모킹
//        doNothing().when(gameEnvService).voteWithUUID(any(UUID.class), anyString(), anyInt());
//
//        // 메서드 호출
//        gameEnvService.voteWithUUID(roomId, place, round);
//
//        // 검증
//        verify(gameEnvService).voteWithUUID(eq(roomId), eq(place), eq(round));
//    }
    
    @Test
    public void testAddScore() {
        // 모킹
        UUID roomId = UUID.randomUUID();
        String userEmail = "test@example.com";
        ScoreMessage scoreMessage = new ScoreMessage(10, 1,GameType.PERSONAL);
        
        // void 메서드 모킹
        doNothing().when(gamePlayService).addScore(any(UUID.class), anyString(), any(ScoreMessage.class));
        
        // 메서드 호출
        gamePlayService.addScore(roomId, userEmail, scoreMessage);
        
        // 검증
        verify(gamePlayService).addScore(eq(roomId), eq(userEmail), eq(scoreMessage));
    }
    
    @Test
    public void testTimeOver() {
        // 모킹
        UUID roomId = UUID.randomUUID();
        String userEmail = "test@example.com";
        TimeOverMessage timeOverMessage = new TimeOverMessage(true, GameType.PERSONAL, 1);
        
        // void 메서드 모킹
        doNothing().when(gamePlayService).timeOver(any(UUID.class), anyString(), any(TimeOverMessage.class));
        
        // 메서드 호출
        gamePlayService.timeOver(roomId, userEmail, timeOverMessage);
        
        // 검증
        verify(gamePlayService).timeOver(eq(roomId), eq(userEmail), eq(timeOverMessage));
    }
} 