package com.snapit.backend.snapit_server.domain;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class GameScore {
    private String email;
    private int score;
    private int score2;
    public GameScore(String email) {
        this.email = email;
        this.score = 0;
        this.score2 = 0;
    }
}
