package com.snapit.backend.snapit_server.service;

import com.snapit.backend.snapit_server.security.jwt.principal.JwtProvider;
import com.snapit.backend.snapit_server.security.jwt.principal.TokenRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
public class TokenService {

    private final JwtProvider jwtProvider;
    private final TokenRepository tokenRepository;


    public TokenRefreshResult refreshToken(String refreshToken) {
        // Refresh Token이 없는 경우
        if (refreshToken == null) {
            throw new TokenException("Refresh token is missing");
        }

        // Refresh Token 유효성 검사
        if (!jwtProvider.validateToken(refreshToken)) {
            throw new TokenException("Invalid refresh token");
        }

        // Refresh Token에서 사용자 이메일 추출
        String email = jwtProvider.getEmailFromToken(refreshToken);

        // 저장소에 있는 Refresh Token과 일치하는지 확인
        if (!tokenRepository.validateRefreshToken(email, refreshToken)) {
            throw new TokenException("Refresh token does not match");
        }

        // 새로운 Access Token 생성
        String newAccessToken = jwtProvider.createToken(email);

        return new TokenRefreshResult(newAccessToken, email);
    }

    public static class TokenRefreshResult {
        private final String accessToken;
        private final String email;

        public TokenRefreshResult(String accessToken, String email) {
            this.accessToken = accessToken;
            this.email = email;
        }

        public String getAccessToken() {
            return accessToken;
        }

        public String getEmail() {
            return email;
        }
    }

    public static class TokenException extends RuntimeException {
        public TokenException(String message) {
            super(message);
        }
    }
}