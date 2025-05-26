package com.snapit.backend.snapit_server.config;

import com.snapit.backend.snapit_server.service.GeminiService;
import org.mockito.Mockito;
import org.springframework.boot.test.context.TestConfiguration;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Primary;

import java.util.Arrays;
import java.util.List;

@TestConfiguration
public class TestGeminiConfig {
    
    @Bean
    @Primary
    public GeminiService geminiService() {
        GeminiService mockGeminiService = Mockito.mock(GeminiService.class);
        
        // 기본 목 응답 데이터 설정
        List<String> mockPlaceList = Arrays.asList("카페", "도서관", "강의실", "공원", "영화관", "편의점");
        List<String> mockStuffList = Arrays.asList("책", "펜", "노트", "컴퓨터", "의자", "테이블", "가방", "물병", "우산", "안경");
        
        Mockito.when(mockGeminiService.generatePlaceList()).thenReturn(mockPlaceList);
        Mockito.when(mockGeminiService.generateStuffList(Mockito.anyString())).thenReturn(mockStuffList);
        
        return mockGeminiService;
    }
} 