package com.snapit.backend.snapit_server.dto.game;

public record TimeOverMessage(
        boolean timeOver,
        String gameType,
        int round
) {
}
