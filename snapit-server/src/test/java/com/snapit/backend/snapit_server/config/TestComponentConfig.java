package com.snapit.backend.snapit_server.config;

import com.snapit.backend.snapit_server.service.MockRoomService;
import org.springframework.boot.test.context.TestConfiguration;
import org.springframework.context.annotation.ComponentScan;
import org.springframework.context.annotation.FilterType;

@TestConfiguration
@ComponentScan(
    basePackages = "com.snapit.backend.snapit_server",
    includeFilters = @ComponentScan.Filter(
        type = FilterType.ASSIGNABLE_TYPE,
        classes = MockRoomService.class
    )
)
public class TestComponentConfig {
} 