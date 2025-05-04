package com.snapit.backend.snapit_server.service;

import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.enums.GameType;
import com.snapit.backend.snapit_server.dto.RoomCreateRequestDto;
import com.snapit.backend.snapit_server.dto.RoomDto;
import com.snapit.backend.snapit_server.dto.RoomListMessage;
import org.springframework.boot.test.context.TestComponent;
import org.springframework.context.annotation.Primary;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

@TestComponent
@Primary
public class MockRoomService {

    private final ConcurrentHashMap<UUID, Room> rooms = new ConcurrentHashMap<>();

    public RoomListMessage getRoomList() {
        List<RoomDto> roomDtos = new ArrayList<>();
        rooms.values().forEach(room -> roomDtos.add(RoomDto.from(room)));
        return RoomListMessage.of(roomDtos);
    }

    public Room createRoom(RoomCreateRequestDto roomDto, String userId) {
        Room room = new Room(
                roomDto.roomUUID(),
                roomDto.title(),
                roomDto.maxCapacity(),
                roomDto.gameType()
        );
        room.getUserList().add(userId);
        room.upCurrentCapacity();
        rooms.put(roomDto.roomUUID(), room);
        return room;
    }

    public void joinRoom(UUID roomId, String userId) {
        Room room = rooms.get(roomId);
        if (room != null) {
            room.getUserList().add(userId);
            room.upCurrentCapacity();
        }
    }

    public void leaveRoom(UUID roomId, String userId) {
        Room room = rooms.get(roomId);
        if (room != null) {
            room.getUserList().remove(userId);
            room.downCurrentCapacity();
            if (room.getCurrentCapacity() == 0) {
                rooms.remove(roomId);
            }
        }
    }
} 