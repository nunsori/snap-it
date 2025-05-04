package com.snapit.backend.snapit_server.dto.game;

import com.snapit.backend.snapit_server.domain.enums.GameType;

public record ScoreMessage(
        int round,
        int score,
        GameType gameType,
        String stuff
) {
    public ScoreMessage(int round, int score, GameType gameType) {
        this(round, score, gameType, null);
    }
}
