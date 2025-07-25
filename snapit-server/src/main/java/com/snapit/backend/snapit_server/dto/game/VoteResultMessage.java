package com.snapit.backend.snapit_server.dto.game;

import com.snapit.backend.snapit_server.domain.enums.GameType;

import java.util.List;

public record VoteResultMessage(
        String header,
        Body body
) {
    public VoteResultMessage(Body body) {
        this("voteResult", body);
    }

    public record Body(int round, String place, GameType gameType) {}
}