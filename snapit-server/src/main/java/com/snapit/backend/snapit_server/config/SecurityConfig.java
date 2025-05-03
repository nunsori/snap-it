package com.snapit.backend.snapit_server.config;

import com.snapit.backend.snapit_server.security.jwt.JwtAuthenticationEntryPoint;
import com.snapit.backend.snapit_server.security.jwt.JwtAuthenticationFilter;
import com.snapit.backend.snapit_server.security.oauth2.OAuth2UserService;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.security.config.annotation.method.configuration.EnableMethodSecurity;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.config.http.SessionCreationPolicy;
import org.springframework.security.oauth2.client.userinfo.DefaultOAuth2UserService;
import org.springframework.security.web.SecurityFilterChain;
import org.springframework.security.web.authentication.AuthenticationSuccessHandler;
import org.springframework.security.web.authentication.UsernamePasswordAuthenticationFilter;
import org.springframework.web.cors.CorsConfiguration;
import org.springframework.web.cors.CorsConfigurationSource;
import org.springframework.web.cors.UrlBasedCorsConfigurationSource;

@Configuration
@EnableMethodSecurity
public class SecurityConfig {

    private final DefaultOAuth2UserService oAuth2UserService;
    private final AuthenticationSuccessHandler oAuth2SuccessHandler;
    private final JwtAuthenticationFilter jwtAuthenticationFilter;
    private final JwtAuthenticationEntryPoint jwtAuthenticationEntryPoint;

    public SecurityConfig(OAuth2UserService oAuth2UserService,
                          AuthenticationSuccessHandler oAuth2SuccessHandler,
                          JwtAuthenticationFilter jwtAuthenticationFilter,
                          JwtAuthenticationEntryPoint jwtAuthenticationEntryPoint) {
        this.oAuth2UserService = oAuth2UserService;
        this.oAuth2SuccessHandler = oAuth2SuccessHandler;
        this.jwtAuthenticationFilter = jwtAuthenticationFilter;
        this.jwtAuthenticationEntryPoint = jwtAuthenticationEntryPoint;
    }

    @Bean
    public SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
        http
                .csrf(csrf -> csrf.disable())
                .cors(cors -> cors.configurationSource(corsConfigurationSource())) // ✅ CORS 설정 추가
                .sessionManagement(sess -> sess.sessionCreationPolicy(SessionCreationPolicy.STATELESS))
                .authorizeHttpRequests(auth -> auth
                        .requestMatchers("/login", "/oauth2/**").permitAll()
                        .anyRequest().authenticated()
                )
                .oauth2Login(oauth2 -> oauth2
                        .loginPage("/login")
                        .userInfoEndpoint(u -> u.userService(oAuth2UserService))
                        .successHandler(oAuth2SuccessHandler)
                )
                // ↓ JWT 검증 필터 등록
                .addFilterBefore(jwtAuthenticationFilter, UsernamePasswordAuthenticationFilter.class)
                // ↓ (선택) 인증 실패 핸들러
                .exceptionHandling(ex ->
                        ex.authenticationEntryPoint(jwtAuthenticationEntryPoint));

        return http.build();
    }
    @Bean
    public CorsConfigurationSource corsConfigurationSource() {
        CorsConfiguration configuration = new CorsConfiguration();
        configuration.addAllowedOrigin("http://localhost:3000"); // ✅ 요청을 허용할 Origin
        configuration.addAllowedMethod("*"); // ✅ 모든 HTTP 메서드 허용 (GET, POST, PUT, DELETE 등)
        configuration.addAllowedHeader("*"); // ✅ 모든 헤더 허용
        configuration.setAllowCredentials(true); // ✅ 쿠키 포함 요청 허용 (credentials: include)

        UrlBasedCorsConfigurationSource source = new UrlBasedCorsConfigurationSource();
        source.registerCorsConfiguration("/**", configuration); // 모든 경로에 대해 적용
        return source;
    }
}