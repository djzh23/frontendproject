namespace ppm_fe.Models;

public class TeamChart
{
    public string TeamName { get; set; }
    public DateTime Month { get; set; }
    public int Male { get; set; }
    public int Female { get; set; }
    public int Total => Male + Female;

    public TeamChart(string teamName, DateTime month, int male, int female)
    {
        TeamName = teamName;
        Month = month;
        Male = male;
        Female = female;
    }

    public override string ToString()
    {
        return $"{TeamName} - {Month:MMM yyyy}: Male={Male}, Female={Female}, Total={Total}";
    }
}