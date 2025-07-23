using Application.Contracts;
using Grpc.Core;
using Messaging.EventHandler;
using Messaging.Grpc.Services;
using Messaging.Protos;
using Messaging.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLayer.Common;
using SharedLayer.Contracts.MessageDTOs;
using System.Collections.Concurrent;

namespace MessageHub.API.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessageChangeStream.MessageChangeStreamClient _grpcClient;

        public MessagesController(MessageChangeStream.MessageChangeStreamClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] RawMessageDto dto)
        {
            var grpcMessage = new RawMessage
            {
                Id = dto.Id,
                Message = dto.Message,
                Sender = dto.Sender,
            };

            try
            {
             //   await _grpcClient.Sen
            }
            catch (Exception)
            {

                throw;
            }
            return Ok();
        }
    }
}
