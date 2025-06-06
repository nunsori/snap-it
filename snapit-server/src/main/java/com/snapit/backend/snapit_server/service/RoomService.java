package com.snapit.backend.snapit_server.service;

import com.snapit.backend.snapit_server.domain.Room;
import com.snapit.backend.snapit_server.domain.RoomCommand;
import com.snapit.backend.snapit_server.domain.enums.JoinResult;
import com.snapit.backend.snapit_server.dto.RoomDto;
import com.snapit.backend.snapit_server.dto.RoomListMessage;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicReference;
import java.util.stream.Collectors;

@Service
public class RoomService {

    private final Map<UUID, Room> rooms = new ConcurrentHashMap<>();

    // 방 조회
    public RoomListMessage getAllRooms() {
        return getAllRoomListMessage();
    }

    // 방 생성하기
    @Transactional
    public RoomListMessage createRoom(RoomCommand cmd,String email) {
        Room room = new Room(
                cmd.roomUUID(),
                cmd.title(),
                cmd.maxCapacity(),
                cmd.gameType()
        );
        room.getUserList().add(email); // 방 생성한 유저를 해당 방 멤버로 저장
        rooms.put(cmd.roomUUID(), room);

        return getAllRoomListMessage();
    }

    // 방 진입
    @Transactional
    public JoinResult joinRoom(UUID roomUUID, String email) {
        AtomicReference<JoinResult> res = new AtomicReference<>(JoinResult.ROOM_NOT_FOUND);
        System.out.println("[방 진입]이메일, UUID 값 : " + email + ", " + roomUUID);
        rooms.compute(roomUUID, (id, room) -> {
            if (room == null) {// 키가 애초에 없던 경우
                System.out.println("[방 진입]UUID 값 : " + roomUUID + " 방이 없습니다.");
                return null;
            }
            if (room.isFull()) {// 꽉 찬 경우  
                System.out.println("[방 진입]UUID 값 : " + roomUUID + " 방이 꽉 찼습니다.");
                res.set(JoinResult.FULL_CAPACITY);
                return room;
            }
            room.upCurrentCapacity();// 정상 입장
            System.out.println("[방 진입]UUID 값 : " + roomUUID + " 방 인원 증가. 현재 인원 : " + room.getCurrentCapacity());
            room.getUserList().add(email);
            System.out.println("[방 진입]UUID 값 : " + roomUUID + " 방 인원 증가. 현재 인원 : " + room.getCurrentCapacity());
            res.set(JoinResult.SUCCESS);
            return room;
        });

        return res.get();
    }

    // 방 떠나기
    @Transactional
    public void leaveRoom(UUID roomUUID,String email) {
        // 방 인원 0명이면 방 삭제해야 함
        rooms.compute(roomUUID, (id, room) -> {
            if (room == null) {
                return null;
            }
            if (room.getCurrentCapacity() <= 1) {
                return null; // 방 삭제
            }
            // 그 외 경우엔 그대로 유지 또는 수정
            room.getUserList().remove(email);
            room.downCurrentCapacity(); // 예시: 인원 감소
            return room;
        });
    }

    // 게임 시작
    @Transactional
    public Room gameStartandDeleteRoom(UUID roomUUID) {
        return rooms.remove(roomUUID);
    }

    private RoomListMessage getAllRoomListMessage() {
        var roomDtos = rooms.values().stream()
                .map(RoomDto::from)
                .collect(Collectors.toList());
        return RoomListMessage.of(roomDtos);
    }

}