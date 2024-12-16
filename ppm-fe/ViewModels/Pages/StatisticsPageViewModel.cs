using ppm_fe.Models;
using ppm_fe.Services;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace ppm_fe.ViewModels.Pages
{
    public partial class  StatisticsPageViewModel : BaseViewModel
    {
        private Chart _chart;
        public Chart Chart
        {
            get => _chart;
            set
            {
                _chart = value;
                OnPropertyChanged();
            }
        }


        private Chart _genderDistributionChart;
        public Chart GenderDistributionChart
        {
            get => _genderDistributionChart;
            set
            {
                _genderDistributionChart = value;
                OnPropertyChanged();
            }
        }


        private string _selectedMonth;
        public string SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (_selectedMonth != value)
                {
                    _selectedMonth = value;
                    FilterCharts();
                    OnPropertyChanged(nameof(SelectedMonth));
                }
            }
        }


        private int _totalMale;
        public int TotalMale
        {
            get => _totalMale;
            set
            {
                _totalMale = value;
                OnPropertyChanged(nameof(TotalMale));
                OnPropertyChanged(nameof(Total));
            }
        }


        private int _totalFemale;
        public int TotalFemale
        {
            get => _totalFemale;
            set
            {
                _totalFemale = value;
                OnPropertyChanged(nameof(TotalFemale));
                OnPropertyChanged(nameof(Total));
            }
        }

        public string Total => $"{TotalMale + TotalFemale}";

        private ObservableCollection<TeamChart> _allTeamCharts;

        private ObservableCollection<TeamChart> _filteredTeamCharts;
        
        public ObservableCollection<TeamChart> TeamCharts { get; set; }

        public ObservableCollection<TeamChart> FilteredTeamCharts
        {
            get => _filteredTeamCharts;
            set
            {
                _filteredTeamCharts = value;
                OnPropertyChanged(nameof(FilteredTeamCharts));
                UpdateChart();
            }
        }

        public List<string> Months { get; } = new List<string>
        {
            "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember"
        };

        public double AverageChildrenPerTeamPerMonth => _allTeamCharts
            .GroupBy(c => c.TeamName)
            .Average(g => g.Average(c => c.Total));

        public double AverageChildrenPerMonth => _allTeamCharts
            .Where(c => c.Month.Year == DateTime.Now.Year && c.Month <= DateTime.Now)
            .GroupBy(c => c.Month.Month)
            .Average(g => g.Sum(c => c.Total));

        public StatisticsPageViewModel(IConnectivityService connectivityService)
        {
            ConnectivityService = connectivityService;

            _allTeamCharts = new ObservableCollection<TeamChart>();
            FilteredTeamCharts = new ObservableCollection<TeamChart>();
            SelectedMonth = Months[DateTime.Now.Month - 1];
            CreateTeamCharts();
            FilterCharts();
        }

        private void CreateTeamCharts()
        {
            // This is just example data. Replace with your actual data source.
            var teams = new[] { "FF1", "FF2", "FF3", "FF4", "FF5", "FF6" };
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
                    _allTeamCharts.Add(teamChart);
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
            var entries = FilteredTeamCharts.Select(c => new ChartEntry(c.Total)
            {
                Label = c.TeamName,
                ValueLabel = c.Total.ToString(),
                Color = SKColor.Parse(GetRandomColor())
            }).ToArray();

            Chart = new BarChart { Entries = entries };
        }

        private void FilterCharts()
        {
            int selectedMonthIndex = Months.IndexOf(SelectedMonth) + 1;
            FilteredTeamCharts = new ObservableCollection<TeamChart>(
                _allTeamCharts.Where(c => c.Month.Month == selectedMonthIndex)
            );
            UpdateTotals();
            UpdateCharts();
        }

        private void UpdateTotals()
        {
            TotalMale = FilteredTeamCharts.Sum(c => c.Male);
            TotalFemale = FilteredTeamCharts.Sum(c => c.Female);
        }

        private void UpdateChart()
        {
            var entries = FilteredTeamCharts.Select(c => new ChartEntry(c.Total)
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
    }
}
