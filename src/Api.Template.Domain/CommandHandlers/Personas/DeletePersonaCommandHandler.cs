using Api.Common.Cqrs.Core.CommandHandlers;
using Api.Common.Repository.Contracts.Core.Repository;
using Api.Template.Domain.Commands.Personas;
using Api.Template.Domain.Models;
using System.Threading.Tasks;

namespace Api.Template.Domain.CommandHandlers.Personas
{
    public class DeletePersonaCommandHandler :
        ICommandHandler<DeletePersonaCommand>
    {
        private readonly IRepositoryAsync<Persona> repository;

        public DeletePersonaCommandHandler(IRepositoryAsync<Persona> repository)
        {
            this.repository = repository;
        }

        public async Task Handle(DeletePersonaCommand command)
        {
            //Persistence
            await repository.Delete(command.Id);

        }
    }
}