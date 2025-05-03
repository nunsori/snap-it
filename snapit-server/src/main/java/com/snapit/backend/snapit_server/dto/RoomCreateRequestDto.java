package com.snapit.backend.snapit_server.dto;

import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.enums.GameType;

import java.util.UUID;

public record RoomCreateRequestDto(
        UUID roomUUID,
        String title,
        int maxCapacity,
        GameType gameType
) {
    public static RoomCreateRequestDto fromRoom(Room room) {
        return new RoomCreateRequestDto(
                room.getUuid(),
                room.getTitle(),
                room.getMaxCapacity(),
                room.getGameType()
        );
    }
}