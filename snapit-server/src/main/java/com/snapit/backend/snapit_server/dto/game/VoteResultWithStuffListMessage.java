package com.snapit.backend.snapit_server.dto.game;

import com.snapit.backend.snapit_server.domain.enums.GameType;

import java.util.List;

public record VoteResultWithStuffListMessage(
        String header,
        Body body
) {
    public VoteResultWithStuffListMessage(Body body) {
        this("voteResult", body);
    }

    public record Body(int round, String place, List<String> stuffList, GameType gameType) {}
}
