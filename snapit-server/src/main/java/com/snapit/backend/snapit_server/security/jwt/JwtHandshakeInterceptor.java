package com.snapit.backend.snapit_server.security.jwt;

import com.snapit.backend.snapit_server.security.jwt.principal.JwtProvider;
import jakarta.servlet.http.Cookie;
import jakarta.servlet.http.HttpServletRequest;
import org.springframework.http.HttpStatus;
import org.springframework.http.server.ServletServerHttpRequest;
import org.springframework.http.server.ServerHttpRequest;
import org.springframework.http.server.ServerHttpResponse;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.WebSocketHandler;
import org.springframework.web.socket.server.HandshakeInterceptor;

import java.util.Map;

@Component
public class JwtHandshakeInterceptor implements HandshakeInterceptor {

    private final JwtProvider jwtProvider;

    public JwtHandshakeInterceptor(JwtProvider jwtProvider) {
        this.jwtProvider = jwtProvider;
    }

    @Override
    public boolean beforeHandshake(
            ServerHttpRequest request,
            ServerHttpResponse response,
            WebSocketHandler wsHandler,
            Map<String, Object> attributes
    ) {
        // 1) 쿠키에서 accessToken 또는 Authorization 헤더 추출
        String jwt = null;
        if (request instanceof ServletServerHttpRequest) {
            HttpServletRequest servletRequest = ((ServletServerHttpRequest) request).getServletRequest();
            Cookie[] cookies = servletRequest.getCookies();
            if (cookies != null) {
                for (Cookie cookie : cookies) {
                    if ("accessToken".equals(cookie.getName())) {
                        jwt = cookie.getValue();
                        break;
                    }
                }
            }
        }
        if (jwt == null) {
            String bearer = request.getHeaders().getFirst("Authorization");
            if (bearer != null && bearer.startsWith("Bearer ")) {
                jwt = bearer.substring(7);
            }
        }
        if (jwt == null) {
            response.setStatusCode(HttpStatus.UNAUTHORIZED);
            return false;
        }

        // 2) 서명·만료 검사
        if (!jwtProvider.validateToken(jwt)) {
            response.setStatusCode(HttpStatus.UNAUTHORIZED);
            return false;
        }

        // 3) 인증 정보 추출 및 세션 속성 저장
        Authentication auth = jwtProvider.getAuthentication(jwt);
        attributes.put("SPRING.AUTH", auth);
        attributes.put("userId", auth.getName());

        return true;
    }

    @Override
    public void afterHandshake(
            ServerHttpRequest request,
            ServerHttpResponse response,
            WebSocketHandler wsHandler,
            Exception exception
    ) {
    }
}