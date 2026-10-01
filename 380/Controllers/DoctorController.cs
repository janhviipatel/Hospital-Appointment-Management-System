using _380.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace _380.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;

        public DoctorController(
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _httpClient = httpClientFactory.CreateClient();
        }

        // =====================================================
        // 1. DOCTOR LIST
        // =====================================================
        public IActionResult Index()
        {
            if (!_cache.TryGetValue(
                "DoctorList",
                out List<Doctor>? doctors))
            {
                doctors = new List<Doctor>
                {
                    new Doctor
                    {
                        DoctorId = 1,
                        DoctorName = "Dr. Raj Patel",
                        Specialization = "Cardiologist",
                        Experience = 10,
                        ConsultationFee = 500
                    },

                    new Doctor
                    {
                        DoctorId = 2,
                        DoctorName = "Dr. Priya Shah",
                        Specialization = "Dermatologist",
                        Experience = 7,
                        ConsultationFee = 400
                    },

                    new Doctor
                    {
                        DoctorId = 3,
                        DoctorName = "Dr. Amit Mehta",
                        Specialization = "Neurologist",
                        Experience = 12,
                        ConsultationFee = 800
                    }
                };

                // Cache Doctor List for 3 minutes
                _cache.Set(
                    "DoctorList",
                    doctors,
                    TimeSpan.FromMinutes(3));
            }

            return View(doctors);
        }


        // =====================================================
        // 2. DOCTOR DETAILS
        // =====================================================
        [ResponseCache(Duration = 60)]
        public IActionResult Details(int id)
        {
            if (!_cache.TryGetValue(
                "DoctorList",
                out List<Doctor>? doctors)
                || doctors == null)
            {
                return RedirectToAction("Index");
            }

            Doctor? doctor =
                doctors.FirstOrDefault(
                    d => d.DoctorId == id);

            if (doctor == null)
            {
                return NotFound();
            }

            // Store selected specialization in Session
            HttpContext.Session.SetString(
                "SelectedSpecialization",
                doctor.Specialization);

            return View(doctor);
        }


        // =====================================================
        // 3. APPOINTMENT FORM
        // =====================================================
        public IActionResult Appointment(int id)
        {
            ViewBag.DoctorId = id;

            return View();
        }


        // =====================================================
        // 4. APPOINTMENT SUBMISSION
        // =====================================================
        [HttpPost]
        public IActionResult Appointment(
            int doctorId,
            string patientName)
        {
            // Store patient name in Cookie
            Response.Cookies.Append(
                "PatientName",
                patientName);

            if (_cache.TryGetValue(
                "DoctorList",
                out List<Doctor>? doctors)
                && doctors != null)
            {
                Doctor? doctor =
                    doctors.FirstOrDefault(
                        d => d.DoctorId == doctorId);

                if (doctor != null)
                {
                    // Store specialization in Session
                    HttpContext.Session.SetString(
                        "SelectedSpecialization",
                        doctor.Specialization);
                }
            }

            return RedirectToAction(
                "Details",
                new { id = doctorId });
        }


        // =====================================================
        // 5. ASYNCHRONOUS API REQUEST
        // =====================================================
        public async Task<IActionResult> Posts()
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync(
                    "https://jsonplaceholder.typicode.com/posts");

            // Check API response
            if (!response.IsSuccessStatusCode)
            {
                return Content(
                    "Unable to retrieve posts.");
            }

            // Read JSON response
            string json =
                await response.Content.ReadAsStringAsync();

            // Convert JSON into C# objects
            List<Post>? posts =
                JsonSerializer.Deserialize<List<Post>>(
                    json,
                    new JsonSerializerOptions
                    {
                        // Important fix
                        PropertyNameCaseInsensitive = true
                    });

            return View(
                posts ?? new List<Post>());
        }
    }


    // =========================================================
    // POST MODEL
    // =========================================================
    public class Post
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;
    }
}