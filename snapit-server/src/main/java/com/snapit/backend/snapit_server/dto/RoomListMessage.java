package com.snapit.backend.snapit_server.dto;

import java.util.List;

public record RoomListMessage(
        String header,
        Body body
) {
    public static record Body(List<RoomDto> roomList) { }

    public static RoomListMessage of(List<RoomDto> roomDtos) {
        return new RoomListMessage(
                "roomList",
                new Body(roomDtos)
        );
    }
}