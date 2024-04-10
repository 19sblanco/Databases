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
            db.Courses.Add(new Course { Name = "algorithms", Number = 2410, Department = "CS" });
            db.Courses.Add(new Course { Name = "drawing", Number = 1010, Department = "ART" });

            DateTime t = new DateTime(2023, 1, 1, 12, 0, 0, 0);
            DateTime t1 = new DateTime(2024, 1, 1, 12, 0, 0, 0);
            Professor p = new Professor { UId = "u0000000", FName = "prof", LName = "teacher",
                Dob = DateOnly.FromDateTime(t1), WorksIn = "CS" };

            db.Classes.Add(new Class { Season="Spring", Year=2024, Location="uofu",
                StartTime=TimeOnly.FromDateTime(t), EndTime=TimeOnly.FromDateTime(t1),
                Listing=c.CatalogId, 
            })

            // TODO: add more objects to the test database


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