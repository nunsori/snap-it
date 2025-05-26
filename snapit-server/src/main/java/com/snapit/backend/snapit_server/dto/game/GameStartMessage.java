package com.snapit.backend.snapit_server.dto.game;

import com.snapit.backend.snapit_server.domain.enums.GameType;

public record GameStartMessage(
        String header,
        GameType gameType) {
}
