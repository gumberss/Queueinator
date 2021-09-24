using Queueinator.Forms.Domain.Enums;
using System.Collections.Generic;

namespace Queueinator.Forms.Domain
{
    public class MoveMessageData
    {
        public OnMoveEnum PostAction { get; set; }

        public List<MessageTree> Messages { get; set; }

        public MoveMessageData(OnMoveEnum postAction, List<MessageTree> messages)
        {
            PostAction = postAction;
            Messages = messages;
        }
    }
}
