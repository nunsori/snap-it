package com.snapit.backend.snapit_server.security.oauth2;

import com.snapit.backend.snapit_server.security.oauth2.userinfo.OAuth2UserInfo;
import com.snapit.backend.snapit_server.security.oauth2.userinfo.factory.OAuth2UserInfoFactory;
import lombok.RequiredArgsConstructor;
import org.springframework.security.core.GrantedAuthority;
import org.springframework.security.core.authority.AuthorityUtils;
import org.springframework.security.oauth2.client.userinfo.DefaultOAuth2UserService;
import org.springframework.security.oauth2.client.userinfo.OAuth2UserRequest;
import org.springframework.security.oauth2.core.OAuth2AuthenticationException;
import org.springframework.security.oauth2.core.user.DefaultOAuth2User;
import org.springframework.security.oauth2.core.user.OAuth2User;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;

@Service
@RequiredArgsConstructor
public class OAuth2UserService extends DefaultOAuth2UserService {
    // 유저 정보를 inmemory상태에 저장하기 위한 임시 필드
    private final Map<String, String> inMemoryUserDb = new ConcurrentHashMap<>();

    @Override
    public OAuth2User loadUser(OAuth2UserRequest userRequest) throws OAuth2AuthenticationException {
        OAuth2User oAuth2User = super.loadUser(userRequest);

        // Role generate
        List<GrantedAuthority> authorities = AuthorityUtils.createAuthorityList("ROLE_ADMIN");

        // nameAttributeKey
        String userNameAttributeName = userRequest.getClientRegistration()
                .getProviderDetails()
                .getUserInfoEndpoint()
                .getUserNameAttributeName();

        // ㅁ1) 공급자 구분
        String registrationId = userRequest.getClientRegistration().getRegistrationId();
        // ㅁ2) 사용자 정보 인터페이스 팩토리로 분기
        OAuth2UserInfo userInfo = OAuth2UserInfoFactory.getOAuth2UserInfo(
                registrationId,
                oAuth2User.getAttributes()
        );

        // ㅁ3) 고유 식별자 및 이메일 추출
        String provider = registrationId;
        String providerId = userInfo.getId();
        String email = userInfo.getEmail();

        // ㅁ4) 회원 가입/로그인 처리
        signUpOrLogin(provider, providerId, email);

        DefaultOAuth2User defaultOAuth2User = new DefaultOAuth2User(authorities,
                oAuth2User.getAttributes(), userNameAttributeName);
        // database에 넣는 부분
        return defaultOAuth2User;
    }
    @Transactional
    public void signUpOrLogin(String provider, String providerId, String email) {
        String principalKey = provider + "_" + providerId;
        // 예시 리포지토리 호출: 조회 후 없으면 생성
        // UserEntity user = userRepository.findByPrincipalKey(principalKey)
        //     .orElseGet(() -> userRepository.save(new UserEntity(principalKey, email)));
        // 필요 시 이메일 업데이트 로직 추가
        // in-memory signup/login
        inMemoryUserDb.computeIfAbsent(principalKey, key -> email);
    }

    public Optional<String> findEmailByPrincipalKey(String provider, String providerId) {
        String key = provider + "_" + providerId;
        return Optional.ofNullable(inMemoryUserDb.get(key));
    }
}