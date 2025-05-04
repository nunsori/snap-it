package com.snapit.backend.snapit_server.security.oauth2.handler;

import com.snapit.backend.snapit_server.security.jwt.principal.JwtProvider;
import com.snapit.backend.snapit_server.security.oauth2.OAuth2UserService;
import com.snapit.backend.snapit_server.security.oauth2.userinfo.OAuth2UserInfo;
import com.snapit.backend.snapit_server.security.oauth2.userinfo.factory.OAuth2UserInfoFactory;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.Cookie;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseCookie;
import org.springframework.security.core.Authentication;
import org.springframework.security.oauth2.client.authentication.OAuth2AuthenticationToken;
import org.springframework.security.oauth2.core.user.OAuth2User;
import org.springframework.security.web.authentication.AuthenticationSuccessHandler;
import org.springframework.stereotype.Component;

import java.io.IOException;

@Component
public class OAuth2SuccessHandler implements AuthenticationSuccessHandler {

    private final JwtProvider jwtProvider;
    private final OAuth2UserService oauth2UserService;


    //    public OAuth2SuccessHandler(JwtProvider jwtProvider, UserRepository userRepository) {
    //        this.jwtProvider = jwtProvider;
    //        this.userRepository = userRepository;
    //    }
    public OAuth2SuccessHandler(JwtProvider jwtProvider, OAuth2UserService oauth2UserService) {
        this.jwtProvider = jwtProvider;
        this.oauth2UserService = oauth2UserService;
    }


    @Override
    public void onAuthenticationSuccess(HttpServletRequest request, HttpServletResponse response,
                                        Authentication authentication) throws IOException {

        OAuth2AuthenticationToken authToken = (OAuth2AuthenticationToken) authentication;
        String registrationId = authToken.getAuthorizedClientRegistrationId();  // "kakao", "google" 등

        OAuth2User oAuth2User = (OAuth2User) authentication.getPrincipal();
        // 1) OAuth2UserInfo 재생성
        OAuth2UserInfo userInfo = OAuth2UserInfoFactory.getOAuth2UserInfo(
                registrationId,
                oAuth2User.getAttributes()
        );
        // 2) 이메일 꺼내기
        String email = userInfo.getEmail();

        String jwt = jwtProvider.createToken(email);

        // ✅ JWT를 쿠키로 설정
        ResponseCookie cookie = ResponseCookie.from("accessToken", jwt)
                .httpOnly(true) // HTTPS 환경에서만 전송
                .path("/")
                .maxAge(60 * 60) // 1시간
                .sameSite("Strict")
                .build();

        response.setHeader(HttpHeaders.SET_COOKIE, cookie.toString());

        // ✅ 프론트로 리다이렉트 (쿼리파라미터로 토큰 추가)
        response.sendRedirect("http://localhost:5173/auth-success?accessToken=" + jwt);

//        response.setContentType(MediaType.APPLICATION_JSON_VALUE);
//        response.setCharacterEncoding("UTF-8");
//        response.setStatus(HttpServletResponse.SC_OK);
//        String responseBody = "{\"accessToken\":\"" + jwt +
//                "\",\"userId\":\"" + email + "\"}";
//        response.getWriter().write(responseBody);
    }
}