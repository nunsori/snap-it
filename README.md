# Snap It 📸🎮

> **AR 기반 협동 영어 학습 게임**  
> AR 기술을 활용한 혁신적인 야외 교육 콘텐츠

[![Unity](https://img.shields.io/badge/Unity-6000.0.41f-black?style=flat-square&logo=unity)](https://unity.com/)
[![Spring Boot](https://img.shields.io/badge/Spring%20Boot-3.4.4-brightgreen?style=flat-square&logo=spring)](https://spring.io/projects/spring-boot)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-Latest-blue?style=flat-square&logo=kubernetes)](https://kubernetes.io/)
[![FastAPI](https://img.shields.io/badge/FastAPI-Latest-009688?style=flat-square&logo=fastapi)](https://fastapi.tiangolo.com/)

#### 🔗 관련 리포지토리

- **메인 리포지토리**: [snap-it](https://github.com/ChabinHwang/snap-it)
- **K8s Manifest**: [snap-it-k8s-manifest](https://github.com/ChabinHwang/snap-it-k8s-manifest)
- **Word2Vec Server**: [snap-it-word2vec](https://github.com/ChabinHwang/snap-it-word2vec)
- **Word2Vec Manifest**: [snap-it-word2vec-manifest](https://github.com/ChabinHwang/snap-it-word2vec-manifest)


## 🎯 프로젝트 개요

**Snap It**은 전통적인 실내 게임의 한계를 벗어나 GPS 기반 위치 정보를 활용한 야외 협동 게임입니다. 플레이어들이 다양한 장소에서 실시간으로 협력하고 경쟁할 수 있는 교육용 콘텐츠를 제공합니다.

### 🎮 핵심 게임플레이

- **AI 주제 선정**: AI가 선정한 주제에 따라 각 플레이어에게 개별 사물 할당
- **시간 제한 촬영**: 15초 제한시간 내 할당된 사물 촬영 및 업로드
- **AI 분석**: AI 이미지 분석을 통한 주제 유사도 판단 및 점수 산정
- **영어 학습**: 촬영된 사물의 영어 단어 표시로 자연스러운 영어 학습 효과
- **2라운드 진행**: 총 2라운드 진행 후 최고 점수자 승리

### 🎥 시연 영상, 사진 

- **YouTube**: [https://youtu.be/BrFO9eYzvFE](https://youtu.be/BrFO9eYzvFE)
- **Google Drive**: [시연 동영상](https://drive.google.com/file/d/1dnsZ7-Lw4wIsdbSmGqbYqKNzrhS9QO56/view?usp=share_link)

<img width="300" src="https://github.com/user-attachments/assets/0bf2a511-ba7d-45d4-8374-de5bcf65fdc2" />

<img width="300" alt="Image" src="https://github.com/user-attachments/assets/95a7f373-f827-4431-8dad-d25beb947f2e" />


## 👥 팀원

<table>
  <tr>
    <td align="center">
      <a href="https://github.com/ChabinHwang">
        <img src="https://avatars.githubusercontent.com/ChabinHwang" width="100px;" alt="chabin"/><br />
        <sub><b>황차빈</b></sub>
      </a><br />
      <small>백엔드/인프라</small>
    </td>
    <td align="center">
      <a href="https://github.com/nunsori">
        <img src="https://avatars.githubusercontent.com/nunsori" width="100px;" alt="yejin"/><br />
        <sub><b>이수민</b></sub>
      </a><br />
      <small>클라이언트</small>
    </td>
    <td align="center">
      <a href="https://github.com/nanadayy">
        <img src="https://avatars.githubusercontent.com/nanadayy" width="100px;" alt="yejin"/><br />
        <sub><b>김민지</b></sub>
      </a><br />
      <small>디자인</small>
    </td>
  </tr>
</table>


## 시스템 아키텍처

<img src="https://github.com/user-attachments/assets/e050c875-9343-4c89-8c4c-ecfd7a63c6a7" />

<img width="300" alt="Image" src="https://github.com/user-attachments/assets/53d46832-6a72-40a5-8241-0d4a5b157ec2" />

## 인프라 아키텍쳐

<img src="https://github.com/user-attachments/assets/4613c88c-3933-43ac-91e5-f1e342466d8d">


### 클라이언트 (Unity)
- **Unity**: 6000.0.41f
- **AR Foundation**: AR 환경 구축
- **Native WebSocket**: 실시간 통신
- **Google Cloud Vision API**: 객체 인식
- **STOMP Protocol**: WebSocket 메시징

### 서버 (Spring Boot)
- **Spring Boot**: 3.4.4
- **Java**: 21
- **WebSocket (STOMP)**: 실시간 게임 진행
- **OAuth2 + JWT**: 소셜 로그인 인증
- **ConcurrentHashMap**: 동시성 제어

### AI
- **Google Gemini API**: 주제/물건 목록 생성
- **Google Cloud Vision API**: 이미지 객체 인식
- **Word2Vec (FastAPI)**: 단어 유사도 분석

### 인프라 & DevOps
- **Kubernetes**: 컨테이너 오케스트레이션
- **ArgoCD**: GitOps 기반 CD
- **GitHub Actions**: CI 파이프라인
- **Prometheus + Grafana**: 모니터링
- **Docker**: 컨테이너화

## 🚀 시작하기

### 사전 요구사항

#### Android
- **OS**: Android 9 (Pie, API 28) 이상
- **RAM**: 5GB 이상 권장
- **CPU**: Snapdragon 845 이상

#### iOS
- **OS**: iOS 13.0 이상
- **RAM**: 4GB 이상 권장
- **CPU**: Apple A9 칩 이상

### 설치 및 실행

#### 1. 리포지토리 클론
```bash
git clone https://github.com/ChabinHwang/snap-it.git
cd snap-it
```

#### 2. 서버 실행 (로컬 개발)
```bash
cd snapit-server
./gradlew bootRun
```

#### 3. 클라이언트 빌드
Unity Editor에서 프로젝트를 열고:
1. Android/iOS 플랫폼 설정
2. AR Foundation 및 필요한 패키지 설치
3. 빌드 및 디바이스에 설치

## 🎯 주요 기능

### 🔐 인증 시스템
- **소셜 로그인**: Google, Kakao OAuth2 지원
- **JWT 토큰**: Access Token + Refresh Token 방식
- **WebSocket 인증**: JWT 기반 핸드셰이크

### 🎮 게임 시스템
- **실시간 멀티플레이**: STOMP 프로토콜 기반
- **방 시스템**: 방 생성, 입장, 퇴장 관리
- **게임 모드**: 개인전(PERSONAL) / 협력전(COOPERATE)
- **점수 시스템**: AI 유사도 기반 점수 산정

### 📱 AR 기능
- **객체 인식**: Google Vision API 연동
- **거리 측정**: ARMesh를 통한 공간 인식
- **물체 선택**: AR 환경에서의 직관적 상호작용

### 🤖 AI 기능
- **주제 생성**: Gemini API를 통한 게임 주제 자동 생성
- **유사도 분석**: Word2Vec 모델 기반 단어 유사도 측정
- **이미지 분석**: 촬영된 이미지의 객체 인식 및 분석

## 📊 성능 테스트 결과

### Word2Vec API 성능 테스트
- **테스트 조건**: 100,000 요청, 200 동시 연결
- **단일 Pod**: 평균 응답시간 ~200ms
- **2개 Pod**: 향상된 처리량과 안정성

### Google Vision API 응답시간
- **Object Detection**: 평균 0.2초, 최대 0.3초
- **Gemini API**: 평균 0.7초, 최대 0.9초

## 🐳 Docker & Kubernetes

### Docker 빌드
```bash
# 게임 서버
cd snapit-server
docker build -t snapit-server .

# Word2Vec 서버
cd word2vec-server
docker build -t word2vec-server .
```

### Kubernetes 배포
```bash
# ArgoCD를 통한 자동 배포
kubectl apply -f manifests/
```

## 📈 모니터링

- **Prometheus**: 메트릭 수집
- **Grafana**: 대시보드 시각화
- **실시간 모니터링**: 시스템 상태, 요청 수, 응답 시간

