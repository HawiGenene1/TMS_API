namespace TmsApi.Entities;

public class Student
{
    public int Id { get; set; }              // Unique ID (surrogate key)
    public required string RegistrationNumber { get; set; }  // Student ID
    public required string Name { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation - A student has many enrollments
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    // Navigation to certificates
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}