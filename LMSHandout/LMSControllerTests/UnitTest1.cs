using LMS.Controllers;
using LMS.Models.LMSModels;
using LMS_CustomIdentity.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LMSControllerTests
{
    public class UnitTest1
    {
        // Uncomment the methods below after scaffolding
        // (they won't compile until then)

        [Fact]
        public void Test1()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDB());

            var allDepts = ctrl.GetDepartments() as JsonResult;

            dynamic x = allDepts.Value;

            Assert.Equal(2, x.Length);
            Assert.Equal("CS", x[0].subject);
        }

        //[Fact]
        //public void TestNumCourses()
        //{
        //    CommonController ctrl = new CommonController(MakeTinyDB());

        //    var allCourses = ctrl.GetCatalog as JsonResult;
        //}


        ///// <summary>
        ///// Make a very tiny in-memory database, containing just one department
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeTinyDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            
            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });
            db.Departments.Add(new Department { Name = "ART", Subject = "ART" });

            Course c = new Course { Name = "intro to programming", Number = 1010, Department = "CS" };
            db.Courses.Add(c);
            Course c1 = new Course { Name = "algorithms", Number = 2410, Department = "CS" };
            db.Courses.Add(c1);
            Course c2 = new Course { Name = "drawing", Number = 1010, Department = "ART" };
            db.Courses.Add(c2);

            DateTime t = new DateTime(2023, 1, 1, 12, 0, 0, 0);
            DateTime t1 = new DateTime(2024, 1, 1, 12, 0, 30, 0);
            Professor p = new Professor { UId = "u0000000", FName = "prof", LName = "teacher",
                Dob = DateOnly.FromDateTime(t1), WorksIn = "CS" };

            Professor p2 = new Professor
            {
                UId = "u0000001",
                FName = "prof2",
                LName = "teacher2",
                Dob = DateOnly.FromDateTime(t1),
                WorksIn = "ART"
            };

            db.Professors.Add(p);
            db.Professors.Add(p2);

            Class x = new Class
            {
                Season = "Spring",
                Year = 2024,
                Location = "uofu",
                StartTime = TimeOnly.FromDateTime(t),
                EndTime = TimeOnly.FromDateTime(t1),
                Listing = c.CatalogId,
                TaughtBy = p.UId
            };
            db.Classes.Add(x);

            Class y = new Class
            {
                Season = "Fall",
                Year = 2024,
                Location = "uofu",
                StartTime = TimeOnly.FromDateTime(t),
                EndTime = TimeOnly.FromDateTime(t1),
                Listing = c2.CatalogId,
                TaughtBy = p2.UId
            };
            db.Classes.Add(y);

            Class z = new Class
            {
                Season = "Spring",
                Year = 2024,
                Location = "usu",
                StartTime = TimeOnly.FromDateTime(t),
                EndTime = TimeOnly.FromDateTime(t1),
                Listing = c1.CatalogId,
                TaughtBy = p.UId
            };
            db.Classes.Add(z);

            Student s = new Student { UId = "u0000002", FName = "stu", LName = "dent", Dob = new DateOnly(1999, 05, 05), Major = "CS" };
            Student s1 = new Student { UId = "u0000003", FName = "joe", LName = "joe", Dob = new DateOnly(2000, 12, 20), Major = "ART" };
            db.Students.Add(s);
            db.Students.Add(s1);

            db.Enrolleds.Add(new Enrolled { Student = s.UId, Class = x.ClassId, Grade = "A" });
            db.Enrolleds.Add(new Enrolled { Student = s1.UId, Class = y.ClassId, Grade = "A" });
                
            db.SaveChanges();

            return db;
        }

        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }

    }
}