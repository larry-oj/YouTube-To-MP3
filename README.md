
# DotNetConverter

A simple, easy to use youtube-to-mp3 converter. Utilises YouTubeExplode library and ffmpeg. Additionally configured for nginx proxy.
## Features

- Creates a .mp3 file from your YouTube link
- Queueing requests to be processed in order
- Supports parallel conversion of multiple links
- Can act as a WebHook
- Automatic cleanup of finished files

## Webhooking

If you provide a URL upon initial conversion request, API will send a POST request with multipart/form-data containing your mp3 to set url upon successful conversion


## API Reference

#### Queue video for conversion

```http
  POST /Converter/videos
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `url` | `string` | **Required**. YouTube video url. |
| `with_callback` | `bool` | Indicates weather you want API to act as a webhook. |
| `callback_url` | `string` | **Required if `with_callback` set to `true`.** Callback url. | 

Returns:
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `string` | Auto-generated ID of your request & file. |


#### Get item status

```http
  GET /Converter/videos/${id}/status
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `id`      | `string` | **Required**. ID of item to fetch. |

Returns:
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `string` | ID of your request & file. |
| `is_failed` | `bool` | Indicates wether conversion has failed. |
| `is_finished` | `bool` | Indicates wether conversion has been finished. |


#### Get mp3

```http
  GET /Converter/videos/${id}
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `id`      | `string` | **Required**. ID of item to fetch. |

Returns:
**.mp3 file**