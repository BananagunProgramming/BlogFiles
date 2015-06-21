using System.Security.Principal;
using System.Web;
using EF.Implementation;
using EntityManager.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EntityManager.Services
{
    public interface IUserService
    {
        string GetUserProperty(string property);
    }

    public class UserService : IUserService
    {
        public DbContextScopeFactory DbContextScopeFactory;
        private readonly IPrincipal _user;

        public UserService(DbContextScopeFactory dbContextScopeFactory)
        {
            DbContextScopeFactory = dbContextScopeFactory;
            _user = HttpContext.Current.User;
        
        }

        public string GetUserProperty(string property)
        {
            using (var dbContextScope = DbContextScopeFactory.CreateReadOnly())
            {
                var context = dbContextScope.DbContexts.Get<ApplicationDbContext>();

                var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

                var currentUser = manager.FindById(_user.Identity.GetUserId());
                return currentUser.GetType().GetProperty(property).GetValue(currentUser, null).ToString();
            }
        }
    }
}