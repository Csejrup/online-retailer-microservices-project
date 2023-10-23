namespace SharedModels.Messages;

public class CustomerValidationResponse
{
    public int CustomerId { get; set; }
    public bool Exists { get; set; }
}
