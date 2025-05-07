namespace Demo.WebApi.Application.Common.FCMNotification;

public class FCMNotificationRequest
{
    public bool IsSilent { get; set; } = false;
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Url { get; set; }
    public string? IconLink { get; set; }
    public FCMImage? Image { get; set; }
    public List<FCMToken>? Tokens { get; set; }
    public bool IsBatch { get; set; }
    public Dictionary<string, string>? Data { get; set; }
    public FCMToken? Token { get; set; }
    public string? Topic { get; set; }
    public int Code { get; set; }
    public bool IsCustom { get; set; }
    public DateTime? ScheduleAt { get; set; }
}

public class FCMToken
{
    public string? Value { get; set; }
    public string? UserId { get; set; }
}

public class FCMImage
{
    public int? Id { get; set; }
    public string? Url { get; set; }
}
