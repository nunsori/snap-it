package com.snapit.backend.snapit_server.dto.game;

import com.snapit.backend.snapit_server.domain.enums.GameType;

import java.util.List;

public record RoundStartMessage(
        String header,
        Body body
) {
    public RoundStartMessage (Body body){
        this("roundStart", body);
    }
    public record Body(int round, List<String> placeList, GameType gameType) {}
}
