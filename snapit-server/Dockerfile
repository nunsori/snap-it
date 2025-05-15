FROM eclipse-temurin:21-jdk-alpine AS builder
WORKDIR /app

# Gradle 래퍼 복사
COPY gradlew .
COPY gradle gradle
# 실행 권한 부여
RUN chmod +x ./gradlew

# 빌드 캐싱을 위한 의존성 파일 복사
COPY build.gradle .
COPY settings.gradle .
# 의존성 다운로드
RUN ./gradlew dependencies

# 소스 코드 복사
COPY src src

# 애플리케이션 빌드
RUN ./gradlew build -x test

# 실행 단계
FROM --platform=linux/amd64 eclipse-temurin:21-jre-alpine
WORKDIR /app

# 환경 변수 설정
ENV SPRING_APPLICATION_NAME=snapit-server

# 빌더 단계에서 생성된 JAR 파일 복사
COPY --from=builder /app/build/libs/*.jar app.jar

# 포트 노출
EXPOSE 8080

# 애플리케이션 실행
ENTRYPOINT ["java", "-jar", "/app/app.jar"] 