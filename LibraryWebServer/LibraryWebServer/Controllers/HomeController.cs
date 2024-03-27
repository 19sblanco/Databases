using LibraryWebServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

[assembly: InternalsVisibleTo( "TestProject1" )]
namespace LibraryWebServer.Controllers
{
    public class HomeController : Controller
    {

        // WARNING:
        // This very simple web server is designed to be as tiny and simple as possible
        // This is NOT the way to save user data.
        // This will only allow one user of the web server at a time (aside from major security concerns).
        private static string user = "";
        private static int card = -1;

        private readonly ILogger<HomeController> _logger;


        /// <summary>
        /// Given a Patron name and CardNum, verify that they exist and match in the database.
        /// If the login is successful, sets the global variables "user" and "card"
        /// </summary>
        /// <param name="name">The Patron's name</param>
        /// <param name="cardnum">The Patron's card number</param>
        /// <returns>A JSON object with a single field: "success" with a boolean value:
        /// true if the login is accepted, false otherwise.
        /// </returns>
        [HttpPost]
        public IActionResult CheckLogin( string name, int cardnum )
        {
            // TODO: Fill in. Determine if login is successful or not.
            bool loginSuccessful = false;

            using (Team47LibraryContext db = new Team47LibraryContext())
            {
                var query = from p in db.Patrons
                            where p.Name == name
                            && p.CardNum == cardnum
                            select p;

                if (query.Count() == 1)
                {
                    loginSuccessful = true;
                }
            }

            if ( !loginSuccessful )
            {
                return Json( new { success = false } );
            }
            else
            {
                user = name;
                card = cardnum;
                return Json( new { success = true } );
            }
        }


        /// <summary>
        /// Logs a user out. This is implemented for you.
        /// </summary>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult LogOut()
        {
            user = "";
            card = -1;
            return Json( new { success = true } );
        }

        /// <summary>
        /// Returns a JSON array representing all known books.
        /// Each book should contain the following fields:
        /// {"isbn" (string), "title" (string), "author" (string), "serial" (uint?), "name" (string)}
        /// Every object in the list should have isbn, title, and author.
        /// Books that are not in the Library's inventory (such as Dune) should have a null serial.
        /// The "name" field is the name of the Patron who currently has the book checked out (if any)
        /// Books that are not checked out should have an empty string "" for name.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult AllTitles()
        {

            // select ISBN, Title, Author, Inventory.Serial,
            // coalesce(Patrons.Name, "") as Name
            // from Titles natural left join Inventory natural left join CheckedOut
            // left join Patrons on CheckedOut.CardNum = Patrons.CardNum;


            // TODO: Implement
            using (Team47LibraryContext db = new Team47LibraryContext())
            {
                var query = from t in db.Titles
                            join i in db.Inventory on t.Isbn equals i.Isbn into Titles_Inventory
                            from sub_titles in Titles_Inventory.DefaultIfEmpty()

                            join c in db.CheckedOut on sub_titles.Serial equals c.Serial into chkOut
                            from j1 in chkOut.DefaultIfEmpty()
                            join p in db.Patrons on j1.CardNum equals p.CardNum into checkout_patrons
                            from j2 in checkout_patrons.DefaultIfEmpty()
                            select new
                            {
                                isbn = t.Isbn,
                                title = t.Title,
                                author = t.Author,
                                serial = sub_titles != null ? sub_titles.Serial : (uint?)null,
                                name = j2 != null ? j2.Name : "",
                            };



                //System.Diagnostics.Debug.WriteLine("Before for each");
                //System.Diagnostics.Debug.WriteLine("\n\n");
                //foreach (var q in query)
                //{
                //    System.Diagnostics.Debug.WriteLine(q.ToString());
                //}
                //System.Diagnostics.Debug.WriteLine("after for each");
                //System.Diagnostics.Debug.WriteLine("\n\n");

                //System.Diagnostics.Debug.WriteLine("\n\n");
                return Json(query.ToArray());
            }

            //return Json( null );

        }

