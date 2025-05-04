package com.snapit.backend.snapit_server.security.jwt.principal;

import io.jsonwebtoken.*;
import io.jsonwebtoken.security.Keys;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.stereotype.Component;

import javax.crypto.SecretKey;
import java.nio.charset.StandardCharsets;
import java.util.Collections;
import java.util.Date;

@Component
public class JwtProvider {

    private final SecretKey secretKey ;
    private final long expirationTime = 1000L * 60 * 60; // 1시간

    // UserDetailsService 제거
    public JwtProvider(@Value("${secret.key}")String secret) {
        this.secretKey = Keys.hmacShaKeyFor(secret.getBytes(StandardCharsets.UTF_8));
    }

    public String createToken(String email) {
        return Jwts.builder()
                .setSubject(email)
                .claim("role", "ROLE_PLAYER")
                .setIssuedAt(new Date())
                .setExpiration(new Date(System.currentTimeMillis() + expirationTime))
                .signWith(secretKey, SignatureAlgorithm.HS256)
                .compact();
    }

    public boolean validateToken(String token) {
        try {
            Jws<Claims> claims = Jwts.parserBuilder()
                    .setSigningKey(secretKey)
                    .build()
                    .parseClaimsJws(token);
            return !claims.getBody().getExpiration().before(new Date());
        } catch (JwtException | IllegalArgumentException e) {
            return false;
        }
    }

    public Authentication getAuthentication(String token) {
        String email = Jwts.parserBuilder()
                .setSigningKey(secretKey)
                .build()
                .parseClaimsJws(token)
                .getBody()
                .getSubject();

        // 더미 권한(ROLE_USER) 한 개를 부여합니다.
        SimpleGrantedAuthority authority = new SimpleGrantedAuthority("ROLE_USER");
        return new UsernamePasswordAuthenticationToken(
                email,                    // principal 대신 이메일
                token,                    // credentials에 토큰 전달
                Collections.singletonList(authority)
        );
    }
}