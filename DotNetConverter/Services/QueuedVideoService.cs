﻿using DotNetConverter.Data;
using DotNetConverter.Data.Models;
using DotNetConverter.Data.Repositories;
using DotNetConverter.Models;
using DotNetConverter.Services.Queues;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace DotNetConverter.Services;

public class QueuedVideoService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<QueuedVideoService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDbContextFactory<ConverterDbContext> _contextFactory;

    private readonly IVideoQueue _queue;
    public IVideoQueue Queue => _queue;


    public QueuedVideoService(IVideoQueue queue, 
        ILogger<QueuedVideoService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IDbContextFactory<ConverterDbContext> contextFactory)
    {
        _queue = queue;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _contextFactory = contextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("QueuedVideoService is starting");

        while(!cancellationToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(cancellationToken);

            try
            {
                await ConvertAsync(workItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing work item");
                await HandleError(ex, workItem);
            }
        }
        
        _logger.LogInformation("QueuedVideoService is stopping");
    }

    private async Task ConvertAsync(WorkItem workItem)
    {
        _logger.LogInformation("Begin conversion");

        _logger.LogInformation("Getting audio stream");
        var audioStreamInfo = workItem.StreamManifest!.GetAudioOnlyStreams().GetWithHighestBitrate();

        var tmpPath = _configuration.GetSection("Converter:TempDir").Value;
        if (!Directory.Exists(tmpPath))
            Directory.CreateDirectory(tmpPath!);

        var ffmpegPath = _configuration.GetSection("Ffmpeg:bin").Value;
        
        const string containerName = "mp3";
        var fileName = $"{workItem.Id}.{containerName}"; 
        var filePath = Path.Combine(tmpPath!, fileName);

        _logger.LogInformation($"Downloading audio stream");
        var ytClient = new YoutubeClient(_httpClientFactory.CreateClient());
        await ytClient.Videos.DownloadAsync(new IStreamInfo[] { audioStreamInfo },
            new ConversionRequestBuilder(filePath)
                .SetContainer(containerName)
                .SetPreset(ConversionPreset.UltraFast)
                .SetFFmpegPath(ffmpegPath!)
                .Build()
        );

        _logger.LogInformation($"Conversion of {workItem.Id} successful");

        using (var repo = new Repo<QueuedItem>(_contextFactory))
        {
            var record = repo.Get(workItem.Id);
            if (record is not null)
            {
                record.IsFinished = true;
                record.TimeFinished = DateTime.UtcNow;
                repo.Update(record);
                await repo.SaveAsync();
            }
        }
        
        if (!workItem.WithCallback) return;
        try
        {
            using (var multipartFormContent = new MultipartFormDataContent())
            {
                //Load the file and set the file's Content-Type header
                var fileStreamContent = new StreamContent(File.OpenRead(filePath));
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");

                var validName = Path.GetInvalidFileNameChars()
                    .Aggregate(workItem.Name, (current, @char) => current.Replace(@char, '-'));

                //Add the file
                multipartFormContent.Add(fileStreamContent, name: "file", fileName: validName + ".mp3");

                using var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync(workItem.CallbackUrl, multipartFormContent);
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception e)
        {
            throw new HttpRequestException();
        }
    }

    private async Task HandleError(Exception ex, WorkItem workItem)
    {
        if (ex is HttpRequestException) return;

        using (var repo = new Repo<QueuedItem>(_contextFactory))
        {
            var record = repo.GetAll().FirstOrDefault(i => i.Id == workItem.Id)!;
            record.IsFailed = true;
            record.TimeFinished = DateTime.UtcNow;
            repo.Update(record);
            await repo.SaveAsync();
        }
        
        if (!workItem.WithCallback) return;

        var error = new ErrorMessage(workItem.Id, "There was an internal error converting your track");
        
        using var content = new StringContent(JsonSerializer.Serialize(error));
        using var client = _httpClientFactory.CreateClient();
        try
        {
            var response = await client.PostAsync(workItem.CallbackUrl, content);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed sending error to callback url");
        }
    }
}