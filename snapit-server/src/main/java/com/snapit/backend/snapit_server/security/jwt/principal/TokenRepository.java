package com.snapit.backend.snapit_server.security.jwt.principal;

import org.springframework.stereotype.Component;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

@Component
public class TokenRepository {
    // 이메일을 키로, Refresh Token을 값으로 저장
    private final Map<String, String> refreshTokenStore = new ConcurrentHashMap<>();

    public void saveRefreshToken(String email, String refreshToken) {
        refreshTokenStore.put(email, refreshToken);
    }

    public String findRefreshToken(String email) {
        return refreshTokenStore.get(email);
    }

    public void removeRefreshToken(String email) {
        refreshTokenStore.remove(email);
    }

    public boolean validateRefreshToken(String email, String refreshToken) {
        String storedToken = refreshTokenStore.get(email);
        return storedToken != null && storedToken.equals(refreshToken);
    }
}
