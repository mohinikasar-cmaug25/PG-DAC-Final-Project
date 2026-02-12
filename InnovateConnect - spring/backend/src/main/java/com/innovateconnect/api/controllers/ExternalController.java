package com.innovateconnect.api.controllers;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.client.RestTemplate;

import java.util.Map;

@RestController
@RequestMapping("/api/external")
public class ExternalController {

    private final RestTemplate restTemplate = new RestTemplate();

    @GetMapping("/leetcode/{username}")
    public ResponseEntity<?> getLeetCodeStats(@PathVariable String username) {
        String url = "https://leetcode-stats-api.herokuapp.com/" + username;
        try {
            Map<?, ?> stats = restTemplate.getForObject(url, Map.class);
            return ResponseEntity.ok(stats);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("Error fetching LeetCode stats");
        }
    }

    @GetMapping("/github/{username}")
    public ResponseEntity<?> getGitHubLanguages(@PathVariable String username) {
        // Simplified for MVP, usually requires multiple calls or a specific tool
        String url = "https://api.github.com/users/" + username + "/repos";
        try {
            Object repos = restTemplate.getForObject(url, Object.class);
            return ResponseEntity.ok(repos);
        } catch (Exception e) {
            return ResponseEntity.status(500).body("Error fetching GitHub repos");
        }
    }
}
