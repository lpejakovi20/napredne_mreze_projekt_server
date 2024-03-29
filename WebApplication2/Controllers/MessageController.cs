﻿using Microsoft.AspNetCore.Mvc;
using MrezeBackend.DTOs.MessageDTOs;
using MrezeBackend.Helpers;
using MrezeBackend.Services;

namespace MrezeBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly InboxService _inboxService;

        public MessageController(UserService userService, InboxService inboxService)
        {
            _inboxService = inboxService;
        }


        [HttpPost("send")]
        public async Task<IActionResult> RecordMessageAsync([FromBody] MessageDTO messageDTO)
        {
            try
            {
                string decryptedContent = EncryptionHelper.DecryptSymmetric(Convert.FromBase64String(messageDTO.Content), messageDTO.SenderId.ToString());
                string decryptedTitle = EncryptionHelper.DecryptSymmetric(Convert.FromBase64String(messageDTO.Title), messageDTO.SenderId.ToString());

                messageDTO.Content = decryptedContent;
                messageDTO.Title = decryptedTitle;
                var message = await _inboxService.saveMessage(messageDTO);

                if (message == null)
                {
                    return StatusCode(404, "Client with the provided email does not exist.");
                }
                return Ok("Message successfully sent.");
            }
            catch (Exception ex)
            {
                return BadRequest("Message content was not properly encrypted on the client side.");
            }
        }

        [HttpGet("getInbox/{clientId}")]
        public async Task<IActionResult> GetInbox(int clientId)
        {
            InboxDTO inbox = await _inboxService.GetClientInbox(clientId);

            return Ok(inbox);
        }
    }
}
