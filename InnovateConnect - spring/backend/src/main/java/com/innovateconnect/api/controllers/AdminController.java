package com.innovateconnect.api.controllers;

import com.innovateconnect.api.models.User;
import com.innovateconnect.api.models.ContactMessage;
import com.innovateconnect.api.repositories.*;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;
import lombok.extern.slf4j.Slf4j;

import org.springframework.transaction.annotation.Transactional;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import java.util.HashMap;
import java.util.Map;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/admin")
@Slf4j
public class AdminController {

    @jakarta.annotation.PostConstruct
    public void init() {
        log.info("===============================================");
        log.info("ADMIN CONTROLLER LOADED - VERSION: DEEP_DELETE_FIX_V2");
        log.info("===============================================");
    }

    @GetMapping("/version")
    public ResponseEntity<String> getVersion() {
        return ResponseEntity.ok("DEEP_DELETE_FIX_V2");
    }

    @Autowired
    private UserRepository userRepository;
    @Autowired
    private StudentRepository studentRepository;
    @Autowired
    private CompanyRepository companyRepository;
    @Autowired
    private IdeaRepository ideaRepository;
    @Autowired
    private InternshipRepository internshipRepository;
    @Autowired
    private ApplicationRepository applicationRepository;
    @Autowired
    private ContactMessageRepository contactMessageRepository;

    @GetMapping("/stats")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<?> getStats() {
        log.info("Admin fetching statistics...");
        Map<String, Long> stats = new HashMap<>();
        stats.put("totalStudents", studentRepository.count());
        stats.put("totalCompanies", companyRepository.count());
        stats.put("totalIdeas", ideaRepository.count());
        stats.put("activeInternships", internshipRepository.count());
        stats.put("pendingMessages", contactMessageRepository.countByStatus("PENDING"));
        log.info("Stats fetched: {}", stats);
        return ResponseEntity.ok(stats);
    }

    @GetMapping("/users")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<?> getAllUsers() {
        log.info("Admin fetching all users list...");
        return ResponseEntity.ok(userRepository.findAll().stream().map(u -> {
            Map<String, Object> map = new HashMap<>();
            map.put("id", u.getId());
            map.put("email", u.getEmail());
            map.put("role", u.getRole());
            map.put("createdAt", u.getCreatedAt());

            String name = u.getEmail() != null ? u.getEmail().split("@")[0] : "User";
            String role = u.getRole() != null ? u.getRole().trim() : "";
            
            if ("Student".equalsIgnoreCase(role)) {
                name = studentRepository.findByUserId(u.getId()).map(s -> s.getFullName()).orElse(name);
            } else if ("Company".equalsIgnoreCase(role)) {
                name = companyRepository.findByUserId(u.getId()).map(c -> c.getCompanyName()).orElse(name);
            }
            map.put("profileName", name);
            return map;
        }).collect(Collectors.toList()));
    }

    @DeleteMapping("/users/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    @Transactional
    public ResponseEntity<?> deleteUser(@PathVariable Integer id) {
        Authentication auth = SecurityContextHolder.getContext().getAuthentication();
        log.info("Admin [{}] attempting DEEP delete for user ID: {}", auth.getName(), id);

        User user = userRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("User not found"));
        
        if ("Admin".equalsIgnoreCase(user.getRole())) {
            return ResponseEntity.badRequest().body("Cannot delete Admin");
        }

        try {
            // Explicitly handle Student children
            studentRepository.findByUserId(id).ifPresent(student -> {
                log.info("Explicitly cleaning up Student [{}] applications and ideas...", student.getId());
                // 1. Delete all applications made by this student
                applicationRepository.deleteAll(applicationRepository.findByStudentId(student.getId()));
                // 2. Delete all ideas shared by this student
                ideaRepository.deleteAll(ideaRepository.findByStudent(student));
                // Flush to ensure deletions are recognized by DB before student profile is removed
                applicationRepository.flush(); 
                ideaRepository.flush();
            });

            // Explicitly handle Company children
            companyRepository.findByUserId(id).ifPresent(company -> {
                log.info("Explicitly cleaning up Company [{}] internships and their applications...", company.getId());
                internshipRepository.findByCompany(company).forEach(internship -> {
                    // 1. Delete all applications for this internship
                    applicationRepository.deleteAll(applicationRepository.findByInternshipId(internship.getId()));
                });
                applicationRepository.flush();
                // 2. Delete the internships themselves
                internshipRepository.deleteAll(internshipRepository.findByCompany(company));
                internshipRepository.flush();
            });

            // Finally delete the User (this will cascade to Student/Company profile via orphanRemoval)
            userRepository.delete(user);
            userRepository.flush(); 

            log.info("User ID: {} and all associated data cleared successfully.", id);
            return ResponseEntity.ok("User deleted successfully");
        } catch (Exception e) {
            log.error("CRITICAL: Error during deep delete of user ID {}: {}", id, e.getMessage());
            return ResponseEntity.status(500).body("Error deleting user: " + e.getMessage());
        }
    }

