using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SIMS_SE06205.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SIMS_SE06205.Controllers
{
    public class StudentController : Controller
    {
        private readonly string filePathStudent = @"E:\APDP-BTec-main (1)\data-sims\data-student.json";
        private readonly string filePathCourse = @"E:\APDP-BTec-main (1)\data-sims\data-courses.json";
        private readonly string filePathClass = @"E:\APDP-BTec-main (1)\data-sims\class.json";
        private readonly ILogger<StudentController> _logger;
        private const int PageSize = 5;

        public StudentController(ILogger<StudentController> logger)
        {
            _logger = logger;
        }

        // Check if the user is logged in
        private bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetString("SessionUserId") != null;
        }

        // Redirect to login if user is not logged in
        private IActionResult RedirectIfNotLoggedIn()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction(nameof(Index), "Login");
            }
            return null;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var redirectResult = RedirectIfNotLoggedIn();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            try
            {
                // Load student data
                string dataJson = System.IO.File.ReadAllText(filePathStudent);
                var students = JsonConvert.DeserializeObject<List<StudentViewModel>>(dataJson) ?? new List<StudentViewModel>();

                // Load course data
                string courseJson = System.IO.File.ReadAllText(filePathCourse);
                var courses = JsonConvert.DeserializeObject<List<CourseViewModel>>(courseJson) ?? new List<CourseViewModel>();

                // Update students with invalid major
                foreach (var student in students)
                {
                    if (courses.All(c => c.NameCourse != student.Major))
                    {
                        student.Major = "Unknown";
                    }
                }

                // Save updated student data
                var updatedStudentJson = JsonConvert.SerializeObject(students, Formatting.Indented);
                System.IO.File.WriteAllText(filePathStudent, updatedStudentJson);

                // Pass data to View
                var studentModel = new StudentModel
                {
                    StudentLists = students
                };

                return View(studentModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading student data.");
                TempData["ErrorMessage"] = "An error occurred while loading the student data.";
                return View(new StudentModel());
            }
        }

        [HttpGet]
        public IActionResult Add()
        {
            var redirectResult = RedirectIfNotLoggedIn();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            ViewBag.CourseList = GetCourseList();
            ViewBag.ClassList = GetClassList();
            return View(new StudentViewModel());
        }

        [HttpPost]
        public IActionResult Add(StudentViewModel studentViewModel)
        {
            var redirectResult = RedirectIfNotLoggedIn();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string dataJson = System.IO.File.ReadAllText(filePathStudent);
                    var students = JsonConvert.DeserializeObject<List<StudentViewModel>>(dataJson) ?? new List<StudentViewModel>();

                    int maxId = students.Any() ? int.Parse(students.Max(s => s.Id)) + 1 : 1;
                    string idIncrement = maxId.ToString();

                    students.Add(new StudentViewModel
                    {
                        Id = idIncrement,
                        Code = studentViewModel.Code,
                        NameStudent = studentViewModel.NameStudent,
                        Major = studentViewModel.Major,
                        Gender = studentViewModel.Gender,
                        Adress = studentViewModel.Adress,
                        Class = studentViewModel.Class
                    });

                    string dtJson = JsonConvert.SerializeObject(students, Formatting.Indented);
                    System.IO.File.WriteAllText(filePathStudent, dtJson);
                    TempData["SaveStatus"] = "Success";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while saving student data.");
                    TempData["SaveStatus"] = "Failed";
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CourseList = GetCourseList();
            ViewBag.ClassList = GetClassList();
            return View(studentViewModel);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var redirectResult = RedirectIfNotLoggedIn();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            try
            {
                // Load student data
                string dataJson = System.IO.File.ReadAllText(filePathStudent);
                var students = JsonConvert.DeserializeObject<List<StudentViewModel>>(dataJson) ?? new List<StudentViewModel>();

                // Find student to delete
                var studentToDelete = students.FirstOrDefault(s => s.Id == id.ToString());
                if (studentToDelete != null)
                {
                    // Delete student
                    students.Remove(studentToDelete);
                    string updatedStudentJson = JsonConvert.SerializeObject(students, Formatting.Indented);
                    System.IO.File.WriteAllText(filePathStudent, updatedStudentJson);

                    TempData["DeleteStatus"] = "Success";
                }
                else
                {
                    TempData["DeleteStatus"] = "Failed";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting student data.");
                TempData["DeleteStatus"] = "Failed";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            var redirectResult = RedirectIfNotLoggedIn();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            try
            {
                string dataJson = System.IO.File.ReadAllText(filePathStudent);
                var students = JsonConvert.DeserializeObject<List<StudentViewModel>>(dataJson) ?? new List<StudentViewModel>();
                var itemStudent = students.FirstOrDefault(item => item.Id == id.ToString());

                if (itemStudent != null)
                {
                    ViewBag.CourseList = GetCourseList();
                    ViewBag.ClassList = GetClassList();

                    var studentModel = new StudentViewModel
                    {
                        Id = itemStudent.Id,
                        Code = itemStudent.Code,
                        NameStudent = itemStudent.NameStudent,
                        Major = itemStudent.Major,
                        Gender = itemStudent.Gender,
                        Adress = itemStudent.Adress,
                        Class = itemStudent.Class
                    };

                    return View(studentModel);
                }

                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading student data for update.");
                TempData["ErrorMessage"] = "An error occurred while loading the student data.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public IActionResult Update(StudentViewModel studentModel)
        {
            var redirectResult = RedirectIfNotLoggedIn();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CourseList = GetCourseList();
                ViewBag.ClassList = GetClassList();
                return View(studentModel);
            }

            try
            {
                string dataJson = System.IO.File.ReadAllText(filePathStudent);
                var students = JsonConvert.DeserializeObject<List<StudentViewModel>>(dataJson) ?? new List<StudentViewModel>();
                var itemStudent = students.FirstOrDefault(item => item.Id == studentModel.Id.ToString());

                if (itemStudent != null)
                {
                    itemStudent.Code = studentModel.Code;
                    itemStudent.NameStudent = studentModel.NameStudent;
                    itemStudent.Major = studentModel.Major;
                    itemStudent.Gender = studentModel.Gender;
                    itemStudent.Adress = studentModel.Adress;
                    itemStudent.Class = studentModel.Class;

                    string updateJson = JsonConvert.SerializeObject(students, Formatting.Indented);
                    System.IO.File.WriteAllText(filePathStudent, updateJson);
                    TempData["UpdateStatus"] = "Success";
                }
                else
                {
                    TempData["UpdateStatus"] = "Failed";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating student data.");
                TempData["UpdateStatus"] = "Failed";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            var redirectResult = RedirectIfNotLoggedIn();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (string.IsNullOrEmpty(searchTerm))
            {
                TempData["ErrorMessage"] = "Please enter a search term.";
                return RedirectToAction(nameof(Index));
            }

            string dataJson = System.IO.File.ReadAllText(filePathStudent);
            StudentModel stuModel = new StudentModel
            {
                StudentLists = new List<StudentViewModel>()
            };

            var student = JsonConvert.DeserializeObject<List<StudentViewModel>>(dataJson);
            if (student != null)
            {
                stuModel.StudentLists = student.Where(s => s.NameStudent.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View(stuModel);
        }

        private IEnumerable<SelectListItem> GetCourseList()
        {
            string courseJson = System.IO.File.ReadAllText(filePathCourse);
            var courses = JsonConvert.DeserializeObject<List<CourseViewModel>>(courseJson) ?? new List<CourseViewModel>();
            return courses.Select(c => new SelectListItem { Value = c.NameCourse, Text = c.NameCourse });
        }

        private IEnumerable<SelectListItem> GetClassList()
        {
            string classJson = System.IO.File.ReadAllText(filePathClass);
            var classes = JsonConvert.DeserializeObject<List<ClassViewModel>>(classJson) ?? new List<ClassViewModel>();
            return classes.Select(c => new SelectListItem { Value = c.ClassName, Text = c.ClassName });
        }
    }
}
