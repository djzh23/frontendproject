using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microcharts;
using ppm_fe.Constants;
using ppm_fe.Models;
using ppm_fe.Services;
using SkiaSharp;

namespace ppm_fe.ViewModels.Pages
{
    public partial class  StatisticsPageViewModel : BaseViewModel
    {
        public StatisticsPageViewModel(IConnectivityService connectivityService)
        {
            ConnectivityService = connectivityService;

            AllTeamCharts = new ObservableCollection<TeamChart>();
            FilteredTeamCharts = new ObservableCollection<TeamChart>();
            SelectedMonth = Properties.Months[DateTime.Now.Month - 1];

            CreateTeamCharts();
            FilterCharts();
        }

        #region properties
        [ObservableProperty]
        private Chart? _chart;

        [ObservableProperty]
        private Chart? _genderDistributionChart;

        [ObservableProperty]
        private string? _selectedMonth;
        partial void OnSelectedMonthChanged(string? value)
        {
            FilterCharts();
        }

        [ObservableProperty]
        private ObservableCollection<TeamChart>? _allTeamCharts;

        [ObservableProperty]
        private ObservableCollection<TeamChart>? _filteredTeamCharts;
        partial void OnFilteredTeamChartsChanged(ObservableCollection<TeamChart>? value)
        {
            UpdateChart();
        }

        [ObservableProperty]
        private int _totalMale;
        partial void OnTotalMaleChanged(int value)
        {
            OnPropertyChanged(nameof(Total));
        }

        [ObservableProperty]
        private int _totalFemale;
        partial void OnTotalFemaleChanged(int value)
        {
            OnPropertyChanged(nameof(Total));
        }
        public string Total => $"{TotalMale + TotalFemale}";

        public ObservableCollection<TeamChart>? TeamCharts { get; set; }
        #endregion

        #region tasks
        public double AverageChildrenPerTeamPerMonth => AllTeamCharts?.GroupBy(c => c.TeamName)
                              ?.Average(g => g.Average(c => c.Total)) ?? 0;

        public double AverageChildrenPerMonth => AllTeamCharts?
            .Where(c => c.Month.Year == DateTime.Now.Year && c.Month <= DateTime.Now)
            ?.GroupBy(c => c.Month.Month)
            ?.Average(g => g.Sum(c => c.Total)) ?? 0;

        private void CreateTeamCharts()
        {
            // This is just example data
            var teams = new[] { "FF1", "FF2", "FF3", "FF4", "FF5" };
            var random = new Random();

            for (int month = 1; month <= 12; month++)
            {
                foreach (var team in teams)
                {
                    var teamChart = new TeamChart(
                        teamName: team,
                        month: new DateTime(DateTime.Now.Year, month, 1),
                        male: random.Next(10, 50),
                        female: random.Next(10, 50)
                    );
                    AllTeamCharts?.Add(teamChart);
                }
            }
        }

        private void UpdateCharts()
        {
            UpdateTeamChart();
            UpdateGenderDistributionChart();
        }

        private void UpdateTeamChart()
        {
            var entries = FilteredTeamCharts?.Select(c => new ChartEntry(c.Total)
            {
                Label = c.TeamName,
                ValueLabel = c.Total.ToString(),
                Color = SKColor.Parse(GetRandomColor())
            }).ToArray();

            Chart = new BarChart { Entries = entries };
        }

        private void FilterCharts()
        {
            if(SelectedMonth != null)
            {
                int selectedMonthIndex = Properties.Months.IndexOf(SelectedMonth) + 1;
                FilteredTeamCharts = new ObservableCollection<TeamChart>(
                    AllTeamCharts?.Where(c => c.Month.Month == selectedMonthIndex) ?? Enumerable.Empty<TeamChart>()
                );
                UpdateTotals();
                UpdateCharts();
            }
        }

        private void UpdateTotals()
        {
            TotalMale = FilteredTeamCharts?.Sum(c => c.Male) ?? 0;
            TotalFemale = FilteredTeamCharts?.Sum(c => c.Female) ?? 0;
        }

        private void UpdateChart()
        {
            var entries = FilteredTeamCharts?.Select(c => new ChartEntry(c.Total)
            {
                Label = c.TeamName,

                ValueLabel = c.Total.ToString(),
                Color = SKColor.Parse(GetRandomColor())
            }).ToArray();

            Chart = new BarChart { Entries = entries };
        }

        private string GetRandomColor()
        {
            Random random = new Random();
            return $"#{random.Next(0x1000000):X6}";
        }

        private void UpdateGenderDistributionChart()
        {
            var entries = new[]
            {
                new ChartEntry(TotalMale)
                {
                    Label = "Jungen",
                    ValueLabel = TotalMale.ToString(),
                    Color = SKColor.Parse("#2c3e50")

                },
                new ChartEntry(TotalFemale)
                {
                    Label = "Mädels",
                    ValueLabel = TotalFemale.ToString(),
                    Color = SKColor.Parse("#e74c3c")
                }
            };

            GenderDistributionChart = new DonutChart { Entries = entries, LabelTextSize = 14f };
        }
        #endregion
    }
}
