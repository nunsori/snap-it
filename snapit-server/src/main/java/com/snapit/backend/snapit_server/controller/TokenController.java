package com.snapit.backend.snapit_server.controller;

import com.snapit.backend.snapit_server.service.TokenService;
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

    private final TokenService tokenService;

    public TokenController(TokenService tokenService) {
        this.tokenService = tokenService;
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

        try {
            TokenService.TokenRefreshResult result = tokenService.refreshToken(refreshToken);

            // 새로운 Access Token을 쿠키에 설정
            ResponseCookie accessTokenCookie = ResponseCookie.from("accessToken", result.getAccessToken())
                    .path("/")
                    .maxAge(60 * 60)      // 1시간
                    .sameSite("Strict")
                    .build();

            return ResponseEntity.ok()
                    .header(HttpHeaders.SET_COOKIE, accessTokenCookie.toString())
                    .body(Map.of(
                            "accessToken", result.getAccessToken(),
                            "userId", result.getEmail()
                    ));

        } catch (TokenService.TokenException e) {
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED)
                    .body(Map.of("error", e.getMessage()));
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body(Map.of("error", "Failed to refresh token"));
        }
    }
}