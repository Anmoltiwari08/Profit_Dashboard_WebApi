
namespace Bonus_Implementation_Policy_WebApi.Models
{
    public class Users
    {
       public string? name { get; set; } = "";
       public ulong Login { get; set; }
       public DateTime RegisteredDate { get; set; } 
       public string? Group { get; set; } ="";
       public uint Leverage { get; set; } = 0;
       public double Deposit { get; set; } = 0;
       public double Balance { get; set; } =0;
       public double Bonus { get; set; } =0;
       public double Floating { get; set; } =0;
       public double Profit { get; set; } =0;
       public double Margin { get; set; } =0;
       public double Equity { get; set; } =0;
       public double Max_Withdrawable_amount {get; set;} = 0;
       public double Extra_Profit {get; set;} = 0;
       public double Withdrawled_processed {get;set ;} = 0;       
       

    }
}
