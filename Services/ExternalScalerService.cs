using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Externalscaler;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using Google.Protobuf.Collections;

namespace kaboom_scaler
{
    public class ExternalScalerService : ExternalScaler.ExternalScalerBase
    {
        private readonly ILogger<ExternalScalerService> _logger;
        static private TwitchClient _client;
        static private MapField<string, string> _metricMetadata;
        static private int state = -1;
        public ExternalScalerService(ILogger<ExternalScalerService> logger)
        {
            _logger = logger;
        }

        // This method is called frequently from the KEDA runtime. It is supposed to create and prepare all the resources required 
        // for the GetMetrics service method to be succesful.
        // Here we create the irc channel client, and we keep it for the lifetime of the scaler.
        // Ideally, there should be a separate client for every different metric, but we currently we support one. We will change this soon.
        public override Task<Empty> New(NewRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"{DateTime.Now} New Is Called, the problem is between the screen and the chair!");

            if(_metricMetadata != null && !_metricMetadata.Equals(request.Metadata)) 
            {
                _logger.LogInformation($"the _metricMetadata: {_metricMetadata} is not equal to request.Metadata: {request.Metadata}");
            }

            //If this is the first time New is called, or the metadata has changed
            if (_metricMetadata == null || !_metricMetadata.Equals(request.Metadata))
            {
                var accessToken = request.Metadata["accessToken"];
                var twitchUserName = request.Metadata["twitchUserName"];
                var channelName = request.Metadata["channelName"];

                _logger.LogInformation($"kaboom scaler new: twitchname:{twitchUserName}, channelName:{channelName}");

                ConnectionCredentials credentials = new ConnectionCredentials(twitchUserName, accessToken);
                var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 750,
                    ThrottlingPeriod = TimeSpan.FromSeconds(30)
                };
                WebSocketClient customClient = new WebSocketClient(clientOptions);
                _client = new TwitchClient(customClient);
                _client.Initialize(credentials, channelName);

                _client.OnMessageReceived += Client_OnMessageReceived;
                _client.OnLog += Client_OnLog;
                _client.Connect();

                //Save the metadata so that we can compare it the the request next time to see if it changed.
                _metricMetadata = request.Metadata;

            }

            return Task.FromResult(new Empty());
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            _logger.LogInformation($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        // This method is invoked on each new message coming from the channel. In it we set a variable that represents the scaling target so 
        // it can be consumed by the GetMetrics method.
        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            _logger.LogInformation($"{DateTime.Now} Twitch message Received: {e.ChatMessage.Message}");

            var input = e.ChatMessage.Message;

            // find any word in the message that is a "kaboom" no matter how many 'o' characters
            // do the same for "fssst"
            // and then find the difference and set the metric to the difference as long as it is not less than 0
            var matchOfOs = Regex.Match(input, "kab([o]+)m");
            var countOfOs = matchOfOs.Groups[1].Length;

            var matchOfFst = Regex.Match(input, "f([s]+)t");
            var countOfFst = matchOfFst.Groups[1].Length;

            _logger.LogInformation($"{DateTime.Now} In Twitch message Received: state before:{state}, countOfOs:{countOfOs}, countOfFst: {countOfFst}.");


            state = Math.Max(0, state + countOfOs - countOfFst);
        }

        public override Task<IsActiveResponse> IsActive(ScaledObjectRef request, ServerCallContext context)
        {
            return Task.FromResult(new IsActiveResponse() { Result = true });
        }

        public override Task<GetMetricSpecResponse> GetMetricSpec(ScaledObjectRef request, ServerCallContext context)
        {
            var metricsSpec = new MetricSpec();
            metricsSpec.MetricName = "kaboom";
            metricsSpec.TargetSize = 1;
            var response = new GetMetricSpecResponse();
            response.MetricSpecs.Add(metricsSpec);

            return Task.FromResult<GetMetricSpecResponse>(response);

        }

        // This method is invoked frequently from KEDA to retrieve the metric. The value we return here is from the 'state' variable
        // which is set by the Client_OnMessageReceived above.
        public override Task<GetMetricsResponse> GetMetrics(GetMetricsRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"{DateTime.Now} GetMetrics Called: state: {state}");
            var response = new GetMetricsResponse();
            response.MetricValues.Add(new MetricValue { MetricName = "kaboom", MetricValue_ = state });
            return Task.FromResult<GetMetricsResponse>(response);
        }

        public override Task<Empty> Close(ScaledObjectRef request, ServerCallContext context)
        {
            return Task.FromResult<Empty>(new Empty());
        }

    }
}