package com.snapit.backend.snapit_server.security.jwt.principal;

import org.springframework.context.annotation.Primary;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.stereotype.Component;

import java.util.Collections;

/**
 * 테스트 환경에서 사용하는 JwtProvider
 * 테스트 환경에서는 모든 토큰이 유효하다고 가정합니다.
 */
@Component
@Primary
public class TestJwtProvider extends JwtProvider {

    public TestJwtProvider() {
        super("test-secret-key-for-testing-purposes-only-not-for-production");
    }

    @Override
    public boolean validateToken(String token) {
        // 테스트 환경에서는 항상 토큰이 유효하다고 간주합니다.
        return true;
    }

    @Override
    public Authentication getAuthentication(String token) {
        // 테스트에서는 고정된 이메일과 권한을 사용합니다.
        SimpleGrantedAuthority authority = new SimpleGrantedAuthority("ROLE_USER");
        return new UsernamePasswordAuthenticationToken(
                "test@example.com",
                token,
                Collections.singletonList(authority)
        );
    }
} 