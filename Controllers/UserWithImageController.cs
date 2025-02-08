using ImageOperation.DB;
using ImageOperation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ImageOperation.Controllers
{
    public class UserWithImageController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public UserWithImageController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var recorsds = await _applicationDbContext.Users.Include(x => x.State).Include(x => x.City).ToListAsync();
            return View(recorsds);
        }
        public async Task<IActionResult> GetUsers(
        string? filterText,
        int pageNumber = 1,
        int pageSize = 3,
        string sortColumn = "Name",
        string sortOrder = "asc")
        {
            // Build the query
            var query =_applicationDbContext.Users.Include(x => x.State).Include(x => x.City)
                .Select(user => new
                {
                    Name = user.Name,
                    State = user.State.Name,
                    City = user.City.Name
                    
                });

            // Apply filtering
            if (!string.IsNullOrEmpty(filterText))
            {
                filterText = filterText.ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(filterText) ||
                    u.City.ToLower().Contains(filterText)||
                    u.State.ToLower().Contains(filterText));
            }

            // Apply sorting
            query = sortColumn.ToLower() switch
            {
                "city" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.City)
                    : query.OrderBy(u => u.City),
                "state" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.State)
                    : query.OrderBy(u => u.State),
                _ => query.OrderBy(u => u.City) // Default sorting
            };

            // Apply pagination
            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();



            // Return DataTable formatted response
            return View(new { data = users, total = query.ToList().Count });
        }
        //public async Task<IActionResult> Index(int page = 1, string sortBy = "Name", bool isAsc = true)
        //{
        //    int pageSize = 3; // Items per page
        //    IQueryable<UserWithImage> query = _applicationDbContext.Users.Include(x => x.State).Include(x => x.City);

        //    // Sorting logic
        //    if (isAsc)
        //    {
        //        query = sortBy switch
        //        {
        //            "Name" => query.OrderBy(x => x.Name),
        //            "State" => query.OrderBy(x => x.State.Name),
        //            "City" => query.OrderBy(x => x.City.Name),
        //            _ => query.OrderBy(x => x.Name)
        //        };
        //    }
        //    else
        //    {
        //        query = sortBy switch
        //        {
        //            "Name" => query.OrderByDescending(x => x.Name),
        //            "State" => query.OrderByDescending(x => x.State.Name),
        //            "City" => query.OrderByDescending(x => x.City.Name),
        //            _ => query.OrderByDescending(x => x.Name)
        //        };
        //    }

        //    // Pagination logic
        //    var totalItems = await query.CountAsync();
        //    var users = await query.Skip((page - 1) * pageSize)
        //                           .Take(pageSize)
        //                           .ToListAsync();

        //    var viewModel = new PaginatedList<UserWithImage>(users, totalItems, page, pageSize);

        //    // Set sorting values in ViewData to use them in the view
        //    ViewData["SortBy"] = sortBy;
        //    ViewData["IsSortAsc"] = isAsc;

        //    return View(viewModel);
        //}


        public async Task<IActionResult> Create(int? id)
        {
            var input = new UserImageAddDto();
            input.States = await GetAllState();
            if (id == null)
            {

                return View(input);
            }
            else
            {

                var result = await _applicationDbContext.Users.Where(x => x.Id == id).FirstAsync();
                input.Cities = await GetAllCity(result.StateId.Value);

                input.Name = result.Name;
                input.StateId = result.StateId.Value;
                input.CityId = result.CityId.Value;
                string imageBase64Data = Convert.ToBase64String(result.Image);
                string imageDataURL = string.Format("data:image/jpg;base64,{0}", imageBase64Data);
                input.ImagePath = imageDataURL;

                return View(input);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create(int? id, UserImageAddDto input)
        {
            input.States = await GetAllState();

            if (!ModelState.IsValid)
            {
                return View(input);
            }
            //Define the base path
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }
            if (id == null)
            {
                var userWithImage = new UserWithImage()
                {
                    Name = input.Name,
                    ImagePath = "",
                    StateId = input.StateId,
                    CityId=input.CityId


                };
                await _applicationDbContext.Users.AddAsync(userWithImage);
                await _applicationDbContext.SaveChangesAsync();
                var userId = userWithImage.Id;
                var userFolder = Path.Combine(imagePath, userId.ToString());
                if (!Directory.Exists(userFolder))
                {
                    Directory.CreateDirectory(userFolder);
                }
                var filePath = Path.Combine(userFolder, input.Image.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    input.Image.CopyTo(stream);
                };
                using (MemoryStream stream = new MemoryStream())
                {
                    input.Image.CopyTo(stream);
                    userWithImage.Image = stream.ToArray();
                }
                userWithImage.ImagePath = Path.Combine("images", userId.ToString(), input.Image.FileName);
                _applicationDbContext.Users.Update(userWithImage);
                await _applicationDbContext.SaveChangesAsync();

                //var userWithImage = new UserWithImage()
                //{
                //    Name = input.Name,
                //    ImagePath = ""
                //};
                //using (MemoryStream stream = new MemoryStream())
                //{
                //    input.Image.CopyTo(stream);
                //    userWithImage.Image = stream.ToArray();
                //}
                //var user=await _applicationDbContext.Users.AddAsync(userWithImage);

            }
            else
            {
                var result = await _applicationDbContext.Users.Where(x => x.Id == id).FirstAsync();
                input.Cities = await GetAllCity(result.StateId.Value);
                result.Name = input.Name;
                result.StateId = input.StateId;
                result.CityId = input.CityId;
                var userFolder = Path.Combine(imagePath, result.Id.ToString());
                var filePath = Path.Combine(userFolder, input.Image.FileName);
                using (MemoryStream stream = new MemoryStream())
                {
                    input.Image.CopyTo(stream);
                    result.Image = stream.ToArray();
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    input.Image.CopyTo(stream);
                }
                result.ImagePath = Path.Combine("images", result.Id.ToString(), input.Image.FileName); // Update the path in DB
                _applicationDbContext.Users.Update(result);
                await _applicationDbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _applicationDbContext.Users.Where(x => x.Id == id).FirstAsync();
            if (result != null)
            {
                _applicationDbContext.Users.Remove(result);
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction("Index");

            }
            return NotFound();

        }
        [HttpGet]
        public async Task<List<SelectListItem>> GetAllState()
        {
            var states = await _applicationDbContext.States.ToListAsync();
            var selectStates = states.Select(x => new SelectListItem( x.Name, x.Id.ToString())).ToList();
            return selectStates;
        }
        [HttpGet]
        public async Task<List<SelectListItem>> GetAllCity(int id)
        {
            var states = await _applicationDbContext.Cities.Where(x=>x.StateId==id).ToListAsync();
            var selectStates = states.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
            return selectStates;
        }


    }
}
