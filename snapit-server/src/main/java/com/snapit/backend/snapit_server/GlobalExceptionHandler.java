package com.snapit.backend.snapit_server;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;

@RestControllerAdvice
public class GlobalExceptionHandler {

    private final String webhookUrl = "https://discord.com/api/webhooks/1376801420681936896/bhSYNBoR2nKfrDqlFsxhCM_234Ws1tR3zZK0rUcGgGc2bT8hk2MH2CXqo4C3Z-Rexp0a";

    @ExceptionHandler(Exception.class)
    public ResponseEntity<String> handleException(Exception ex) {
        String firstLine = getFirstLineOfException(ex);
        sendToDiscordWebhook(firstLine);
        return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body("Internal Server Error");
    }

    private String getFirstLineOfException(Exception ex) {
        return ex.toString(); // ex.getClass().getName() + ": " + ex.getMessage()
    }

    private void sendToDiscordWebhook(String message) {
        try {
            URL url = new URL(webhookUrl);
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();
            conn.setRequestMethod("POST");
            conn.setDoOutput(true);
            conn.setRequestProperty("Content-Type", "application/json");

            String jsonPayload = "{\"content\": \"" + escapeJson(message) + "\"}";

            try (OutputStream os = conn.getOutputStream()) {
                os.write(jsonPayload.getBytes(StandardCharsets.UTF_8));
            }

            conn.getResponseCode(); // 204 expected
        } catch (Exception e) {
            System.err.println("Webhook 전송 실패: " + e.getMessage());
        }
    }

    private String escapeJson(String str) {
        return str
                .replace("\\", "\\\\")
                .replace("\"", "\\\"")
                .replace("\n", "\\n")
                .replace("\r", "\\r");
    }
}