package com.snapit.backend.snapit_server.domain;

import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.dto.RoomCreateRequestDto;

import java.util.UUID;


public record RoomCommand(
        UUID roomUUID,
        String title,
        int maxCapacity,
        GameType gameType
) {
    public static RoomCommand fromRequest(RoomCreateRequestDto dto) {
        return new RoomCommand(
                dto.roomUUID(),
                dto.title(),
                dto.maxCapacity(),
                dto.gameType()
        );
    }
}
