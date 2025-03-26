using MetaQuotes.MT5ManagerAPI;
using MetaQuotes.MT5CommonAPI;
using System;

public class MT5Connection
{
    uint MT5_CONNECT_TIMEOUT = 52000000;

    public CIMTManagerAPI m_admin = null!;

    public MT5Connection()
    {
        LoggedInManager();
    }

    public void CreateManager(string? path)
    {
        string message = string.Empty;

        MTRetCode res = MTRetCode.MT_RET_OK_NONE;

        string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

        string dllPath = Path.Combine(appDirectory, "MT5APIManager64.dll");

        if ((res = SMTManagerAPIFactory.Initialize(dll_path : dllPath)) != MTRetCode.MT_RET_OK)
        {
            message = string.Format("Loading manager API failed ({0})", res.ToString());
            Console.WriteLine("manager unitialized issue " + message);
            return;
        }

        m_admin = SMTManagerAPIFactory.CreateManager(SMTManagerAPIFactory.ManagerAPIVersion, path != null ? path : "./", out res);
        if ((res != MTRetCode.MT_RET_OK) || (m_admin == null))
        {
            SMTManagerAPIFactory.Shutdown();
            message = string.Format("Creating manager interface failed ({0})", (res == MTRetCode.MT_RET_OK ? "Managed API is null" : res.ToString()));
            Console.WriteLine("Manager creation issue"+message);
            return;
        }

        Console.WriteLine("Creating manager");
    }

    public bool Login(string server, UInt64 login, string password)
    {
        //--- connect
        MTRetCode res = m_admin.Connect(server, login, password, null, CIMTManagerAPI.EnPumpModes.PUMP_MODE_FULL, MT5_CONNECT_TIMEOUT);
        if (res != MTRetCode.MT_RET_OK)
        {
            string message = string.Format(res.ToString());

            Console.WriteLine("Connect failed  ({0})"+ message);
            
            m_admin.LoggerOut(EnMTLogCode.MTLogErr, "Connection failed ({0})  , error can be incorrect login or password", res);
            SMTManagerAPIFactory.Shutdown();
            m_admin.Release();
            return (false);
        }
        Console.WriteLine("Connection success");

        return (true);
    }

    public void LoggedInManager()
    {
        CreateManager("./trades");

        if (m_admin != null)
        {

            ulong login = ulong.Parse(Environment.GetEnvironmentVariable("MT5_LOGIN")!);
            string? password = Environment.GetEnvironmentVariable("MT5_PASSWORD");
            string? ip = Environment.GetEnvironmentVariable("MT5_SERVER");

            if (password != null && ip != null)
            {
                if (Login(ip, login, password))
                {
                    Console.WriteLine("LOGIN  success");

                }
                else
                {
                    Console.WriteLine("LOGIN  failed invalid credentials");
                }
            }

        }

    }

}
