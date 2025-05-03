package com.snapit.backend.snapit_server.domain.enums;

public enum JoinResult {
    SUCCESS, // 참가 성공
    FULL_CAPACITY, // 인원초과
    ROOM_NOT_FOUND, // 방 없음
    UNEXPECTED_ERROR // 예외 발생
}