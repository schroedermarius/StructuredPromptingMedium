namespace StructuredPromptingMedium.Models;

public class Employee
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Department { get; set; }
    public decimal Salary { get; set; }
    public required Address Address { get; set; }
    public required ContactInfo ContactInfo { get; set; }
    public required EmploymentDetails EmploymentDetails { get; set; }
    public List<Skill> Skills { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
}

public class Address
{
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string ZipCode { get; set; }
    public required string Country { get; set; }
    public GeoCoordinates? Coordinates { get; set; }
}

public class GeoCoordinates
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class ContactInfo
{
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public string? LinkedIn { get; set; }
    public EmergencyContact? EmergencyContact { get; set; }
}

public class EmergencyContact
{
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public required string Relationship { get; set; }
}

public class EmploymentDetails
{
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public required string JobTitle { get; set; }
    public required string EmploymentType { get; set; }
    public Manager? Manager { get; set; }
    public List<string> Benefits { get; set; } = new();
    public PerformanceReview? LastReview { get; set; }
}

public class Manager
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class PerformanceReview
{
    public DateTime ReviewDate { get; set; }
    public int Rating { get; set; }
    public required string Comments { get; set; }
    public List<string> Goals { get; set; } = new();
}

public class Skill
{
    public required string Name { get; set; }
    public int Level { get; set; } // 1-10
    public int YearsExperience { get; set; }
    public DateTime? LastUsed { get; set; }
    public List<string> Certifications { get; set; } = new();
}

public class Project
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public required string Status { get; set; }
    public List<string> Technologies { get; set; } = new();
    public ProjectMetrics? Metrics { get; set; }
}

public class ProjectMetrics
{
    public int LinesOfCode { get; set; }
    public int BugsFixed { get; set; }
    public int FeaturesDelivered { get; set; }
    public double CustomerSatisfaction { get; set; }
}