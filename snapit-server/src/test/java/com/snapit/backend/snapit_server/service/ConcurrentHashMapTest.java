package com.snapit.backend.snapit_server.service;

import org.junit.jupiter.api.Test;

import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.atomic.AtomicInteger;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class ConcurrentHashMapTest {

    @Test
    public void testAtomicOperations() throws InterruptedException {
        // 테스트할 ConcurrentHashMap 생성
        Map<UUID, AtomicInteger> countsMap = new ConcurrentHashMap<>();
        UUID testUUID = UUID.randomUUID();
        countsMap.put(testUUID, new AtomicInteger(0));
        
        // 스레드 수 정의
        int threadCount = 100;
        CountDownLatch latch = new CountDownLatch(threadCount);
        ExecutorService executorService = Executors.newFixedThreadPool(threadCount);
        
        // 각 스레드에서 동일한 키에 대해 값을 증가시키는 작업 수행
        for (int i = 0; i < threadCount; i++) {
            executorService.submit(() -> {
                try {
                    // compute 메서드를 사용한 원자적 업데이트
                    countsMap.compute(testUUID, (key, value) -> {
                        value.incrementAndGet();
                        return value;
                    });
                } finally {
                    latch.countDown();
                }
            });
        }
        
        // 모든 스레드가 작업을 마칠 때까지 대기
        latch.await();
        executorService.shutdown();
        
        // 결과 확인: 100개 스레드가 각각 1씩 증가시켰으므로 결과는 100이어야 함
        assertEquals(threadCount, countsMap.get(testUUID).get());
    }
    
    @Test
    public void testComputeIfAbsent() throws InterruptedException {
        Map<UUID, AtomicInteger> countsMap = new ConcurrentHashMap<>();
        UUID testUUID = UUID.randomUUID();
        
        int threadCount = 100;
        CountDownLatch latch = new CountDownLatch(threadCount);
        ExecutorService executorService = Executors.newFixedThreadPool(threadCount);
        
        // 여러 스레드에서 computeIfAbsent 메서드 호출
        for (int i = 0; i < threadCount; i++) {
            executorService.submit(() -> {
                try {
                    // 키가 없을 때만 값을 생성
                    countsMap.computeIfAbsent(testUUID, key -> new AtomicInteger(0));
                    // 생성된 값 증가
                    countsMap.get(testUUID).incrementAndGet();
                } finally {
                    latch.countDown();
                }
            });
        }
        
        latch.await();
        executorService.shutdown();
        
        // computeIfAbsent가 한 번만 실행되었는지 확인하고
        // 모든 스레드에서의 증가 작업이 반영되었는지 확인
        assertEquals(threadCount, countsMap.get(testUUID).get());
    }
    
    @Test
    public void testGamePlayServiceConcurrency() throws InterruptedException {
        Map<UUID, Integer> counts = new ConcurrentHashMap<>();
        UUID roomUUID = UUID.randomUUID();
        counts.put(roomUUID, 0);
        
        int threadCount = 50;
        CountDownLatch latch = new CountDownLatch(threadCount);
        ExecutorService executorService = Executors.newFixedThreadPool(threadCount);
        
        // GamePlayService의 addCount 메서드처럼 compute 메서드 사용
        for (int i = 0; i < threadCount; i++) {
            executorService.submit(() -> {
                try {
                    counts.compute(roomUUID, (key, val) -> val == null ? 1 : val + 1);
                } finally {
                    latch.countDown();
                }
            });
        }
        
        latch.await();
        executorService.shutdown();
        
        // 모든 증가 작업이 원자적으로 수행되었는지 확인
        assertEquals(threadCount, counts.get(roomUUID));
    }
} 