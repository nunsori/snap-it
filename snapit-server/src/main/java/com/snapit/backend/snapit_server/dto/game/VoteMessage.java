package com.snapit.backend.snapit_server.dto.game;

import com.snapit.backend.snapit_server.domain.enums.GameType;

public record VoteMessage(
        String place,
        GameType gameType,
        int round
) {
}
