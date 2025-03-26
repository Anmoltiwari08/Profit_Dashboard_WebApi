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

        private List<CIMTUser> UsersList()
        {
            var Userarrays = _mt5AccountandData.UserByGroup(out CIMTUserArray userArrays);
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

        private List<Users> GetCombinedData()
        {
            var users = UsersList();
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
                            query = query.Where(u => u.Balance == balance);
                        break;
                    case "bonus":
                        if (double.TryParse(search, out double bonus))
                            query = query.Where(u => u.Bonus == bonus);
                        break;
                    case "profit":
                        if (double.TryParse(search, out double profit))
                            query = query.Where(u => u.Profit == profit);
                        break;
                    case "margin":
                        if (double.TryParse(search, out double margin))
                            query = query.Where(u => u.Margin == margin);
                        break;
                    case "floating":
                        if (double.TryParse(search, out double floating))
                            query = query.Where(u => u.Floating == floating);
                        break;
                    case "equity":
                        if (double.TryParse(search, out double equity))
                            query = query.Where(u => u.Equity == equity);
                        break;
                    case "registereddate":
                        if (DateTime.TryParse(search, out DateTime registeredDate))
                            query = query.Where(u => u.RegisteredDate.ToString().Contains(search));
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
                // _ => query.OrderBy(u => u.Login)
            };
            return query;
        }

        private IQueryable<Users> ApplyPagination(IQueryable<Users> query, int pageNumber, int pageSize)
        {
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        }

        [HttpGet("Data")]
        public IActionResult Data(int pageNumber = 1, int pageSize = 100, string? search = null, string? searchField = null, string sortBy = "", string sortOrder = "asc")
        {
            var combinedData = GetCombinedData().AsQueryable();

            if (search != null && searchField != null) { combinedData = ApplySearchFilters(combinedData, search, searchField); }
            if (!String.IsNullOrEmpty(sortBy) && !String.IsNullOrEmpty(sortOrder)) { combinedData = ApplySorting(combinedData, sortBy, sortOrder); }
            var pagedData = ApplyPagination(combinedData, pageNumber, pageSize);
                
            var res = _backgroundService.GetAllInfo();

            foreach (var item in pagedData)
            {

               if (res.ContainsKey(item.Login)) {
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
        public IActionResult GetAllDictionaryDeals(){

            var res = _backgroundService.GetAllInfo();

            return Ok(new { res = res , message = "Data retrieved successfully" , Length = res.Count } );

        }

    }
}
