using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {

            var courseID = from c in db.Courses
                           where c.Department == subject
                           && c.Number == num
                           select c.CatalogId;
            if (courseID.Count() == 0)
            {
                return Json(null);
            }
            uint cid = courseID.Single();

            var classID = from c in db.Classes
                          where c.Season == season
                          && c.Year == year
                          && c.Listing == cid
                          select c.ClassId;
            if (classID.Count() == 0)
            {
                return Json(null);
            }
            uint classid = classID.Single();

            var students = from e in db.Enrolleds
                           where e.Class == classid
                           select new
                           {
                               fname = e.StudentNavigation.FName,
                               lname = e.StudentNavigation.LName,
                               uid = e.StudentNavigation.UId,
                               dob = e.StudentNavigation.Dob,
                               grade = e.Grade,
                           };
            return Json(students.ToArray());

        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {

            var courseID = from c in db.Courses
                           where c.Department == subject
                           && c.Number == num
                           select c.CatalogId;
            if (courseID.Count() == 0)
            {
                return Json(null);
            }
            uint cid = courseID.Single();

            var classID = from c in db.Classes
                          where c.Season == season
                          && c.Year == year
                          && c.Listing == cid
                          select c.ClassId;
            if (classID.Count() == 0)
            {
                return Json(null);
            }
            uint classid = classID.Single();
            

            if (category == null)
            {

                var assignmentCatID = from ac in db.AssignmentCategories
                                      where ac.InClass == classid
                                      select ac.CategoryId;
                List<uint> assignCatArray = assignmentCatID.ToList();
                var assign = db.Assignments.Where(row => assignCatArray.Contains(row.Category))
                    .Select(row => new
                    {
                        aname = row.Name,
                        cname = row.Category,
                        due = row.Due,
                        submissions = row.Submissions,
                    })
                    .ToList();
                return Json(assign.ToArray());
            }
            else
            {

                var assignmentCatID = from ac in db.AssignmentCategories
                                      where ac.Name == category
                                      && ac.InClass == classid
                                      select ac.CategoryId;
                if (classID.Count() == 0)
                {
                    return Json(null);
                }
                uint assignCatID = assignmentCatID.Single();

                var assignments = from a in db.Assignments
                                  where a.Category == assignCatID
                                  select new
                                  {
                                      aname = a.Name,
                                      cname = a.Category,
                                      due = a.Due,
                                      submissions = a.Submissions,
                                  };
                return Json(assignments.ToArray());

            }
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            /*
             * subject - Course
             * num - Course
             * season - class
             * year - class
             * 
             * get the course
             * get the class
             * 
             * get assignemnt category with the class
             */
            var courseID = from c in db.Courses
                           where c.Department == subject
                           && c.Number == num
                           select c.CatalogId;
            if (courseID.Count() == 0)
            {
                return Json(null);
            }
            uint cid = courseID.Single();

            var classID = from c in db.Classes
                          where c.Season == season
                          && c.Year == year
                          && c.Listing == cid
                          select c.ClassId;
            if (classID.Count() == 0)
            {
                return Json(null);
            }
            uint classid = classID.Single();

            var assignmentCat = from ac in db.AssignmentCategories
                                where ac.InClass == classid
                                select new
                                {
                                    name = ac.Name,
                                    weight = ac.Weight
                                };


            return Json(assignmentCat.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            /*
             * create assignment category unless it already exists
             * 
             * name - assignement cat
             * weight - assignemtn cat
             * classid - assignment cat
             * 
             * subject - course
             * num - course
             * 
             * season - class
             * year - class
             */
            /*
             * get class with specified info
             * 
             * make assignement categority pointing to that class
             */
            var courseID = from c in db.Courses
                         where c.Department == subject
                         && c.Number == num
                         select c.CatalogId;
            if (courseID.Count() == 0)
            {
                return Json(new { success = false });
            }
            uint cid = courseID.Single();

            var classID = from c in db.Classes
                          where c.Season == season
                          && c.Year == year
                          && c.Listing == cid
                          select c.ClassId;
            if (classID.Count() == 0)
            {
                return Json(new { success = false });
            }
            uint classid = classID.Single();


            AssignmentCategory ac = new AssignmentCategory
            {
                Name = category,
                Weight = (uint)catweight,
                InClass = classid,
            };
            try
            {
                db.AssignmentCategories.Add(ac);
                db.SaveChanges();
                return Json(new { success = true });
            } 
            catch (Exception e)
            {
                return Json(new { success = false });
            }
    
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            
            var courseID = from c in db.Courses
                           where c.Department == subject
                           && c.Number == num
                           select c.CatalogId;
            if (courseID.Count() == 0)
            {
                return Json(new { success = false });
            }
            uint cid = courseID.Single();

            var classID = from c in db.Classes
                          where c.Season == season
                          && c.Year == year
                          && c.Listing == cid
                          select c.ClassId;
            if (classID.Count() == 0)
            {
                return Json(new { success = false });
            }
            uint classid = classID.Single();

            var assignmentCatID = from ac in db.AssignmentCategories
                                where ac.InClass == classid
                                && ac.Name == category
                                select ac.CategoryId;
            if (assignmentCatID.Count() == 0)
            {
                return Json(new { success = false });
            }
            uint assignCatID = assignmentCatID.Single();

            Assignment assignment = new Assignment
            {
                Name = asgname,
                Contents = asgcontents,
                Due = asgdue,
                MaxPoints = (uint)asgpoints,
                Category = assignCatID,
            };
            try
            {
                db.Assignments.Add(assignment);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }


            
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            return Json(null);
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            return Json(new { success = false });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            /*
             * subject - courses
             * number - courses
             * name - courses
             * season - class
             * year - class
             * 
             */
            var query = from c in db.Classes
                        where c.TaughtBy == uid
                        select new
                        {
                            subject = c.ListingNavigation.Department,
                            number = c.ListingNavigation.Number,
                            name = c.ListingNavigation.Name,
                            season = c.Season,
                            year = c.Year,
                        };
            return Json(query.ToArray());
        }


        
        /*******End code to modify********/
    }
}

