# ImageDownloader API

A .NET Web API for downloading images from URLs and retrieving them as Base64 encoded strings.

## Features

- **POST** `/api/image/download-request-images` - Download multiple images with configurable concurrency
- **GET** `/api/image/get-image-by-name/{imageName}` - Retrieve downloaded images as Base64 strings
- Asynchronous downloads with SemaphoreSlim for concurrency control
- Unique filename generation using GUIDs
- Comprehensive error handling and logging
- Input validation and security measures

## Quick Start

1. **Run the application:**
   ```bash
   dotnet run
   ```

2. **Access Swagger UI:**
   Navigate to `https://localhost:7000/swagger` (or the port shown in the console)

## API Endpoints

### Download Images
**POST** `/api/image/download-request-images`

**Request Body:**
```json
{
    "imageUrls": [
      "https://images.pexels.com/photos/546819/pexels-photo-546819.jpeg",
      "https://images.pexels.com/photos/577585/pexels-photo-577585.jpeg",
      "https://images.pexels.com/photos/1089438/pexels-photo-1089438.jpeg",
      "https://images.pexels.com/photos/2653362/pexels-photo-2653362.jpeg",
      "https://images.pexels.com/photos/965345/pexels-photo-965345.jpeg",
      "https://images.pexels.com/photos/276452/pexels-photo-276452.jpeg",
      "https://images.pexels.com/photos/2004161/pexels-photo-2004161.jpeg",
      "https://images.pexels.com/photos/270404/pexels-photo-270404.jpeg",
      "https://images.pexels.com/photos/207580/pexels-photo-207580.jpeg",
      "https://images.pexels.com/photos/225250/pexels-photo-225250.jpeg"
    ],
    "maxDownloadAtOnce": 3
}
```

**Response:**
```json
{
  "success": true,
  "message": "Successfully downloaded 2 images",
  "urlAndNames": {
    "https://images.pexels.com/photos/276452/pexels-photo-276452.jpeg": "a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
    "https://images.pexels.com/photos/225250/pexels-photo-225250.jpeg": "b2c3d4e5-f6g7-8901-bcde-f12345678901.jpg"
  }
}
```

### Get Image by Name
**GET** `/api/image/get-image-by-name/{imageName}`

**Response (200 OK):**
```json
{
  "imageName": "a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
  "base64Data": "/9j/4AAQSkZJRgABAQAAAQABAAD...",
  "message": "Image retrieved successfully."
}
```

**Response (404 Not Found):**
```json
{
  "message": "Image 'nonexistent.jpg' not found."
}
```

## Configuration

- **Image Storage:** Images are saved to `wwwroot/images/downloaded/`
- **HttpClient Timeout:** 5 minutes
- **Supported Image Types:** JPEG, PNG, GIF, WebP, BMP
- **Max Concurrent Downloads:** Configurable per request (1-50)

## Testing with Postman

1. Set the `baseUrl` variable to your API's base URL (default: `https://localhost:7000`)
2. Run the "Download Images" request first
3. The collection will automatically save the first downloaded image name
4. Run the "Get Image by Name" request to retrieve the downloaded image

## Project Structure

```
ImageDownloader/
├── Controllers/
│   └── ImageController.cs
├── Models/
│   ├── RequestDownload.cs
│   └── ResponseDownload.cs
├── Services/
│   ├── IImageDownloadService.cs
│   └── ImageDownloadService.cs
├── wwwroot/
│   └── images/
│       └── downloaded/
└── Program.cs
```

## Error Handling

The API includes comprehensive error handling:
- Invalid URLs are logged and skipped
- Network timeouts are handled
- Invalid image content types are rejected
- Directory traversal attacks are prevented
- Partial failures return detailed error information