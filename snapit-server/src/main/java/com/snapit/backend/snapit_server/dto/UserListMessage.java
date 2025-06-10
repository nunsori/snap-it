package com.snapit.backend.snapit_server.dto;

import java.util.List;

public record UserListMessage(
        String header,
        Body body

) {
    public UserListMessage(Body body) {
        this("userList",body);
    }
    public record Body(List<String> userList){}
}
