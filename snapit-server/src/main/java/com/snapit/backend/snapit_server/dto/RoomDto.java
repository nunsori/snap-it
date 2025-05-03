package com.snapit.backend.snapit_server.dto;

import com.snapit.backend.snapit_server.domain.Room;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

public record RoomDto(
        UUID roomUUID,
        String title,
        int currentCapacity,
        int maxCapacity,
        String gameType,
        List<String> userList
) {
    public static RoomDto from(Room room) {
        return new RoomDto(
                room.getUuid(),
                room.getTitle(),
                room.getCurrentCapacity(),
                room.getMaxCapacity(),
                room.getGameType().name(),
                room.getUserList()
        );
    }
}