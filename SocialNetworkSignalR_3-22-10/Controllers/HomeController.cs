using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkSignalR_3_22_10.Data;
using SocialNetworkSignalR_3_22_10.Entities;
using SocialNetworkSignalR_3_22_10.Models;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace SocialNetworkSignalR_3_22_10.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly SocialNetworkDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<CustomIdentityUser> userManager, SocialNetworkDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            ViewBag.User = user;
            return View();
        }

        public async Task<IActionResult> GetAllUsers()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var myrequests = _context.FriendRequests.Where(r => r.SenderId == user.Id);

            var users =await _context.Users
                .Where(u => u.Id != user.Id)
                .OrderByDescending(u => u.IsOnline)
                .Select(u => new CustomIdentityUser
                {
                     Id=u.Id,
                        HasRequestPending = (myrequests.FirstOrDefault(r => r.ReceiverId == u.Id && r.Status == "Request") != null),
                        UserName = u.UserName,
                        IsOnline=u.IsOnline,
                        Image=u.Image,
                        Email=u.Email,
                })
                .ToListAsync();


            

            //foreach (var item in users)
            //{
            //   // var request = 
            //    if (request != null)
            //    {
            //        item.HasRequestPending = true;
            //    }
            //}

            return Ok(users);
        }

        public async Task<IActionResult> SendFollow(string id)
        {
            var sender=await _userManager.GetUserAsync(HttpContext.User);
            var receiverUser=await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if(receiverUser != null)
            {
                _context.FriendRequests.Add(new FriendRequest
                {
                    Content=$"{sender.UserName} sent friend request at {DateTime.Now.ToLongDateString()}",
                    SenderId = sender.Id,
                    Sender=sender,
                    ReceiverId=id,
                    Status="Request"
                });

                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
