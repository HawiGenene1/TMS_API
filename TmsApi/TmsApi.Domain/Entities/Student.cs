namespace TmsApi.Domain.Entities;

public class Student
{
    public int Id { get; set; }
    public required string RegistrationNumber { get; set; }  // Student ID
    public required string Name { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;

    // Concurrency token — auto-incremented by the DB on every update
    public uint Version { get; set; }

    // Soft delete flag — record stays in DB but is hidden from queries
    public bool IsDeleted { get; set; } = false;

    // Navigation - A student has many enrollments
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    // Navigation to certificates
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}
