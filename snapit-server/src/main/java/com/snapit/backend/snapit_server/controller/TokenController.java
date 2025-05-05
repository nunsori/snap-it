package com.snapit.backend.snapit_server.controller;


import com.snapit.backend.snapit_server.security.jwt.principal.JwtProvider;
import com.snapit.backend.snapit_server.security.jwt.principal.TokenRepository;
import jakarta.servlet.http.Cookie;
import jakarta.servlet.http.HttpServletRequest;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseCookie;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@RestController
@RequestMapping("/api/token")
public class TokenController {

    private final JwtProvider jwtProvider;
    private final TokenRepository tokenRepository;

    public TokenController(JwtProvider jwtProvider, TokenRepository tokenRepository) {
        this.jwtProvider = jwtProvider;
        this.tokenRepository = tokenRepository;
    }
    @GetMapping("/test")
    public ResponseEntity<String> test(HttpServletRequest request) {
        return ResponseEntity.ok("테스트 성공!");
    }

    @PostMapping("/refresh")
    public ResponseEntity<?> refreshToken(
            @CookieValue(value = "refreshToken", required = false) String cookieRefreshToken,
            HttpServletRequest request) {

        System.out.println("재발급 시작");

        // 1️⃣ 쿠키 → 2️⃣ HTTP Body 순서로 Refresh Token 탐색
        String refreshToken = cookieRefreshToken;
        if (refreshToken == null) {
            refreshToken = request.getHeader("refreshToken");
        }

        // Refresh Token이 없는 경우
        if (refreshToken == null) {
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED)
                    .body(Map.of("error", "Refresh token is missing"));
        }

        try {
            // Refresh Token 유효성 검사
            if (!jwtProvider.validateToken(refreshToken)) {
                return ResponseEntity.status(HttpStatus.UNAUTHORIZED)
                        .body(Map.of("error", "Invalid refresh token"));
            }

            // Refresh Token에서 사용자 이메일 추출
            String email = jwtProvider.getEmailFromToken(refreshToken);

            // 저장소에 있는 Refresh Token과 일치하는지 확인
            if (!tokenRepository.validateRefreshToken(email, refreshToken)) {
                return ResponseEntity.status(HttpStatus.UNAUTHORIZED)
                        .body(Map.of("error", "Refresh token does not match"));
            }

            // 새로운 Access Token 생성
            String newAccessToken = jwtProvider.createToken(email);

            // 새로운 Access Token을 쿠키에 설정
            ResponseCookie accessTokenCookie = ResponseCookie.from("accessToken", newAccessToken)
                    .httpOnly(true)
                    .path("/")
                    .maxAge(60 * 60)      // 1시간
                    .sameSite("Strict")
                    .build();

            return ResponseEntity.ok()
                    .header(HttpHeaders.SET_COOKIE, accessTokenCookie.toString())
                    .body(Map.of(
                            "accessToken", newAccessToken,
                            "userId", email
                    ));

        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body(Map.of("error", "Failed to refresh token"));
        }
    }
}