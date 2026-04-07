using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SV22T1020013.Shop.AppCodes;

namespace SV22T1020013.Shop.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            try
            {
                ViewBag.NavCategories = CategoryRepository.GetAll();
            }
            catch { }
        }
    }
}
