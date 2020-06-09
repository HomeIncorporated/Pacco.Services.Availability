using System.Threading.Tasks;
using Convey.CQRS.Commands;

namespace Pacco.Services.Availability.Application.Commands.Handlers
{
    public class ReserveResourceHandler : ICommandHandler<ReserveResource>
    {
        public Task HandleAsync(ReserveResource command)
        {
            return Task.CompletedTask;
        }
    }
}