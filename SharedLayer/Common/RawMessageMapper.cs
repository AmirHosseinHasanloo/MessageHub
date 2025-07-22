using Messaging.Protos;
using SharedLayer.Contracts.MessageDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLayer.Common
{
    public static class MessageMapper
    {
        public static RawMessageDto ToDto(RawMessage protoMessage)
        {
            return new RawMessageDto
            {
                Id = protoMessage.Id,
                Sender = protoMessage.Sender,
                Message = protoMessage.Message
            };
        }

        public static RawMessage ToProto(RawMessageDto dto)
        {
            return new RawMessage
            {
                Id = dto.Id,
                Sender = dto.Sender,
                Message = dto.Message
            };
        }
    }
}
