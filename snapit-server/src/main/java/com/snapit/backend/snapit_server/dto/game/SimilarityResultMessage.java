package com.snapit.backend.snapit_server.dto.game;

public record SimilarityResultMessage(
        String header,
        Body bod
) {
    public SimilarityResultMessage(Body body) {
        this("similarity",body);
    }
    public record Body(String email, String firstWord, String secondWord, Double similarity) {}
}
