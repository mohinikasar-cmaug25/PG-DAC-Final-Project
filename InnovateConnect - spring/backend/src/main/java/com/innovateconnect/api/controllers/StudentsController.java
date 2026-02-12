package com.innovateconnect.api.controllers;

import com.innovateconnect.api.models.*;
import com.innovateconnect.api.repositories.*;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

import java.util.Optional;

@RestController
@RequestMapping("/api/students")
public class StudentsController {

    @Autowired
    private StudentRepository studentRepository;

    @Autowired
    private UserRepository userRepository;

    @Autowired
    private StudentResumeRepository resumeRepository;

    @GetMapping("/profile")
    public ResponseEntity<?> getProfile(Authentication authentication) {
        String email = authentication.getName();
        User user = userRepository.findByEmail(email).get();
        Student student = studentRepository.findByUser(user)
                .orElseThrow(() -> new RuntimeException("Student not found"));
        return ResponseEntity.ok(student);
    }

    @PutMapping("/profile")
    public ResponseEntity<?> updateProfile(Authentication authentication, @RequestBody Student studentUpdate) {
        String email = authentication.getName();
        User user = userRepository.findByEmail(email)
                .orElseThrow(() -> new RuntimeException("User not found"));
        Student student = studentRepository.findByUser(user)
                .orElseThrow(() -> new RuntimeException("Student not found"));

        student.setFullName(studentUpdate.getFullName());
        student.setGitHubLink(studentUpdate.getGitHubLink());
        student.setLeetCodeLink(studentUpdate.getLeetCodeLink());
        student.setUniversity(studentUpdate.getUniversity());
        student.setBio(studentUpdate.getBio());
        student.setSkills(studentUpdate.getSkills());
        student.setLocation(studentUpdate.getLocation());

        return ResponseEntity.ok(studentRepository.save(student));
    }

    @GetMapping("/{id}")
    public ResponseEntity<?> getStudentById(@PathVariable Integer id) {
        return studentRepository.findById(id)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    @PostMapping("/upload-resume")
    public ResponseEntity<?> uploadResume(Authentication authentication, @RequestParam("file") org.springframework.web.multipart.MultipartFile file) {
        try {
            String email = authentication.getName();
            User user = userRepository.findByEmail(email).get();
            Student student = studentRepository.findByUser(user).get();

            StudentResume resume = resumeRepository.findByStudentId(student.getId())
                    .orElse(StudentResume.builder().student(student).build());

            resume.setResumeData(file.getBytes());
            resume.setResumeFileName(file.getOriginalFilename());
            resume.setResumeContentType(file.getContentType());
            resumeRepository.save(resume);

            return ResponseEntity.ok("Resume uploaded successfully");
        } catch (Exception e) {
            return ResponseEntity.status(500).body("Error uploading resume");
        }
    }

    @GetMapping("/{id}/resume")
    public ResponseEntity<?> getResume(@PathVariable Integer id) {
        StudentResume resume = resumeRepository.findByStudentId(id)
                .orElseThrow(() -> new RuntimeException("Resume not found"));

        return ResponseEntity.ok()
                .header(org.springframework.http.HttpHeaders.CONTENT_DISPOSITION, "inline; filename=\"" + resume.getResumeFileName() + "\"")
                .contentType(org.springframework.http.MediaType.parseMediaType(resume.getResumeContentType()))
                .body(resume.getResumeData());
    }

    @PostMapping("/upload-cover")
    public ResponseEntity<?> uploadCoverImage(Authentication authentication, @RequestParam("file") org.springframework.web.multipart.MultipartFile file) {
        try {
            String email = authentication.getName();
            User user = userRepository.findByEmail(email).get();
            Student student = studentRepository.findByUser(user).get();

            student.setCoverImageData(file.getBytes());
            student.setCoverImageType(file.getContentType());
            student.setHasCoverImage(true);
            studentRepository.save(student);

            return ResponseEntity.ok("Cover image uploaded successfully");
        } catch (Exception e) {
            return ResponseEntity.status(500).body("Error uploading cover image");
        }
    }

    @GetMapping("/{id}/cover")
    public ResponseEntity<?> getCoverImage(@PathVariable Integer id) {
        Student student = studentRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Student not found"));

        if (student.getHasCoverImage() == null || !student.getHasCoverImage() || student.getCoverImageData() == null) {
             return ResponseEntity.notFound().build();
        }

        return ResponseEntity.ok()
                .contentType(org.springframework.http.MediaType.parseMediaType(student.getCoverImageType()))
                .body(student.getCoverImageData());
    }
}
