# 빌드 단계
FROM eclipse-temurin:21-jdk-alpine AS build-stage
WORKDIR /app

# Gradle 래퍼 복사
COPY gradlew .
COPY gradle gradle
RUN chmod +x ./gradlew

# 빌드 캐싱을 위한 의존성 파일 복사
COPY build.gradle .
COPY settings.gradle .
RUN ./gradlew dependencies

# 소스 코드 복사
COPY src src

# 애플리케이션 빌드 (테스트 제외)
RUN ./gradlew build -x test

# 실행 단계
FROM eclipse-temurin:21-jre-alpine AS runtime
WORKDIR /app

# 환경 변수 설정
ENV SPRING_APPLICATION_NAME=snapit-server

# 빌더에서 생성된 JAR 파일 복사 (빌더 이름 명시)
COPY --from=build-stage /app/build/libs/*.jar app.jar

# 포트 노출
EXPOSE 8080

# 애플리케이션 실행
ENTRYPOINT ["java", "-jar", "/app/app.jar"]