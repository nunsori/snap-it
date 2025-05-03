package com.snapit.backend.snapit_server.dto.game;

public record VoteResultMessage(
        String header,
        Body body
) {
    public VoteResultMessage(Body body) {
        this("voteResult", body);
    }

    public record Body(int round, String place) {}
}