﻿using DotNetConverter.Models;
using DotNetConverter.Services;
using DotNetConverter.Services.Queues;
using Microsoft.AspNetCore.Mvc;

namespace DotNetConverter.Controllers;

[ApiController]
[Route("[controller]")]
public class ConverterController : ControllerBase
{
    private readonly IVideoQueue _queue;
    private readonly ILogger<ConverterController> _logger;

    public ConverterController(IVideoQueue queue, 
        ILogger<ConverterController> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> QueueVideo([FromBody] QueueVideoRequest request, CancellationToken token)
    {
        if (!ModelState.IsValid)    
            return BadRequest(ModelState);

        try
        {
            await _queue.QueueWorkItemAsync(token, request.Id, request.Url, request.CallbackUrl);
        }
        catch (Exception ex)
        {
            if (ex is ArgumentNullException or ArgumentException)
            {
                _logger.LogError("Returning error response");
                return BadRequest(ex.Message);
            }
            else
            {
                _logger.LogError(ex, "Returning server error response");
                return StatusCode(500, "Sorry, something went wrong");
            }
        }

        return Ok();
    }
}