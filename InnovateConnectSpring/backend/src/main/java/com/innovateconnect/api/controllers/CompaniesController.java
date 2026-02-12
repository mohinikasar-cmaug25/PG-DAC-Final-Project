package com.innovateconnect.api.controllers;

import com.innovateconnect.api.models.*;
import com.innovateconnect.api.repositories.*;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/companies")
public class CompaniesController {

    @Autowired
    private CompanyRepository companyRepository;

    @Autowired
    private UserRepository userRepository;

    @GetMapping("/profile")
    public ResponseEntity<?> getProfile(Authentication authentication) {
        String email = authentication.getName();
        User user = userRepository.findByEmail(email).get();
        Company company = companyRepository.findByUser(user)
                .orElseThrow(() -> new RuntimeException("Company not found"));
        return ResponseEntity.ok(company);
    }

    @PutMapping("/profile")
    public ResponseEntity<?> updateProfile(Authentication authentication, @RequestBody Company companyUpdate) {
        String email = authentication.getName();
        User user = userRepository.findByEmail(email)
                .orElseThrow(() -> new RuntimeException("User not found"));
        Company company = companyRepository.findByUser(user)
                .orElseThrow(() -> new RuntimeException("Company not found"));

        company.setCompanyName(companyUpdate.getCompanyName());
        company.setLocation(companyUpdate.getLocation());
        company.setWebsite(companyUpdate.getWebsite());

        return ResponseEntity.ok(companyRepository.save(company));
    }

    @PostMapping("/upload-cover")
    public ResponseEntity<?> uploadCoverImage(Authentication authentication, @RequestParam("file") org.springframework.web.multipart.MultipartFile file) {
        try {
            String email = authentication.getName();
            User user = userRepository.findByEmail(email).get();
            Company company = companyRepository.findByUser(user).get();

            company.setCoverImageData(file.getBytes());
            company.setCoverImageType(file.getContentType());
            company.setHasCoverImage(true);
            companyRepository.save(company);

            return ResponseEntity.ok("Cover image uploaded successfully");
        } catch (Exception e) {
            return ResponseEntity.status(500).body("Error uploading cover image");
        }
    }

    @GetMapping("/{id}/cover")
    public ResponseEntity<?> getCoverImage(@PathVariable Integer id) {
        Company company = companyRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Company not found"));

        if (company.getHasCoverImage() == null || !company.getHasCoverImage() || company.getCoverImageData() == null) {
             return ResponseEntity.notFound().build();
        }

        return ResponseEntity.ok()
                .contentType(org.springframework.http.MediaType.parseMediaType(company.getCoverImageType()))
                .body(company.getCoverImageData());
    }
}
