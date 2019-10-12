using Api.Common.Cqrs.Core.CommandHandlers;
using Api.Common.Repository.Contracts.Core.Repository;
using Api.Template.Domain.Commands.Personas;
using Api.Template.Domain.Models;
using System.Threading.Tasks;

namespace Api.Template.Domain.CommandHandlers.Personas
{
    public class CreatePersonaCommandHandler :
        ICommandHandlerTwoWay<CreatePersonaCommand, Persona>
    {
        private readonly IRepositoryAsync<Persona> repository;

        public CreatePersonaCommandHandler(IRepositoryAsync<Persona> repository)
        {
            this.repository = repository;
        }

        public Task<Persona> Handle(CreatePersonaCommand command)
        {
            return Task.Run(async () =>
            {
                //Domain Changes
                var instance = new Persona
                {
                    Name = command.Name,
                    IsActive = command.IsActive
                };

                //Persistence
                await repository.Insert(instance);

                return instance;
            });
        }
    }
}