using Microsoft.AspNetCore.Mvc;
using Bonus_Implementation_Policy_WebApi.Models;
using MetaQuotes.MT5ManagerAPI;
using MetaQuotes.MT5CommonAPI;
using System.Numerics;

namespace Bonus_Implementation_Policy_WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ManagerController : ControllerBase
    {

        private readonly MT5AccountandData _mt5AccountandData;
        private readonly BackgroundWorkerService _backgroundService;

        public ManagerController(MT5AccountandData mT5AccountandData, BackgroundWorkerService backgroundworkerService)
        {
            _mt5AccountandData = mT5AccountandData;
            _backgroundService = backgroundworkerService;
        }

        private List<CIMTUser> UsersList(string mask)
        {

            var Userarrays = _mt5AccountandData.UserByGroup(mask, out CIMTUserArray userArrays);
            var TotalUsers = _mt5AccountandData.TotalUsersinArray(userArrays);

            List<CIMTUser> users = new List<CIMTUser>();
            for (uint i = 0; i < TotalUsers; i++)
            {
                var user = _mt5AccountandData.UserGetByIndex(userArrays, i);
                users.Add(user);
            }

            return users;
        }

        private List<CIMTAccount> AccountsList(List<ulong> Logins)
        {
            var UsersAccountArray = _mt5AccountandData.GetAccountsByLogins(Logins.ToArray(), out CIMTAccountArray? Accounts);
            var TotalAccounts = _mt5AccountandData.TotalAccountsinArray(Accounts);

            List<CIMTAccount> accounts = new List<CIMTAccount>();
            for (uint i = 0; i < TotalAccounts; i++)
            {
                var account = _mt5AccountandData.GetUserAccount(Accounts, i);
                accounts.Add(account);
            }

            return accounts;
        }

        private List<Users> GetCombinedData(string mask)
        {
            var users = UsersList(mask);
            List<ulong> logins = users.Select(user => user.Login()).ToList();
            var accounts = AccountsList(logins);

            return users
                .Join(accounts,
                      user => user.Login(),
                      account => account.Login(),
                      (user, account) => new Users
                      {
                          Login = user.Login(),
                          name = user.Name(),
                          Group = user.Group(),
                          Balance = user.Balance(),
                          RegisteredDate = DateTimeOffset.FromUnixTimeSeconds(user.Registration()).DateTime,
                          //   Bonus = 0,
                          //   Deposit = 0,
                          //   Extra_Profit = 0,
                          //   Withdrawled_processed = 0,
                          Leverage = user.Leverage(),
                          Profit = account.Profit(),
                          Margin = account.Margin(),
                          Floating = account.Floating(),
                          Equity = account.Equity(),
                          //   Max_Withdrawable_amount = 0

                      }).ToList();

        }

        private IQueryable<Users> ApplySearchFilters(IQueryable<Users> query, string? search, string? searchField)
        {
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(searchField))
            {
                switch (searchField.ToLower())
                {
                    case "name":
                        query = query.Where(u => u.name.Contains(search));
                        break;
                    case "group":
                        query = query.Where(u => u.Group.Contains(search));
                        break;
                    case "login":
                        if (ulong.TryParse(search, out ulong login))
                            query = query.Where(u => u.Login.ToString().Contains(search));
                        break;
                    case "balance":
                        if (double.TryParse(search, out double balance))
                            query = query.Where(u => u.Balance.ToString().Contains(search));
                        break;
                    case "bonus":
                        if (double.TryParse(search, out double bonus))
                            query = query.Where(u => u.Bonus.ToString().Contains(search));
                        break;
                    case "profit":
                        if (double.TryParse(search, out double profit))
                            query = query.Where(u => u.Profit.ToString().Contains(search));
                        break;
                    case "margin":
                        if (double.TryParse(search, out double margin))
                            query = query.Where(u => u.Margin.ToString().Contains(search));
                        break;
                    case "floating":
                        if (double.TryParse(search, out double floating))
                            query = query.Where(u => u.Floating.ToString().Contains(search));
                        break;
                    case "equity":
                        if (double.TryParse(search, out double equity))
                            query = query.Where(u => u.Equity.ToString().Contains(search));
                        break;
                    case "registereddate":
                        if (search.ToLower() == "today")
                        {
                            DateTime today = DateTime.Today;
                            query = query.Where(u => u.RegisteredDate >= today && u.RegisteredDate < today.AddDays(1));
                        }
                        else if (search.ToLower() == "yesterday")
                        {
                            DateTime yesterday = DateTime.Today.AddDays(-1);
                            query = query.Where(u => u.RegisteredDate >= yesterday && u.RegisteredDate < yesterday.AddDays(1));
                        }
                        else if (search.ToLower() == "last7days")
                        {
                            DateTime last7Days = DateTime.Today.AddDays(-7);
                            query = query.Where(u => u.RegisteredDate >= last7Days && u.RegisteredDate < DateTime.Today.AddDays(1));
                        }
                        else if (search.ToLower() == "last15days")
                        {
                            DateTime last15Days = DateTime.Today.AddDays(-15);
                            query = query.Where(u => u.RegisteredDate >= last15Days && u.RegisteredDate < DateTime.Today.AddDays(1));
                        }
                        else if (search.ToLower() == "last30days")
                        {
                            DateTime last30Days = DateTime.Today.AddDays(-30);
                            query = query.Where(u => u.RegisteredDate >= last30Days && u.RegisteredDate < DateTime.Today.AddDays(1));
                        }
                        else if (search.Contains(" to ")) 
                        {
                            var dates = search.Split(" to ");
                            if (DateTime.TryParse(dates[0], out DateTime startDate) && DateTime.TryParse(dates[1], out DateTime endDate))
                            {
                                query = query.Where(u => u.RegisteredDate >= startDate && u.RegisteredDate <= endDate.AddDays(1));
                            }
                        }
                        else if (DateTime.TryParse(search, out DateTime registeredDate)) 
                        {
                            query = query.Where(u => u.RegisteredDate >= registeredDate.Date &&
                                                     u.RegisteredDate < registeredDate.Date.AddDays(1));
                        }
                        break;

                }
            }

            return query;
        }

        private IQueryable<Users> ApplySorting(IQueryable<Users> query, string sortBy, string sortOrder)
        {
            bool descending = sortOrder.ToLower() == "desc";

            query = sortBy.ToLower() switch
            {
                "login" => descending ? query.OrderByDescending(u => u.Login) : query.OrderBy(u => u.Login),
                "name" => descending ? query.OrderByDescending(u => u.name) : query.OrderBy(u => u.name),
                "group" => descending ? query.OrderByDescending(u => u.Group) : query.OrderBy(u => u.Group),
                "balance" => descending ? query.OrderByDescending(u => u.Balance) : query.OrderBy(u => u.Balance),
                "bonus" => descending ? query.OrderByDescending(u => u.Bonus) : query.OrderBy(u => u.Bonus),
                "profit" => descending ? query.OrderByDescending(u => u.Profit) : query.OrderBy(u => u.Profit),
                "margin" => descending ? query.OrderByDescending(u => u.Margin) : query.OrderBy(u => u.Margin),
                "floating" => descending ? query.OrderByDescending(u => u.Floating) : query.OrderBy(u => u.Floating),
                "equity" => descending ? query.OrderByDescending(u => u.Equity) : query.OrderBy(u => u.Equity),
                "registereddate" => descending ? query.OrderByDescending(u => u.RegisteredDate) : query.OrderBy(u => u.RegisteredDate),
                "leverage" => descending ? query.OrderByDescending(u => u.Leverage) : query.OrderBy(u => u.Leverage),
                "deposit" => descending ? query.OrderByDescending(u => u.Deposit) : query.OrderBy(u => u.Deposit),
                "max_withdrawable_amount" => descending ? query.OrderByDescending(u => u.Max_Withdrawable_amount) : query.OrderBy(u => u.Max_Withdrawable_amount),
                "Extra_Profit" => descending ? query.OrderByDescending(u => u.Extra_Profit) : query.OrderBy(u => u.Extra_Profit),
                "Withdrawled_processed" => descending ? query.OrderByDescending(u => u.Withdrawled_processed) : query.OrderBy(u => u.Withdrawled_processed),
                // _ => query.OrderBy(u => u.Login)
            };
            return query;
        }

        private IQueryable<Users> ApplyPagination(IQueryable<Users> query, int pageNumber, int pageSize)
        {
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        }

        [HttpGet("Data")]
        public IActionResult Data(int pageNumber = 1, int pageSize = 100, string? search = null, string? searchField = null, string sortBy = "", string sortOrder = "asc", string mask = "*")
        {
            Console.WriteLine("mask: " + mask);

            if (mask == null) { return BadRequest("Please pass group mask "); }

            var combinedData = GetCombinedData(mask).AsQueryable();

            if (search != null && searchField != null) { combinedData = ApplySearchFilters(combinedData, search, searchField); }
            if (!String.IsNullOrEmpty(sortBy) && !String.IsNullOrEmpty(sortOrder)) { combinedData = ApplySorting(combinedData, sortBy, sortOrder); }
            var pagedData = ApplyPagination(combinedData, pageNumber, pageSize);

            var res = _backgroundService.GetAllInfo();

            foreach (var item in pagedData)
            {

                if (res.ContainsKey(item.Login))
                {
                    item.Deposit = res[item.Login].Deposit;
                    item.Bonus = res[item.Login].Bonus;
                    item.Max_Withdrawable_amount = res[item.Login].Max_Withdrawable_Amount;
                    item.Withdrawled_processed = res[item.Login].Withdrawn_Amount;

                }

            }

            var totalPages = (int)Math.Ceiling((double)combinedData.Count() / pageSize);

            return Ok(new
            {
                TotalRecords = combinedData.Count(),
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = pagedData.ToList()
            });

        }

        [HttpGet("GetData")]
        public IActionResult GetAllDictionaryDeals()
        {

            var res = _backgroundService.GetAllInfo();

            return Ok(new { res = res, message = "Data retrieved successfully", Length = res.Count });

        }

        [HttpGet("TotalGroups")]
        public IActionResult TotalGroups()
        {

            var res = _mt5AccountandData.TotalGroups();

            if (res == 0) { return new JsonResult(new { message = "No groups found" }); }

            string[] GroupNames = new string[res];

            for (uint i = 0; i < res; i++)
            {
                _mt5AccountandData.GroupByIndex(i, out CIMTConGroup? group);
                GroupNames[i] = group?.Group() ?? "";

            }

            return new JsonResult(new { GroupNames });

        }



    }
}
