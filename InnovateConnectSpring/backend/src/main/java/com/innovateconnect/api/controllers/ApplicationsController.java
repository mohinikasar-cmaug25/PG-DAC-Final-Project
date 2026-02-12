package com.innovateconnect.api.controllers;

import com.innovateconnect.api.models.*;
import com.innovateconnect.api.repositories.*;
import com.innovateconnect.api.services.EmailService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Optional;

@RestController
@RequestMapping("/api/applications")
@lombok.extern.slf4j.Slf4j
public class ApplicationsController {

    @Autowired
    private ApplicationRepository applicationRepository;

    @Autowired
    private InternshipRepository internshipRepository;

    @Autowired
    private StudentRepository studentRepository;

    @Autowired
    private UserRepository userRepository;

    @Autowired
    private CompanyRepository companyRepository;

    @Autowired
    private EmailService emailService;

    @PostMapping("/{internshipId}")
    public ResponseEntity<?> apply(Authentication authentication, @PathVariable Integer internshipId) {
        String email = authentication.getName();
        log.info("Application attempt by user: {} for internshipId: {}", email, internshipId);

        Optional<User> userOpt = userRepository.findByEmail(email);
        if (userOpt.isEmpty()) {
            log.error("User not found for email: {}. This might be an orphaned user record.", email);
            return ResponseEntity.status(403).body("User profile not fully initialized. Please contact support or try re-registering.");
        }
        User user = userOpt.get();

        Optional<Student> studentOpt = studentRepository.findByUser(user);
        if (studentOpt.isEmpty()) {
            log.warn("Non-student user {} (Role: {}) tried to apply for internship. This could be a data inconsistency.", email, user.getRole());
            return ResponseEntity.status(403).body("Application restricted to student accounts only.");
        }
        Student student = studentOpt.get();

        Optional<Internship> internshipOpt = internshipRepository.findById(internshipId);
        if (internshipOpt.isEmpty()) {
            log.warn("Internship {} not found for application attempt by {}", internshipId, email);
            return ResponseEntity.status(404).body("Internship not found");
        }
        Internship internship = internshipOpt.get();

        if (applicationRepository.findByInternshipAndStudent(internship, student).isPresent()) {
            log.info("User {} already applied for internship {}", email, internshipId);
            return ResponseEntity.badRequest().body("Already applied");
        }

        Application application = Application.builder()
                .internship(internship)
                .student(student)
                .status("Applied")
                .build();

        applicationRepository.save(application);

        // Send Email to Company
        if (internship.getCompany() != null && internship.getCompany().getUser() != null) {
            emailService.sendEmail(
                internship.getCompany().getUser().getEmail(),
                "New Application Received",
                "Student " + student.getFullName() + " has applied for " + internship.getTitle()
            );
        }

        return ResponseEntity.ok("Application submitted successfully");
    }

    @GetMapping("/my")
    public ResponseEntity<?> getMyApplications(Authentication authentication) {
        String email = authentication.getName();
        User user = userRepository.findByEmail(email)
                .orElseThrow(() -> new RuntimeException("User not found"));
        Student student = studentRepository.findByUser(user)
                .orElseThrow(() -> new RuntimeException("Student not found"));
        return ResponseEntity.ok(applicationRepository.findByStudentId(student.getId()));
    }

    @GetMapping("/company")
    public ResponseEntity<?> getCompanyApplications(Authentication authentication) {
        String email = authentication.getName();
        User user = userRepository.findByEmail(email)
                .orElseThrow(() -> new RuntimeException("User not found"));
        Company company = companyRepository.findByUser(user)
                .orElseThrow(() -> new RuntimeException("Company not found"));
        return ResponseEntity.ok(applicationRepository.findByInternship_CompanyId(company.getId()));
    }

    @PutMapping("/{id}/status")
    public ResponseEntity<?> updateStatus(@PathVariable Integer id, @RequestBody String status) {
        // Clean up status string if it comes with quotes
        status = status.replace("\"", "");
        Application application = applicationRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Application not found"));
        application.setStatus(status);
        applicationRepository.save(application);

        // Notify student (Email errors are now handled inside sendEmail)
        emailService.sendEmail(
                application.getStudent().getUser().getEmail(),
                "Internship Application Status Update",
                "Hi " + application.getStudent().getFullName() + ",\n\n" +
                        "Your application for " + application.getInternship().getTitle() +
                        " has been updated to: " + status + ".\n\n" +
                        "Best regards,\nInnovateConnect Team"
        );

        return ResponseEntity.ok("Application status updated to " + status);
    }
}
