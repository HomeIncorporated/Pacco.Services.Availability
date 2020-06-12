using System;
using System.Threading.Tasks;
using NSubstitute;
using Pacco.Services.Availability.Application.Commands;
using Pacco.Services.Availability.Application.Commands.Handlers;
using Pacco.Services.Availability.Application.Exceptions;
using Pacco.Services.Availability.Application.Services;
using Pacco.Services.Availability.Core.Entities;
using Pacco.Services.Availability.Core.Repositories;
using Shouldly;
using Xunit;

namespace Pacco.Services.Availability.Tests.Unit.Application.Handlers
{
    public class AddResourceHandlerTests
    {
        Task Act(AddResource command)
            => _commandHandler.HandleAsync(command);

        [Fact]
        public async Task given_resource_id_that_already_exists_the_handler_should_throw_an_exception()
        {
            var command = new AddResource(Guid.NewGuid(), new []{"tags"});
            _repository.ExistsAsync(command.ResourceId).Returns(true);

            var exception = await Record.ExceptionAsync(() => Act(command));

            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<ResourceAlreadyExistsException>();
        }

        [Fact]
        public async Task given_valid_id_and_tags_the_resource_should_be_persisted_using_repository()
        {
            var command = new AddResource(Guid.NewGuid(), new []{"tags"});
            _repository.ExistsAsync(command.ResourceId).Returns(false);

            await Act(command);

            await _repository
                .Received(1)
                .AddAsync(Arg.Is<Resource>(r => r.Id == command.ResourceId));
        }

        #region ARRANGE

        private readonly IResourcesRepository _repository;
        private readonly IEventProcessor _eventProcessor;
        private readonly AddResourceHandler _commandHandler;
        
        public AddResourceHandlerTests()
        {
            _repository = Substitute.For<IResourcesRepository>();
            _eventProcessor = Substitute.For<IEventProcessor>();
            _commandHandler = new AddResourceHandler(_repository, _eventProcessor);
        }

        #endregion
    }
}