package com.snapit.backend.snapit_server.domain;

import com.snapit.backend.snapit_server.domain.enums.GameType;
import lombok.Getter;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Getter
public class Room {
    private UUID uuid;
    private String title;
    private int maxCapacity;
    private int currentCapacity;
    private GameType gameType;
    private List<String> userList;

    public Room(UUID uuid,
                String title,
                int maxCapacity,
                GameType gameType) {
        this.uuid = uuid;
        this.title = title;
        this.maxCapacity = maxCapacity;
        this.currentCapacity = 0;
        this.gameType = gameType;
        this.userList = new ArrayList<>();
    }
    public synchronized void upCurrentCapacity() {
        this.currentCapacity++;
    }
    public synchronized void downCurrentCapacity() {
        this.currentCapacity--;
    }
    public synchronized boolean isFull() {
        return this.currentCapacity >= this.maxCapacity;
    }
}
