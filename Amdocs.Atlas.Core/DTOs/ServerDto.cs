namespace Amdocs.Atlas.Core.DTOs
{
    public sealed class ServerDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;

        public int EnvironmentId { get; set; }
        public int RoleId { get; set; }

        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}