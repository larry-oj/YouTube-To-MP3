using DotNetConverter.Data.Models;
using DotNetConverter.Data.Repositories;
using DotNetConverter.Models;
using DotNetConverter.Services.Queues;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace DotNetConverter.Controllers;

[ApiController]
[Route("[controller]")]
public class ConverterController : ControllerBase
{
    private readonly IVideoQueue _queue;
    private readonly ILogger<ConverterController> _logger;
    private readonly IRepo<QueuedItem> _repo;
    private readonly IConfiguration _configuration;

    public ConverterController(IVideoQueue queue, 
        ILogger<ConverterController> logger,
        IRepo<QueuedItem> repo,
        IConfiguration configuration)
    {
        _queue = queue;
        _logger = logger;
        _repo = repo;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("videos")]
    public async Task<IActionResult> QueueVideo([FromBody] QueueVideoRequest request, CancellationToken token)
    {
        if (!ModelState.IsValid)    
            return BadRequest(ModelState);

        string id;
        
        try
        {
            id = await _queue.QueueWorkItemAsync(token, request.Url, request.WithCallback, request.CallbackUrl);
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

        return Ok(new QueueVideoResponse(id));
    }

    [HttpGet]
    [Route("videos/{id}/status")]
    public IActionResult CheckVideo(string id)
    {
        if (!ModelState.IsValid)    
            return BadRequest(ModelState);

        try
        {
            var record = _repo.Get(id);
            if (record is null) 
                return BadRequest("No video found");
            
            return Ok(new CheckVideoResponse(id, record.IsFailed, record.IsFinished));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Returning server error response");
            return StatusCode(500, "Sorry, something went wrong");
        }
    }

    [HttpGet]
    [Route("videos/{id}")]
    public IActionResult GetFile(string id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var record = _repo.Get(id);
            if (record is null)
            {
                return BadRequest("Video is not found in queue");
            }
            if (!record.IsFinished || record.IsFailed)
            {
                return BadRequest("Video has failed to convert or has not been yet converted");
            }
        
            var filepath = Path.Combine(_configuration.GetSection("Converter:TempDir").Value!, id + ".mp3");
            filepath = Path.Combine(Directory.GetCurrentDirectory(), filepath);
            var validName = Path.GetInvalidFileNameChars()
                .Aggregate(record.Name, (current, @char) => current.Replace(@char, '-'));

            return PhysicalFile(filepath, "audio/mpeg", validName + ".mp3");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Returning server error response");
            return StatusCode(500, "Sorry, something went wrong");
        }
    }
}