package com.snapit.backend.snapit_server.dto.game;

import com.snapit.backend.snapit_server.domain.enums.GameType;

import java.util.List;

public record FoundStuffRemainMessage(
        String header,
        Body body
) {
    public FoundStuffRemainMessage(Body body){
        this("gameInfo", body);
    }

    public record Body(int round,
                       boolean isEnd,
                       String foundStuff,
                       int remainCount) {
    }
}
