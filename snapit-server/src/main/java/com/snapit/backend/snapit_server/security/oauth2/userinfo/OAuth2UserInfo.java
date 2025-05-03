package com.snapit.backend.snapit_server.security.oauth2.userinfo;

import java.util.Map;

public interface OAuth2UserInfo {
    String getId();
    String getEmail();
    String getName();
    Map<String, Object> getAttributes();
}