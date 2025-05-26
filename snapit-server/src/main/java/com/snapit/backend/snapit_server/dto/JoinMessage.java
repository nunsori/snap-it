package com.snapit.backend.snapit_server.dto;

import java.util.UUID;

public record JoinMessage(
        UUID roomUUID
) {
}
