using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Pacco.Services.Availability.Core.Entities;
using Pacco.Services.Availability.Core.Events;
using Pacco.Services.Availability.Core.Exceptions;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace Pacco.Services.Availability.Tests.Unit.Core.Entities
{
    public class CreateResourceTests
    {
        Resource Act(AggregateId id, IEnumerable<string> tags)
            => Resource.Create(id, tags);
        
        [Fact]
        public void given_valid_id_and_tags_resource_should_be_created()
        {
            var id = new AggregateId();
            var tags = new[] {"tag"};

            var resource = Act(id, tags);
            
            resource.ShouldNotBeNull();
            resource.Id.ShouldBe(id);
            resource.Tags.ShouldBe(tags);
            
            resource.Events.Count().ShouldBe(1);
            var domainEvent = resource.Events.First();
            domainEvent.ShouldBeOfType<ResourceCreated>();
        }

        [Theory]
        [InlineData("null")]
        [InlineData("[]")]
        public void given_empty_tags_resource_should_throw_an_exception(string value)
        {
            var id = new AggregateId();
            var tags = JsonConvert.DeserializeObject<string[]>(value);
            
            var exception = Record.Exception(() => Act(id, tags));
            
            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<MissingResourceTagsException>();
        }

        [Theory]
        [InlineData(1,1, 2)]
        [InlineData(2,2, 4)]
        [InlineData(2,3, 6)]
        public void given_valid_arguments_the_sum_should_be_calculated_correctly(int arg1, int arg2, int expectedResult)
        {
            var result = arg1 + arg2;
            Assert.Equal(result, expectedResult);
        }
    }
}