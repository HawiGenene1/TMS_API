namespace TmsApi.Entities;

public class Course
{
    public int Id { get; set; }              // Unique ID
    public required string Code { get; set; }  // "CS-101"
    public required string Title { get; set; }
    public int Capacity { get; set; }
    
    // Navigation - A course has many enrollments
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    // Navigation to assessments
    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();

    // Navigation to certificates
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}