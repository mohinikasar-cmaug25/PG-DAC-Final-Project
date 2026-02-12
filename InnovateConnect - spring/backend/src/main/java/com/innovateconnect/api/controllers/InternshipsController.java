package com.innovateconnect.api.controllers;

import com.innovateconnect.api.models.Internship;
import com.innovateconnect.api.models.Company;
import com.innovateconnect.api.models.User;
import com.innovateconnect.api.repositories.InternshipRepository;
import com.innovateconnect.api.repositories.CompanyRepository;
import com.innovateconnect.api.repositories.UserRepository;
import com.innovateconnect.api.repositories.ApplicationRepository;
import jakarta.validation.Valid;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/internships")
public class InternshipsController {

    @Autowired
    private InternshipRepository internshipRepository;

    @Autowired
    private CompanyRepository companyRepository;

    @Autowired
    private UserRepository userRepository;

    @GetMapping
    public ResponseEntity<List<Internship>> getAllInternships() {
        return ResponseEntity.ok(internshipRepository.findAll());
    }

    @GetMapping("/my-internships")
    public ResponseEntity<?> getMyInternships(Authentication authentication) {
        String email = authentication.getName();
        User user = userRepository.findByEmail(email)
                .orElseThrow(() -> new RuntimeException("User not found"));
        Company company = companyRepository.findByUser(user)
                .orElseThrow(() -> new RuntimeException("Company not found"));
        return ResponseEntity.ok(internshipRepository.findByCompany(company));
    }

    @PostMapping
    public ResponseEntity<?> postInternship(Authentication authentication, @Valid @RequestBody Internship internship) {
        String email = authentication.getName();
        User user = userRepository.findByEmail(email)
                .orElseThrow(() -> new RuntimeException("User not found"));
        Company company = companyRepository.findByUser(user)
                .orElseThrow(() -> new RuntimeException("Company not found"));
        
        internship.setCompany(company);
        return ResponseEntity.ok(internshipRepository.save(internship));
    }

    @Autowired
    private ApplicationRepository applicationRepository;

    @GetMapping("/{id}/applicants")
    public ResponseEntity<?> getApplicants(@PathVariable Integer id) {
        return ResponseEntity.ok(applicationRepository.findByInternshipId(id));
    }
}
