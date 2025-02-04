using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Email
{
    [BsonId]
    public int _ID { get; set; }
    public int EmpId { get; set; }
    public string Subject { get; set; }
    public string emailrecive { get; set; }
    public string Body { get; set; }
    public DateTime SentDate { get; set; }
}

public class Notification
{
    [BsonId]
    public int ID { get; set; }
    public int EmpId { get; set; }
    public string Title { get; set; }
    public string emailrecive { get; set; }
    public string Message { get; set; }
    public DateTime CreatedDate { get; set; }
}
public class Counter
{
    public string Id { get; set; }
    public int SequenceValue { get; set; }
}