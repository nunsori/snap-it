package com.snapit.backend.snapit_server.security.oauth2.userinfo.factory;

import com.snapit.backend.snapit_server.security.oauth2.userinfo.OAuth2UserInfo;
import com.snapit.backend.snapit_server.security.oauth2.userinfo.impl.GoogleOAuth2UserInfo;
import com.snapit.backend.snapit_server.security.oauth2.userinfo.impl.KakaoOAuth2UserInfo;
import org.springframework.stereotype.Component;

import java.util.Map;

@Component
public class OAuth2UserInfoFactory {
    public static OAuth2UserInfo getOAuth2UserInfo(String registrationId,
                                                   Map<String,Object> attributes) {
        if ("google".equalsIgnoreCase(registrationId)) {
            return new GoogleOAuth2UserInfo(attributes);
        }
        if ("kakao".equalsIgnoreCase(registrationId)) {
            return new KakaoOAuth2UserInfo(attributes);
        }
        throw new IllegalArgumentException(
                "지원하지 않는 OAuth2 공급자: " + registrationId);
    }
}