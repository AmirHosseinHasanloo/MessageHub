using Grpc.Core;
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
        private readonly QueueSimulator _queueSimulator;

        public MessagesController(QueueSimulator queueSimulator)
        {
            _queueSimulator = queueSimulator;
        }

        [HttpPost("send")]
        public IActionResult SendMessage([FromBody] RawMessageDto dto)
        {

            _queueSimulator.DoEnqueue(dto);

            return Ok(new { Status = "Message queued" });
        }
    }
}
