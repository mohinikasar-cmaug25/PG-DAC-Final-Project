package com.innovateconnect.api.controllers;

import com.innovateconnect.api.models.Idea;
import com.innovateconnect.api.models.Student;
import com.innovateconnect.api.models.User;
import com.innovateconnect.api.repositories.IdeaRepository;
import com.innovateconnect.api.repositories.StudentRepository;
import com.innovateconnect.api.repositories.UserRepository;
import jakarta.validation.Valid;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/ideas")
public class IdeasController {

    @Autowired
    private IdeaRepository ideaRepository;

    @Autowired
    private StudentRepository studentRepository;

    @Autowired
    private UserRepository userRepository;

    @GetMapping
    public ResponseEntity<List<Idea>> getAllIdeas() {
        return ResponseEntity.ok(ideaRepository.findAll());
    }

    @GetMapping("/my")
    public ResponseEntity<?> getMyIdeas(Authentication authentication) {
        String email = authentication.getName();
        User user = userRepository.findByEmail(email).get();
        Student student = studentRepository.findByUser(user).get();
        return ResponseEntity.ok(ideaRepository.findByStudent(student));
    }

    @PostMapping
    public ResponseEntity<?> postIdea(Authentication authentication, @Valid @RequestBody Idea idea) {
        String email = authentication.getName();
        User user = userRepository.findByEmail(email).get();
        Student student = studentRepository.findByUser(user).get();
        
        idea.setStudent(student);
        return ResponseEntity.ok(ideaRepository.save(idea));
    }
}
