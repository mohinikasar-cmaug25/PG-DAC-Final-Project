package com.innovateconnect.api.controllers;

import com.innovateconnect.api.models.ContactMessage;
import com.innovateconnect.api.repositories.ContactMessageRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import lombok.extern.slf4j.Slf4j;

import java.util.Map;

@RestController
@RequestMapping("/api/contact")
@Slf4j
public class ContactController {
    
    @Autowired
    private ContactMessageRepository contactMessageRepository;
    
    @PostMapping("/submit")
    public ResponseEntity<?> submitContactMessage(@RequestBody Map<String, String> payload) {
        log.info("Received contact message submission");
        
        try {
            // Validate input
            String name = payload.get("name");
            String email = payload.get("email");
            String message = payload.get("message");
            
            if (name == null || name.trim().isEmpty()) {
                return ResponseEntity.badRequest().body("Name is required");
            }
            
            if (email == null || email.trim().isEmpty()) {
                return ResponseEntity.badRequest().body("Email is required");
            }
            
            if (message == null || message.trim().isEmpty()) {
                return ResponseEntity.badRequest().body("Message is required");
            }
            
            // Basic email validation
            if (!email.matches("^[A-Za-z0-9+_.-]+@(.+)$")) {
                return ResponseEntity.badRequest().body("Invalid email format");
            }
            
            // Create and save contact message
            ContactMessage contactMessage = new ContactMessage(
                name.trim(),
                email.trim(),
                message.trim()
            );
            
            contactMessageRepository.save(contactMessage);
            
            log.info("Contact message saved successfully from: {}", email);
            return ResponseEntity.ok(Map.of(
                "success", true,
                "message", "Your message has been sent successfully. We'll get back to you soon!"
            ));
            
        } catch (Exception e) {
            log.error("Error saving contact message: {}", e.getMessage());
            return ResponseEntity.status(500).body("Failed to send message. Please try again later.");
        }
    }
}
