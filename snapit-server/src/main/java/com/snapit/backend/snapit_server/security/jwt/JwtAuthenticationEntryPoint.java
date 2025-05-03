package com.snapit.backend.snapit_server.security.jwt;

import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.http.MediaType;
import org.springframework.security.core.AuthenticationException;
import org.springframework.security.web.AuthenticationEntryPoint;
import org.springframework.stereotype.Component;

import java.io.IOException;
import java.io.PrintWriter;

/**
 * <h4>인증 실패 처리 클래스</h4>
 * 1. 인증 실패시 401 에러를 반환
 * 2. 인증 실패시 클라이언트에게 에러 메시지를 전송
 */
@Component
public class JwtAuthenticationEntryPoint implements AuthenticationEntryPoint {


    @Override
    public void commence(HttpServletRequest request, HttpServletResponse response, AuthenticationException authException) throws IOException, ServletException {
        response.setStatus(HttpServletResponse.SC_UNAUTHORIZED);
        response.setContentType(MediaType.APPLICATION_JSON_VALUE);

        // 간단한 JSON 형태로 에러 응답
        try (PrintWriter writer = response.getWriter()) {
            String message = authException.getMessage() != null
                    ? authException.getMessage()
                    : "Unauthorized";
            writer.write("{\"error\":\"Unauthorized\",\"message\":\"" + message + "\"}");
        }
    }
}