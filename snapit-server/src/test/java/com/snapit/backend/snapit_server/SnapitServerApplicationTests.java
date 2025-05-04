package com.snapit.backend.snapit_server;

import org.junit.jupiter.api.Test;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.context.annotation.Import;
import com.snapit.backend.snapit_server.config.TestSecurityConfig;

@SpringBootTest
@ActiveProfiles("test")
@Import(TestSecurityConfig.class)
class SnapitServerApplicationTests {

	@Test
	void contextLoads() {
	}

}