    @GetMapping("/ideas")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<?> getAllIdeas() {
        log.info("Admin fetching all ideas list...");
        return ResponseEntity.ok(ideaRepository.findAll().stream().map(idea -> {
            Map<String, Object> map = new HashMap<>();
            map.put("id", idea.getId());
            map.put("title", idea.getTitle());
            map.put("technologyUsed", idea.getTechnologyUsed());
            map.put("postedDate", idea.getPostedDate());
            map.put("studentName", idea.getStudentName());
            return map;
        }).collect(Collectors.toList()));
    }

    @DeleteMapping("/ideas/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    @Transactional
    public ResponseEntity<?> deleteIdea(@PathVariable Integer id) {
        log.info("Admin deleting idea ID: {}", id);
        ideaRepository.deleteById(id);
        return ResponseEntity.ok("Idea deleted");
    }

    @GetMapping("/internships")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<?> getAllInternships() {
        log.info("Admin fetching all internships list...");
        return ResponseEntity.ok(internshipRepository.findAll().stream().map(intern -> {
            Map<String, Object> map = new HashMap<>();
            map.put("id", intern.getId());
            map.put("title", intern.getTitle());
            map.put("companyName", intern.getCompanyName());
            map.put("stipend", intern.getStipend());
            map.put("postedDate", intern.getPostedDate());
            return map;
        }).collect(Collectors.toList()));
    }

    @DeleteMapping("/internships/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    @Transactional
    public ResponseEntity<?> deleteInternship(@PathVariable Integer id) {
        log.info("Admin deleting internship ID: {}", id);
        internshipRepository.deleteById(id);
        return ResponseEntity.ok("Internship deleted");
    }

    // Contact Message Management
    @GetMapping("/messages")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<?> getAllMessages() {
        log.info("Admin fetching all contact messages...");
        return ResponseEntity.ok(contactMessageRepository.findAllByOrderByCreatedAtDesc().stream().map(msg -> {
            Map<String, Object> map = new HashMap<>();
            map.put("id", msg.getId());
            map.put("name", msg.getName());
            map.put("email", msg.getEmail());
            map.put("message", msg.getMessage());
            map.put("status", msg.getStatus());
            map.put("createdAt", msg.getCreatedAt());
            return map;
        }).collect(Collectors.toList()));
    }

    @PutMapping("/messages/{id}/status")
    @PreAuthorize("hasRole('ADMIN')")
    @Transactional
    public ResponseEntity<?> updateMessageStatus(@PathVariable Integer id, @RequestBody Map<String, String> payload) {
        log.info("Admin updating message ID: {} status", id);
        return contactMessageRepository.findById(id)
            .map(msg -> {
                String newStatus = payload.get("status");
                if (newStatus != null && (newStatus.equals("PENDING") || newStatus.equals("REVIEWED"))) {
                    msg.setStatus(newStatus);
                    contactMessageRepository.save(msg);
                    return ResponseEntity.ok("Message status updated");
                }
                return ResponseEntity.badRequest().body("Invalid status");
            })
            .orElse(ResponseEntity.notFound().build());
    }

    @DeleteMapping("/messages/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    @Transactional
    public ResponseEntity<?> deleteMessage(@PathVariable Integer id) {
        log.info("Admin deleting message ID: {}", id);
        contactMessageRepository.deleteById(id);
        return ResponseEntity.ok("Message deleted");
    }
}

