package com.innovateconnect.api.controllers;

import com.innovateconnect.api.dto.*;
import com.innovateconnect.api.models.*;
import com.innovateconnect.api.repositories.*;
import com.innovateconnect.api.security.JwtTokenProvider;
import org.springframework.transaction.annotation.Transactional;
import jakarta.validation.Valid;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/auth")
public class AuthController {

    @Autowired
    private AuthenticationManager authenticationManager;

    @Autowired
    private UserRepository userRepository;

    @Autowired
    private StudentRepository studentRepository;

    @Autowired
    private CompanyRepository companyRepository;

    @Autowired
    private PasswordEncoder passwordEncoder;

    @Autowired
    private JwtTokenProvider tokenProvider;

    @Autowired
    private com.innovateconnect.api.services.EmailService emailService;

    @PostMapping("/login")
    public ResponseEntity<?> authenticateUser(@Valid @RequestBody LoginRequest loginRequest) {
        Authentication authentication = authenticationManager.authenticate(
                new UsernamePasswordAuthenticationToken(
                        loginRequest.getEmail(),
                        loginRequest.getPassword()
                )
        );

        SecurityContextHolder.getContext().setAuthentication(authentication);
        String jwt = tokenProvider.generateToken(authentication);

        User user = userRepository.findByEmail(loginRequest.getEmail()).get();
        String name = "";

        if ("Student".equalsIgnoreCase(user.getRole())) {
            name = studentRepository.findByUserId(user.getId()).map(Student::getFullName).orElse(user.getEmail().split("@")[0]);
        } else if ("Company".equalsIgnoreCase(user.getRole())) {
            name = companyRepository.findByUserId(user.getId()).map(Company::getCompanyName).orElse(user.getEmail().split("@")[0]);
        } else if ("Admin".equalsIgnoreCase(user.getRole())) {
            name = "Administrator";
        } else {
             name = user.getEmail().split("@")[0];
        }

        return ResponseEntity.ok(AuthResponse.builder()
                .token(jwt)
                .user(AuthResponse.UserDto.builder()
                        .id(user.getId())
                        .email(user.getEmail())
                        .role(user.getRole())
                        .name(name)
                        .build())
                .build());
    }

    @PostMapping("/register")
    @Transactional
    public ResponseEntity<?> register(@Valid @RequestBody RegisterRequest request) {
        if (userRepository.findByEmail(request.getEmail()).isPresent()) {
            return ResponseEntity.badRequest().body("Email already in use");
        }

        User user = User.builder()
                .email(request.getEmail())
                .passwordHash(passwordEncoder.encode(request.getPassword()))
                .role(request.getRole())
                .build();
        user = userRepository.save(user);

        if ("Student".equalsIgnoreCase(request.getRole())) {
            Student student = Student.builder()
                    .user(user)
                    .fullName(request.getFullName())
                    .university(request.getUniversity())
                    .gitHubLink(request.getGitHubLink())
                    .leetCodeLink(request.getLeetCodeLink())
                    .build();
            studentRepository.save(student);
        } else if ("Company".equalsIgnoreCase(request.getRole())) {
            Company company = Company.builder()
                    .user(user)
                    .companyName(request.getCompanyName())
                    .location(request.getLocation())
                    .website(request.getWebsite())
                    .build();
            companyRepository.save(company);
        }

        try {
            String name = "Student".equalsIgnoreCase(request.getRole()) ? request.getFullName() : request.getCompanyName();
            emailService.sendRegistrationEmail(request.getEmail(), name, request.getRole());
        } catch (Exception e) {
            System.err.println("Failed to send registration email: " + e.getMessage());
        }

        return ResponseEntity.ok("Registered successfully");
    }

    @PostMapping("/register/student")
    @Transactional
    public ResponseEntity<?> registerStudent(@Valid @RequestBody StudentRegisterRequest request) {
        if (userRepository.findByEmail(request.getEmail()).isPresent()) {
            return ResponseEntity.badRequest().body("Email already in use");
        }

        User user = User.builder()
                .email(request.getEmail())
                .passwordHash(passwordEncoder.encode(request.getPassword()))
                .role("Student")
                .build();
        user = userRepository.save(user);

        Student student = Student.builder()
                .user(user)
                .fullName(request.getFullName())
                .university(request.getUniversity())
                .build();
        studentRepository.save(student);

        try {
            emailService.sendRegistrationEmail(request.getEmail(), request.getFullName(), "Student");
        } catch (Exception e) {
            System.err.println("Failed to send registration email: " + e.getMessage());
        }

        return ResponseEntity.ok("Student registered successfully");
    }

    @PostMapping("/register/company")
    @Transactional
    public ResponseEntity<?> registerCompany(@Valid @RequestBody CompanyRegisterRequest request) {
        if (userRepository.findByEmail(request.getEmail()).isPresent()) {
            return ResponseEntity.badRequest().body("Email already in use");
        }

        User user = User.builder()
                .email(request.getEmail())
                .passwordHash(passwordEncoder.encode(request.getPassword()))
                .role("Company")
                .build();
        user = userRepository.save(user);

        Company company = Company.builder()
                .user(user)
                .companyName(request.getCompanyName())
                .location(request.getLocation())
                .build();
        companyRepository.save(company);

        try {
            emailService.sendRegistrationEmail(request.getEmail(), request.getCompanyName(), "Company");
        } catch (Exception e) {
            System.err.println("Failed to send registration email: " + e.getMessage());
        }

        return ResponseEntity.ok("Company registered successfully");
    }
}
