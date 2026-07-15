namespace CoursesApp.Domain.Interfaces.Repositories;

public record GroupCapacityInfo(int? MaxCapacity, int Count)
{
    public int Remaining => MaxCapacity is null ? int.MaxValue : Math.Max(0, MaxCapacity.Value - Count);
    public bool IsFull => Remaining <= 0;
}