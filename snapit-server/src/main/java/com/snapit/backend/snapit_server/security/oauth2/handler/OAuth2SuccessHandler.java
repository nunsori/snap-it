package com.snapit.backend.snapit_server.security.oauth2.handler;

import com.snapit.backend.snapit_server.security.jwt.principal.JwtProvider;
import com.snapit.backend.snapit_server.security.jwt.principal.TokenRepository;
import com.snapit.backend.snapit_server.security.oauth2.OAuth2UserService;
import com.snapit.backend.snapit_server.security.oauth2.userinfo.OAuth2UserInfo;
import com.snapit.backend.snapit_server.security.oauth2.userinfo.factory.OAuth2UserInfoFactory;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.Cookie;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import lombok.RequiredArgsConstructor;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseCookie;
import org.springframework.security.core.Authentication;
import org.springframework.security.oauth2.client.authentication.OAuth2AuthenticationToken;
import org.springframework.security.oauth2.core.user.OAuth2User;
import org.springframework.security.web.authentication.AuthenticationSuccessHandler;
import org.springframework.stereotype.Component;

import java.io.IOException;
import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.time.Duration;

@Component
public class OAuth2SuccessHandler implements AuthenticationSuccessHandler {

    private final JwtProvider jwtProvider;
    private final TokenRepository tokenRepository;



    public OAuth2SuccessHandler(JwtProvider jwtProvider,  TokenRepository tokenRepository) {
        this.jwtProvider = jwtProvider;
        this.tokenRepository = tokenRepository;
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

        // Access Token 생성
        String accessToken = jwtProvider.createToken(email);

        // Refresh Token 생성
        String refreshToken = jwtProvider.createRefreshToken(email);

        // Refresh Token 저장
        tokenRepository.saveRefreshToken(email, refreshToken);

        // Access Token은 쿠키로 설정
        ResponseCookie accessTokenCookie = ResponseCookie.from("accessToken", accessToken)
                .httpOnly(false)
                .path("/")
                .maxAge(60 * 60) // 1시간
                .sameSite("Strict")
                .build();

        // Refresh Token도 쿠키로 설정
        ResponseCookie refreshTokenCookie = ResponseCookie.from("refreshToken", refreshToken)
                .httpOnly(false)
                .path("/api/token/refresh") // Refresh 엔드포인트에서만 사용 가능
                .maxAge(60 * 60 * 24 * 7) // 7일
                .sameSite("Strict")
                .build();
        // [쿠키] 이메일도 넣게끔 설정
        ResponseCookie emailCookie = ResponseCookie.from("email", email)
                .httpOnly(false) // JS에서 접근 가능하게 하려면 false
                .path("/")
                .maxAge(Duration.ofDays(14))
                .build();
        // [쿠키] 응답에 담기
        response.addHeader(HttpHeaders.SET_COOKIE, accessTokenCookie.toString());
        response.addHeader(HttpHeaders.SET_COOKIE, refreshTokenCookie.toString());
        response.addHeader(HttpHeaders.SET_COOKIE, emailCookie.toString());

        // 응답 바디 구성
        response.setContentType(MediaType.APPLICATION_JSON_VALUE);
        response.setCharacterEncoding("UTF-8");
        response.setStatus(HttpServletResponse.SC_OK);

        String responseBody = "{\"accessToken\":\"" + accessToken +
                "\",\"refreshToken\":\"" + refreshToken +
                "\",\"userId\":\"" + email + "\"}";
        // GET요청 반환값으로 지정
//        response.getWriter().write(responseBody);
//        response.sendRedirect("http://localhost:5173");


        // ✅ 프론트로 리다이렉트 (쿼리파라미터로 email 추가)
//        response.sendRedirect("http://localhost:5173/auth-success?email=" + email);

        // 리다이렉트 URI 확인 (Unity 클라이언트용)
//        String redirectUri = request.getParameter("redirect_uri");
//
//        if (redirectUri != null && redirectUri.contains("localhost")) {
//            // Unity 클라이언트로 리다이렉트 (쿼리 파라미터로 이메일과 토큰 추가)
//            String encodedEmail = URLEncoder.encode(email, StandardCharsets.UTF_8.toString());
//            String encodedToken = URLEncoder.encode(jwt, StandardCharsets.UTF_8.toString());
//
//            response.sendRedirect(redirectUri + "?email=" + encodedEmail + "&token=" + encodedToken);
//        } else {
//            // 일반 웹 클라이언트 응답
//            response.setContentType(MediaType.APPLICATION_JSON_VALUE);
//            response.setCharacterEncoding("UTF-8");
//            response.setStatus(HttpServletResponse.SC_OK);
//            String responseBody = "{\"accessToken\":\"" + jwt +
//                    "\",\"userId\":\"" + email + "\"}";
//            response.getWriter().write(responseBody);
//        }
    }
}