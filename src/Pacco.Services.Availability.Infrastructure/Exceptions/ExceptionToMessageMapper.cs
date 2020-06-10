using System;
using Convey.MessageBrokers.RabbitMQ;
using Pacco.Services.Availability.Application.Commands;
using Pacco.Services.Availability.Application.Events.Rejected;
using Pacco.Services.Availability.Application.Exceptions;
using Pacco.Services.Availability.Core.Exceptions;

namespace Pacco.Services.Availability.Infrastructure.Exceptions
{
    internal sealed class ExceptionToMessageMapper : IExceptionToMessageMapper
    {
        public object Map(Exception exception, object message) 
            => exception switch
            {
                MissingResourceTagsException ex => new AddResourceRejected(((AddResource)message).ResourceId, 
                    ex.Code, ex.Message),
                InvalidResourceTagsException ex => new AddResourceRejected(((AddResource)message).ResourceId, 
                    ex.Code, ex.Message),
                ResourceAlreadyExistsException ex => message switch
                {
                    AddResource cmd => new AddResourceRejected(cmd.ResourceId,ex.Code, ex.Message),
                    _ => null
                },
                _ => null
            };
    }
}