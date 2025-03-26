using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Bonus_Implementation_Policy_WebApi.Models;
using MetaQuotes.MT5CommonAPI;
using Microsoft.AspNetCore.Mvc;

public class BackgroundWorkerService : BackgroundService
{
    private readonly MT5AccountandData _mt5AccountandData;
    private readonly ILogger<BackgroundWorkerService> _logger;
    public ConcurrentDictionary<ulong, UserTransactionInfo> _userData = new();
    private long _lastFetchTime = 0; // Stores last fetched 'from' time
    private long currenttime;

    public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger, MT5AccountandData mT5AccountandData)
    {
        _mt5AccountandData = mT5AccountandData;
        _logger = logger;
        _lastFetchTime = 0;
    }

    public ConcurrentDictionary<ulong, UserTransactionInfo> GetAllInfo()
    {
        return _userData;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Service Started.");

        // Initial Fetch: Get all deals from 1st January 1970 to now
        currenttime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await ProcessDealsAsync(_lastFetchTime, currenttime);

        // Now update every 1 minute
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            _lastFetchTime = currenttime;
            currenttime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await ProcessDealsAsync(_lastFetchTime, currenttime);
        }
    }

    private async Task ProcessDealsAsync(long from, long to)
    {
        try
        {
            var res = _mt5AccountandData.UserDealArray(from, to, out CIMTDealArray dealArray);

            Console.WriteLine("Res is :", res);

            if (res != MTRetCode.MT_RET_OK)
            {
                _logger.LogError($"MT5 API Error: UserDealArray failed with code {res} - {res.ToString()}");
                return;
            }

            for (uint i = 0; i < dealArray.Total(); i++)
            {
                CIMTDeal deal = dealArray.Next(i);
                ulong login = deal.Login();
                uint action = deal.Action();
                double profit = deal.Profit();

                if (!_userData.ContainsKey(login))
                {
                    _userData[login] = new UserTransactionInfo();
                }

                if (action == 2 && profit > 0)
                {
                    _userData[login].Deposit += profit;
                }

                if ((action == 3 || action == 6) && profit > 0)
                {
                    _userData[login].Bonus += profit;
                }

                if (action == 2 && profit < 0)
                {
                    _userData[login].Withdrawn_Amount += profit;
                }

                if (_userData[login].Max_Withdrawable_Amount == 0)
                {
                    _userData[login].Max_Withdrawable_Amount = _userData[login].Deposit * 3;
                }

            }

            // foreach (var kvp in _userData)
            // {
            //     ulong login = kvp.Key;
            //     UserTransactionInfo userInfo = kvp.Value;

            //     if (userInfo.Deposit > userInfo.Max_Withdrawable_Amount)
            //     {
            //         try
            //         {
            //             userInfo.Extra_Profit = userInfo.Deposit - userInfo.Max_Withdrawable_Amount;

            //             uint type = 2;
            //             string comment = "Testing";
            //             var updateResult = _mt5AccountandData.UpdateCredit(login, -userInfo.Extra_Profit, type, comment, out ulong dealid);

            //             if (updateResult != MTRetCode.MT_RET_REQUEST_DONE)
            //             {
            //                 _logger.LogError($"MT5 API Error: Failed to update credit for Login {login}. Error Code: {updateResult}");
            //             }
            //             else
            //             {
            //                 userInfo.Max_Withdrawable_Amount = 0;

            //                 _logger.LogInformation($"Credit updated successfully for Login {login}. Extra Profit: {userInfo.Extra_Profit}");
            //             }
            //         }
            //         catch (Exception ex)
            //         {
            //             _logger.LogError($"Error updating credit for Login {login}: {ex.Message}");
            //         }
            //     }
            // }

        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing deals: {ex.Message}");
        }
    }

}

