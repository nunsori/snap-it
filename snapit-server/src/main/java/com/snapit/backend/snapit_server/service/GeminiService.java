package com.snapit.backend.snapit_server.service;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.http.*;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;


import java.util.List;
import java.util.Map;
@Service
public class GeminiService {
    @Value("${gemini.api.key}")
    private String apiKey;

    private final RestTemplate restTemplate = new RestTemplate();

    public List<String> generatePlaceList(){

        String placeListPrompt = "10~20대가 접근하기 쉬운 공간(현실에 존재하는 공간)을 6개 랜덤하게 선택해서, 아래 예시와 같은 json형식으로 반환해줘." +
                " 이때, 공간은 단순히 공간명을 의미해. 공간은 특정성(스타벅스, 맥도날드)를 띄지 말고, 추상적으로 표현(카페, 음식점) 해줘. " +
                "학교, 대학교 안에 존재하는 공간을 2개 이상 포함해줘.\n" +
                "예시:\n" +
                "{\n" +
                "  \"placeList\": [\"카페\",\"마트\",\"영화관\",\"강의실\",\"거리\",\"도서관\"]\n" +
                "}";

        String url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent?key=" + apiKey;
        // 1) 스키마 정의
        Map<String, Object> props = Map.of(
                "placeList", Map.of(
                        "type", "ARRAY",
                        "items", Map.of("type", "STRING")
                )
        );
        Map<String, Object> responseSchema = Map.of(
                "type", "OBJECT",
                "properties", props
        );

        // 2) generation_config (snake_case)
        Map<String, Object> generationConfig = Map.of(
                "response_mime_type", "application/json",
                "response_schema", responseSchema
        );

        // 3) requestBody에 role 추가, generation_config 키 이름 수정
        Map<String, Object> requestBody = Map.of(
                "contents", List.of(
                        Map.of(
                                "role", "user",
                                "parts", List.of(
                                        Map.of("text", placeListPrompt)
                                )
                        )
                ),
                "generation_config", generationConfig
        );

        // 5. 헤더 설정
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        // 6. HttpEntity 생성
        HttpEntity<Map<String, Object>> request = new HttpEntity<>(requestBody, headers);

        // 7. 호출
        ResponseEntity<Map> response = restTemplate.exchange(
                url,
                HttpMethod.POST,
                request,
                Map.class
        );

        // 8. 결과 확인
        Map<?, ?> body = response.getBody();
        System.out.println("처음 결과"+body);


        Map<?,?> root = response.getBody();

        // 2) candidates → content → parts → text 추출
        @SuppressWarnings("unchecked")
        List<Map<String,Object>> candidates =
                (List<Map<String,Object>>) root.get("candidates");
        Map<String,Object> content = (Map<String,Object>) candidates.get(0).get("content");

        @SuppressWarnings("unchecked")
        List<Map<String,String>> parts =
                (List<Map<String,String>>) content.get("parts");
        String json = parts.get(0).get("text");

        // 3) JSON 문자열을 실제 Map<String,List<String>> 으로 파싱
        ObjectMapper mapper = new ObjectMapper();
        Map<String, List<String>> placeListMap = null;
        try {
            placeListMap = mapper.readValue(
                    json,
                    new TypeReference<Map<String, List<String>>>() {}
            );
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
        return placeListMap.get("placeList");
    }

    public List<String> generateStuffList(String place){

        String stuffListPrompt = place +"에 있을법한 물건 10개를 랜덤하게 골라서\n" +
                "key는 stuffList, value는 물건 이름  String 배열로 JSON형식으로 답해줘";

        String url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent?key=" + apiKey;
        // 1) 스키마 정의 (stuffList)
        Map<String, Object> stuffListProps = Map.of(
                "stuffList", Map.of(
                        "type", "ARRAY",
                        "items", Map.of("type", "STRING")
                )
        );
        Map<String, Object> responseSchema = Map.of(
                "type", "OBJECT",
                "properties", stuffListProps
        );

        // 2) generation_config (snake_case)
        Map<String, Object> generationConfig = Map.of(
                "response_mime_type", "application/json",
                "response_schema", responseSchema
        );

        // 3) requestBody: role 추가, prompt 변경
        Map<String, Object> requestBody = Map.of(
                "contents", List.of(
                        Map.of(
                                "role", "user",
                                "parts", List.of(
                                        Map.of("text", stuffListPrompt)
                                )
                        )
                ),
                "generation_config", generationConfig
        );

        // 5. 헤더 설정
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        // 6. HttpEntity 생성
        HttpEntity<Map<String, Object>> request = new HttpEntity<>(requestBody, headers);

        // 7. 호출
        ResponseEntity<Map> response = restTemplate.exchange(
                url,
                HttpMethod.POST,
                request,
                Map.class
        );

        Map<?,?> root = response.getBody();

        // 2) candidates → content → parts → text 추출
        @SuppressWarnings("unchecked")
        List<Map<String,Object>> candidates =
                (List<Map<String,Object>>) root.get("candidates");
        Map<String,Object> content = (Map<String,Object>) candidates.get(0).get("content");

        @SuppressWarnings("unchecked")
        List<Map<String,String>> parts =
                (List<Map<String,String>>) content.get("parts");
        String json = parts.get(0).get("text");

        // 4) JSON 파싱 및 검증
        ObjectMapper mapper = new ObjectMapper();
        Map<String, List<String>> stuffListMap = null;
        try {
            stuffListMap = mapper.readValue(
                    json,
                    new TypeReference<Map<String, List<String>>>() {}
            );
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
        return stuffListMap.get("stuffList");
    }
}
