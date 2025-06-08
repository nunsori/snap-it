package com.snapit.backend.snapit_server.dto;

import java.util.List;

public record UserListMessage(
        List<String> userList
) {
}
