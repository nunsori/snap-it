package com.snapit.backend.snapit_server.security.oauth2.userinfo.impl;

import com.snapit.backend.snapit_server.security.oauth2.userinfo.OAuth2UserInfo;

import java.util.Map;

// KakaoUserInfo.java

// Kakao
public class KakaoOAuth2UserInfo implements OAuth2UserInfo {
    private final Map<String, Object> attrs;

    public KakaoOAuth2UserInfo(Map<String, Object> attrs) {
        this.attrs = attrs;
    }

    @SuppressWarnings("unchecked")
    public String getId() {
        // 최상위의 "id" 필드
        return attrs.get("id").toString();
    }

    @SuppressWarnings("unchecked")
    public String getEmail() {
        Map<String, Object> account = (Map<String, Object>) attrs.get("kakao_account");
        return (String) account.get("email");
    }

    @SuppressWarnings("unchecked")
    public String getName() {
        Map<String, Object> account = (Map<String, Object>) attrs.get("kakao_account");
        Map<String, Object> profile = (Map<String, Object>) account.get("profile");
        return (String) profile.get("name");
    }

    public Map<String, Object> getAttributes() {
        return attrs;
    }
}