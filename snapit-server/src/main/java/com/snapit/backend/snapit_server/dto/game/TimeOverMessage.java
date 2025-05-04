package com.snapit.backend.snapit_server.dto.game;

import com.snapit.backend.snapit_server.domain.enums.GameType;

public record TimeOverMessage(
        boolean timeOver,
        GameType gameType,
        int round
) {
}
