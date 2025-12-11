using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using SensoreApp.Data;
using SensoreApp.Models;
using System.Threading.Tasks;

namespace SensoreApp.Controllers
{
    // --- Data Model for Historical Trend Points ---
    public class HistoricalDataPoint
    {
        public DateTime Timestamp { get; set; }
        public int PeakPressureIndex { get; set; }
        public double ContactAreaPercentage { get; set; }
    }

    // --- View Model to Pass Data to the Dashboard View ---
    public class DashboardViewModel
    {
        public string PatientName { get; set; } = string.Empty;
        public string AlertStatus { get; set; } = "OK";
        // Holds the 32x32 pressure matrix as a JSON string
        public string PressureDataJson { get; set; } = string.Empty;
        // Holds the time-series historical data as a JSON string
        public string HistoricalDataJson { get; set; } = string.Empty;
        public string FeedbackMessage { get; set; } = string.Empty;
        public string FeedbackError { get; set; } = string.Empty;
    }
    // ----------------------------------------
    
    public class PatientController : Controller
    {
        private readonly AppDbContext _context; 
        private const int CurrentPatientId = 1; // Simulates the logged-in patient

        public PatientController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Patient/Dashboard (Home/Heatmap view)
        [HttpGet]
        public IActionResult Dashboard()
        {
            var viewModel = LoadDashboardData();
            return View(viewModel);
        }

        // POST: /Patient/SubmitFeedback
        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(string userComment)
        {
            // We use LoadDashboardData() to ensure the heatmap and graphs are re-rendered
            var viewModel = LoadDashboardData(); 

            if (string.IsNullOrWhiteSpace(userComment))
            {
                viewModel.FeedbackError = "Please enter a comment before submitting.";
                return View("Dashboard", viewModel);
            }

            var feedback = new UserFeedback
            {
                UserId = CurrentPatientId, 
                Timestamp = DateTime.UtcNow, 
                CommentText = userComment,
                IsReviewed = false
            };

            _context.UserFeedback.Add(feedback);
            await _context.SaveChangesAsync();
            
            viewModel.FeedbackMessage = "Thank you! Your feedback has been successfully logged for clinician review.";
            
            // Reload the view with the success message
            return View("Dashboard", LoadDashboardData());
        }

        // GET: /Patient/Appointments
        [HttpGet]
        public IActionResult Appointments()
        {
            ViewData["Title"] = "Appointments";
            return View(); 
        }

        // GET: /Patient/Alerts
        [HttpGet]
        public IActionResult Alerts()
        {
            ViewData["Title"] = "Alerts History";
            return View(); 
        }

        // GET: /Patient/Feedback
        [HttpGet]
        public IActionResult Feedback()
        {
            ViewData["Title"] = "My Feedback History";
            return View(); 
        }


        // --- Data Generation and Loading Methods ---

        private DashboardViewModel LoadDashboardData()
        {
            var viewModel = new DashboardViewModel
            {
                PatientName = $"Patient ID {CurrentPatientId}",
            };
            
            // 1. Current Pressure Data (32x32 Matrix)
            int[][] pressureMatrix = GenerateMockPressureData();
            viewModel.PressureDataJson = JsonSerializer.Serialize(pressureMatrix); 

            // 2. Historical Trend Data
            List<HistoricalDataPoint> historicalData = GenerateMockHistoricalData(TimeSpan.FromHours(24));
            viewModel.HistoricalDataJson = JsonSerializer.Serialize(historicalData);

            return viewModel;
        }

        // Generates mock 32x32 data as a jagged array (int[][])
        private int[][] GenerateMockPressureData()
        {
            int size = 32;
            var random = new Random();
            int[][] matrix = new int[size][];

            for (int i = 0; i < size; i++)
            {
                matrix[i] = new int[size]; 
                for (int j = 0; j < size; j++)
                {
                    matrix[i][j] = random.Next(1, 256);
                }
            }
            
            // Simulate a hot spot
            for (int i = 18; i < 24; i++)
            {
                for (int j = 20; j < 26; j++)
                {
                    matrix[i][j] = random.Next(200, 256); 
                }
            }

            return matrix;
        }

        private List<HistoricalDataPoint> GenerateMockHistoricalData(TimeSpan duration)
        {
            var data = new List<HistoricalDataPoint>();
            var now = DateTime.Now;
            var random = new Random();
            int points = 50; 

            for (int i = 0; i < points; i++)
            {
                var timestamp = now.Subtract(duration.Divide(points) * (points - i));
                int ppi = random.Next(150, 210); 
                double contactArea = random.Next(700, 800) / 10.0; 

                data.Add(new HistoricalDataPoint
                {
                    Timestamp = timestamp,
                    PeakPressureIndex = ppi,
                    ContactAreaPercentage = contactArea
                });
            }
            return data;
        }
    }
}