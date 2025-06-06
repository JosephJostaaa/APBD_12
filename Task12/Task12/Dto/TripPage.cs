namespace Task12.Dto;

public class TripPage
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int AllPages { get; set; }
    public List<TripDto> Trips { get; set; }
}