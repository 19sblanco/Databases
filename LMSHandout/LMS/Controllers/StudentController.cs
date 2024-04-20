using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var myClasses = from enroll in db.Enrolleds
                            where enroll.Student == uid
                            select new
                            {
                                subject = enroll.ClassNavigation.ListingNavigation.Department,
                                number = enroll.ClassNavigation.ListingNavigation.Number,
                                name = enroll.ClassNavigation.ListingNavigation.Name,
                                season = enroll.ClassNavigation.Season,
                                year = enroll.ClassNavigation.Year,
                                grade = enroll.Grade
                            };

            return Json(myClasses.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
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

            var assignmentCatID = from ac in db.AssignmentCategories
                                  where ac.InClass == classid
                                  select ac.CategoryId;
            List<uint> assignCatArray = assignmentCatID.ToList();
            var assign = db.Assignments.Where(row => assignCatArray.Contains(row.Category))
                .Select(row => row);

            //join s in db.Submissions on new { A = q.AssignmentId, B = uid } equals new { A = s.Assignment, B = s.Student

            var result = from a in assign
                         join s in db.Submissions on new { A = a.AssignmentId, B = uid} equals new {A = s.Assignment, B = s.Student} 
                         into temp //a.AssignmentId equals s.Assignment into temp
                         from s in temp.DefaultIfEmpty()
                         select new
                         {
                             aname = a.Name,
                             cname = a.CategoryNavigation.Name,
                             due = a.Due,
                             score = s == null ? null : (uint?)s.Score,
                         };




            return Json(result.ToArray());

        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
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
                                  where ac.InClass == classid && ac.Name == category
                                  select ac.CategoryId;
            if(assignmentCatID.Count() == 0)
            {
                return Json(new { success = false });
            }
            uint assignCID = assignmentCatID.Single();

            var assignID = from aid in db.Assignments
                           where aid.Name == asgname && aid.Category == assignCID
                           select aid.AssignmentId;
            if (assignID.Count() == 0)
            {
                return Json(new { success = false });
            }
            uint aID = assignID.Single();

            var submission = from sub in db.Submissions
                             where sub.Assignment == aID && sub.Student == uid
                             select sub;

            if(submission.Count() != 1)
            {
                //First submission
                // AssignmentID, uid, score, SubmissionContents, Time
                Submission firstSub = new Submission
                {
                    Assignment = aID,
                    Student = uid,
                    Score = 0,
                    SubmissionContents = contents,
                    Time = DateTime.Now
                };
                db.Submissions.Add(firstSub);
                try
                {
                    db.SaveChanges();
                }
                catch(Exception e)
                {
                    return Json(new { success = false });
                }
                return Json(new { success = true });
            }

            // new attempt
            foreach(Submission newAttempt in submission)
            {
                newAttempt.Time = DateTime.Now;
                newAttempt.SubmissionContents = contents;
            }

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
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            // Check to see if student is already enrolled
            // Tables: Enrolled to find all ClassIds in Classes
            // Classes corresponds to Season and year
            var liting = from course in db.Courses
                            where course.Number == num
                            && course.Department == subject
                            select course.CatalogId;
            // Check to see if course exists
            if(liting.Count() != 1)
            {
                return Json(new { success = false });
            }
            uint listingID = liting.Single();

            // check to see if class offering exists
            var getClassIds = from classez in db.Classes
                              where classez.Listing == listingID
                              && classez.Season == season
                              && classez.Year == year
                              select classez.ClassId;
            if(getClassIds.Count() != 1)
            {
                return Json(new { success = false });
            }
            uint classID = getClassIds.Single();

            // Check to see if they are already enrolled
            var checkEnrollment = from enroll in db.Enrolleds
                                  where enroll.Student == uid
                                  && enroll.Class == classID
                                  select enroll;
            if(checkEnrollment.Count() != 0)
            {
                return Json(new { success = false });
            }

            // Enroll them in if not already
            Enrolled e = new Enrolled { Class = classID, Student = uid, Grade = "--" };
            db.Enrolleds.Add(e);
            try
            {
                db.SaveChanges();
            }
            catch(Exception l) {
                return Json(new { success = false });
            }

            return Json(new { success = true});
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            // Check for no class
            var enrolledClasses = from ec in db.Enrolleds
                                  where ec.Student == uid
                                  select ec;
            if (enrolledClasses.Count() == 0)
            {
                return Json(new { gpa = 0.0 });
            }

            // From Enrolled get all classes and grades
            int noGrade = 0;
            double gradeEarned = 0.0;
            foreach (Enrolled x in enrolledClasses)
            {
                if (x.Grade == "A") { gradeEarned += 4 * 4.0; }
                else if (x.Grade == "A-") { gradeEarned += 4 * 3.7; }
                else if (x.Grade == "B+") { gradeEarned += 4 * 3.3; }
                else if (x.Grade == "B") { gradeEarned += 4 * 3.0; }
                else if (x.Grade == "B-") { gradeEarned += 4 * 2.7; }
                else if (x.Grade == "C+") { gradeEarned += 4 * 2.3; }
                else if (x.Grade == "C") { gradeEarned += 4 * 2.0; }
                else if (x.Grade == "C-") { gradeEarned += 4 * 1.7; }
                else if (x.Grade == "D+") { gradeEarned += 4 * 1.3; }
                else if (x.Grade == "D") { gradeEarned += 4 * 1.0; }
                else if (x.Grade == "D-") { gradeEarned += 4 * 0.7; }
                else if (x.Grade == "E") { gradeEarned += 4 * 0.0; }
                else if (x.Grade == "--") { noGrade++; }


            }

            double GPA = gradeEarned / ((enrolledClasses.Count() * 4) - (noGrade * 4));

            return Json(new {gpa = GPA});
        }
                
        /*******End code to modify********/

    }
}

