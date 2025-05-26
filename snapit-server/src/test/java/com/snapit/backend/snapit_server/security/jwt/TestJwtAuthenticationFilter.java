package com.snapit.backend.snapit_server.security.jwt;

import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.web.filter.OncePerRequestFilter;

import java.io.IOException;
import java.util.Collections;

public class TestJwtAuthenticationFilter extends OncePerRequestFilter {

    @Override
    protected void doFilterInternal(HttpServletRequest request, HttpServletResponse response, FilterChain filterChain)
            throws ServletException, IOException {
        
        String authHeader = request.getHeader("Authorization");
        
        // 항상 WWW-Authenticate 헤더를 추가
        if (!response.containsHeader("WWW-Authenticate")) {
            response.setHeader("WWW-Authenticate", "Bearer realm=\"snapit\"");
        }
        
        // 테스트에서는 웹소켓 엔드포인트로의 요청을 항상 인증된 것으로 처리
        if (request.getRequestURI().contains("/ws") || 
            (authHeader != null && !authHeader.isEmpty())) {
            
            // 테스트용 인증 정보 생성
            UsernamePasswordAuthenticationToken authentication = new UsernamePasswordAuthenticationToken(
                    "test@example.com",
                    null,
                    Collections.singletonList(new SimpleGrantedAuthority("ROLE_USER"))
            );
            
            // SecurityContext에 인증 정보 설정
            SecurityContextHolder.getContext().setAuthentication(authentication);
        }
        
        filterChain.doFilter(request, response);
    }
} 