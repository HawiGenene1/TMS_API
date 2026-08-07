namespace TmsApi.Entities;

public class Assessment
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; }      // 0.30 = 30% of grade
    
    // Foreign key to Course
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}