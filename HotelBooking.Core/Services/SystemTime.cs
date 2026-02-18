using System;

public class SystemTime
{
    private DateTime? _date;

    public void Set(DateTime customDate) => _date = customDate;
    public void Reset() => _date = null;

    public DateTime Now => _date ?? DateTime.Now;
    public DateTime Today => Now.Date;
}
