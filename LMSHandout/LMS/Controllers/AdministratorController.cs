﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            // Check to see if department exists
            var check = from dept in db.Departments
                        where dept.Subject == subject
                        select dept;

            // If it does return false
            if(check.Count() > 0)
            {
                return Json(new { success = false });
            }

            // else it doesnt' exits and we added it in and return ture
            Department newDept = new Department();
            newDept.Subject = subject;
            newDept.Name = name;
            db.Departments.Add(newDept);

            try
            {
                db.SaveChanges();
            }
            catch (Exception e) {
                // If it fails do we return false?
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var allCourses = from course in db.Courses
                             where course.Department == subject
                             select new
                             {
                                 number = course.Number,
                                 name = course.Name
                             };

            return Json(allCourses.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var getProf = from profs in db.Professors
                          where profs.WorksIn == subject
                          select new
                          {
                              lname = profs.LName,
                              fname = profs.FName,
                              uid = profs.UId
                          };
            
            return Json(getProf.ToArray());
            
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            // Checking to see if Course exist in database
            var check = from course in db.Courses
                        where course.Number == number
                        && course.Department == subject
                        select course;

            // if it does just return false
            if(check.Count() > 0) 
            {
                return Json(new { success = false });
            }

            // else put into courses table and return true
            Course newCourse = new Course();
            newCourse.Number = (uint)number;
            newCourse.Department = subject;
            newCourse.Name = name;

            db.Courses.Add(newCourse);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e) 
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            // Unique key Season, Year and Listing(Maps to CourseId, maps to subject and number)

            // Find vaild course ID
            var courseID = from cID in db.Courses
                           where cID.Number == number &&
                           cID.Department == subject
                           select cID.CatalogId;

            // If course ID does not exit return false
            if(courseID.Count() == 0)
            {
                return Json(new { success = false });
            }

            // If course ID does exist check to make sure its not being offered twice
            // Check for same location and same start-end time
            var checkLocAndTime = from classes in db.Classes
                                  where classes.Location == location
                                  && ((classes.StartTime <= TimeOnly.FromDateTime(start)
                                  && TimeOnly.FromDateTime(start) <= classes.EndTime) ||
                                  (classes.StartTime <= TimeOnly.FromDateTime(end) 
                                  && TimeOnly.FromDateTime(end) <= classes.EndTime))
                                  select classes;
            
            if(checkLocAndTime.Count() != 0)
            {
                return Json(new { success = false });
            }

            // Check Same Course same semester
            var checkIfClasssExist = from classes in db.Classes
                                where classes.Listing == courseID.ElementAtOrDefault(0)
                                && classes.Year == year
                                && classes.Season == season
                                select classes;

            if(checkIfClasssExist.Count() != 0)
            {
                return Json(new { success = false });
            }

            // Valid to be put into Classes table
            Class newClass = new Class();
            newClass.Season = season;
            newClass.Year = (uint)year;
            newClass.Location = location;
            newClass.StartTime = TimeOnly.FromDateTime(start);
            newClass.EndTime = TimeOnly.FromDateTime(end);
            newClass.Listing = courseID.ElementAtOrDefault(0);
            newClass.TaughtBy = instructor;

            db.Classes.Add(newClass);

            try
            {
                db.SaveChanges();
            }
            catch(Exception e)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true});
        }


        /*******End code to modify********/

    }
}

