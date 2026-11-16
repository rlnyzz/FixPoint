namespace FixPoint.Api.DTOs;

public class CreateFeedbackDto
{
    public string Type { get; set; } = "Question"; // Question или RevisionRequest
    public string Message { get; set; } = string.Empty;
}