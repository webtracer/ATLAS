using System.ComponentModel.DataAnnotations;

namespace Amdocs.Atlas.Core.DTOs
{
    public sealed class ServerCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Hostname { get; set; } = string.Empty;

        [Required]
        public int EnvironmentId { get; set; }

        [Required]
        public int RoleId { get; set; }

        // Optional fields, adjust to your actual entity
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}