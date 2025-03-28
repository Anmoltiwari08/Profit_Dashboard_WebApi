using MetaQuotes.MT5ManagerAPI;
using MetaQuotes.MT5CommonAPI;

public class MT5AccountandData
{
    private CIMTManagerAPI _connection;

    public MT5AccountandData(MT5Connection connection)
    {
        _connection = connection.m_admin;
    }

    public uint TotalUsers()
    {
        var res = _connection.UserTotal();
        return res;
    }

    public uint TotalGroups()
    {
        uint res = _connection.GroupTotal();
        return res;
    }

    public MTRetCode GroupByIndex(uint index, out CIMTConGroup? group)
    {

        CIMTConGroup Group = _connection.GroupCreate();

        MTRetCode res = _connection.GroupNext(
             index,
            Group
        );

        if (res != MTRetCode.MT_RET_OK)
        {
            group = null;
            return res;
        }

        group = Group;
        return res;
    }

    public MTRetCode UserByGroup(string mask, out CIMTUserArray userArrays)
    {
        CIMTUserArray userArray = _connection.UserCreateArray();

        var res = _connection.UserGetByGroup(mask, userArray);

        userArrays = userArray;

        return res;
    }

    public uint TotalUsersinArray(CIMTUserArray userArray)
    {
        var res = userArray.Total();
        return res;
    }

    public CIMTUser UserGetByIndex(CIMTUserArray userArray, uint index)
    {
        var res = userArray.Next(index);
        return res;
    }

    public MTRetCode GetAccountsByLogins(ulong[] logins, out CIMTAccountArray Accounts)
    {
        CIMTAccountArray accountArray = _connection.UserCreateAccountArray();

        MTRetCode res = _connection.UserAccountGetByLogins(
            logins,
            accountArray
        );

        Accounts = accountArray;
        return res;
    }

    public uint TotalAccountsinArray(CIMTAccountArray useraccountArray)
    {
        var res = useraccountArray.Total();
        return res;
    }

    public CIMTAccount GetUserAccount(CIMTAccountArray cIMTAccountArray, uint index)
    {
        var account = cIMTAccountArray.Next(index);
        return account;

    }

    public MTRetCode UserDealArray(long from, long to, out CIMTDealArray dealArray)
    {
        CIMTDealArray dealArray1 = _connection.DealCreateArray();
        string group = "*";

        Console.WriteLine($"from {from} ");
        Console.WriteLine($"to {to} ");

        MTRetCode res = _connection.DealRequestByGroup(group, from, to, dealArray1);
        // Console.WriteLine($"res is {res}");

        dealArray = dealArray1;
        return res;

    }

    // public MTRetCode UpdateCredit(ulong login, double credit, uint type, string comment, out ulong dealid)
    // {

    //     MTRetCode res = _connection.DealerBalanceRaw(
    //         login,
    //         credit,
    //         type,
    //         comment,
    //         out ulong id
    //     );
    //     dealid = id;
    //     return res;

    // }

}
