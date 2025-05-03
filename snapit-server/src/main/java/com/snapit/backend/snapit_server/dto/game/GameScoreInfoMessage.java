package com.snapit.backend.snapit_server.dto.game;

import java.util.List;

public record GameScoreInfoMessage(
        String header,
        Body body
) {
    public GameScoreInfoMessage(Body body) {
        this("gameInfo", body);
    }

    public record Body(
            int round,
            boolean isEnd,
            List<UserInfo> userInfoList
    ) {}

    public record UserInfo(
            String email,
            int score,
            int score2
    ) {}
}