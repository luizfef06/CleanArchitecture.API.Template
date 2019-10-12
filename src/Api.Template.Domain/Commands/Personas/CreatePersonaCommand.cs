using Api.Common.Cqrs.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Api.Template.Domain.Commands.Personas
{
    public class CreatePersonaCommand : Command
    {
        public CreatePersonaCommand(string name, bool isActive)
        {
            Name = name;
            IsActive = isActive;
        }

        [MinLength(2)]
        [MaxLength(255)]
        [Required]
        public string Name { get; protected set; }
        public bool IsActive { get; protected set; }
    }
}