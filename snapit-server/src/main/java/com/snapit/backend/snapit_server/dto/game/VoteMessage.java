package com.snapit.backend.snapit_server.dto.game;

public record VoteMessage(
        String place,
        String gameType,
        int round
) {
}
