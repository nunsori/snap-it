package com.snapit.backend.snapit_server.config;

import com.snapit.backend.snapit_server.security.jwt.JwtHandshakeInterceptor;
import org.springframework.context.annotation.Configuration;
import org.springframework.messaging.simp.config.ChannelRegistration;
import org.springframework.messaging.simp.config.MessageBrokerRegistry;
import org.springframework.messaging.support.ChannelInterceptor;
import org.springframework.web.socket.config.annotation.EnableWebSocketMessageBroker;
import org.springframework.web.socket.config.annotation.StompEndpointRegistry;
import org.springframework.web.socket.config.annotation.WebSocketMessageBrokerConfigurer;
import org.springframework.messaging.Message;
import org.springframework.messaging.MessageChannel;
import org.springframework.messaging.simp.stomp.StompHeaderAccessor;
import org.springframework.messaging.simp.stomp.StompCommand;
import org.springframework.messaging.support.MessageHeaderAccessor;
import org.springframework.security.core.Authentication;

/**
 * <h5>클라이언트가 구독할 접두사를 설정하는 WebSocketConfig 클래스</h5>
 */
@Configuration
@EnableWebSocketMessageBroker
public class WebSocketConfig implements WebSocketMessageBrokerConfigurer {

    private final JwtHandshakeInterceptor jwtHandshakeInterceptor;

    public WebSocketConfig(JwtHandshakeInterceptor jwtHandshakeInterceptor) {
        this.jwtHandshakeInterceptor = jwtHandshakeInterceptor;
    }

    @Override
    public void configureMessageBroker(MessageBrokerRegistry config) {
        // 클라이언트가 구독할 수 있는 주제 접두사 설정
        config.enableSimpleBroker("/topic");

        // 클라이언트가 메시지를 보낼 목적지 접두사 설정
        config.setApplicationDestinationPrefixes("/app");

        // 특정 사용자에게 메시지를 보내기 위한 접두사 설정
        config.setUserDestinationPrefix("/user");
    }

    @Override
    public void registerStompEndpoints(StompEndpointRegistry registry) {
        // WebSocket 연결 엔드포인트 등록
        registry.addEndpoint("/ws")
                .addInterceptors(jwtHandshakeInterceptor) // 여기 추가
                .setAllowedOrigins("*"); // SockJS 지원 추가
    }

    @Override
    public void configureClientInboundChannel(ChannelRegistration registration) {
        registration.interceptors(new ChannelInterceptor() {
            @Override
            public Message<?> preSend(Message<?> message, MessageChannel channel) {
                StompHeaderAccessor accessor = MessageHeaderAccessor.getAccessor(message, StompHeaderAccessor.class);
                if (StompCommand.CONNECT.equals(accessor.getCommand())) {
                    // HandshakeInterceptor 에서 저장한 Authentication 객체 꺼내서
                    Authentication user = (Authentication) accessor.getSessionAttributes().get("SPRING.AUTH");
                    // STOMP 메시지의 Principal 로 설정
                    accessor.setUser(user);
                }
                return message;
            }
        });
    }

}
