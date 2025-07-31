using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos;

public class UpdateCommunicationDto
{
    [Required, StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public List<string> ValidStatusCodes { get; set; } = [];
}
