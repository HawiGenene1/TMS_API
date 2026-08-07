using System;

namespace TmsApi.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }       // Foreign key to Student
    public int CourseId { get; set; }        // Foreign key to Course
    public decimal? Grade { get; set; }      // Nullable (not graded yet)
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}