        /// <summary>
        /// Returns a JSON array representing all books checked out by the logged in user 
        /// The logged in user is tracked by the global variable "card".
        /// Every object in the array should contain the following fields:
        /// {"title" (string), "author" (string), "serial" (uint) (note this is not a nullable uint) }
        /// Every object in the list should have a valid (non-null) value for each field.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult ListMyBooks()
        {
            // TODO: Implement

            // select * from (Titles join Inventory on isbn
            // 
            // where cardNum = 1;
            //
            // 
            // select Title, Author, Serial from CheckedOut natural join Inventory natural join Titles natural join Patrons where Patrons.Cardnum = 1;

            using (Team47LibraryContext db = new Team47LibraryContext())
            {
                var query = from t in db.Titles
                            join i in db.Inventory on t.Isbn equals i.Isbn into j1
                            from title_inventory in j1
                            join c in db.CheckedOut on title_inventory.Serial equals c.Serial into j2
                            from checkout_inventory_titles in j2
                            join p in db.Patrons on checkout_inventory_titles.CardNum equals p.CardNum into j3
                            from table_row in j3
                            where table_row.CardNum == card
                            select new
                            {
                                title = t.Title,
                                author = t.Author,
                                serial = checkout_inventory_titles.Serial,
                            };
                return Json(query.ToArray());

            }

            //return Json( null );
        }


        /// <summary>
        /// Updates the database to represent that
        /// the given book is checked out by the logged in user (global variable "card").
        /// In other words, insert a row into the CheckedOut table.
        /// You can assume that the book is not currently checked out by anyone.
        /// </summary>
        /// <param name="serial">The serial number of the book to check out</param>
        /// <returns>success</returns>
        [HttpPost]
        public ActionResult CheckOutBook( int serial )
        {
            // You may have to cast serial to a (uint)
            // insert into checkedOut values (card, serial);
            
            using (Team47LibraryContext db = new Team47LibraryContext())
            {
                CheckedOut checkedOut = new CheckedOut();
                checkedOut.CardNum = (uint)card;
                checkedOut.Serial = (uint)serial;
                db.CheckedOut.Add(checkedOut);
                try
                {
                    db.SaveChanges();
                }
                catch
                {

                }
            }

                return Json( new { success = true } );
        }

        /// <summary>
        /// Returns a book currently checked out by the logged in user (global variable "card").
        /// In other words, removes a row from the CheckedOut table.
        /// You can assume the book is checked out by the user.
        /// </summary>
        /// <param name="serial">The serial number of the book to return</param>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult ReturnBook( int serial )
        {
            // You may have to cast serial to a (uint)
            using (Team47LibraryContext db = new Team47LibraryContext())
            {
                // select * from checkOut where card = cardNum and serial = serail
                var query = from c in db.CheckedOut
                            where c.Serial == serial && c.CardNum == card
                            select c;
                db.CheckedOut.RemoveRange(query);
                try
                {
                    db.SaveChanges();
                }
                catch
                {

                }
            }

                return Json( new { success = true } );
        }


        /*******************************************/
        /****** Do not modify below this line ******/
        /*******************************************/


        public IActionResult Index()
        {
            if ( user == "" && card == -1 )
                return View( "Login" );

            return View();
        }


        /// <summary>
        /// Return the Login page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            user = "";
            card = -1;

            ViewData["Message"] = "Please login.";

            return View();
        }

        /// <summary>
        /// Return the MyBooks page.
        /// </summary>
        /// <returns></returns>
        public IActionResult MyBooks()
        {
            if ( user == "" && card == -1 )
                return View( "Login" );

            return View();
        }

        public HomeController( ILogger<HomeController> logger )
        {
            _logger = logger;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache( Duration = 0, Location = ResponseCacheLocation.None, NoStore = true )]
        public IActionResult Error()
        {
            return View( new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier } );
        }
    }
}