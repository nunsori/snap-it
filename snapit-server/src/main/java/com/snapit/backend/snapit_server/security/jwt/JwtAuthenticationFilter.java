package com.snapit.backend.snapit_server.security.jwt;

import com.snapit.backend.snapit_server.security.jwt.principal.JwtProvider;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.Cookie;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;

import java.io.IOException;
/**
 * <h4>JWT 토큰 부분 검증</h4>
 * HTTP 요청 헤더에서 JWT 토큰 추출
 * 유효성 검증
 * SecurityContextHolder 에 인증정보 저장
 */


@Component
public class JwtAuthenticationFilter extends OncePerRequestFilter {
    private final JwtProvider jwtProvider;

    public JwtAuthenticationFilter(JwtProvider jwtProvider) {
        this.jwtProvider = jwtProvider;
    }

    @Override
    protected void doFilterInternal(HttpServletRequest req, HttpServletResponse res, FilterChain chain)
            throws ServletException, IOException {
        String requestURI = req.getRequestURI();
        System.out.println("JwtAuthenticationFilter - 요청 URL: " + requestURI);
        
        String token = resolveToken(req);
        if (token != null) {
            System.out.println("JwtAuthenticationFilter - 토큰 발견, 검증 시작");
            if (jwtProvider.validateToken(token)) {
                // Spring Security 내부의 인증·인가 흐름을 단일 타입으로 일원화하기 위해
                // UsernamePasswordAuthenticationToken을 사용
                Authentication auth = jwtProvider.getAuthentication(token);
                SecurityContextHolder.getContext().setAuthentication(auth);
                System.out.println("JwtAuthenticationFilter - 인증 성공: " + auth.getName());
            } else {
                System.out.println("JwtAuthenticationFilter - 토큰 검증 실패");
            }
        } else {
            System.out.println("JwtAuthenticationFilter - 토큰 없음");
        }
        chain.doFilter(req, res);
    }

    private String resolveToken(HttpServletRequest req) {
        // 1) 쿠키에서 accessToken 찾기
        Cookie[] cookies = req.getCookies();
        if (cookies != null) {
            for (Cookie cookie : cookies) {
                if ("accessToken".equals(cookie.getName())) {
                    return cookie.getValue();
                }
            }
        }

        // 2) URL 쿼리 파라미터에서 토큰 추출 시도
        String tokenParam = req.getParameter("token");
        if (tokenParam != null && !tokenParam.isEmpty()) {
            System.out.println("JwtAuthenticationFilter - 토큰 파라미터 발견: " + tokenParam.substring(0, Math.min(10, tokenParam.length())) + "...");
            return tokenParam;
        }

        // 3) 헤더의 Bearer 토큰도 함께 처리하려면 아래 로직 유지
        String bearer = req.getHeader("Authorization");
        if (bearer != null && bearer.startsWith("Bearer ")) {
            return bearer.substring(7);
        }

        return null;
    }
}