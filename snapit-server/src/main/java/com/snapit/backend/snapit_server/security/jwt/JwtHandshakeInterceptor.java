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
        System.out.println("beforeHandshake");
        // 1) 인증 토큰 추출 (쿠키, 헤더, URL 쿼리 파라미터 순으로 시도)
        String jwt = null;

        // 1-1) 쿠키에서 토큰 추출 시도
        if (request instanceof ServletServerHttpRequest) {
            HttpServletRequest servletRequest = ((ServletServerHttpRequest) request).getServletRequest();
            Cookie[] cookies = servletRequest.getCookies();
            if (cookies != null) {
                for (Cookie cookie : cookies) {
                    if ("accessToken".equals(cookie.getName())) {
                        System.out.println("JWT cookie found");
                        jwt = cookie.getValue();
                        break;
                    }
                }
            }

            // 1-2) URL 쿼리 파라미터에서 토큰 추출 시도
            if (jwt == null) {
                String tokenParam = servletRequest.getParameter("token");
                if (tokenParam != null && !tokenParam.isEmpty()) {
                    System.out.println("토큰 파라미터 발견: " + tokenParam.substring(0, Math.min(10, tokenParam.length())) + "...");
                    jwt = tokenParam;
                } else {
                    System.out.println("토큰 파라미터 없음. 요청 파라미터: " + servletRequest.getParameterMap().keySet());
                }
            }
        }

        // 1-3) Authorization 헤더에서 토큰 추출 시도
        if (jwt == null) {
            String bearer = request.getHeaders().getFirst("Authorization");
            if (bearer != null && bearer.startsWith("Bearer ")) {
                jwt = bearer.substring(7);
            }
        }

        // 토큰을 찾지 못한 경우 인증 실패
        if (jwt == null) {
            System.out.println("토큰이 없어 인증 실패");
            response.setStatusCode(HttpStatus.UNAUTHORIZED);
            return false;
        }

        // 2) 서명·만료 검사
        try {
            if (!jwtProvider.validateToken(jwt)) {
                System.out.println("토큰 유효성 검사 실패");
                response.setStatusCode(HttpStatus.UNAUTHORIZED);
                return false;
            }
        } catch (Exception e) {
            System.out.println("토큰 검증 중 예외 발생: " + e.getMessage());
            response.setStatusCode(HttpStatus.UNAUTHORIZED);
            return false;
        }

        // 3) 인증 정보 추출 및 세션 속성 저장
        Authentication auth = jwtProvider.getAuthentication(jwt);
        attributes.put("SPRING.AUTH", auth);
        attributes.put("userId", auth.getName());
        System.out.println("웹소켓 핸드셰이크 성공: 사용자 " + auth.getName());

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