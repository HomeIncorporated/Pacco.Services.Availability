using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pacco.Services.Availability.Api;
using Pacco.Services.Availability.Application.Commands;
using Pacco.Services.Availability.Infrastructure.Mongo.Documents;
using Pacco.Services.Availability.Tests.Shared.Factories;
using Pacco.Services.Availability.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace Pacco.Services.Availability.Tests.EndToEnd.Sync
{
    public class AddResourceApiTests : IClassFixture<PaccoApplicationFactory<Program>>, IDisposable
    {
        Task<HttpResponseMessage> Act(AddResource command)
            => _httpClient.PostAsync("resources", GetContent(command));

        [Fact]
        public async Task given_valid_resource_data_endpoint_should_return_http_status_code_created()
        {
            var command = new AddResource(Guid.NewGuid(), new[] {"tag"} );

            var response = await Act(command);
            
            response.ShouldNotBeNull();
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
        }
        
        [Fact]
        public async Task given_valid_resource_data_endpoint_should_return_location_header()
        {
            var command = new AddResource(Guid.NewGuid(), new[] {"tag"} );

            var response = await Act(command);

            var locationHeader = response.Headers
                .FirstOrDefault(h => h.Key is "Location").Value.First();
            
            locationHeader.ShouldNotBeNull();
            locationHeader.ShouldBe($"resources/{command.ResourceId}");
        }

        [Fact]
        public async Task given_valid_resource_data_new_document_should_be_persisted_into_db()
        {
            var command = new AddResource(Guid.NewGuid(), new[] {"tag"} );

            await Act(command);

            var document = await _mongoDbFixture.GetAsync(command.ResourceId);

            document.ShouldNotBeNull();
            document.Id.ShouldBe(command.ResourceId);
            document.Tags.ShouldBe(command.Tags);
        }
        

        #region ARRANGE

        private readonly HttpClient _httpClient;
        private readonly MongoDbFixture<ResourceDocument, Guid> _mongoDbFixture;
        
        public AddResourceApiTests(PaccoApplicationFactory<Program> factory)
        {
            _mongoDbFixture = new MongoDbFixture<ResourceDocument, Guid>("resources");
            factory.Server.AllowSynchronousIO = true;
            _httpClient = factory.CreateClient();
        }
        
        private static StringContent GetContent(object @object)
            => new StringContent(JsonConvert.SerializeObject(@object), Encoding.UTF8, "application/json");
        
        public void Dispose()
        {
            _httpClient?.Dispose();
            _mongoDbFixture?.Dispose();
        }
        
        #endregion
    }
}