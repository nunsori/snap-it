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
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
/**
 * <h4>JWT 토큰 부분 검증</h4>
 * HTTP 요청 헤더에서 JWT 토큰 추출
 * 유효성 검증
 * SecurityContextHolder 에 인증정보 저장
 */


@Component
public class JwtAuthenticationFilter extends OncePerRequestFilter {
    private final JwtProvider jwtProvider;
    private final DateTimeFormatter formatter = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss");

    public JwtAuthenticationFilter(JwtProvider jwtProvider) {
        this.jwtProvider = jwtProvider;
    }
    
    private String getTimePrefix() {
        return "[" + LocalDateTime.now().format(formatter) + "] ";
    }

    @Override
    protected void doFilterInternal(HttpServletRequest req, HttpServletResponse res, FilterChain chain)
            throws ServletException, IOException {
        String requestURI = req.getRequestURI();
        System.out.println(getTimePrefix() + "JwtAuthenticationFilter - 요청 URL: " + requestURI);
        
        // 쿠키 디버깅
        Cookie[] cookies = req.getCookies();
        if (cookies != null) {
            System.out.println(getTimePrefix() + "JwtAuthenticationFilter - 쿠키 개수: " + cookies.length);
            for (Cookie cookie : cookies) {
                System.out.println(getTimePrefix() + "JwtAuthenticationFilter - 쿠키 발견: " + cookie.getName() + " (도메인: " + cookie.getDomain() + ", 경로: " + cookie.getPath() + ")");
            }
        } else {
            System.out.println(getTimePrefix() + "JwtAuthenticationFilter - 쿠키 없음");
        }
        
        // 헤더 디버깅
        String authHeader = req.getHeader("Authorization");
        System.out.println(getTimePrefix() + "JwtAuthenticationFilter - Authorization 헤더: " + authHeader);
        
        String token = resolveToken(req);
        if (token != null) {
            System.out.println(getTimePrefix() + "JwtAuthenticationFilter - 토큰 발견, 검증 시작: " + token.substring(0, Math.min(10, token.length())) + "...");
            if (jwtProvider.validateToken(token)) {
                // Spring Security 내부의 인증·인가 흐름을 단일 타입으로 일원화하기 위해
                // UsernamePasswordAuthenticationToken을 사용
                Authentication auth = jwtProvider.getAuthentication(token);
                SecurityContextHolder.getContext().setAuthentication(auth);
                System.out.println(getTimePrefix() + "JwtAuthenticationFilter - 인증 성공: " + auth.getName());
            } else {
                System.out.println(getTimePrefix() + "JwtAuthenticationFilter - 토큰 검증 실패: 유효하지 않은 토큰");
            }
        } else {
            System.out.println(getTimePrefix() + "JwtAuthenticationFilter - 토큰 없음");
        }
        chain.doFilter(req, res);
    }

    private String resolveToken(HttpServletRequest req) {
        // 1) 쿠키에서 accessToken 찾기
        Cookie[] cookies = req.getCookies();
        if (cookies != null) {
            for (Cookie cookie : cookies) {
                if ("accessToken".equals(cookie.getName())) {
                    System.out.println(getTimePrefix() + "JwtAuthenticationFilter - 쿠키에서 토큰 발견");
                    return cookie.getValue();
                }
            }
        }

        // 2) URL 쿼리 파라미터에서 토큰 추출 시도
        String tokenParam = req.getParameter("token");
        if (tokenParam != null && !tokenParam.isEmpty()) {
            System.out.println(getTimePrefix() + "JwtAuthenticationFilter - 토큰 파라미터 발견: " + tokenParam.substring(0, Math.min(10, tokenParam.length())) + "...");
            return tokenParam;
        }

        // 3) 헤더의 Bearer 토큰도 함께 처리하려면 아래 로직 유지
        String bearer = req.getHeader("Authorization");
        if (bearer != null && bearer.startsWith("Bearer ")) {
            System.out.println(getTimePrefix() + "JwtAuthenticationFilter - 헤더에서 Bearer 토큰 발견");
            return bearer.substring(7);
        }

        return null;
    }
}